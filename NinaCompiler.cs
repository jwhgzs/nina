using System.Reflection;

namespace Nina;

static class NinaCompiler {
    public static NinaExprTree resolve_expr(List<NinaCodeBlock> _blocks,
            ref int _i, NinaOperatorType? _ender_op1 = null,
            NinaOperatorType? _ender_op2 = null) {
        if (_blocks.Count == 0)
            return new NinaExprTree(false);

        NinaExprTree? tree = null;
        NinaExprTree? ptr = null;
        NinaExprTree? buf = null;
        NinaCodeBlock? buf_op = null;
        int opLvMin = 2023;
        int initI = ++ _i;

        for (; _i <= _blocks.Count; ++ _i) {
            NinaCodeBlock? v = _i < _blocks.Count ? _blocks[_i] : null;
            bool isEof = _i == _blocks.Count;
            bool isReturner = ! isEof
                && ((_ender_op1 != null || _ender_op2 != null)
                        ? (v!.Value.type == NinaCodeBlockType.Operator
                            && (v!.Value.val_op == _ender_op1
                                || v!.Value.val_op == _ender_op2))
                        : (v!.Value.val_op == NinaOperatorType.Com
                            || v!.Value.val_sy == NinaSymbolType.Sem)
                    );
            bool isBeginner = _i == initI;
            bool isEnd = isEof || isReturner;
            bool isScoperL = ! isEof && v!.Value.val_sy == NinaSymbolType.CBraL
                && (isBeginner || _blocks[_i - 1].type == NinaCodeBlockType.Operator);
            if (! isBeginner && v != null && v.Value.val_op_unary == true
                    && _blocks[_i - 1].val_sy == NinaSymbolType.CBraR
                    && NinaCodeBlockUtil.operators_vagues.ContainsValue(
                        (NinaOperatorType) v.Value.val_op !)) {
                NinaCodeBlock nblock = v.Value;
                nblock.val_op
                    = NinaCodeBlockUtil.operators_vagues
                        .First(a => a.Value == v.Value.val_op).Key;
                nblock.val_op_unary = false;
                nblock.val_op_lv
                    = NinaCodeBlockUtil.operatorsRank[(NinaOperatorType) nblock.val_op !];
                v = nblock;
                _blocks[_i] = (NinaCodeBlock) v !;
            }

            if (isBeginner && isReturner)
                return new NinaExprTree(false);
            if (isEnd || v!.Value.type == NinaCodeBlockType.Operator || isScoperL) {
                bool isBracket = ! isEof && ! isScoperL
                    && (v!.Value.val_op == NinaOperatorType.BraL
                        || v!.Value.val_op == NinaOperatorType.BraR
                        || v!.Value.val_op == NinaOperatorType.MBraL
                        || v!.Value.val_op == NinaOperatorType.MBraR);
                bool isGrouperL = ! isEof && ! isScoperL
                    && v!.Value.val_op == NinaOperatorType.BraL
                    && (isBeginner || (
                        _blocks[_i - 1].type == NinaCodeBlockType.Operator
                            && _blocks[_i - 1].val_op != NinaOperatorType.BraR
                            && _blocks[_i - 1].val_op != NinaOperatorType.MBraR
                    ));
                bool isUnary = ! isEnd && ! isScoperL && v!.Value.val_op_unary == true;
                int? currOpLv = ! isEnd && ! isScoperL ? (int) v!.Value.val_op_lv ! : null;
                int? bufOpLv = buf_op != null && ! isScoperL ? buf_op.Value.val_op_lv : null;

                if (buf == null && ! isUnary && ! isGrouperL && ! isScoperL) {
                    if (! isEof)
                        NinaError.error("invalid expression.", 651968,
                            new NinaErrorPosition(v!.Value.file, v!.Value.line,
                                v!.Value.col));
                    if (buf_op != null)
                        NinaError.error("invalid expression.", 328298,
                            new NinaErrorPosition(buf_op.Value.file,
                                buf_op.Value.line, buf_op.Value.col));
                    else
                        NinaError.error("unexpected error.", 425707);
                }
                
                if (isUnary) {
                    if (buf != null) {
                        NinaError.error("invalid unary expression.", 355915,
                            new NinaErrorPosition(v!.Value.file, v!.Value.line,
                                v!.Value.col));
                    }
                    else {
                        buf = new NinaExprTree(true);
                    }
                }

                if (isGrouperL) {
                    buf = resolve_expr(
                        _blocks: _blocks,
                        _i: ref _i,
                        _ender_op1: NinaOperatorType.BraR
                    );
                    continue;
                }
                else if (isScoperL) {
                    buf = new NinaExprTree(
                        compile(
                            _blocks: _blocks,
                            _i: ref _i,
                            _scope: NinaScopeType.Function,
                            _cscope: NinaScopeType.Function
                        )
                    );
                    continue;
                }
                else if (tree == null) {
                    if (buf_op != null) {
                        NinaError.error("invalid expression.", 357258,
                            new NinaErrorPosition(buf_op.Value.file,
                                buf_op.Value.line, buf_op.Value.col));
                    }
                    else {
                        tree = buf;
                        ptr = tree;
                    }
                }
                else {
                    if (bufOpLv < opLvMin) {
                        NinaExprTree ntree = new NinaExprTree((NinaCodeBlock) buf_op !);
                        tree!.abdicate(ntree);
                        ntree.append(buf !);
                        ptr = ntree;
                        tree = ptr;
                    }
                    else {
                        NinaExprTree? p = ptr!.get();
                        bool isHandled = false;

                        do {
                            if (p!.boss == null
                                    || p.boss.block.val_op_lv < bufOpLv
                                    || (buf_op!.Value.val_op_unary == true
                                        && p.boss.block.val_op_unary == true)) {
                                NinaExprTree ntree
                                    = new NinaExprTree((NinaCodeBlock) buf_op !);
                                p.abdicate(ntree);
                                ntree.append(buf !);
                                ptr = ntree;
                                if (p == tree)
                                    tree = ptr;
                                isHandled = true;
                                break;
                            }
                        }
                        while ((p = p.boss) != null);

                        if (! isHandled) {
                            NinaError.error("unexpected error.", 669365);
                        }
                    }
                }

                if (! isEnd) {
                    if (buf_op != null && bufOpLv < opLvMin) {
                        opLvMin = (int) bufOpLv !;
                    }
                    buf_op = v;
                    if (isBracket) {
                        if (v!.Value.val_op == NinaOperatorType.BraL
                                || v!.Value.val_op == NinaOperatorType.MBraL) {
                            buf = resolve_expr(
                                _blocks: _blocks,
                                _i: ref _i,
                                _ender_op1:
                                    v!.Value.val_op == NinaOperatorType.BraL
                                    ? NinaOperatorType.BraR
                                    : NinaOperatorType.MBraR
                            );
                        }
                        else if (v!.Value.val_op == NinaOperatorType.BraR
                                || v!.Value.val_op == NinaOperatorType.MBraR) {
                            NinaError.error("unpaired brackets.", 761864,
                                new NinaErrorPosition(v!.Value.file,
                                    v!.Value.line, v!.Value.col));
                        }
                    }
                    else {
                        buf = null;
                    }
                }
                else if (isReturner) {
                    return tree ?? new NinaExprTree(false);
                }
            }
            else {
                if (buf != null) {
                    NinaError.error("unexpected token.", 481044,
                        new NinaErrorPosition(v!.Value.file, v!.Value.line, v!.Value.col));
                }
                buf = new NinaExprTree((NinaCodeBlock) v !);
            }
        }

        return tree ?? new NinaExprTree(false);
    }
    public static NinaASTBlockExpression compile(List<NinaCodeBlock> _blocks,
            ref int _i, NinaScopeType _scope = NinaScopeType.Root,
            NinaScopeType _cscope = NinaScopeType.Root) {
        NinaASTBlockExpression block = new NinaASTBlockExpression(
            new NinaErrorPosition()
        );
        if (_blocks.Count == 0)
            return block;
        if (_blocks.Last().val_sy != NinaSymbolType.Sem
                && _blocks.Last().val_sy != NinaSymbolType.CBraR) {
            NinaError.error("invalid end of file.",
                500695,
                new NinaErrorPosition(_blocks.Last().file,
                    _blocks.Last().line, _blocks.Last().col));
        }
        bool allow_elseif = false;
        List<(ANinaASTExpression, NinaASTBlockExpression)> buf_elses
            = new List<(ANinaASTExpression, NinaASTBlockExpression)>();

        for (++ _i; _i < _blocks.Count; ++ _i) {
            NinaCodeBlock v = _blocks[_i];
            if (v.val_sy == NinaSymbolType.CBraR) {
                if (_scope == NinaScopeType.Root) {
                    NinaError.error("unpaired curly-brackets.",
                        316852,
                        new NinaErrorPosition(v.file, v.line, v.col));
                }
                else {
                    if (_cscope == NinaScopeType.Function) {
                        NinaErrorPosition tmpPos
                            = new NinaErrorPosition(
                                v.file, v.line, v.col
                            );
                        block.stms.Add(
                            new NinaASTWordStatement(
                                _type: NinaKeywordType.Return,
                                _expr: new NinaASTLiteralExpression(
                                    tmpPos
                                ),
                                _pos: tmpPos
                            )
                        );
                    }
                    return block;
                }
            }
            else if (v.type == NinaCodeBlockType.Keyword) {
                if (v.val_kw == NinaKeywordType.Var
                        || v.val_kw == NinaKeywordType.Const) {
                    NinaASTVarStatement vars = new NinaASTVarStatement(
                        _isGlobal:
                            (_scope & NinaScopeType.Function) != NinaScopeType.Function,
                        _isConst: v.val_kw == NinaKeywordType.Const,
                        _pos: new NinaErrorPosition(
                            v.file, v.line, v.col
                        )
                    );
                    bool isRoot
                        = (_scope & NinaScopeType.Function) != NinaScopeType.Function;
                    
                    if (_i + 1 <= _blocks.Count - 1
                            && _blocks[_i + 1].val_sy != NinaSymbolType.Sem) {
                        do {
                            NinaExprTree expr = resolve_expr(
                                _blocks: _blocks,
                                _i: ref _i
                            );
                            if (expr.type == NinaExprTreeType.Void || (
                                    expr.type != NinaExprTreeType.Data
                                    ? (expr.block.val_op != NinaOperatorType.Equ
                                        || expr.l!.block.type != NinaCodeBlockType.Identifier)
                                    : expr.block.type != NinaCodeBlockType.Identifier)) {
                                NinaError.error("invalid variable declaration statement.",
                                    845706,
                                    new NinaErrorPosition(v.file, v.line, v.col));
                            }
                            else if (expr.type != NinaExprTreeType.Data) {
                                ANinaASTExpression? val
                                    = expr.r!.compile() as ANinaASTExpression;
                                if (val == null) {
                                    NinaError.error("invalid variable initialization statement.",
                                        123533,
                                        new NinaErrorPosition(v.file, v.line, v.col));
                                }
                                else {
                                    string id = NinaCompilerUtil.format_identifier(
                                        expr.l!.block.code
                                    );
                                    vars.vars.list.Add(
                                        (id, val)
                                    );
                                }
                            }
                            else {
                                vars.vars.list.Add(
                                    (expr.block.code, null)
                                );
                            }
                        }
                        while (_i < _blocks.Count
                                && _blocks[_i].val_sy != NinaSymbolType.Sem);
                    }
                    else {
                        NinaError.error("empty variable declaration statement.",
                            952990,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }

                    block.stms.Add(vars);
                }
                else if (v.val_kw == NinaKeywordType.If
                        || v.val_kw == NinaKeywordType.Else
                        || v.val_kw == NinaKeywordType.Elseif
                        || v.val_kw == NinaKeywordType.While) {
                    if (_i + 1 > _blocks.Count - 1) {
                        NinaError.error("invalid logical control statement.",
                            611922,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }
                    if (v.val_kw != NinaKeywordType.Else
                            && _blocks[++ _i].val_op != NinaOperatorType.BraL) {
                        NinaError.error("invalid logical control statement.",
                            390479,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }

                    NinaExprTree? tree
                        = v.val_kw != NinaKeywordType.Else
                            ? resolve_expr(
                                _blocks: _blocks,
                                _i: ref _i,
                                _ender_op1: NinaOperatorType.BraR
                            )
                            : null;
                    ANinaASTExpression? expr
                        = v.val_kw != NinaKeywordType.Else
                            ? tree!.compile() as ANinaASTExpression
                            : null;
                    NinaCodeBlock? cBraL
                        = _i + 1 > _blocks.Count - 1
                            ? null
                            : _blocks[++ _i];
                    if (cBraL == null
                            || cBraL.Value.val_sy != NinaSymbolType.CBraL) {
                        NinaError.error("invalid logical control statement.",
                            885155,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }
                    else if (v.val_kw != NinaKeywordType.Else && expr == null) {
                        NinaError.error("invalid logical control statement.",
                            723745,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }
                    else {
                        NinaErrorPosition tmpPos
                            = new NinaErrorPosition(
                                v.file, v.line, v.col
                            );
                        if (v.val_kw != NinaKeywordType.Else) {
                            expr = new NinaASTBinaryExpression(
                                _type: NinaOperatorType.BraL,
                                _expr_l: new NinaASTIdentifierExpression(
                                    "NinaAPIUtil__toBool",
                                    tmpPos
                                ),
                                _expr_r: expr !,
                                tmpPos
                            );
                        }
                        NinaScopeType nscope;
                        if (v.val_kw == NinaKeywordType.If)
                            nscope = NinaScopeType.If;
                        else if (v.val_kw == NinaKeywordType.Else)
                            nscope = NinaScopeType.Else;
                        else if (v.val_kw == NinaKeywordType.Elseif)
                            nscope = NinaScopeType.Elseif;
                        else
                            nscope = NinaScopeType.While;
                        NinaASTBlockExpression body = compile(
                            _blocks: _blocks,
                            _i: ref _i,
                            _scope: _scope | nscope,
                            _cscope: nscope
                        );
                        if (v.val_kw == NinaKeywordType.If) {
                            block.stms.Add(
                                new NinaASTIfStatement(
                                    _expr: expr !,
                                    _block: body,
                                    tmpPos
                                )
                            );
                            allow_elseif = true;
                        }
                        else if (v.val_kw == NinaKeywordType.While) {
                            block.stms.Add(
                                new NinaASTWhileStatement(
                                    _expr: expr !,
                                    _block: body,
                                    tmpPos
                                )
                            );
                        }
                        else {
                            if (block.stms.Count == 0) {
                                NinaError.error("unexpected logical clause statement.",
                                    898170,
                                    new NinaErrorPosition(v.file, v.line, v.col));
                            }
                            NinaASTIfStatement? main
                                = block.stms.Last() as NinaASTIfStatement;
                            if (main == null) {
                                NinaError.error("unexpected logical clause statement.",
                                    606056,
                                    new NinaErrorPosition(v.file, v.line, v.col));
                            }
                            else if (v.val_kw == NinaKeywordType.Else) {
                                if (main.block_else != null) {
                                    NinaError.error("unexpected logical clause statement.",
                                        432612,
                                        new NinaErrorPosition(v.file, v.line, v.col));
                                }
                                buf_elses.Add(
                                    (
                                        new NinaASTIdentifierExpression(
                                            NinaConstsProviderUtil.NINA_ID_PREFIX
                                                + "true",
                                            tmpPos
                                        ),
                                        body
                                    )
                                );
                                main.block_else
                                    = NinaCompilerUtil.resolve_elses(
                                        buf_elses,
                                        v
                                    );
                                buf_elses.Clear();
                                allow_elseif = false;
                            }
                            else {
                                if (! allow_elseif) {
                                    NinaError.error("unexpected elseif clause statement.",
                                        299924,
                                        new NinaErrorPosition(v.file, v.line, v.col));
                                }
                                buf_elses.Add(
                                    (expr !, body)
                                );
                            }
                        }
                    }
                }
                else if (v.val_kw == NinaKeywordType.Return) {
                    ANinaASTExpression? expr = resolve_expr(
                        _blocks: _blocks,
                        _i: ref _i
                    ).compile() as ANinaASTExpression;
                    if (_i > _blocks.Count || _blocks[_i].val_sy != NinaSymbolType.Sem
                            || (_scope & NinaScopeType.Function) != NinaScopeType.Function) {
                        NinaError.error("invalid return statement.", 916850,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }
                    else {
                        block.stms.Add(
                            new NinaASTWordStatement(
                                _type: NinaKeywordType.Return,
                                _expr: expr,
                                _pos: expr.pos
                            )
                        );
                    }
                }
                else if (v.val_kw == NinaKeywordType.Break
                        || v.val_kw == NinaKeywordType.Continue) {
                    if (_i + 1 > _blocks.Count - 1 || _blocks[++ _i].val_sy != NinaSymbolType.Sem
                            || (_scope & NinaScopeType.While) != NinaScopeType.While) {
                        NinaError.error("invalid loop escape statement.", 327023,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }
                    else {
                        block.stms.Add(
                            new NinaASTWordStatement(
                                (NinaKeywordType) v.val_kw !,
                                new NinaErrorPosition(
                                    v.file, v.line, v.col
                                )
                            )
                        );
                    }
                }
                else if (v.val_kw == NinaKeywordType.Func) {
                    if (_i + 3 > _blocks.Count() - 1) {
                        NinaError.error("invalid named function declaration statement.",
                            894659,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }
                    NinaCodeBlock name = _blocks[++ _i];
                    NinaCodeBlock braL = _blocks[++ _i];
                    if (name.type != NinaCodeBlockType.Identifier
                            || braL.val_op != NinaOperatorType.BraL) {
                        NinaError.error("invalid named function declaration statement.",
                            526905,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }
                    NinaASTSuperListExpression plist = new NinaASTSuperListExpression(
                        new NinaErrorPosition(
                            braL.file, braL.line, braL.col
                        )
                    );
                    
                    if (_i + 1 <= _blocks.Count - 1
                            && _blocks[_i + 1].val_op != NinaOperatorType.BraR) {
                        bool comfortable = false;
                        do {
                            NinaExprTree expr = resolve_expr(
                                _blocks: _blocks,
                                _i: ref _i,
                                _ender_op1: NinaOperatorType.Com,
                                _ender_op2: NinaOperatorType.BraR
                            );
                            if (expr.type == NinaExprTreeType.Void || (
                                    expr.type != NinaExprTreeType.Data
                                    ? (expr.block.val_op != NinaOperatorType.Equ
                                        || expr.l!.block.type != NinaCodeBlockType.Identifier)
                                    : expr.block.type != NinaCodeBlockType.Identifier)) {
                                NinaError.error("invalid function declaration statement.",
                                    635117,
                                    new NinaErrorPosition(v.file, v.line, v.col));
                            }
                            else if (expr.type != NinaExprTreeType.Data) {
                                ANinaASTExpression? val
                                    = expr.r!.compile() as ANinaASTExpression;
                                if (val == null) {
                                    NinaError.error("invalid function initialization statement.",
                                        116820,
                                        new NinaErrorPosition(v.file, v.line, v.col));
                                }
                                else {
                                    plist.list.Add(
                                        (
                                            NinaCompilerUtil.format_identifier(
                                                expr.l!.block.code
                                            ),
                                            val
                                        )
                                    );
                                    comfortable = true;
                                }
                            }
                            else {
                                if (comfortable) {
                                    NinaError.error("invalid function parameter initialization.",
                                        535545,
                                        new NinaErrorPosition(v.file, v.line, v.col));
                                }
                                plist.list.Add(
                                    (
                                        NinaCompilerUtil.format_identifier(
                                            expr.block.code
                                        ),
                                        null
                                    )
                                );
                            }
                        }
                        while (_i < _blocks.Count
                                && _blocks[_i].val_op != NinaOperatorType.BraR);
                    }
                    else {
                        ++ _i;
                    }
                    if (_i + 1 <= _blocks.Count - 1) {
                        NinaCodeBlock cBraL = _blocks[++ _i];
                        if (cBraL.val_sy != NinaSymbolType.CBraL) {
                            NinaError.error("invalid function initialization statement.",
                                985609,
                                new NinaErrorPosition(v.file, v.line, v.col));
                        }
                    }
                    else {
                        NinaError.error("invalid function initialization statement.",
                            252671,
                            new NinaErrorPosition(v.file, v.line, v.col));
                    }
                    
                    NinaASTBlockExpression body = compile(
                        _blocks: _blocks,
                        _i: ref _i,
                        _scope: NinaScopeType.Function,
                        _cscope: NinaScopeType.Function
                    );
                    string id = NinaCompilerUtil.format_identifier(name.code);
                    NinaErrorPosition tmpPos
                        = new NinaErrorPosition(
                            v.file, v.line, v.col
                        );
                    block.stms.Add(
                        new NinaASTVarStatement(
                            _isGlobal:
                                (_scope & NinaScopeType.Function)
                                    != NinaScopeType.Function,
                            _vars: new NinaASTSuperListExpression(
                                new List<(string, ANinaASTExpression?)> {
                                    (
                                        id,
                                        new NinaASTBinaryExpression(
                                            _type: NinaOperatorType.Arr,
                                            _expr_l: plist,
                                            _expr_r: body,
                                            plist.pos
                                        )
                                    )
                                },
                                tmpPos
                            ),
                            _pos: tmpPos
                        )
                    );
                }
                else {
                    NinaError.error("unexpected error.",
                        284747,
                        new NinaErrorPosition(v.file, v.line, v.col));
                }
            }
            else {
                -- _i;
                ANinaASTExpression? expr = resolve_expr(
                    _blocks: _blocks,
                    _i: ref _i
                ).compile() as ANinaASTExpression;
                if (expr == null) {}
                else {
                    block.stms.Add(
                        new NinaASTExpressionStatement(
                            expr,
                            expr.pos
                        )
                    );
                }
            }
        }

        return block;
    }
    public static void execute(List<NinaCodeBlock> _blocks) {
        int i = - 1;
        NinaASTBlockExpression block = compile(
            _blocks: _blocks,
            _i: ref i
        );
        NinaILCompiler.run(block);
    }
}