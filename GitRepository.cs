using Cangjie.TypeSharp.System;

namespace Cangjie.TypeSharp.Cli;
public class GitRepository
{
    private string GitRepositoryDirectory { get; } = Path.Combine(Path.GetTempPath(), "553809D6-8840-47B7-BDCA-AE6A17518B86");

    private string CliDirectory => Path.Combine(GitRepositoryDirectory, "cli");

    public async Task Update()
    {
        if (!Directory.Exists(GitRepositoryDirectory))
        {
            Directory.CreateDirectory(GitRepositoryDirectory);
        }
        // 判断是否存在.git文件夹，如果不存在，则执行git clone
        var gitDirectory = Path.Combine(GitRepositoryDirectory, ".git");
        if (Directory.Exists(gitDirectory) == false)
        {
            await context.cmdAsync(GitRepositoryDirectory, "git clone --depth 1 https://github.com/Cangjier/type-sharp.git .");
        }
        // 执行git pull
        await context.cmdAsync(GitRepositoryDirectory, "git pull");
    }

    public IEnumerable<string> List()
    {
        if (Directory.Exists(CliDirectory) == false) return [];
        return Directory.GetDirectories(CliDirectory).Select(item => Path.GetFileName(item));
    }

    public string? GetScriptPath(string scriptName)
    {
        var scriptDirectory = Path.Combine(CliDirectory, scriptName);
        if (Directory.Exists(scriptDirectory) == false) return null;
        var files = Directory.GetFiles(scriptDirectory, "*.ts");
        var mainFile = files.FirstOrDefault(item => Path.GetFileName(item).ToLower() == "main.ts" || Path.GetFileName(item).ToLower() == "index.ts");
        return mainFile;
    }
}
