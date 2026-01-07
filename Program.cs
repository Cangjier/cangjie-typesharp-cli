using Cangjie.TypeSharp;
using Cangjie.TypeSharp.Cli;
using Cangjie.TypeSharp.FullNameScript;
using Cangjie.TypeSharp.System;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TidyHPC.LiteJson;
using TidyHPC.Loggers;
using TidyHPC.Routers.Args;
using CLIUtil = Cangjie.TypeSharp.Cli.Util;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
    var path = Environment.ProcessPath;
    if (path == null)
    {
        return;
    }
    if (path.EndsWith(".bak.exe") == true)
    {
        return;
    }
    var updatePath = path + ".update";
    if (File.Exists(updatePath))
    {
        var bakPath = Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path) + ".bak.exe");
        if (File.Exists(bakPath) == false)
        {
            File.Copy(path, bakPath);
        }
        var script = """
        await Task.Delay(1000);
        let fileName = Path.GetFileNameWithoutExtension(Environment.ProcessPath);
        fileName = Path.GetFileNameWithoutExtension(fileName)+".exe";
        let path = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath),fileName);
        let updatePath = path + ".update";
        File.Copy(updatePath,path,true);
        File.Delete(updatePath);
        """;
        var scriptPath = Path.Combine(Path.GetTempPath(), "update-tscl.bak.ts");
        File.WriteAllText(scriptPath, script, CLIUtil.UTF8);
        staticContext.start(new()
        {
            filePath = bakPath,
            arguments = [
                "run",
                scriptPath
            ]
        });
    }
};

void help()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  tscl [run] <script_path> <script_args>");
    Console.WriteLine("  tscl [run] <script_url> <script_args>");
    Console.WriteLine("  tscl [run] <script_name> <script_args>");
    Console.WriteLine("  tscl [list]");
    Console.WriteLine("  tscl [update]");
    Console.WriteLine("  tscl [init]");
    Console.WriteLine("  tscl [api] -i <input_path> -o <output_path> -a <args_path>");
    Console.WriteLine("  tscl [viz] <port>");
    Console.WriteLine("  tscl");
    Console.WriteLine("Options:");
    Console.WriteLine("  run: run script");
    Console.WriteLine("  list: list script names in repository");
    Console.WriteLine("  update: update tsc");
    Console.WriteLine("  init: init project");
    Console.WriteLine("  api: request http/https api");
    Console.WriteLine("  viz: start viz server");
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
    Console.WriteLine($" tscl {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.WriteLine($" where {Environment.ProcessPath}");
}

ArgsRouter argsRouter = new();
argsRouter.Register(["run"], CommonCommands.Run);
argsRouter.Register(["where"], CommonCommands.Where);
argsRouter.Register(["service"], async (
    [Args] string[] fullArgs) =>
{
    string[] routeArgs = ["run", "service", .. fullArgs[1..]];
    await argsRouter.Route(routeArgs);
});
argsRouter.Register(["app"], async (
    [Args] string[] fullArgs) =>
{
    string[] routeArgs = ["run", "app", .. fullArgs[1..]];
    await argsRouter.Route(routeArgs);
});
argsRouter.Register(["list"], CommonCommands.ListScripts);
argsRouter.Register(["eval"], async (
    [SubArgs] string[] subArgs) =>
{
    Context context = new();
    for (int i = 0; i < subArgs.Length; i++)
    {
        var result = await TSScriptEngine.RunAsync(string.Empty, subArgs[i], context, stepContext =>
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
    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
    {
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
        staticContext.start(new()
        {
            filePath = scriptPath
        });
    }
    else
    {
        var result = await staticContext.cmdAsync(Environment.CurrentDirectory, "git config http.proxy");
        if (result.output.Length > 0)
        {
            var proxy = result.output;
            await staticContext.cmdAsync(Directory.GetCurrentDirectory(), $"wget -e \"https_proxy={proxy}\" --no-cache -qO- https://raw.githubusercontent.com/Cangjier/type-sharp/main/install.sh | bash");
        }
        else
        {
            await staticContext.cmdAsync(Directory.GetCurrentDirectory(), "wget --no-cache -qO- https://raw.githubusercontent.com/Cangjier/type-sharp/main/install.sh | bash");
        }
    }
});
argsRouter.Register(["init"], async () =>
{
    await argsRouter.Route(["run", "init"]);
});
argsRouter.Register(["api"], ApiCommands.Run);
argsRouter.Register(["package"], async (
    [ArgsIndex] string path) =>
{
    // 对脚本进行打包成可执行文件
    // ----F974135D-D9A0-43E5-BEAD-4DA7FBD4DF34----
    // 支持path是File、Directory、仓库地址
    // 如果是File则直接打包
    var splitBytes = Cangjie.TypeSharp.Util.UTF8.GetBytes("----F974135D-D9A0-43E5-BEAD-4DA7FBD4DF34----");
    bool needDelete = false;
    var outputProgramPath = string.Empty;
    PackageFlag flag = PackageFlag.File;
    if (path.StartsWith("http://") || path.StartsWith("https://"))
    {
        flag = PackageFlag.Url;
    }
    else if (File.Exists(path))
    {
        flag = PackageFlag.File;
        outputProgramPath = Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileNameWithoutExtension(path) + ".exe");
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var tempFilePath = Path.Combine(tempDirectory, "index.ts");
        File.Copy(path, tempFilePath);
        path = tempDirectory;
        needDelete = true;
    }
    else
    {
        flag = PackageFlag.Directory;
        outputProgramPath = Path.Combine(Path.GetDirectoryName(path) ?? "", Path.GetFileName(path) + ".exe");
    }
    // 将path下所有文件进行打包成zip流的bytes
    byte[] contentBytes;
    if (flag == PackageFlag.File || flag == PackageFlag.Directory)
    {
        using (MemoryStream memoryStream = new())
        {
            using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create, true))
            {
                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var entryPath = file.Replace(path, "").TrimStart(Path.DirectorySeparatorChar);
                    Console.WriteLine($"add {entryPath}");
                    var entry = zipArchive.CreateEntry(entryPath, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(file);
                    await fileStream.CopyToAsync(entryStream);
                }
            }
            contentBytes = memoryStream.ToArray();
        }
    }
    else
    {
        contentBytes = CLIUtil.UTF8.GetBytes(path);
    }
    byte[] md5Bytes;
    // 使用 MD5 计算哈希值
    using (MD5 md5 = MD5.Create())
    {
        md5Bytes = md5.ComputeHash(contentBytes);
    }
    // 将zipBytes进行base64编码
    var programBytes = File.ReadAllBytes(Environment.ProcessPath ?? throw new NullReferenceException());
    // 将base64String写入到程序的末尾
    using var outputProgramStream = File.OpenWrite(outputProgramPath);
    await outputProgramStream.WriteAsync(programBytes);
    await outputProgramStream.WriteAsync(splitBytes);
    await outputProgramStream.WriteAsync(contentBytes);
    await outputProgramStream.WriteAsync(BitConverter.GetBytes(contentBytes.Length));
    await outputProgramStream.WriteAsync(md5Bytes); // 16 bytes
    await outputProgramStream.WriteAsync(BitConverter.GetBytes((int)flag));
    await outputProgramStream.WriteAsync(splitBytes);
    await outputProgramStream.FlushAsync();
    if (needDelete)
    {
        Directory.Delete(path, true);
    }
});
argsRouter.Register([string.Empty], async () =>
{
    var axios = new Axios(new Context()
    {
        Logger = Logger.LoggerFile
    });
    await Task.CompletedTask;
    var splitBytes = Cangjie.TypeSharp.Util.UTF8.GetBytes("----F974135D-D9A0-43E5-BEAD-4DA7FBD4DF34----");
    // 读取程序的末尾，判断是否是splitBytes
    string mainPath = string.Empty;
    using (var processStream = File.OpenRead(Environment.ProcessPath ?? throw new NullReferenceException()))
    {
        processStream.Seek(-splitBytes.Length, SeekOrigin.End);
        byte[] lastBytes = new byte[splitBytes.Length];
#if NET10_0_OR_GREATER
        await processStream.ReadExactlyAsync(lastBytes);
#else
        await processStream.ReadAsync(lastBytes);
#endif
        if (lastBytes.SequenceEqual(splitBytes))
        {
            // 读取标识符
            int offset = -splitBytes.Length - sizeof(int);
            processStream.Seek(offset, SeekOrigin.End);
            byte[] flagBytes = new byte[sizeof(int)];
#if NET10_0_OR_GREATER
            await processStream.ReadExactlyAsync(flagBytes);
#else
            await processStream.ReadAsync(flagBytes);
#endif
            PackageFlag flag = (PackageFlag)BitConverter.ToInt32(flagBytes);
            // 读取md5 bytes
            offset += -16;
            processStream.Seek(offset, SeekOrigin.End);
            byte[] md5Bytes = new byte[16];
#if NET10_0_OR_GREATER
            await processStream.ReadExactlyAsync(md5Bytes);
#else
            await processStream.ReadAsync(md5Bytes);
#endif
            var md5Hex = CLIUtil.BytesToHexString(md5Bytes);
            var tempDirectory = Path.Combine(Path.GetTempPath(), md5Hex);
            if (File.Exists(Path.Combine(tempDirectory, ".lock")))
            {
                if (flag == PackageFlag.Url)
                {
                    if (Directory.Exists(Path.Combine(tempDirectory, ".git")))
                    {
                        GitRepository repository = new();
                        repository.GitRepositoryDirectory = tempDirectory;
                        await repository.Update();
                    }
                }
            }
            else
            {
                // 读取contentBytes的长度
                offset += -sizeof(int);
                processStream.Seek(offset, SeekOrigin.End);
                byte[] lengthBytes = new byte[sizeof(int)];
#if NET10_0_OR_GREATER
                await processStream.ReadExactlyAsync(lengthBytes);
#else
                await processStream.ReadAsync(lengthBytes);
#endif
                int contentLength = BitConverter.ToInt32(lengthBytes);

                // 读取contentBytes
                offset += -contentLength;
                processStream.Seek(offset, SeekOrigin.End);
                byte[] contentBytes = new byte[contentLength];
#if NET10_0_OR_GREATER
                await processStream.ReadExactlyAsync(contentBytes);
#else
                await processStream.ReadAsync(contentBytes);
#endif
                // 读取splitBytes
                offset += -splitBytes.Length;
                processStream.Seek(offset, SeekOrigin.End);
                byte[] splitBytesBytes = new byte[splitBytes.Length];
#if NET10_0_OR_GREATER
                await processStream.ReadExactlyAsync(splitBytesBytes);
#else
                await processStream.ReadAsync(splitBytesBytes);
#endif
                if (splitBytesBytes.SequenceEqual(splitBytes))
                {
                    Directory.CreateDirectory(tempDirectory);
                    if (flag == PackageFlag.File || flag == PackageFlag.Directory)
                    {
                        // 解压zipBytes
                        using MemoryStream memoryStream = new(contentBytes);
                        using ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Read, true);
                        zipArchive.ExtractToDirectory(tempDirectory);
                    }
                    else
                    {
                        var url = CLIUtil.UTF8.GetString(contentBytes);
                        // 判断是否是.git结尾
                        if (url.EndsWith(".git"))
                        {
                            var gitRepository = new GitRepository();
                            gitRepository.RepositoryUrl = url;
                            gitRepository.GitRepositoryDirectory = tempDirectory;
                            await gitRepository.Update();
                        }
                        else
                        {
                            await axios.download(url, Path.Combine(tempDirectory, "index.ts"));
                        }
                        File.Create(Path.Combine(tempDirectory, ".lock")).Close();
                    }
                }
                else
                {
                    Console.WriteLine("Package is invalid");
                }
            }
            string[] mainFileNames = ["main.ts", "index.ts"];
            foreach (var mainFileName in mainFileNames)
            {
                if (File.Exists(Path.Combine(tempDirectory, mainFileName)))
                {
                    mainPath = Path.Combine(tempDirectory, mainFileName);
                    break;
                }
            }
            if (mainPath == string.Empty)
            {
                Console.WriteLine("Package main.ts or index.ts not found");
            }
        }
    }

    if (File.Exists(mainPath))
    {
        await argsRouter.Route(["run", mainPath]);
    }
    else
    {
        help();
    }
});
argsRouter.Register(["text"], async (
    [ArgsIndex] string scriptPath
    ) =>
{
    await Task.CompletedTask;
    var interfaces = typescript.getAllInterfaces(File.ReadAllText(scriptPath, Encoding.UTF8));
    Console.WriteLine(interfaces);
});

Logger.Info($"tscl {Assembly.GetExecutingAssembly().GetName().Version}");
Console.CancelKeyPress += (sender, e) =>
{
    Environment.Exit(0);
};
await argsRouter.Route(args);
Logger.Info("Waiting for logger to be empty");
await Logger.LoggerFile.QueueLogger.WaitForEmpty(TimeSpan.FromSeconds(1));
Logger.Info("Logger is empty");
