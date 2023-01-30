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
    Var, Const, Func, Class,
    If, Else, Elseif, While, Return, Break, Continue
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
    Class = 1 << 2,
    Function = 1 << 3,
    If = 1 << 4,
    Else = 1 << 5,
    Elseif = 1 << 6,
    While = 1 << 7
}