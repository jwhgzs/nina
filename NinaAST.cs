namespace Nina;

abstract class ANinaAST {
    private List<string> annos = new List<string>();
    public void add_annos(string _content) {
        annos.Add(_content);
    }
    public bool has_annos(string _content) {
        return annos.Contains(_content);
    }
}

abstract class ANinaASTExpression : ANinaAST {
    public NinaOperatorType? type;
}
abstract class ANinaASTCommonExpression : ANinaASTExpression {}

class NinaASTLiteralExpression : ANinaASTCommonExpression {
    public new NinaCodeBlockType type;
    public string? val_s;
    public double? val_d;
    public NinaASTLiteralExpression(string _val) {
        type = NinaCodeBlockType.String;
        val_s = _val;
    }
    public NinaASTLiteralExpression(double _val) {
        type = NinaCodeBlockType.Number;
        val_d = _val;
    }
    public NinaASTLiteralExpression() {
        type = NinaCodeBlockType.None;
    }
}
class NinaASTIdentifierExpression : ANinaASTCommonExpression {
    public new NinaCodeBlockType type;
    public string name;
    public NinaASTIdentifierExpression(string _idname) {
        type = NinaCodeBlockType.Identifier;
        name = _idname;
    }
}
class NinaASTBinaryExpression : ANinaASTCommonExpression {
    public ANinaASTExpression expr_l;
    public ANinaASTExpression expr_r;
    public NinaASTBinaryExpression(
            NinaOperatorType _type, ANinaASTExpression _expr_l,
            ANinaASTExpression _expr_r) {
        type = _type;
        expr_l = _expr_l;
        expr_r = _expr_r;
    }
}
class NinaASTUnaryExpression : ANinaASTCommonExpression {
    public ANinaASTExpression expr;
    public NinaASTUnaryExpression(
            NinaOperatorType _type, ANinaASTExpression _expr) {
        type = _type;
        expr = _expr;
    }
}
class NinaASTListExpression : ANinaASTExpression {
    public List<ANinaASTExpression> list;
    public NinaASTListExpression(List<ANinaASTExpression> _list) {
        list = _list;
    }
    public NinaASTListExpression() {
        list = new List<ANinaASTExpression>();
    }
}
class NinaASTSuperListExpression : ANinaASTExpression {
    public List<(string, ANinaASTExpression?)> list;
    public NinaASTSuperListExpression(List<(string, ANinaASTExpression?)> _list) {
        list = _list;
    }
    public NinaASTSuperListExpression() {
        list = new List<(string, ANinaASTExpression?)>();
    }
}
class NinaASTBlockExpression : ANinaASTExpression {
    public List<ANinaASTStatement> stms;
    public NinaASTBlockExpression(List<ANinaASTStatement> _stms) {
        stms = _stms;
    }
    public NinaASTBlockExpression() {
        stms = new List<ANinaASTStatement>();
    }
}
class NinaASTObjectExpression : ANinaASTCommonExpression {
    public bool isArray;
    public NinaASTBlockExpression? block;
    public NinaASTListExpression? list;
    public NinaASTObjectExpression(NinaASTBlockExpression _block) {
        block = _block;
        isArray = false;
    }
    public NinaASTObjectExpression(NinaASTListExpression _list) {
        list = _list;
        isArray = true;
    }
}

abstract class ANinaASTStatement : ANinaAST {
    public ANinaASTExpression? expr;
    public NinaASTBlockExpression? block;
}

class NinaASTExpressionStatement : ANinaASTStatement {
    public NinaASTExpressionStatement(ANinaASTExpression _expr) {
        expr = _expr;
    }
}
class NinaASTIfStatement : ANinaASTStatement {
    public NinaASTBlockExpression? block_else;
    public NinaASTIfStatement(
            ANinaASTExpression _expr, NinaASTBlockExpression _block,
            NinaASTBlockExpression? _block_else = null) {
        expr = _expr;
        block = _block;
        block_else = _block_else;
    }
}
class NinaASTWhileStatement : ANinaASTStatement {
    public NinaASTWhileStatement(
            ANinaASTExpression _expr, NinaASTBlockExpression _block) {
        expr = _expr;
        block = _block;
    }
}
class NinaASTVarStatement : ANinaASTStatement {
    public bool isConst, isGlobal;
    public NinaASTSuperListExpression vars;
    public NinaASTVarStatement(
            bool _isGlobal,
            NinaASTSuperListExpression? _vars = null,
            bool _isConst = false) {
        vars = _vars ?? new NinaASTSuperListExpression();
        isConst = _isConst;
        isGlobal = _isGlobal;
    }
}
class NinaASTWordStatement : ANinaASTStatement {
    public NinaKeywordType type;
    public NinaASTWordStatement(
            NinaKeywordType _type, ANinaASTExpression? _expr = null) {
        type = _type;
        expr = _expr;
    }
}