namespace Nina;

static class NinaCore {
    public static object? execute(
            string _src, string _code, object? _arg = null) {
        List<NinaCodeBlock> blocks = NinaCodeResolver.blocking(_src, _code);
        NinaASTBlockExpression ast = NinaCompiler.compile(_src, blocks);
        return NinaILCompiler.execute(ast, _arg);
    }
    public static void execute(string _src) {
        string code = "";
        try {
            code = File.ReadAllText(_src);
        }
        catch {
            NinaError.error("Nina 读取源文件失败.", 144178);
        }
        execute(_src, code);
    }
}