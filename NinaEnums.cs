namespace Nina;

enum NinaCodeBlockType {
    None,
    Operator, Symbol, Keyword, Identifier, String, Number
}

enum NinaExprTreeType {
    None,
    Void, Operator, Data, Placeholder, CompiledBlock
}

enum NinaSymbolType {
    None,
    Sem, CBraL, CBraR
}

enum NinaKeywordType {
    None,
    Var, Const, Func,
    If, Else, Elseif, While, Return, Break, Continue,
    Try, Catch,
    With
}

enum NinaOperatorType {
    None,
    Com, Equ,
    LOr, LAnd, Or, XOr, And, LEqu, LNEqu,
    More, Less, MoreE, LessE,
    SftL, SftR,
    Add, Sub, Mut, Div, Rem, Pow,
    Not, LNot, Pos, Neg, Typeof, Object, Array, At,
    BraL, BraR, MBraL, MBraR, Dot, Arr
}

enum NinaScopeType {
    None = 0,
    Root = 1 << 1,
    Function = 1 << 2,
    If = 1 << 3,
    Else = 1 << 4,
    Elseif = 1 << 5,
    While = 1 << 6,
    Try = 1 << 7,
    Catch = 1 << 8
}

enum NinaWithStatementTypes {
    None,
    Strongly, Chinese
}