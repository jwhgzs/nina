namespace Nina;

static class NinaCore {
    public static void Main(string[] _args) {
        if (_args.Length < 1) {
            NinaError.error("Nina needs source file's location.", 125433);
        }
        string src = _args[0];
        string code = "";
        try { code = File.ReadAllText(src); }
        catch {
            NinaError.error("Nina fails to read the source file.", 144178);
        }
        List<NinaCodeBlock> blocks = NinaCodeResolver.blocking(src, code);
        NinaCompiler.execute(blocks);
    }
}