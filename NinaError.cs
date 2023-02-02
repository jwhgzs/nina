namespace Nina;

public struct NinaErrorPosition {
    public string file;
    public int line, col;
    public NinaErrorPosition(string _file, int _line, int _col) {
        file = _file;
        line = _line;
        col = _col;
    }
}

public static class NinaError {
    public static void error(string _msg, int _uniqueCode, NinaErrorPosition? _pos = null,
            bool _vague = false) {
        string err = "";
        err += "[Nina Error";
        if (_uniqueCode >= 0)
            err += " #" + _uniqueCode;
        err += "] ";
        if (_pos != null) {
            int linei = _pos.Value.line + 1;
            int coli = _pos.Value.col + 1;
            string file = _pos.Value.file;
            string line = linei >= 0 ? linei.ToString() : "unknown";
            string col = coli >= 0 ? coli.ToString() : "unknown";
            if (! _vague)
                err += "at " + file + " (" + line + ", " + col + ")";
            else
                err += "at " + file + " , ABOUT at line " + line;
        }
        err += "\n" + _msg;
        Console.WriteLine(err);
        Environment.Exit(- 1);
    }
    public static void hot(string _msg, int _uniqueCode) {
        throw new Exception(_msg + " (#" + _uniqueCode + ")");
    }
}