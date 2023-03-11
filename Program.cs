namespace Nina;

static class NinaCLI {
    public static void Main(string[] _args) {
        NinaError.env(() => {
            if (_args.Length > 0) {
                NinaCore.execute(_args[0]);
            }
            else {
                Console.WriteLine("[Nina CLI - Nina 命令行工具]");
                Console.WriteLine("* 命令列表：");
                Console.WriteLine("- R. 运行 Demo 源文件");
                Console.WriteLine("- C. 运行自定义源文件");
                Console.WriteLine("- E. 在这里编辑代码并运行");
                Console.Write("* 请输入命令字母代号：");
                string code = (Console.ReadLine() ?? "").ToUpper();
                if (code == "R") {
                    DirectoryInfo dir = new DirectoryInfo(NinaConstsProviderUtil.NINA_PATH_DEMOS);
                    FileInfo[] finfo = dir.GetFiles();
                    int i = 0;
                    foreach (FileInfo v in finfo) {
                        Console.WriteLine("- " + v.Name.Replace(".nina", "", true, null));
                        ++ i;
                    }
                    Console.Write("* 请输入 Demo 名称：");
                    string demo = (Console.ReadLine() ?? "").ToLower();
                    NinaCore.execute(NinaConstsProviderUtil.NINA_PATH_DEMOS + demo + ".nina");
                }
                else if (code == "C") {
                    Console.Write("* 请输入自定义源文件路径：");
                    string path = Console.ReadLine() ?? "";
                    NinaCore.execute(path);
                }
                else if (code == "E") {
                    Console.WriteLine("* 请在下面输入源代码，要结束输入请连续回车两次：");
                    string src = "";
                    string? buf = "";
                    while ((buf = Console.ReadLine()) != null && buf != "")
                        src += buf + "\n";
                    NinaCore.execute(
                        "[临时代码: " + Guid.NewGuid().ToString("N") + "]",
                        src
                    );
                }
                else {
                    NinaError.error("Nina 无法识别你的命令.", 679012);
                }
            }
        });
    }
}