namespace Nina;

class NinaExprTree {
    public NinaExprTree? boss = null;
    public NinaExprTreeType type = NinaExprTreeType.None;
    public NinaCodeBlock block;
    public NinaASTBlockExpression? compiledBlock = null;
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
    public NinaExprTree(NinaASTBlockExpression _compiledBlock) {
        compiledBlock = _compiledBlock;
        type = NinaExprTreeType.CompiledBlock;
    }
    public NinaExprTree? get(int _n) {
        if (_n == 0 && l != null) return l;
        else if (_n == 1 && r != null) return r;
        else {
            NinaError.error("莫名其妙的错误.", 101253);
            return null;
        }
    }
    public NinaExprTree? get() {
        if (type == NinaExprTreeType.Data) return this;
        else if (r != null) return r;
        else if (l != null) return l;
        else {
            NinaError.error("莫名其妙的错误.", 613545);
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
            NinaError.error("莫名其妙的错误.", 164914);
        }
    }
    public void remove(NinaExprTree _v) {
        if (l == _v)
            remove(0);
        else if (r == _v)
            remove(1);
        else
            NinaError.error("莫名其妙的错误.", 200329);
    }
    public void append(NinaExprTree _tree) {
        if (l == null)
            l = _tree;
        else if (r == null)
            r = _tree;
        else
            NinaError.error("莫名其妙的错误.", 944907);
        
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
            NinaError.error("莫名其妙的错误.", 496672);
        }
    }
    public void replace(NinaExprTree _ov, NinaExprTree _nv) {
        if (l == _ov)
            replace(0, _nv);
        else if (r == _ov)
            replace(1, _nv);
        else
            NinaError.error("莫名其妙的错误.", 157782);
    }
    public void abdicate(NinaExprTree _tree) {
        if (boss != null)
            boss.replace(this, _tree);
        _tree.append(this);
    }
    public ANinaASTExpression compile(
            HashSet<NinaWithStatementTypes> _withs, bool _isRoot = true) {
        NinaErrorPosition pos
            = new NinaErrorPosition(
                block.file, block.line, block.col
            );
        if (type == NinaExprTreeType.CompiledBlock) {
            return compiledBlock !;
        }
        else if (type == NinaExprTreeType.Void) {
            return new NinaASTListExpression(pos);
        }
        else if (type == NinaExprTreeType.Data) {
            if (block.type == NinaCodeBlockType.Identifier) {
                if (block.val_id_chinese == true
                        && ! _withs.Contains(NinaWithStatementTypes.Chinese)) {
                    NinaError.error(
                        "无效的标识符.", 568912,
                        new NinaErrorPosition(block.file, block.line, block.col)
                    );
                }
                return
                    new NinaASTIdentifierExpression(
                        NinaCompilerUtil.format_identifier(block.code),
                        pos
                    );
            }
            else if (block.type == NinaCodeBlockType.Number) {
                return
                    new NinaASTLiteralExpression(
                        (double) block.val_num !,
                        pos
                    );
            }
            else if (block.type == NinaCodeBlockType.String) {
                return
                    new NinaASTLiteralExpression(
                        block.val_str !,
                        pos
                    );
            }
            else {
                NinaError.error(
                    "无效的表达式.", 119832,
                    new NinaErrorPosition(block.file, block.line, block.col)
                );
            }
        }
        else if (type == NinaExprTreeType.Operator) {
            if (block.val_op_chinese == true
                    && ! _withs.Contains(NinaWithStatementTypes.Chinese)) {
                NinaError.error(
                    "无效的保留字.", 356193,
                    new NinaErrorPosition(block.file, block.line, block.col)
                );
            }
            if (l == null || r == null) {
                NinaError.error("莫名其妙的错误.", 584489);
            }
            else if (block.val_op_unary == false) {
                ANinaASTExpression node_l = l!.compile(_withs: _withs, _isRoot: false);
                ANinaASTExpression node_r = r!.compile(_withs: _withs, _isRoot: false);
                ANinaASTExpression? expr_l = node_l as ANinaASTCommonExpression;
                ANinaASTExpression? expr_r = node_r as ANinaASTCommonExpression;
                NinaASTListExpression? list_l = node_l as NinaASTListExpression;
                NinaASTListExpression? list_r = node_r as NinaASTListExpression;
                NinaASTBlockExpression? compiledBlock_l = node_l as NinaASTBlockExpression;
                NinaASTBlockExpression? compiledBlock_r = node_r as NinaASTBlockExpression;

                if (block.val_op == NinaOperatorType.Arr) {
                    if ((list_l == null && expr_l == null)
                            || compiledBlock_r == null) {
                        NinaError.error(
                            "无效的匿名函数创建表达式.", 186009,
                            new NinaErrorPosition(block.file, block.line, block.col)
                        );
                    }
                    else {
                        NinaASTSuperListExpression plist
                            = list_l != null
                                ? NinaCompilerUtil.transfer_list2params(list_l, block)
                                : NinaCompilerUtil.transfer_list2params(expr_l !, block);
                        return new NinaASTBinaryExpression(
                            _type: NinaOperatorType.Arr,
                            _expr_l: plist,
                            _expr_r: compiledBlock_r,
                            pos
                        );
                    }
                }
                else if (compiledBlock_l != null || compiledBlock_r != null) {
                    NinaError.error(
                        "表达式的两侧发现无效的语句块表达式.",
                        201919,
                        new NinaErrorPosition(block.file, block.line, block.col)
                    );
                }
                else if (block.val_op == NinaOperatorType.Com) {
                    if (_isRoot) {
                        NinaError.error(
                            "无效的列表表达式.", 924405,
                            new NinaErrorPosition(block.file, block.line, block.col)
                        );
                    }
                    else if (list_l != null && list_r != null) {
                        list_l.list.AddRange(list_r.list);
                        return list_l;
                    }
                    else if (list_l != null && expr_r != null) {
                        list_l.list.Add(expr_r);
                        return list_l;
                    }
                    else if (expr_l != null && list_r != null) {
                        List<ANinaASTExpression> list
                            = new List<ANinaASTExpression>() {
                                expr_l
                            };
                        list.AddRange(list_r.list);
                        list_l = new NinaASTListExpression(
                            list,
                            pos
                        );
                        return list_l;
                    }
                    else if (expr_l != null && expr_r != null) {
                        return
                            new NinaASTListExpression(
                                new List<ANinaASTExpression>() {
                                    expr_l, expr_r
                                },
                                pos
                            );
                    }
                    else {
                        NinaError.error(
                            "无效的列表表达式.", 301923,
                            new NinaErrorPosition(block.file, block.line, block.col)
                        );
                    }
                }
                else if (block.val_op == NinaOperatorType.BraL) {
                    if (expr_l == null) {
                        NinaError.error(
                            "函数调用表达式的左侧表达式无效.",
                            593929,
                            new NinaErrorPosition(block.file, block.line, block.col)
                        );
                    }
                    else if (list_r != null) {
                        return
                            new NinaASTBinaryExpression(
                                _type: NinaOperatorType.BraL,
                                _expr_l: expr_l,
                                _expr_r: list_r,
                                pos
                            );
                    }
                    else if (expr_r != null) {
                        return
                            new NinaASTBinaryExpression(
                                _type: NinaOperatorType.BraL,
                                _expr_l: expr_l,
                                _expr_r: new NinaASTListExpression(
                                    new List<ANinaASTExpression>() {
                                        expr_r
                                    },
                                    pos
                                ),
                                pos
                            );
                    }
                    else {
                        NinaError.error(
                            "函数调用表达式的右侧表达式无效.",
                            194021,
                            new NinaErrorPosition(block.file, block.line, block.col)
                        );
                    }
                }
                else if (expr_l == null || expr_r == null) {
                    NinaError.error(
                        "表达式两侧的表达式无效.",
                        249439,
                        new NinaErrorPosition(block.file, block.line, block.col)
                    );
                }
                else if (block.val_op == NinaOperatorType.MBraL) {
                    NinaASTBinaryExpression ret
                        = new NinaASTBinaryExpression(
                            _type: NinaOperatorType.MBraL,
                            _expr_l: expr_l,
                            _expr_r: expr_r,
                            pos
                        );
                    if (_withs.Contains(NinaWithStatementTypes.Strongly))
                        ret.annos.Add(NinaConstsProviderUtil.NINA_ANNO_STRONGLY);
                    return ret;
                }
                else if (block.val_op == NinaOperatorType.Dot) {
                    NinaASTIdentifierExpression? id
                        = expr_r as NinaASTIdentifierExpression;
                    if (id == null) {
                        NinaError.error(
                            "成员访问语法糖表达式的右侧表达式无效.",
                            595886,
                            new NinaErrorPosition(block.file, block.line, block.col)
                        );
                    }
                    else {
                        NinaASTBinaryExpression ret
                            = new NinaASTBinaryExpression(
                                _type: NinaOperatorType.MBraL,
                                _expr_l: expr_l,
                                _expr_r: new NinaASTLiteralExpression(
                                    NinaCompilerUtil.unformat_identifier(id.name),
                                    pos
                                ),
                                pos
                            );
                        if (_withs.Contains(NinaWithStatementTypes.Strongly))
                            ret.annos.Add(NinaConstsProviderUtil.NINA_ANNO_STRONGLY);
                        return ret;
                    }
                }
                else if (block.val_op == NinaOperatorType.Equ) {
                    return
                        new NinaASTBinaryExpression(
                            _type: NinaOperatorType.Equ,
                            _expr_l: expr_l,
                            _expr_r: expr_r,
                            pos
                        );
                }
                else {
                    NinaASTBinaryExpression ret
                        = new NinaASTBinaryExpression(
                            _type: (NinaOperatorType) block.val_op !,
                            _expr_l: expr_l,
                            _expr_r: expr_r,
                            pos
                        );
                    if (_withs.Contains(NinaWithStatementTypes.Strongly))
                        ret.annos.Add(NinaConstsProviderUtil.NINA_ANNO_STRONGLY);
                    return ret;
                }
            }
            else {
                ANinaASTExpression node = r!.compile(_withs: _withs, _isRoot: false);
                ANinaASTExpression? expr = node as ANinaASTCommonExpression;
                NinaASTListExpression? list = node as NinaASTListExpression;
                NinaASTBlockExpression? compiledBlock = node as NinaASTBlockExpression;
                
                if (block.val_op == NinaOperatorType.Object) {
                    if (compiledBlock == null) {
                        NinaError.error(
                            "对象创建表达式的右侧表达式无效.",
                            301529,
                            new NinaErrorPosition(block.file, block.line, block.col)
                        );
                    }
                    return
                        new NinaASTObjectExpression(
                            NinaCompilerUtil.transfer_block2init(compiledBlock !, block),
                            pos
                        );
                }
                else if (block.val_op == NinaOperatorType.Array) {
                    if (list != null) {
                        return
                            new NinaASTObjectExpression(
                                list !,
                                pos
                            );
                    }
                    else if (expr != null) {
                        return
                            new NinaASTObjectExpression(
                                new NinaASTListExpression(
                                    new List<ANinaASTExpression>() {
                                        expr
                                    },
                                    pos
                                ),
                                pos
                            );
                    }
                    else {
                        NinaError.error(
                            "数组创建表达式的右侧表达式无效.",
                            103928,
                            new NinaErrorPosition(block.file, block.line, block.col)
                        );
                    }
                }
                else if (expr == null) {
                    NinaError.error(
                        "单目运算表达式的右侧表达式无效.",
                        113938,
                        new NinaErrorPosition(block.file, block.line, block.col)
                    );
                }
                else if (block.val_op == NinaOperatorType.At) {
                    node.annos.Add(NinaConstsProviderUtil.NINA_ANNO_SPECIALARG);
                    return node;
                }
                else {
                    NinaASTUnaryExpression ret
                        = new NinaASTUnaryExpression(
                            _type: (NinaOperatorType) block.val_op !,
                            _expr: expr,
                            pos
                        );
                    if (_withs.Contains(NinaWithStatementTypes.Strongly))
                        ret.annos.Add(NinaConstsProviderUtil.NINA_ANNO_STRONGLY);
                    return ret;
                }
            }
        }
        else {
            NinaError.error("莫名其妙的错误.", 214128);
        }

        return new NinaASTLiteralExpression(pos);
    }
}