// 用于将指定目录下的文件上传至Git-Release
import { Environment } from "../type-sharp/cli/.tsc/System/Environment";
import { cmdAsync, execAsync } from "../type-sharp/cli/.tsc/staticContext";
import { args } from "../type-sharp/cli/.tsc/Context";
import { File } from "../type-sharp/cli/.tsc/System/IO/File";
import { Console } from "../type-sharp/cli/.tsc/System/Console";

let main = async () => {
    let gitUrl = "https://github.com/Cangjier/type-sharp.git";
    let tagName = "latest";
    let toReleaseFiles = [
        `${Environment.CurrentDirectory}/bin/Release/net10.0/linux-x64/publish/tscl`,
        `${Environment.CurrentDirectory}/bin/Release/net10.0/win-x64/publish/tscl.exe`
    ];
    let token = "";
    if (File.Exists("token.txt")) {
        token = File.ReadAllText("token.txt");
    }
    if (token == "") {
        console.log(`Please input your token`);
        token = Console.ReadLine();
    }

    if (token == null || token == "") {
        console.log(`Token is empty`);
        return;
    }

    File.WriteAllText("token.txt", token);

    // 先编译
    let cmdLinuxX64 = "dotnet publish -p:PublishProfile=linux-x64 -f:net10.0 --no-cache";
    let cmdWinX64 = "dotnet publish -p:PublishProfile=win-x64 -f:net10.0 --no-cache";
    let cmdResult = await cmdAsync(Environment.CurrentDirectory, cmdLinuxX64,{redirect:true});
    if (cmdResult.exitCode != 0) {
        console.log(`cmd: ${cmdLinuxX64}`);
        console.log(cmdResult.output);
        console.log(cmdResult.error);
    }
    cmdResult = await cmdAsync(Environment.CurrentDirectory, cmdWinX64,{redirect:true});
    if (cmdResult.exitCode != 0) {
        console.log(`cmd: ${cmdWinX64}`);
        console.log(cmdResult.output);
        console.log(cmdResult.error);
    }
    console.log(`Is need to upload files?`);
    let isNeed = Console.ReadLine();
    if (isNeed != "y" && isNeed != "Y") return;
    await execAsync({
        filePath: Environment.ProcessPath,
        arguments: ["run", "gitapis", "release", gitUrl, tagName,
            "--files", toReleaseFiles.join(","),
            "--token", token]
    });
};

await main();