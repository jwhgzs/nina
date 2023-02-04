using System.Reflection;

namespace Nina;

static class NinaConstsProviderUtil {
    public const string NINA_ID_PREFIX = "NinaGlobal__";
    public const string NINA_ANNO_SPECIALARG = "NINA_ANNO_SPECIALARG";
    public const string CSHARP_NINAAPI_PREFIX = "NinaAPI__";
    public const string CSHARP_NINAAPIUTIL_PREFIX = "NinaAPIUtil__";
    public const string IL_ENTRYCLASS_ID = "NinaEntry";
    public const string IL_BUILTIN_ID_PREFIX = "NINA_BUILTIN_";
    public const string IL_CLOSURECLASS_ID_PREFIX = "NINA_CLOSURECLASS_";
    public const string IL_CLOSURECLASS_FIELD_PREFIX = "NINA_CLOSURECLASS_FIELD_";
}

static class NinaCodeBlockUtil {
    public static Dictionary<char, NinaSymbolType> symbols
            = new Dictionary<char, NinaSymbolType> {
        [';'] = NinaSymbolType.Sem,
        ['{'] = NinaSymbolType.CBraL,
        ['}'] = NinaSymbolType.CBraR
    };
    public static Dictionary<string, NinaKeywordType> keywords
            = new Dictionary<string, NinaKeywordType> {
        ["var"] = NinaKeywordType.Var,
        ["const"] = NinaKeywordType.Const,
        ["func"] = NinaKeywordType.Func,
        ["if"] = NinaKeywordType.If,
        ["else"] = NinaKeywordType.Else,
        ["elseif"] = NinaKeywordType.Elseif,
        ["while"] = NinaKeywordType.While,
        ["return"] = NinaKeywordType.Return,
        ["break"] = NinaKeywordType.Break,
        ["continue"] = NinaKeywordType.Continue,
        ["try"] = NinaKeywordType.Try,
        ["catch"] = NinaKeywordType.Catch
    };
    public static List<string> specialIdentifiers
            = new List<string> {
        "self", "this", "argument", "exception"
    };
    public static Dictionary<string, NinaOperatorType> operators
            = new Dictionary<string, NinaOperatorType> {
        ["+"] = NinaOperatorType.Add,
        ["-"] = NinaOperatorType.Sub,
        ["*"] = NinaOperatorType.Mut,
        ["/"] = NinaOperatorType.Div,
        ["%"] = NinaOperatorType.Rem,
        ["**"] = NinaOperatorType.Pow,
        ["="] = NinaOperatorType.Equ,
        ["("] = NinaOperatorType.BraL,
        [")"] = NinaOperatorType.BraR,
        ["["] = NinaOperatorType.MBraL,
        ["]"] = NinaOperatorType.MBraR,
        ["."] = NinaOperatorType.Dot,
        [","] = NinaOperatorType.Com,
        ["&"] = NinaOperatorType.And,
        ["|"] = NinaOperatorType.Or,
        ["^"] = NinaOperatorType.XOr,
        ["~"] = NinaOperatorType.Not,
        ["&&"] = NinaOperatorType.LAnd,
        ["||"] = NinaOperatorType.LOr,
        ["!"] = NinaOperatorType.LNot,
        ["=="] = NinaOperatorType.LEqu,
        ["!="] = NinaOperatorType.LNEqu,
        [">"] = NinaOperatorType.More,
        ["<"] = NinaOperatorType.Less,
        [">="] = NinaOperatorType.MoreE,
        ["<="] = NinaOperatorType.LessE,
        ["typeof"] = NinaOperatorType.Typeof,
        ["=>"] = NinaOperatorType.Arr,
        ["<<"] = NinaOperatorType.SftL,
        [">>"] = NinaOperatorType.SftR,
        ["object"] = NinaOperatorType.Object,
        ["array"] = NinaOperatorType.Array,
        ["@"] = NinaOperatorType.At
    };
    public static Dictionary<char, NinaOperatorType> operators_ch
            = new Dictionary<char, NinaOperatorType> {
        ['+'] = NinaOperatorType.Add,
        ['-'] = NinaOperatorType.Sub,
        ['*'] = NinaOperatorType.Mut,
        ['/'] = NinaOperatorType.Div,
        ['%'] = NinaOperatorType.Rem,
        ['='] = NinaOperatorType.Equ,
        ['('] = NinaOperatorType.BraL,
        [')'] = NinaOperatorType.BraR,
        ['['] = NinaOperatorType.MBraL,
        [']'] = NinaOperatorType.MBraR,
        ['.'] = NinaOperatorType.Dot,
        [','] = NinaOperatorType.Com,
        ['&'] = NinaOperatorType.And,
        ['|'] = NinaOperatorType.Or,
        ['^'] = NinaOperatorType.XOr,
        ['~'] = NinaOperatorType.Neg,
        ['!'] = NinaOperatorType.LNot,
        ['>'] = NinaOperatorType.More,
        ['<'] = NinaOperatorType.Less,
        ['@'] = NinaOperatorType.At
    };
    public static Dictionary<NinaOperatorType, int> operatorsRank
            = new Dictionary<NinaOperatorType, int> {
        [NinaOperatorType.None] = 0,
        [NinaOperatorType.Com] = 1,
        [NinaOperatorType.Equ] = 2,
        [NinaOperatorType.LOr] = 3,
        [NinaOperatorType.LAnd] = 4,
        [NinaOperatorType.Or] = 5,
        [NinaOperatorType.XOr] = 6,
        [NinaOperatorType.And] = 7,
        [NinaOperatorType.LEqu] = 8,
        [NinaOperatorType.LNEqu] = 8,
        [NinaOperatorType.More] = 8,
        [NinaOperatorType.Less] = 8,
        [NinaOperatorType.MoreE] = 8,
        [NinaOperatorType.LessE] = 8,
        [NinaOperatorType.SftL] = 9,
        [NinaOperatorType.SftR] = 9,
        [NinaOperatorType.Add] = 10,
        [NinaOperatorType.Sub] = 11,
        [NinaOperatorType.Mut] = 12,
        [NinaOperatorType.Div] = 13,
        [NinaOperatorType.Rem] = 14,
        [NinaOperatorType.Pow] = 15,
        [NinaOperatorType.Arr] = 16,
        [NinaOperatorType.Not] = 17,
        [NinaOperatorType.Pos] = 17,
        [NinaOperatorType.Neg] = 17,
        [NinaOperatorType.LNot] = 17,
        [NinaOperatorType.Typeof] = 17,
        [NinaOperatorType.Object] = 17,
        [NinaOperatorType.Array] = 17,
        [NinaOperatorType.At] = 17,
        [NinaOperatorType.BraL] = 18,
        [NinaOperatorType.BraR] = 18,
        [NinaOperatorType.MBraL] = 18,
        [NinaOperatorType.MBraR] = 18,
        [NinaOperatorType.Dot] = 18
    };
    public static List<NinaOperatorType> operators_unarys
            = new List<NinaOperatorType>() {
                NinaOperatorType.Not,
                NinaOperatorType.Pos,
                NinaOperatorType.Neg,
                NinaOperatorType.LNot,
                NinaOperatorType.Typeof,
                NinaOperatorType.Object,
                NinaOperatorType.Array,
                NinaOperatorType.At
            };
    public static Dictionary<NinaOperatorType, NinaOperatorType> operators_vagues
            = new Dictionary<NinaOperatorType, NinaOperatorType>() {
                [NinaOperatorType.Add] = NinaOperatorType.Pos,
                [NinaOperatorType.Sub] = NinaOperatorType.Neg
            };
    public static bool supposeSymbol(char _ch, out NinaSymbolType _out) {
        return symbols.TryGetValue(_ch, out _out);
    }
    public static bool supposeKeyword(string _code, out NinaKeywordType _out) {
        return keywords.TryGetValue(_code, out _out);
    }
    public static bool supposeOperator(string _code, out NinaOperatorType _out, out int _lv) {
        bool ok = operators.TryGetValue(_code, out _out);
        if (ok) operatorsRank.TryGetValue(_out, out _lv);
        else operatorsRank.TryGetValue((int) NinaOperatorType.None, out _lv);
        return ok;
    }
    public static bool supposeOperator(char _ch, out NinaOperatorType _out, out int _lv) {
        bool ok = operators_ch.TryGetValue(_ch, out _out);
        if (ok) operatorsRank.TryGetValue(_out, out _lv);
        else operatorsRank.TryGetValue((int) NinaOperatorType.None, out _lv);
        return ok;
    }
    public static bool isVoid(char _ch) {
        return _ch == ' ' || _ch == '\n' || _ch == '\t' || _ch == '\0';
    }
    public static bool isQuote(char _ch) {
        return _ch == '"' || _ch == '\'';
    }
    public static char unescape(char _ch) {
        switch (_ch) {
            case 'n':
                return '\n';
            case 't':
                return '\t';
            case '"':
                return '"';
            case '\'':
                return '\'';
            default:
                return _ch;
        }
    }
}

static class NinaCompilerUtil {
    public static string format_identifier(string _id) {
        if (NinaCodeBlockUtil.specialIdentifiers.Contains(_id))
            return _id;
        return NinaConstsProviderUtil.NINA_ID_PREFIX + _id;
    }
    public static string unformat_identifier(string _id) {
        if (_id.StartsWith(NinaConstsProviderUtil.NINA_ID_PREFIX))
            return _id.Remove(0, NinaConstsProviderUtil.NINA_ID_PREFIX.Length);
        return _id;
    }
    public static NinaASTBlockExpression? resolve_elses(
            List<(ANinaASTExpression, NinaASTBlockExpression)> _list,
            NinaCodeBlock _posBlock) {
        NinaASTIfStatement? ret = null;
        for (int i = _list.Count - 1; i >= 0; -- i) {
            var (cond, block) = _list[i];
            NinaASTIfStatement nif = new NinaASTIfStatement(
                _expr: cond,
                _block: block,
                block.pos
            );
            if (ret != null) {
                nif.block_else = new NinaASTBlockExpression(
                    new List<ANinaASTStatement>() {
                        ret
                    },
                    block.pos
                );
            }
            ret = nif;
        }
        return ret != null
            ? new NinaASTBlockExpression(
                new List<ANinaASTStatement>() {
                    ret
                },
                ret.pos
            )
            : new NinaASTBlockExpression(
                new NinaErrorPosition(
                    _posBlock.file, _posBlock.line, _posBlock.col
                )
            );
    }
    public static NinaASTSuperListExpression transfer_list2params(
            NinaASTListExpression _list, NinaCodeBlock _posBlock) {
        List<(string, ANinaASTExpression?)> ret
            = new List<(string, ANinaASTExpression?)>();
        List<ANinaASTExpression> list = _list.list;
        for (int i = 0; i < list.Count; ++ i) {
            ANinaASTExpression v = list[i];
            if (v.type == NinaOperatorType.Equ
                    && v is NinaASTBinaryExpression binary
                    && binary.expr_l is NinaASTIdentifierExpression id) {
                ret.Add(
                    (id.name, binary.expr_r)
                );
            }
            else if (v is NinaASTIdentifierExpression id2) {
                ret.Add(
                    (id2.name, null)
                );
            }
            else {
                NinaError.error(
                    "形参列表表达式中有无效表达式.",
                    102931,
                    new NinaErrorPosition(_posBlock.file,
                        _posBlock.line, _posBlock.col)
                );
            }
        }
        return new NinaASTSuperListExpression(
            ret,
            new NinaErrorPosition(_posBlock.file,
                _posBlock.line, _posBlock.col)
        );
    }
    public static NinaASTSuperListExpression transfer_list2params(
            ANinaASTExpression _expr, NinaCodeBlock _posBlock) {
        return transfer_list2params(
            new NinaASTListExpression(
                new List<ANinaASTExpression>() {
                    _expr
                },
                new NinaErrorPosition(_posBlock.file,
                    _posBlock.line, _posBlock.col)
            ),
            _posBlock
        );
    }
    public static NinaASTBlockExpression transfer_block2init(
            NinaASTBlockExpression _block, NinaCodeBlock _posBlock) {
        List<ANinaASTStatement> stms = _block.stms;
        for (int i = 0; i < stms.Count; ++ i) {
            ANinaASTStatement v = stms[i];
            if (v is NinaASTVarStatement vars) {
                List<(string, ANinaASTExpression?)> w = vars.vars.list;
                for (int j = 0; j < w.Count; ++ j) {
                    w[j] = (
                        NinaCompilerUtil.unformat_identifier(
                            w[j].Item1
                        ),
                        w[j].Item2
                    );
                }
            }
            else if (v is NinaASTWordStatement tmp
                    && tmp.type == NinaKeywordType.Return
                    && i == stms.Count - 1) {
                stms.RemoveAt(i);
                break;
            }
            else {
                NinaError.error(
                    "对象初始化表达式中有无效表达式.",
                    799124,
                    new NinaErrorPosition(_posBlock.file,
                        _posBlock.line, _posBlock.col)
                );
            }
        }
        return _block;
    }
    public static Dictionary<T1, T2>
            merge_dictionaries<T1, T2>(params Dictionary<T1, T2>[] _arr)
                where T1 : notnull {
        Dictionary<T1, T2> ret = new Dictionary<T1, T2>();
        for (int i = 0; i < _arr.Length; ++ i) {
            Dictionary<T1, T2> v = _arr[i];
            for (int j = 0; j < v.Count; ++ j) {
                KeyValuePair<T1, T2> p = v.ElementAt(j);
                ret[p.Key] = p.Value;
            }
        }
        return ret;
    }
    public static string snapshot_method(string _file, MethodBase _mtd) {
        return
            _file
            + ":"
            + _mtd.DeclaringType!.FullName !
            + "."
            + _mtd.Name;
    }
}