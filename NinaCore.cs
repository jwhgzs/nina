// #define MODE_DEBUG

using System.Reflection;

namespace Nina;

static class NinaCore {
    public static object? execute(
            string _src, string _code, object? _arg = null) {
        List<NinaCodeBlock> blocks = NinaCodeResolver.blocking(_src, _code);
        NinaASTBlockExpression ast = NinaCompiler.compile(_src, blocks);
        return NinaILCompiler.execute(ast, _arg);
    }
    public static void Main(string[] _args) {
        try {
            if (_args.Length < 1) {
                NinaError.error("Nina 需要源文件路径哦!", 125433);
            }
            string src = _args[0];
            string code = "";
            try {
                code = File.ReadAllText(src);
            }
            catch {
                NinaError.error("Nina 读取源文件失败.", 144178);
            }
            execute(src, code);
        }
        #if ! MODE_DEBUG
        catch (TargetInvocationException tex) {
            Console.WriteLine(tex.InnerException!.Message);
            Environment.Exit(- 1);
        }
        #endif
        catch (Exception ex) {
            #if ! MODE_DEBUG
            Console.WriteLine(ex.Message);
            #else
            Console.WriteLine(ex.ToString());
            #endif
            Environment.Exit(- 1);
        }
    }
}