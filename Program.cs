using TidyHPC.Routers.Args;
using Cangjie.TypeSharp;
using Cangjie.TypeSharp.System;
using Cangjie.TypeSharp.Cli;
using CLIUtil = Cangjie.TypeSharp.Cli.Util;

TSScriptEngine.Run("""
    for(let item of test()){
        console.log(item);
    }
    """);

return;

ArgsRouter argsRouter = new();
argsRouter.Register(["run"], async (
    [ArgsIndex]string path) =>
{
    context.args = args.Skip(2).ToArray();
    if (File.Exists(path))
    {
        context.script_path = Path.GetFullPath(path);
        await TSScriptEngine.RunAsync(File.ReadAllText(context.script_path, CLIUtil.UTF8));
    }
    else if (Directory.Exists(path))
    {
        var files = Directory.GetFiles(path, "*.ts", SearchOption.AllDirectories);
        // 找到main.ts或者index.ts文件
        var mainFile = files.FirstOrDefault(item => Path.GetFileName(item).ToLower() == "main.ts" || Path.GetFileName(item).ToLower() == "index.ts");
        if (mainFile == null)
        {
            Console.WriteLine("main.ts or index.ts not found");
            return;
        }
        context.script_path = Path.GetFullPath(mainFile);
        await TSScriptEngine.RunAsync(File.ReadAllText(context.script_path, CLIUtil.UTF8));
    }
    else if (path.StartsWith("http://") || path.StartsWith("https://"))
    {
        context.script_path = path;
        var url = CLIUtil.GetRawUrl(path);
        var content = await CLIUtil.HttpGetAsString(url);
        Console.WriteLine($"get script from {url}");
        await TSScriptEngine.RunAsync(content);
    }
    else
    {
        GitRepository gitRepository = new();
        await gitRepository.Update();
        var listResult = gitRepository.List();
        if (listResult.Contains(path) == false)
        {
            Console.WriteLine($"script {path} not found");
            return;
        }
        var scriptPath = gitRepository.GetScriptPath(path);
        if (scriptPath == null)
        {
            Console.WriteLine($"script {path} not found");
            return;
        }
        context.script_path = scriptPath;
        await TSScriptEngine.RunAsync(File.ReadAllText(context.script_path, CLIUtil.UTF8));
    }
});
argsRouter.Register(["list"],async () =>
{
    GitRepository gitRepository = new();
    await gitRepository.Update();
    var listResult = gitRepository.List().Where(item => item != ".tsc");
    int index = 0;
    int count = listResult.Count();
    foreach (var item in listResult)
    {
        Console.WriteLine($"{index++,3}/{count,-3}{item}");
    }
});
argsRouter.Register(["eval"], async (
    [SubArgs] string[] subArgs) =>
{

    for (int i = 0; i < subArgs.Length; i++)
    {
        var result = await TSScriptEngine.RunAsync(subArgs[i], stepContext =>
        {
            stepContext.UsingNamespaces.Add("TidyHPC.LiteJson");
            stepContext.UsingNamespaces.Add("System.Text");
            stepContext.UsingNamespaces.Add("System.IO");
        }, runtimeContext =>
        {

        });
        if (subArgs[i].Contains(";"))
        {
            
            Console.WriteLine($"[{i}]: multiply line");
        }
        else
        {
            Console.WriteLine($"[{i}]: {result}");
        }
    }
});

argsRouter.Register(["update"], async () =>
{
    await Task.CompletedTask;
    if(Environment.OSVersion.Platform== PlatformID.Win32NT)
    {
        //https://github.com/Cangjier/type-sharp/releases/download/latest/tscl.exe
        var script = $"""
        @echo off
        setlocal
        set "url=https://github.com/Cangjier/type-sharp/releases/download/latest/tscl.exe"
        set "target={Environment.ProcessPath}"
        set "tempPath={Path.GetTempFileName()}"
        echo download %url% to %tempPath%
        powershell -Command "Invoke-WebRequest -Uri '%url%' -OutFile '%tempPath%'"

        if exist "%tempPath%" (
            :: move temp file to target
            move /y "%tempPath%" "%target%"
            echo download success.
        ) else (
            echo download failed.
        )
        endlocal
        pause
        
        """;
        var scriptPath = Path.Combine(Path.GetTempPath(), "update-tscl.bat");
        await File.WriteAllTextAsync(scriptPath, script, CLIUtil.UTF8);
        context.start(scriptPath);
    }
    else
    {
        context.startCmd(Directory.GetCurrentDirectory(), "wget --no-cache -qO- https://raw.githubusercontent.com/Cangjier/type-sharp/main/install.sh | bash");
    }
    
    Environment.Exit(0);
});
argsRouter.Register(["init"],async () =>
{
    await argsRouter.Route(["run", "init"]);
});
argsRouter.Register(["api"], ApiCommands.Run);
argsRouter.Register([string.Empty], async () =>
{
    await Task.CompletedTask;
    Console.WriteLine("TypeSharp CLI 1.8.20241001.15");
    Console.WriteLine("Usage:");
    Console.WriteLine("  tscl [run] <script_path> <script_args>");
    Console.WriteLine("  tscl [run] <script_url> <script_args>");
    Console.WriteLine("  tscl [run] <script_name> <script_args>");
    Console.WriteLine("  tscl [list]");
    Console.WriteLine("  tscl [update]");
    Console.WriteLine("  tscl [init]");
    Console.WriteLine("  tscl [api] -i <input_path> -o <output_path> -a <args_path>");
    Console.WriteLine("  tscl");
    Console.WriteLine("Options:");
    Console.WriteLine("  run: run script");
    Console.WriteLine("  list: list script names in repository");
    Console.WriteLine("  update: update tsc");
    Console.WriteLine("  init: init project");
    Console.WriteLine("  api: request http/https api");
    Console.WriteLine("  script_path: script file path");
    Console.WriteLine("  script_url: script url");
    Console.WriteLine("  script_name: script name in respository");
    Console.WriteLine("  script_args: script arguments");
    Console.WriteLine("  input_path: input file path");
    Console.WriteLine("  output_path: output file path");
    Console.WriteLine("  args_path: arguments file path");
    Console.WriteLine("Examples:");
    Console.WriteLine("  tscl run ./main.ts");
    Console.WriteLine("  tscl run ./main.ts arg1 arg2");
    Console.WriteLine("  tscl run https://raw.githubusercontent.com/Cangjier/type-sharp/main/cli/create-react-component/main.ts");
    Console.WriteLine("  tscl run https://raw.githubusercontent.com/Cangjier/type-sharp/main/cli/create-react-component/main.ts arg1 arg2");
    Console.WriteLine("  tscl");
});
await argsRouter.Route(args);