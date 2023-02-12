namespace Nina;

struct NinaCodeBlock {
    public NinaCodeBlockType type;
    public string file, code;
    public int line, col;
    public double? val_num;
    public bool? val_num_dotted;
    public string? val_str;
    public bool? val_id_chinese;
    public NinaSymbolType? val_sy;
    public NinaKeywordType? val_kw;
    public bool? val_kw_chinese;
    public NinaOperatorType? val_op;
    public int? val_op_lv;
    public bool? val_op_unary;
    public bool? val_op_chinese;
    public NinaCodeBlock(string _file, int _line, int _col, string _code,
            NinaCodeBlockType _assert, NinaSymbolType? _assert_sy = null,
            NinaOperatorType? _assert_op = null, int? _assert_op_lv = null,
            NinaKeywordType? _assert_kw = null, double? _assert_num = null,
            string? _assert_str = null, bool? _assert_id_chinese = null) {
        type = _assert;
        file = _file;
        line = _line;
        col = _col;
        val_id_chinese = _assert_id_chinese;

        if (_assert == NinaCodeBlockType.String || _assert == NinaCodeBlockType.Identifier) {
            code = _code;
            val_num = null;
            val_num_dotted = null;
            val_str = _assert_str;
            val_sy = null;
            val_kw = null;
            val_kw_chinese = null;
            val_op = null;
            val_op_lv = null;
            val_op_unary = null;
            val_op_chinese = null;
        }
        else if (_assert == NinaCodeBlockType.Symbol) {
            code = _code;
            val_num = null;
            val_num_dotted = null;
            val_str = null;
            val_sy = _assert_sy;
            val_kw = null;
            val_kw_chinese = null;
            val_op = null;
            val_op_lv = null;
            val_op_unary = null;
            val_op_chinese = null;
        }
        else if (_assert == NinaCodeBlockType.Operator) {
            code = _code;
            val_num = null;
            val_num_dotted = null;
            val_str = null;
            val_sy = null;
            val_kw = null;
            val_kw_chinese = null;
            val_op = _assert_op;
            val_op_lv = _assert_op_lv;
            val_op_unary
                = NinaCodeBlockUtil.operators_unarys.Contains(
                    (NinaOperatorType) val_op !
                );
            val_op_chinese
                = NinaCodeBlockUtil.operators_chinese.ContainsKey(code);
        }
        else if (_assert == NinaCodeBlockType.Keyword) {
            code = _code;
            val_num = null;
            val_num_dotted = null;
            val_str = null;
            val_sy = null;
            val_kw = _assert_kw;
            val_kw_chinese
                = NinaCodeBlockUtil.keywords_chinese.ContainsKey(code);
            val_op = null;
            val_op_lv = null;
            val_op_unary = null;
            val_op_chinese = null;
        }
        else if (_assert == NinaCodeBlockType.Number) {
            code = _code;
            val_num = _assert_num;
            val_num_dotted
                = Math.Floor((double) val_num !) != val_num!.Value;
            val_str = null;
            val_sy = null;
            val_kw = null;
            val_kw_chinese = null;
            val_op = null;
            val_op_lv = null;
            val_op_unary = null;
            val_op_chinese = null;
        }
        else {
            code = _code;
            val_num = null;
            val_num_dotted = null;
            val_str = null;
            val_sy = null;
            val_kw = null;
            val_kw_chinese = null;
            val_op = null;
            val_op_lv = null;
            val_op_unary = null;
            val_op_chinese = null;
            NinaError.error("莫名其妙的错误.", 697790);
        }
    }
}