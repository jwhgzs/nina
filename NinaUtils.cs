using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Nina;

static class NinaConstsProviderUtil {
    public const string CSHARP_ID_PREFIX = "NinaGlobal__";
    public const string CSHARP_NINAAPI_PREFIX = "NinaAPI__";
    public const string CSHARP_NINAAPIUTIL_PREFIX = "NinaAPIUtil__";
    public const string CSHARP_ANNO_CONST = "NINA_ANNO_CONST";
    public const string CSHARP_ANNO_SPECIALARG = "NINA_ANNO_SPECIALARG";
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
        ["class"] = NinaKeywordType.Class,
        ["if"] = NinaKeywordType.If,
        ["else"] = NinaKeywordType.Else,
        ["elseif"] = NinaKeywordType.Elseif,
        ["while"] = NinaKeywordType.While,
        ["return"] = NinaKeywordType.Return,
        ["break"] = NinaKeywordType.Break,
        ["continue"] = NinaKeywordType.Continue
    };
    public static List<string> specialIdentifiers
            = new List<string> {
        "self", "this"
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
    public static Dictionary<int, int> operatorsRank
            = new Dictionary<int, int> {
        [(int) NinaOperatorType.None] = 0,
        [(int) NinaOperatorType.Com] = 1,
        [(int) NinaOperatorType.Equ] = 2,
        [(int) NinaOperatorType.LOr] = 3,
        [(int) NinaOperatorType.LAnd] = 4,
        [(int) NinaOperatorType.Or] = 5,
        [(int) NinaOperatorType.XOr] = 6,
        [(int) NinaOperatorType.And] = 7,
        [(int) NinaOperatorType.LEqu] = 8,
        [(int) NinaOperatorType.LNEqu] = 8,
        [(int) NinaOperatorType.More] = 8,
        [(int) NinaOperatorType.Less] = 8,
        [(int) NinaOperatorType.MoreE] = 8,
        [(int) NinaOperatorType.LessE] = 8,
        [(int) NinaOperatorType.SftL] = 9,
        [(int) NinaOperatorType.SftR] = 9,
        [(int) NinaOperatorType.Add] = 10,
        [(int) NinaOperatorType.Sub] = 11,
        [(int) NinaOperatorType.Mut] = 12,
        [(int) NinaOperatorType.Div] = 13,
        [(int) NinaOperatorType.Rem] = 14,
        [(int) NinaOperatorType.Pow] = 15,
        [(int) NinaOperatorType.Arr] = 16,
        [(int) NinaOperatorType.Not] = 17,
        [(int) NinaOperatorType.Pos] = 17,
        [(int) NinaOperatorType.Neg] = 17,
        [(int) NinaOperatorType.LNot] = 17,
        [(int) NinaOperatorType.Typeof] = 17,
        [(int) NinaOperatorType.Object] = 17,
        [(int) NinaOperatorType.Array] = 17,
        [(int) NinaOperatorType.At] = 17,
        [(int) NinaOperatorType.BraL] = 18,
        [(int) NinaOperatorType.BraR] = 18,
        [(int) NinaOperatorType.MBraL] = 18,
        [(int) NinaOperatorType.MBraR] = 18,
        [(int) NinaOperatorType.Dot] = 18
    };
    public static List<int> operators_unarys = new List<int>() {
        (int) NinaOperatorType.Not,
        (int) NinaOperatorType.Pos,
        (int) NinaOperatorType.Neg,
        (int) NinaOperatorType.LNot,
        (int) NinaOperatorType.Typeof,
        (int) NinaOperatorType.Object,
        (int) NinaOperatorType.Array,
        (int) NinaOperatorType.At
    };
    public static Dictionary<int, SyntaxKind> operators_csharp
            = new Dictionary<int, SyntaxKind> {
        [(int) NinaOperatorType.LOr] = SyntaxKind.LogicalOrExpression,
        [(int) NinaOperatorType.LAnd] = SyntaxKind.LogicalAndExpression,
        [(int) NinaOperatorType.Or] = SyntaxKind.BitwiseOrExpression,
        [(int) NinaOperatorType.XOr] = SyntaxKind.ExclusiveOrExpression,
        [(int) NinaOperatorType.And] = SyntaxKind.BitwiseAndExpression,
        [(int) NinaOperatorType.LEqu] = SyntaxKind.EqualsExpression,
        [(int) NinaOperatorType.LNEqu] = SyntaxKind.NotEqualsExpression,
        [(int) NinaOperatorType.More] = SyntaxKind.GreaterThanExpression,
        [(int) NinaOperatorType.Less] = SyntaxKind.LessThanExpression,
        [(int) NinaOperatorType.MoreE] = SyntaxKind.GreaterThanOrEqualExpression,
        [(int) NinaOperatorType.LessE] = SyntaxKind.LessThanOrEqualExpression,
        [(int) NinaOperatorType.SftL] = SyntaxKind.LeftShiftExpression,
        [(int) NinaOperatorType.SftR] = SyntaxKind.RightShiftExpression,
        [(int) NinaOperatorType.Add] = SyntaxKind.AddExpression,
        [(int) NinaOperatorType.Sub] = SyntaxKind.SubtractExpression,
        [(int) NinaOperatorType.Mut] = SyntaxKind.MultiplyExpression,
        [(int) NinaOperatorType.Div] = SyntaxKind.DivideExpression,
        [(int) NinaOperatorType.Rem] = SyntaxKind.ModuloExpression,
        [(int) NinaOperatorType.Not] = SyntaxKind.BitwiseNotExpression,
        [(int) NinaOperatorType.Pos] = SyntaxKind.UnaryPlusExpression,
        [(int) NinaOperatorType.Neg] = SyntaxKind.UnaryMinusExpression,
        [(int) NinaOperatorType.LNot] = SyntaxKind.LogicalNotExpression
    };
    public static int operatorsRank_unary = operatorsRank[(int) NinaOperatorType.Pos];
    public static bool supposeSymbol(char _ch, out NinaSymbolType _out) {
        return symbols.TryGetValue(_ch, out _out);
    }
    public static bool supposeKeyword(string _code, out NinaKeywordType _out) {
        return keywords.TryGetValue(_code, out _out);
    }
    public static bool supposeOperator(string _code, out NinaOperatorType _out, out int _lv) {
        bool ok = operators.TryGetValue(_code, out _out);
        if (ok) operatorsRank.TryGetValue((int) _out, out _lv);
        else operatorsRank.TryGetValue((int) NinaOperatorType.None, out _lv);
        return ok;
    }
    public static bool supposeOperator(char _ch, out NinaOperatorType _out, out int _lv) {
        bool ok = operators_ch.TryGetValue(_ch, out _out);
        if (ok) operatorsRank.TryGetValue((int) _out, out _lv);
        else operatorsRank.TryGetValue((int) NinaOperatorType.None, out _lv);
        return ok;
    }
    public static bool supposeOperator_csharp(int _id, out SyntaxKind _out) {
        bool ok = operators_csharp.TryGetValue(_id, out _out);
        if (! ok) _out = SyntaxKind.None;
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
        return NinaConstsProviderUtil.CSHARP_ID_PREFIX + _id;
    }
    public static string unformat_identifier(string _id) {
        if (_id.StartsWith(NinaConstsProviderUtil.CSHARP_ID_PREFIX))
            return _id.Remove(0, NinaConstsProviderUtil.CSHARP_ID_PREFIX.Length);
        return _id;
    }
    public static ArgumentListSyntax transfer_list2args(
            ArgumentListSyntax _list, NinaCodeBlock _block) {
        SeparatedSyntaxList<ArgumentSyntax> list = _list.Arguments;
        SeparatedSyntaxList<ArgumentSyntax> ret
            = new SeparatedSyntaxList<ArgumentSyntax>();
        for (int i = 0; i < list.Count; ++ i) {
            ExpressionSyntax v = list[i].Expression;
            ret = ret.Add(Argument(v));
        }
        return ArgumentList(ret);
    }
    public static ArgumentListSyntax transfer_list2args(
            ExpressionSyntax _expr, NinaCodeBlock _block) {
        return transfer_list2args(ArgumentList(
            new SeparatedSyntaxList<ArgumentSyntax>()
                .Add(Argument(_expr))
        ), _block);
    }
    public static ParameterListSyntax transfer_list2params(
            ArgumentListSyntax _list, NinaCodeBlock _block) {
        SeparatedSyntaxList<ArgumentSyntax> list = _list.Arguments;
        SeparatedSyntaxList<ParameterSyntax> ret
            = new SeparatedSyntaxList<ParameterSyntax>();
        bool isComfortable = false;
        for (int i = 0; i < list.Count; ++ i) {
            ExpressionSyntax v = list[i].Expression;
            AssignmentExpressionSyntax? v_assign
                = v as AssignmentExpressionSyntax;
            if (v_assign == null && isComfortable) {
                NinaError.error("invalid param initialization expression.",
                    152183,
                    new NinaErrorPosition(_block.file, _block.line,
                        _block.col));
            }
            else if (v_assign != null) {
                IdentifierNameSyntax? key
                    = v_assign.Left as IdentifierNameSyntax;
                ExpressionSyntax val = v_assign.Right;
                if (key == null) {
                    NinaError.error("invalid param initialization expression.",
                        301634,
                        new NinaErrorPosition(_block.file, _block.line,
                            _block.col));
                }
                ret = ret.Add(
                    Parameter(key!.Identifier)
                    .WithType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                    .WithDefault(
                        EqualsValueClause(val)
                    )
                );
                isComfortable = true;
            }
            else {
                IdentifierNameSyntax? key = v as IdentifierNameSyntax;
                if (key == null) {
                    NinaError.error("invalid parameter initialization expression.",
                        631130,
                        new NinaErrorPosition(_block.file, _block.line,
                            _block.col));
                }
                ret = ret.Add(
                    Parameter(key!.Identifier)
                        .WithType(PredefinedType(Token(SyntaxKind.ObjectKeyword)))
                );
            }
        }
        return ParameterList(ret);
    }
    public static ParameterListSyntax transfer_list2params(
            ExpressionSyntax _expr, NinaCodeBlock _block) {
        return transfer_list2params(
            ArgumentList(
                new SeparatedSyntaxList<ArgumentSyntax>()
                    .Add(Argument(_expr))
            ),
            _block
        );
    }
    public static InitializerExpressionSyntax transfer_block2init(
            BlockSyntax _block, NinaCodeBlock _eblock) {
        SeparatedSyntaxList<ExpressionSyntax> list
            = new SeparatedSyntaxList<ExpressionSyntax>();
        SyntaxList<StatementSyntax> stms = _block.Statements;

        for (int i = 0; i < stms.Count; ++ i) {
            StatementSyntax v = stms[i];
            if (v is LocalDeclarationStatementSyntax vars) {
                SeparatedSyntaxList<VariableDeclaratorSyntax> vs
                    = vars.Declaration.Variables;
                for (int j = 0; j < vs.Count; ++ j) {
                    VariableDeclaratorSyntax w = vs[j];
                    list = list.Add(
                        AssignmentExpression(
                            kind: SyntaxKind.SimpleAssignmentExpression,
                            left: ImplicitElementAccess(
                                argumentList: BracketedArgumentList(
                                    new SeparatedSyntaxList<ArgumentSyntax>()
                                        .Add(
                                            Argument(
                                                LiteralExpression(
                                                    kind: SyntaxKind.StringLiteralExpression,
                                                    token: Literal(
                                                        NinaCompilerUtil.unformat_identifier(
                                                            w.Identifier.ValueText
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                )
                            )
                            .WithAdditionalAnnotations(
                                vars.IsConst
                                    ? new SyntaxAnnotation(
                                        NinaConstsProviderUtil.CSHARP_ANNO_CONST
                                    )
                                    : new SyntaxAnnotation()
                            ),
                            right: w.Initializer != null
                                ? w.Initializer.Value
                                : LiteralExpression(
                                    SyntaxKind.NullLiteralExpression
                                )
                        )
                    );
                }
            }
            else if (v is ReturnStatementSyntax && i == stms.Count - 1) {}
            else {
                NinaError.error(
                    "unexpected syntax in the member initialization block.",
                    894872,
                    new NinaErrorPosition(_eblock.file, _eblock.line, _eblock.col));
            }
        }

        return InitializerExpression(
            kind: SyntaxKind.ObjectInitializerExpression,
            expressions: list
        );
    }
    public static BlockSyntax resolve_elses(
            List<(ExpressionSyntax, BlockSyntax)> _list) {
        IfStatementSyntax? ret = null;
        for (int i = _list.Count - 1; i >= 0; -- i) {
            var (cond, block) = _list[i];
            IfStatementSyntax nif = IfStatement(
                condition: cond,
                statement: block
            );
            if (ret == null) {
                ret = nif;
            }
            else {
                ret = nif.WithElse(
                    ElseClause(
                        elseKeyword: Token(SyntaxKind.ElseKeyword),
                        statement: Block(ret)
                    )
                );
            }
        }
        return ret != null ? Block(ret) : Block();
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
}