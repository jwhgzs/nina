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
    public const string header = "\n[Nina Error]\n";
    public static string trim_header(string _str) {
        return _str.StartsWith(header)
            ? _str.Substring(header.Length)
            : _str;
    }
    public static string gen_report(
            string _msg, int _uniqueCode,
            List<NinaErrorPosition> _posList) {
        string err = header;
        if (_uniqueCode >= 0)
            err += "(#" + _uniqueCode + ") ";
        string msg = _msg;
        err += trim_header(msg);
        for (int i = 0; i < _posList.Count; ++ i) {
            NinaErrorPosition v = _posList[i];
            int linei = v.line + 1;
            int coli = v.col + 1;
            string file = v.file;
            string line = linei >= 0 ? linei.ToString() : "unknown";
            string col = coli >= 0 ? coli.ToString() : "unknown";
            err += "\n\tat " + file + " ABOUT line " + line;
        }
        return err;
    }
    public static void error(string _msg, int _uniqueCode,
            List<NinaErrorPosition> _posList) {
        string err = gen_report(
            _msg, _uniqueCode,
            _posList
        );
        throw new Exception(err);
    }
    public static void error(string _msg, int _uniqueCode,
            NinaErrorPosition? _pos = null) {
        error(
            _msg, _uniqueCode,
            _pos == null
                ? new List<NinaErrorPosition>()
                : new List<NinaErrorPosition>() {
                    (NinaErrorPosition) _pos !
                }
        );
    }
}