using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Nina;

class NinaExprTree {
    public NinaExprTree? boss = null;
    public NinaExprTreeType type = NinaExprTreeType.None;
    public NinaCodeBlock block;
    public BlockSyntax? compiledBlock = null;
    public NinaExprTree? l = null;
    public NinaExprTree? r = null;
    public NinaExprTree(bool _isPlaceholder) {
        block = new NinaCodeBlock();
        type = ! _isPlaceholder
            ? NinaExprTreeType.Void
            : NinaExprTreeType.Placeholder;
    }
    public NinaExprTree(NinaCodeBlock _block) {
        block = _block;
        type = _block.type != NinaCodeBlockType.Operator
            ? NinaExprTreeType.Data
            : NinaExprTreeType.Operator;
    }
    public NinaExprTree(BlockSyntax _compiledBlock) {
        compiledBlock = _compiledBlock;
        type = NinaExprTreeType.CompiledBlock;
    }
    public NinaExprTree? get(int _n) {
        if (_n == 0 && l != null) return l;
        else if (_n == 1 && r != null) return r;
        else {
            NinaError.error("unexpected error.", 101253);
            return null;
        }
    }
    public NinaExprTree? get() {
        if (type == NinaExprTreeType.Data) return this;
        else if (r != null) return r;
        else if (l != null) return l;
        else {
            NinaError.error("unexpected error.", 613545);
            return null;
        }
    }
    public void remove(int _n) {
        if (_n == 0 && l != null) {
            l.boss = null;
            l = null;
        }
        else if (_n == 1 && r != null) {
            r.boss = null;
            r = null;
        }
        else {
            NinaError.error("unexpected error.", 164914);
        }
    }
    public void remove(NinaExprTree _v) {
        if (l == _v)
            remove(0);
        else if (r == _v)
            remove(1);
        else
            NinaError.error("unexpected error.", 200329);
    }
    public void append(NinaExprTree _tree) {
        if (l == null)
            l = _tree;
        else if (r == null)
            r = _tree;
        else
            NinaError.error("unexpected error.", 944907);
        
        if (_tree.boss != null)
            _tree.boss.remove(_tree);
        _tree.boss = this;
    }
    public void replace(int _n, NinaExprTree _v) {
        if (_v.boss != null)
            _v.boss.remove(_v);
        if (_n == 0 && l != null) {
            _v.boss = this;
            l.boss = null;
            l = _v;
        }
        else if (_n == 1 && r != null) {
            _v.boss = this;
            r.boss = null;
            r = _v;
        }
        else {
            NinaError.error("unexpected error.", 496672);
        }
    }
    public void replace(NinaExprTree _ov, NinaExprTree _nv) {
        if (l == _ov)
            replace(0, _nv);
        else if (r == _ov)
            replace(1, _nv);
        else
            NinaError.error("unexpected error.", 157782);
    }
    public void abdicate(NinaExprTree _tree) {
        if (boss != null)
            boss.replace(this, _tree);
        _tree.append(this);
    }
    public CSharpSyntaxNode compile(bool _isRoot = true) {
        if (type == NinaExprTreeType.CompiledBlock) {
            return compiledBlock !;
        }
        else if (type == NinaExprTreeType.Void) {
            return ArgumentList();
        }
        else if (type == NinaExprTreeType.Data) {
            if (block.type == NinaCodeBlockType.Identifier) {
                return
                    IdentifierName(
                        name: NinaCompilerUtil.format_identifier(block.code)
                    );
            }
            else if (block.type == NinaCodeBlockType.Number) {
                return
                    LiteralExpression(
                        kind: SyntaxKind.NumericLiteralExpression,
                        token: Literal((double) block.val_num !)
                    );
            }
            else if (block.type == NinaCodeBlockType.String) {
                return
                    LiteralExpression(
                        kind: SyntaxKind.StringLiteralExpression,
                        token: Literal(block.val_str !)
                    );
            }
            else {
                NinaError.error("unexpected token.", 119832,
                    new NinaErrorPosition(block.file, block.line, block.col));
            }
        }
        else if (type == NinaExprTreeType.Operator) {
            if (l == null || r == null) {
                NinaError.error("unexpected error.", 584489);
            }
            else if (block.val_op_unary == false) {
                CSharpSyntaxNode node_l = l!.compile(false);
                CSharpSyntaxNode node_r = r!.compile(false);
                ExpressionSyntax? expr_l = node_l as ExpressionSyntax;
                ExpressionSyntax? expr_r = node_r as ExpressionSyntax;
                ArgumentListSyntax? list_l = node_l as ArgumentListSyntax;
                ArgumentListSyntax? list_r = node_r as ArgumentListSyntax;
                BlockSyntax? compiledBlock_l = node_l as BlockSyntax;
                BlockSyntax? compiledBlock_r = node_r as BlockSyntax;

                if (block.val_op == NinaOperatorType.Arr) {
                    if (compiledBlock_r == null) {
                        NinaError.error(
                            "invalid right-hand expression " +
                            "for inline lambda creation operator.", 186009,
                            new NinaErrorPosition(block.file, block.line, block.col));
                    }
                    else {
                        ParameterListSyntax plist
                            = list_l != null
                                ? NinaCompilerUtil.transfer_list2params(list_l, block)
                                : NinaCompilerUtil.transfer_list2params(expr_l !, block);
                        return ParenthesizedLambdaExpression(
                            modifiers: TokenList(),
                            parameterList: plist,
                            block: compiledBlock_r,
                            expressionBody: null
                        );
                    }
                }
                else if (compiledBlock_l != null || compiledBlock_r != null) {
                    NinaError.error(
                        "unexpected block expression " +
                        "for the specific operator.",
                        201919,
                        new NinaErrorPosition(block.file, block.line, block.col));
                }
                else if (block.val_op == NinaOperatorType.Com) {
                    if (_isRoot) {
                        NinaError.error("unexpected list expression.", 924405,
                            new NinaErrorPosition(block.file, block.line, block.col));
                    }
                    else if (list_l != null && list_r != null) {
                        list_l = list_l.AddArguments(
                            list_r.Arguments.ToArray()
                        );
                        return list_l;
                    }
                    else if (list_l != null && list_r == null) {
                        list_l = list_l.AddArguments(
                            Argument(expr_r !)
                        );
                        return list_l;
                    }
                    else if (list_l == null && list_r != null) {
                        list_l = ArgumentList(
                            new SeparatedSyntaxList<ArgumentSyntax>()
                                .Add(Argument(expr_l !))
                                .AddRange(list_r.Arguments)
                        );
                        return list_l;
                    }
                    else {
                        return
                            ArgumentList(
                                new SeparatedSyntaxList<ArgumentSyntax>()
                                    .Add(Argument(expr_l !))
                                    .Add(Argument(expr_r !))
                            );
                    }
                }
                else if (block.val_op == NinaOperatorType.BraL) {
                    if (list_l != null) {
                        NinaError.error(
                            "invalid left-hand expression " +
                            "for function calling operator.",
                            593929,
                            new NinaErrorPosition(block.file, block.line, block.col));
                    }
                    else if (list_r != null) {
                        return
                            InvocationExpression(
                                expression: expr_l !,
                                argumentList:
                                    NinaCompilerUtil.transfer_list2args(list_r, block)
                            );
                    }
                    else {
                        return
                            InvocationExpression(
                                expression: expr_l !,
                                argumentList:
                                    NinaCompilerUtil.transfer_list2args(expr_r !, block)
                            );
                    }
                }
                else if (list_l != null || list_r != null) {
                    NinaError.error(
                        "unexpected list expression " +
                        "for the specific operator.",
                        249439,
                        new NinaErrorPosition(block.file, block.line, block.col));
                }
                else if (block.val_op == NinaOperatorType.MBraL) {
                    return
                        ElementAccessExpression(
                            expression: expr_l !,
                            argumentList: BracketedArgumentList(
                                new SeparatedSyntaxList<ArgumentSyntax>()
                                    .Add(Argument(expr_r !))
                            )
                        );
                }
                else if (block.val_op == NinaOperatorType.Dot) {
                    IdentifierNameSyntax? id = expr_r as IdentifierNameSyntax;
                    if (id == null) {
                        NinaError.error(
                            "invalid right-hand expression " +
                            "for member access operator.",
                            595886,
                            new NinaErrorPosition(block.file, block.line, block.col));
                    }
                    else {
                        SeparatedSyntaxList<ArgumentSyntax> slist
                            = new SeparatedSyntaxList<ArgumentSyntax>()
                                .Add(
                                    Argument(
                                        LiteralExpression(
                                            kind: SyntaxKind.StringLiteralExpression,
                                            token: Literal(
                                                NinaCompilerUtil.unformat_identifier(
                                                    id.Identifier.Text
                                                )
                                            )
                                        )
                                    )
                                );
                        return
                            ElementAccessExpression(
                                expression: expr_l !,
                                argumentList: BracketedArgumentList(
                                    slist
                                )
                            );
                    }
                }
                else if (block.val_op == NinaOperatorType.Equ) {
                    return
                        AssignmentExpression(
                            kind: SyntaxKind.SimpleAssignmentExpression,
                            left: expr_l !,
                            right: expr_r !
                        );
                }
                else if (block.val_op == NinaOperatorType.LOr
                        || block.val_op == NinaOperatorType.LAnd) {
                    return
                        BinaryExpression(
                            kind: block.val_op == NinaOperatorType.LOr
                                ? SyntaxKind.LogicalOrExpression
                                : SyntaxKind.LogicalAndExpression,
                            left: expr_l !,
                            right: expr_r !
                        );
                }
                else {
                    return
                        InvocationExpression(
                            expression:
                                IdentifierName(
                                    NinaConstsProviderUtil.CSHARP_NINAAPIUTIL_PREFIX
                                        + "op"
                                        + block.val_op.ToString()
                                ),
                            argumentList: ArgumentList(
                                new SeparatedSyntaxList<ArgumentSyntax>()
                                    .Add(Argument(expr_l !))
                                    .Add(Argument(expr_r !))
                            )
                        );
                }
            }
            else {
                CSharpSyntaxNode node = r!.compile(false);
                ExpressionSyntax? expr = node as ExpressionSyntax;
                BlockSyntax? compiledBlock = node as BlockSyntax;
                
                if (block.val_op == NinaOperatorType.Object
                        || block.val_op == NinaOperatorType.Array) {
                    if (compiledBlock == null) {
                        NinaError.error(
                            "invalid right-hand expression " +
                            "for object creation operator.",
                            301529,
                            new NinaErrorPosition(block.file, block.line, block.col));
                    }
                    return ObjectCreationExpression(
                        type: block.val_op == NinaOperatorType.Object
                            ? IdentifierName("object")
                            : IdentifierName("array"),
                        argumentList: ArgumentList(),
                        initializer:
                            NinaCompilerUtil.transfer_block2init(compiledBlock !, block)
                    );
                }
                else if (expr == null || compiledBlock != null) {
                    NinaError.error(
                        "invalid right-hand expression " +
                        "for unary operator.",
                        113938,
                        new NinaErrorPosition(block.file, block.line, block.col));
                }
                else if (block.val_op == NinaOperatorType.At) {
                    return node.WithAdditionalAnnotations(
                        new SyntaxAnnotation(
                            NinaConstsProviderUtil.CSHARP_ANNO_SPECIALARG
                        )
                    );
                }
                else {
                    return
                        InvocationExpression(
                            expression:
                                IdentifierName(
                                    NinaConstsProviderUtil.CSHARP_NINAAPIUTIL_PREFIX
                                        + "op"
                                        + block.val_op.ToString()
                                ),
                            argumentList: ArgumentList(
                                new SeparatedSyntaxList<ArgumentSyntax>()
                                    .Add(Argument(expr !))
                            )
                        );
                }
            }
        }
        else {
            NinaError.error("unexpected error.", 214128);
        }

        return LiteralExpression(SyntaxKind.NullLiteralExpression);
    }
}