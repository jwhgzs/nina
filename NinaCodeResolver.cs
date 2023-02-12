namespace Nina;

static class NinaCodeResolver {
    public static List<NinaCodeBlock> blocking(string _file, string _code) {
        char[] code = _code.ReplaceLineEndings("\n").ToCharArray();
        List<NinaCodeBlock> ret = new List<NinaCodeBlock>();
        int currLine = 0, currCol = - 1;
        int? bufLine, bufCol;
        bufLine = bufCol = null;
        string buf = "";
        string buf_str = "";
        char? quote = null;
        bool isBSlash = false;
        bool isLineComment, isBlockComment, isNumber, isIdentifier;
        bool isChineseIdentifier;
        isLineComment = isBlockComment = false;
        isNumber = isIdentifier = true;
        isChineseIdentifier = false;

        char v;
        string? vs, con;
        NinaSymbolType sy;
        NinaOperatorType op;
        int op_lv;
        bool isSymbol, isOperator, isVoid, isQuote;
        
        for (int i = 0; i <= code.Length; ++ i) {
            v = i < code.Length ? code[i] : '\0';
            if (v == '\n') {
                ++ currLine;
                currCol = - 1;
            }
            else if (v != '\0') {
                ++ currCol;
            }

            if (isLineComment) {
                if (v == '\n')
                    isLineComment = false;
            }
            else if (isBlockComment) {
                if (v == '/' && code[i - 1] == '*')
                    isBlockComment = false;
            }
            else if (quote != null) {
                if (bufLine == null) {
                    bufLine = currLine;
                    bufCol = currCol - 1;
                    buf += quote;
                }
                if (! isBSlash && v == quote) {
                    buf += quote;
                    ret.Add(
                        new NinaCodeBlock(
                            _file, (int) bufLine !, (int) bufCol !,
                            buf, NinaCodeBlockType.String,
                            _assert_str: buf_str
                        )
                    );
                    quote = null;
                    buf = buf_str = "";
                }
                else {
                    if (i == code.Length) {
                        NinaError.error(
                            "不成对的引号.", 413153,
                            new NinaErrorPosition(_file, currLine, currCol)
                        );
                    }
                    buf += v;
                    if (! isBSlash) {
                        if (v != '\\')
                            buf_str += v;
                        else
                            isBSlash = true;
                    }
                    else {
                        buf_str += NinaCodeBlockUtil.unescape(v);
                        isBSlash = false;
                    }
                }
            }
            else {
                isSymbol = NinaCodeBlockUtil.supposeSymbol(v, out sy);
                isOperator = NinaCodeBlockUtil.supposeOperator(v, out op, out op_lv);
                if (isOperator
                        && (
                            NinaCodeBlockUtil.operators_vagues
                                .ContainsKey(op)
                        )
                        && (
                            buf.Length == 0 && ret.Count > 0
                                ? (ret.Last().type == NinaCodeBlockType.Symbol
                                    || ret.Last().type == NinaCodeBlockType.Keyword
                                    || (ret.Last().type == NinaCodeBlockType.Operator
                                        && ret.Last().val_op != NinaOperatorType.BraR
                                        && ret.Last().val_op != NinaOperatorType.MBraR))
                                : false
                        )) {
                    op = NinaCodeBlockUtil.operators_vagues[op];
                    op_lv = NinaCodeBlockUtil.operatorsRank[op];
                }
                isVoid = NinaCodeBlockUtil.isVoid(v);
                isQuote = NinaCodeBlockUtil.isQuote(v);

                if (isSymbol || isOperator || isVoid || isQuote) {
                    if (buf.Length > 0) {
                        if (isNumber) {
                            if (ret.Count >= 2
                                    && ret[ret.Count - 2].val_num_dotted == false
                                    && ret.Last().val_op == NinaOperatorType.Dot) {
                                ret.RemoveAt(ret.Count - 1);
                                con = ret.Last().code + "." + buf;
                                ret.Add(
                                    new NinaCodeBlock(
                                        _file, ret.Last().line,
                                        ret.Last().col, con, NinaCodeBlockType.Number,
                                        _assert_num: double.Parse(con)
                                    )
                                );
                                ret.RemoveAt(ret.Count - 2);
                            }
                            else {
                                ret.Add(
                                    new NinaCodeBlock(
                                        _file, bufLine!.Value,
                                        bufCol!.Value, buf, NinaCodeBlockType.Number,
                                        _assert_num: double.Parse(buf)
                                    )
                                );
                            }
                        }
                        else if (NinaCodeBlockUtil.supposeKeyword(buf, out NinaKeywordType kw)) {
                            ret.Add(
                                new NinaCodeBlock(
                                    _file, bufLine!.Value,
                                    bufCol!.Value, buf, NinaCodeBlockType.Keyword,
                                    _assert_kw: kw
                                )
                            );
                        }
                        else if (NinaCodeBlockUtil.supposeOperator(buf, out NinaOperatorType op2,
                                out int op2_lv)) {
                            ret.Add(
                                new NinaCodeBlock(
                                    _file, bufLine!.Value,
                                    bufCol!.Value, buf, NinaCodeBlockType.Operator,
                                    _assert_op: op2, _assert_op_lv: op2_lv
                                )
                            );
                        }
                        else if (isIdentifier) {
                            ret.Add(
                                new NinaCodeBlock(
                                    _file, bufLine!.Value,
                                    bufCol!.Value, buf, NinaCodeBlockType.Identifier,
                                    _assert_id_chinese: isChineseIdentifier
                                )
                            );
                        }
                        else {
                            NinaError.error(
                                "不支持的语法.", 641981,
                                new NinaErrorPosition(_file, bufLine!.Value, bufCol!.Value)
                            );
                        }
                    }
                    
                    if (! isVoid && ! isQuote) {
                        if (isSymbol) {
                            vs = v.ToString();
                            ret.Add(new NinaCodeBlock(_file, currLine, currCol, vs,
                                NinaCodeBlockType.Symbol, _assert_sy: sy));
                        }
                        else if (isOperator) {
                            vs = v.ToString();
                            con = ret.Count > 0 && ret.Last().type == NinaCodeBlockType.Operator
                                ? ret.Last().code + vs : null;
                            if (op == NinaOperatorType.Div && ret.Count > 0
                                    && ret.Last().val_op == NinaOperatorType.Div) {
                                ret.RemoveAt(ret.Count - 1);
                                isLineComment = true;
                            }
                            else if (op == NinaOperatorType.Mut && ret.Count > 0
                                    && ret.Last().val_op == NinaOperatorType.Div) {
                                ret.RemoveAt(ret.Count - 1);
                                isBlockComment = true;
                            }
                            else if (con != null && NinaCodeBlockUtil.supposeOperator(
                                    con, out NinaOperatorType op2, out int op2_lv)) {
                                ret.Add(new NinaCodeBlock(_file, ret.Last().line,
                                    ret.Last().col, con, NinaCodeBlockType.Operator,
                                    _assert_op: op2, _assert_op_lv: op2_lv));
                                ret.RemoveAt(ret.Count - 2);
                            }
                            else {
                                ret.Add(new NinaCodeBlock(_file, currLine, currCol,
                                    vs, NinaCodeBlockType.Operator,
                                    _assert_op: op, _assert_op_lv: op_lv));
                            }
                        }
                    }
                    else if (isQuote) {
                        quote = v;
                    }

                    buf = buf_str = "";
                    isNumber = isIdentifier = true;
                    isChineseIdentifier = false;
                    bufLine = bufCol = null;
                }
                else {
                    if (bufLine == null) {
                        bufLine = currLine;
                        bufCol = currCol;
                    }
                    if (! char.IsNumber(v))
                        isNumber = false;
                    bool isChinese = v >= 0x4e00 && v <= 0x9fbb;
                    if (! (char.IsLetter(v) || (char.IsNumber(v) && buf.Length > 0)
                            || v == '$' || v == '_' || isChinese))
                        isIdentifier = false;
                    if (isChinese)
                        isChineseIdentifier = true;
                    buf += v;
                }
            }
        }

        return ret;
    }
}