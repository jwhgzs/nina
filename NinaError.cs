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
    public static void error(string _msg, int _uniqueCode, NinaErrorPosition? _pos = null) {
        string err = "";
        err += "[Nina Error #" + _uniqueCode + "] ";
        if (_pos != null)
            err += "at " + _pos.Value.file + " ("
                + (_pos.Value.line + 1) + ", " + (_pos.Value.col + 1) + ")";
        err += "\n" + _msg;
        Console.WriteLine(err);
        Environment.Exit(- 1);
    }
}