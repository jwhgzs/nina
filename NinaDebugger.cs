namespace Nina;

static class NinaDebugger {
    private static Dictionary<string, long> timers = new Dictionary<string, long>();
    public static long time() {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return (long) (ts.TotalMilliseconds);
    }
    public static void timer_start(string _tag = "untagged") {
        if (timers.ContainsKey(_tag)) {
            NinaError.error("指定的计时器标签已存在.", 465513);
        }
        timers[_tag] = time();
    }
    public static void timer_stop(string _tag = "untagged") {
        if (! timers.ContainsKey(_tag)) {
            NinaError.error("指定的计时器标签不存在.", 984015);
        }
        long ms = time() - timers[_tag];
        Console.WriteLine("[Nina 调试器] 计时器:" + _tag + " 计时: " + ms + " ms");
        timers.Remove(_tag);
    }
    public static void print_codeBlocks(List<NinaCodeBlock> _blocks) {
        int i = 0;
        foreach (NinaCodeBlock v in _blocks) {
            Console.Write(String.Format("{0, -5}", i));
            Console.Write(String.Format("{0, -12}",
                "(" + (v.line + 1) + ", " + (v.col + 1) + ")"));
            Console.Write(String.Format("{0, -15}", v.type));
            if (v.val_op != null)
                Console.Write(String.Format("{0, -10}", v.val_op));
            else if (v.val_sy != null)
                Console.Write(String.Format("{0, -10}", v.val_sy));
            else if (v.val_kw != null)
                Console.Write(String.Format("{0, -10}", v.val_kw));
            else
                Console.Write(String.Format("{0, -10}", ""));
            Console.Write(v.code);
            Console.WriteLine();
            ++ i;
        }
    }
    public static void read_ILCode(byte[] _arr, ref int _i) {
        byte b = _arr[_i ++];
        if (b == 0xfe) {
            ++ _i;
        }
    }
}