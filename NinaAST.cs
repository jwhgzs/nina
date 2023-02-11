namespace Nina;

abstract class ANinaAST {
    public HashSet<string> annos = new HashSet<string>();
    public NinaErrorPosition pos;
    public ANinaAST(NinaErrorPosition _pos) {
        pos = _pos;
    }
}

class NinaASTPlaceholder : ANinaAST {
    public NinaASTPlaceholder(NinaErrorPosition _pos)
            : base(_pos) {}
}

abstract class ANinaASTExpression : ANinaAST {
    public NinaOperatorType? type;
    public ANinaASTExpression(NinaErrorPosition _pos)
            : base(_pos) {}
}
abstract class ANinaASTCommonExpression : ANinaASTExpression {
    public ANinaASTCommonExpression(NinaErrorPosition _pos)
            : base(_pos) {}
}

class NinaASTLiteralExpression : ANinaASTCommonExpression {
    public new NinaCodeBlockType type;
    public string? val_s;
    public double? val_d;
    public NinaASTLiteralExpression(string _val, NinaErrorPosition _pos)
            : base(_pos) {
        type = NinaCodeBlockType.String;
        val_s = _val;
    }
    public NinaASTLiteralExpression(double _val, NinaErrorPosition _pos)
            : base(_pos) {
        type = NinaCodeBlockType.Number;
        val_d = _val;
    }
    public NinaASTLiteralExpression(NinaErrorPosition _pos)
            : base(_pos) {
        type = NinaCodeBlockType.None;
    }
}
class NinaASTIdentifierExpression : ANinaASTCommonExpression {
    public new NinaCodeBlockType type;
    public string name;
    public NinaASTIdentifierExpression(string _idname, NinaErrorPosition _pos)
            : base(_pos) {
        type = NinaCodeBlockType.Identifier;
        name = _idname;
    }
}
class NinaASTBinaryExpression : ANinaASTCommonExpression {
    public ANinaASTExpression expr_l;
    public ANinaASTExpression expr_r;
    public NinaASTBinaryExpression(
            NinaOperatorType _type, ANinaASTExpression _expr_l,
            ANinaASTExpression _expr_r, NinaErrorPosition _pos)
            : base(_pos) {
        type = _type;
        expr_l = _expr_l;
        expr_r = _expr_r;
    }
}
class NinaASTUnaryExpression : ANinaASTCommonExpression {
    public ANinaASTExpression expr;
    public NinaASTUnaryExpression(
            NinaOperatorType _type, ANinaASTExpression _expr, NinaErrorPosition _pos)
            : base(_pos) {
        type = _type;
        expr = _expr;
    }
}
class NinaASTListExpression : ANinaASTExpression {
    public List<ANinaASTExpression> list;
    public NinaASTListExpression(List<ANinaASTExpression> _list, NinaErrorPosition _pos)
            : base(_pos) {
        list = _list;
    }
    public NinaASTListExpression(NinaErrorPosition _pos)
            : base(_pos) {
        list = new List<ANinaASTExpression>();
    }
}
class NinaASTSuperListExpression : ANinaASTExpression {
    public List<(string, ANinaASTExpression?)> list;
    public NinaASTSuperListExpression(List<(string, ANinaASTExpression?)> _list,
            NinaErrorPosition _pos)
            : base(_pos) {
        list = _list;
    }
    public NinaASTSuperListExpression(NinaErrorPosition _pos)
            : base(_pos) {
        list = new List<(string, ANinaASTExpression?)>();
    }
}
class NinaASTBlockExpression : ANinaASTExpression {
    public List<ANinaASTStatement> stms;
    public NinaASTBlockExpression(List<ANinaASTStatement> _stms, NinaErrorPosition _pos)
            : base(_pos) {
        stms = _stms;
    }
    public NinaASTBlockExpression(NinaErrorPosition _pos)
            : base(_pos) {
        stms = new List<ANinaASTStatement>();
    }
}
class NinaASTObjectExpression : ANinaASTCommonExpression {
    public bool isArray;
    public NinaASTBlockExpression? block;
    public NinaASTListExpression? list;
    public NinaASTObjectExpression(NinaASTBlockExpression _block, NinaErrorPosition _pos)
            : base(_pos) {
        block = _block;
        isArray = false;
    }
    public NinaASTObjectExpression(NinaASTListExpression _list, NinaErrorPosition _pos)
            : base(_pos) {
        list = _list;
        isArray = true;
    }
}

abstract class ANinaASTStatement : ANinaAST {
    public ANinaASTExpression? expr;
    public NinaASTBlockExpression? block;
    public ANinaASTStatement(NinaErrorPosition _pos)
            : base(_pos) {}
}

class NinaASTExpressionStatement : ANinaASTStatement {
    public NinaASTExpressionStatement(ANinaASTExpression _expr, NinaErrorPosition _pos)
            : base(_pos) {
        expr = _expr;
    }
}
class NinaASTIfStatement : ANinaASTStatement {
    public NinaASTBlockExpression? block_else;
    public NinaASTIfStatement(
            ANinaASTExpression _expr, NinaASTBlockExpression _block,
            NinaErrorPosition _pos,
            NinaASTBlockExpression? _block_else = null)
            : base(_pos) {
        expr = _expr;
        block = _block;
        block_else = _block_else;
    }
}
class NinaASTWhileStatement : ANinaASTStatement {
    public NinaASTWhileStatement(
            ANinaASTExpression _expr, NinaASTBlockExpression _block,
            NinaErrorPosition _pos)
            : base(_pos) {
        expr = _expr;
        block = _block;
    }
}
class NinaASTVarStatement : ANinaASTStatement {
    public bool isConst, isGlobal;
    public NinaASTSuperListExpression vars;
    public NinaASTVarStatement(
            bool _isGlobal,
            NinaErrorPosition _pos,
            NinaASTSuperListExpression? _vars = null,
            bool _isConst = false)
            : base(_pos) {
        vars = _vars ?? new NinaASTSuperListExpression(_pos);
        isConst = _isConst;
        isGlobal = _isGlobal;
    }
}
class NinaASTWordStatement : ANinaASTStatement {
    public NinaKeywordType type;
    public NinaASTWordStatement(
            NinaKeywordType _type, NinaErrorPosition _pos,
            ANinaASTExpression? _expr = null)
            : base(_pos)  {
        type = _type;
        expr = _expr;
    }
}
class NinaASTTryStatement : ANinaASTStatement {
    public NinaASTBlockExpression? block_catch;
    public NinaASTTryStatement(NinaASTBlockExpression _block,
            NinaErrorPosition _pos,
            NinaASTBlockExpression? _block_catch = null)
            : base(_pos) {
        block = _block;
        block_catch = _block_catch;
    }
}