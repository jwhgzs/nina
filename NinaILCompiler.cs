using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Reflection;
using System.Reflection.Emit;

namespace Nina;

static class NinaILCompiler {
    public static ConstructorInfo compile_innerCtor(string _type) {
        switch (_type) {
            case "array":
                return typeof(NinaDataArray).GetConstructors()[0];
            case "object":
                return typeof(NinaDataObject).GetConstructors()[0];
        }
        NinaError.error("undefined inner constructor.", 256651);
        return null !;
    }
    public static MethodInfo? compile_innerFunc(string _func) {
        MethodInfo? ret = null;
        if (_func.StartsWith(NinaConstsProviderUtil.CSHARP_NINAAPI_PREFIX)) {
            string rname = _func.Remove(0,
                NinaConstsProviderUtil.CSHARP_NINAAPI_PREFIX.Length);
            ret = typeof(NinaAPI).GetMethod(rname,
                BindingFlags.Public | BindingFlags.Static);
        }
        else if (_func.StartsWith(NinaConstsProviderUtil.CSHARP_NINAAPIUTIL_PREFIX)) {
            string rname = _func.Remove(0,
                NinaConstsProviderUtil.CSHARP_NINAAPIUTIL_PREFIX.Length);
            ret = typeof(NinaAPIUtil).GetMethod(rname,
                BindingFlags.Public | BindingFlags.Static);
        }
        else if (_func.StartsWith(NinaConstsProviderUtil.CSHARP_ID_PREFIX)) {
            string rname = _func.Remove(0,
                NinaConstsProviderUtil.CSHARP_ID_PREFIX.Length);
            ret = typeof(NinaAPI).GetMethod(rname,
                BindingFlags.Public | BindingFlags.Static);
        }
        return ret !;
    }
    public static void compile_identifier(
            IdentifierNameSyntax _id, ILGenerator _g,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, FieldInfo> _fields,
            Dictionary<string, FieldInfo> _field_consts,
            Dictionary<string, LocalBuilder> _locals,
            Dictionary<string, LocalBuilder> _local_consts,
            bool _isSetting = false,
            TypeBuilder? _closure_builder = null) {
        string idname = _id.Identifier.ValueText;
        if (! _isSetting) {
            if (_locals.TryGetValue(idname,
                    out LocalBuilder? from_locals)) {
                _g.Emit(OpCodes.Ldloc, from_locals);
            }
            else if (_local_consts.TryGetValue(idname,
                    out LocalBuilder? from_local_consts)) {
                _g.Emit(OpCodes.Ldloc, from_local_consts);
            }
            else if (_fields.TryGetValue(idname,
                    out FieldInfo? from_fields)) {
                _g.Emit(OpCodes.Ldsfld, from_fields);
            }
            else if (_field_consts.TryGetValue(idname,
                    out FieldInfo? from_field_consts)) {
                _g.Emit(OpCodes.Ldsfld, from_field_consts);
            }
            else if (_globs.TryGetValue(idname,
                    out FieldInfo? from_glob)) {
                _g.Emit(OpCodes.Ldsfld, from_glob);
            }
            else if (_glob_consts.TryGetValue(idname,
                    out FieldInfo? from_glob_const)) {
                _g.Emit(OpCodes.Ldsfld, from_glob_const);
            }
            else {
                NinaError.error("undefined variable.", 997023);
            }
        }
        else {
            if (_locals.TryGetValue(idname,
                    out LocalBuilder? from_locals)) {
                _g.Emit(OpCodes.Stloc, from_locals);
            }
            else if (_local_consts.TryGetValue(idname,
                    out LocalBuilder? from_local_consts)) {
                NinaError.error("invalid assignment to constant.", 491293);
            }
            else if (_fields.TryGetValue(idname,
                    out FieldInfo? from_field)) {
                _g.Emit(OpCodes.Stsfld, from_field);
            }
            else if (_field_consts.TryGetValue(idname, out _)) {
                NinaError.error("invalid assignment to constant.", 529931);
            }
            else if (_globs.TryGetValue(idname,
                    out FieldInfo? from_glob)) {
                _g.Emit(OpCodes.Stsfld, from_glob);
            }
            else if (_glob_consts.TryGetValue(idname, out _)) {
                NinaError.error("invalid assignment to constant.", 493929);
            }
            else if (idname == "aux") {
                _g.Emit(OpCodes.Pop);
            }
            else {
                Console.WriteLine(idname);
                NinaError.error("undefined variable.", 194161);
            }
        }
    }
    public static void compile_expr(
            ModuleBuilder _mb, TypeBuilder _cl, ILGenerator _g, ExpressionSyntax _expr,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, FieldInfo> _fields,
            Dictionary<string, FieldInfo> _field_consts,
            Dictionary<string, LocalBuilder> _locals,
            Dictionary<string, LocalBuilder> _local_consts,
            TypeBuilder? _closure_builder = null) {
        if (_expr is LiteralExpressionSyntax lit) {
            switch (lit.Kind()) {
                case SyntaxKind.NumericLiteralExpression:
                    _g.Emit(OpCodes.Ldc_R8, (double) lit.Token.Value !);
                    _g.Emit(OpCodes.Box, typeof(double));
                    break;
                case SyntaxKind.StringLiteralExpression:
                    _g.Emit(OpCodes.Ldstr, (string) lit.Token.Value !);
                    break;
                case SyntaxKind.TrueLiteralExpression:
                    _g.Emit(OpCodes.Ldc_I4_1);
                    break;
                case SyntaxKind.FalseLiteralExpression:
                    _g.Emit(OpCodes.Ldc_I4_0);
                    break;
                case SyntaxKind.NullLiteralExpression:
                    _g.Emit(OpCodes.Ldnull);
                    break;
                default:
                    NinaError.error("unexpected error.", 691934);
                    break;
            }
        }
        else if (_expr is IdentifierNameSyntax id) {
            compile_identifier(
                _id: id,
                _g: _g,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: _fields,
                _field_consts: _field_consts,
                _locals: _locals,
                _local_consts: _local_consts
            );
        }
        else if (_expr is BinaryExpressionSyntax binary) {
            if (_expr.IsKind(SyntaxKind.LogicalOrExpression)
                    || _expr.IsKind(SyntaxKind.LogicalAndExpression)) {
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: binary.Left,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _closure_builder: _closure_builder,
                    _fields: _fields,
                    _field_consts: _field_consts,
                    _locals: _locals,
                    _local_consts: _local_consts
                );
                _g.Emit(OpCodes.Dup);
                _g.Emit(OpCodes.Call, typeof(NinaAPIUtil).GetMethod("toBool") !);
                Label label = _g.DefineLabel();
                _g.Emit(
                    _expr.IsKind(SyntaxKind.LogicalOrExpression)
                        ? OpCodes.Brtrue
                        : OpCodes.Brfalse,
                    label
                );
                _g.Emit(OpCodes.Pop);
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: binary.Right,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _closure_builder: _closure_builder,
                    _fields: _fields,
                    _field_consts: _field_consts,
                    _locals: _locals,
                    _local_consts: _local_consts
                );
                _g.MarkLabel(label);
            }
            else {
                NinaError.error("unexpected error.", 220291);
            }
        }
        else if (_expr is AssignmentExpressionSyntax assign) {
            ExpressionSyntax l = assign.Left;
            ExpressionSyntax r = assign.Right;
            if (l is ElementAccessExpressionSyntax elmacc) {
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: elmacc.Expression,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _closure_builder: _closure_builder,
                    _fields: _fields,
                    _field_consts: _field_consts,
                    _locals: _locals,
                    _local_consts: _local_consts
                );
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: elmacc.ArgumentList.Arguments[0].Expression,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _closure_builder: _closure_builder,
                    _fields: _fields,
                    _field_consts: _field_consts,
                    _locals: _locals,
                    _local_consts: _local_consts
                );
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: r,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _closure_builder: _closure_builder,
                    _fields: _fields,
                    _field_consts: _field_consts,
                    _locals: _locals,
                    _local_consts: _local_consts
                );
                _g.EmitCall(
                    opcode: OpCodes.Call,
                    methodInfo: typeof(NinaAPIUtil).GetMethod("member_set") !,
                    optionalParameterTypes: null
                );
            }
            else if (l is IdentifierNameSyntax nid) {
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: r,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _closure_builder: _closure_builder,
                    _fields: _fields,
                    _field_consts: _field_consts,
                    _locals: _locals,
                    _local_consts: _local_consts
                );
                _g.Emit(OpCodes.Dup);
                compile_identifier(
                    _id: nid,
                    _g: _g,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _closure_builder: _closure_builder,
                    _fields: _fields,
                    _field_consts: _field_consts,
                    _locals: _locals,
                    _local_consts: _local_consts,
                    _isSetting: true
                );
            }
            else {
                NinaError.error("invalid lvalue.", 807500);
            }
        }
        else if (_expr is ElementAccessExpressionSyntax elmacc) {
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: elmacc.Expression,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: _fields,
                _field_consts: _field_consts,
                _locals: _locals,
                _local_consts: _local_consts
            );
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: elmacc.ArgumentList.Arguments[0].Expression,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: _fields,
                _field_consts: _field_consts,
                _locals: _locals,
                _local_consts: _local_consts
            );
            _g.EmitCall(
                opcode: OpCodes.Call,
                methodInfo: typeof(NinaAPIUtil).GetMethod("member_get") !,
                optionalParameterTypes: null
            );
        }
        else if (_expr is InvocationExpressionSyntax invo) {
            IdentifierNameSyntax? nid
                = invo.Expression as IdentifierNameSyntax;
            MethodInfo? inner = nid != null
                ? compile_innerFunc(nid.Identifier.ValueText)
                : null;
            bool isInner = inner != null;
            List<ArgumentSyntax> args
                = invo.ArgumentList.Arguments.ToList();
            if (! isInner) {
                LocalBuilder tmp = _g.DeclareLocal(typeof(object));
                if (nid == null
                        && invo.Expression is ElementAccessExpressionSyntax ea) {
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: ea.Expression,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _closure_builder: _closure_builder,
                        _fields: _fields,
                        _field_consts: _field_consts,
                        _locals: _locals,
                        _local_consts: _local_consts
                    );
                    _g.Emit(OpCodes.Stloc, tmp);
                    _g.Emit(OpCodes.Ldloc, tmp);
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: ea.ArgumentList.Arguments[0].Expression,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _closure_builder: _closure_builder,
                        _fields: _fields,
                        _field_consts: _field_consts,
                        _locals: _locals,
                        _local_consts: _local_consts
                    );
                    _g.EmitCall(
                        opcode: OpCodes.Call,
                        methodInfo: typeof(NinaAPIUtil).GetMethod("member_get") !,
                        optionalParameterTypes: null
                    );
                }
                else {
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: invo.Expression,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _closure_builder: _closure_builder,
                        _fields: _fields,
                        _field_consts: _field_consts,
                        _locals: _locals,
                        _local_consts: _local_consts
                    );
                    if (nid != null && nid.Identifier.ValueText == "self") {
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: _g,
                            _expr: IdentifierName("this"),
                            _globs: _globs,
                            _glob_consts: _glob_consts,
                            _closure_builder: _closure_builder,
                            _fields: _fields,
                            _field_consts: _field_consts,
                            _locals: _locals,
                            _local_consts: _local_consts
                        );
                    }
                    else {
                        _g.Emit(OpCodes.Ldnull);
                    }
                    _g.Emit(OpCodes.Stloc, tmp);
                }
                _g.Emit(OpCodes.Dup);
                _g.Emit(OpCodes.Isinst, typeof(Func<object[], object>));
                Label label = _g.DefineLabel();
                _g.Emit(OpCodes.Brtrue, label);
                _g.Emit(OpCodes.Ldstr, "invalid function to call.");
                _g.Emit(OpCodes.Ldc_I4, 949921);
                _g.Emit(OpCodes.Call, typeof(NinaAPIUtil).GetMethod("error") !);
                _g.MarkLabel(label);
                _g.Emit(OpCodes.Ldc_I4, args.Count + 1);
                _g.Emit(OpCodes.Newarr, typeof(object));
                _g.Emit(OpCodes.Dup);
                _g.Emit(OpCodes.Ldc_I4_0);
                if (args.Count > 0 && args[0].Expression.HasAnnotations(
                        NinaConstsProviderUtil.CSHARP_ANNO_SPECIALARG)) {
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: args[0].Expression,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _closure_builder: _closure_builder,
                        _fields: _fields,
                        _field_consts: _field_consts,
                        _locals: _locals,
                        _local_consts: _local_consts
                    );
                    args.RemoveAt(0);
                }
                else {
                    _g.Emit(OpCodes.Ldloc, tmp);
                }
                _g.Emit(OpCodes.Stelem_Ref);
                for (int i = 0; i < args.Count; ++ i) {
                    ExpressionSyntax v = args[i].Expression;
                    _g.Emit(OpCodes.Dup);
                    _g.Emit(OpCodes.Ldc_I4, i + 1);
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: v,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _closure_builder: _closure_builder,
                        _fields: _fields,
                        _field_consts: _field_consts,
                        _locals: _locals,
                        _local_consts: _local_consts
                    );
                    _g.Emit(OpCodes.Stelem_Ref);
                }
                
                _g.EmitCall(
                    opcode: OpCodes.Callvirt,
                    methodInfo: typeof(Func<object[], object>).GetMethod("Invoke") !,
                    optionalParameterTypes: null
                );
            }
            else {
                if (args.Count != inner!.GetParameters().Count()) {
                    NinaError.error(
                        "you must line up your arguments " +
                        "when calling built-in functions.",
                        645668);
                }
                for (int i = 0; i < args.Count; ++ i) {
                    ExpressionSyntax v = args[i].Expression;
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: v,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _closure_builder: _closure_builder,
                        _fields: _fields,
                        _field_consts: _field_consts,
                        _locals: _locals,
                        _local_consts: _local_consts
                    );
                }
                _g.EmitCall(
                    opcode: OpCodes.Call,
                    methodInfo: inner !,
                    optionalParameterTypes: null
                );
            }
        }
        else if (_expr is ObjectCreationExpressionSyntax objcr) {
            LocalBuilder tmp = _g.DeclareLocal(typeof(object[]));
            IdentifierNameSyntax? name
                = objcr.Type as IdentifierNameSyntax;
            if (name == null)
                NinaError.error("unexpected error.", 415113);
            ConstructorInfo cinfo
                = compile_innerCtor(name!.Identifier.ValueText);
            List<ArgumentSyntax> args
                = objcr.ArgumentList != null
                    ? objcr.ArgumentList.Arguments.ToList()
                    : new List<ArgumentSyntax>();
            if (args.Count != 0) {
                NinaError.error("unexpected error", 559252);
            }
            _g.Emit(OpCodes.Newobj, cinfo);
            InitializerExpressionSyntax? init = objcr.Initializer;
            if (init != null) {
                List<ExpressionSyntax> list = init.Expressions.ToList();
                for (int i = 0; i < list.Count; ++ i) {
                    AssignmentExpressionSyntax? v
                        = list[i] as AssignmentExpressionSyntax;
                    if (v == null) {
                        NinaError.error("unexpected error.", 124991);
                    }
                    else {
                        ImplicitElementAccessSyntax? key_s
                            = v.Left as ImplicitElementAccessSyntax;
                        ExpressionSyntax val = v.Right;
                        if (key_s == null) {
                            NinaError.error("unexpected error.", 419022);
                        }
                        else {
                            ExpressionSyntax key
                                = key_s.ArgumentList.Arguments[0].Expression;
                            _g.Emit(OpCodes.Dup);
                            compile_expr(
                                _mb: _mb,
                                _cl: _cl,
                                _g: _g,
                                _expr: key,
                                _globs: _globs,
                                _glob_consts: _glob_consts,
                                _closure_builder: _closure_builder,
                                _fields: _fields,
                                _field_consts: _field_consts,
                                _locals: _locals,
                                _local_consts: _local_consts
                            );
                            compile_expr(
                                _mb: _mb,
                                _cl: _cl,
                                _g: _g,
                                _expr: val,
                                _globs: _globs,
                                _glob_consts: _glob_consts,
                                _closure_builder: _closure_builder,
                                _fields: _fields,
                                _field_consts: _field_consts,
                                _locals: _locals,
                                _local_consts: _local_consts
                            );
                            if (key_s.HasAnnotations(
                                    NinaConstsProviderUtil.CSHARP_ANNO_CONST))
                                _g.Emit(OpCodes.Ldc_I4_1);
                            else
                                _g.Emit(OpCodes.Ldc_I4_0);
                            _g.EmitCall(
                                opcode: OpCodes.Call,
                                methodInfo:
                                    typeof(NinaAPIUtil).GetMethod("member_init") !,
                                optionalParameterTypes: null
                            );
                            _g.Emit(OpCodes.Pop);
                        }
                    }
                }
            }
        }
        else if (_expr is ParenthesizedLambdaExpressionSyntax lambda) {
            TypeBuilder cl
                = _mb.DefineType(
                    name: NinaConstsProviderUtil.IL_CLOSURECLASS_ID_PREFIX
                        + Guid.NewGuid().ToString("N"),
                    attr: TypeAttributes.Public | TypeAttributes.Sealed
                );
            Dictionary<string, FieldInfo> fields
                = new Dictionary<string, FieldInfo>(_fields);
            Dictionary<string, FieldInfo> field_consts
                = new Dictionary<string, FieldInfo>(_field_consts);
            Dictionary<string, LocalBuilder> locals
                = new Dictionary<string, LocalBuilder>();
            Dictionary<string, LocalBuilder> local_consts
                = new Dictionary<string, LocalBuilder>();
            MethodBuilder mb = cl.DefineMethod(
                name: "func",
                attributes: MethodAttributes.Public | MethodAttributes.Static,
                callingConvention: CallingConventions.Standard,
                returnType: typeof(object),
                parameterTypes: new [] { typeof(object[]) }
            );
            FieldBuilder fb_self
                = cl.DefineField(
                    fieldName: "self",
                    type: typeof(Func<object[], object>),
                    attributes: FieldAttributes.Public | FieldAttributes.Static
                );
            field_consts["self"] = fb_self;
            ILGenerator g = mb.GetILGenerator();

            List<ParameterSyntax> paramsList
                = lambda.ParameterList.Parameters.ToList();
            paramsList.Insert(
                0,
                Parameter(
                    attributeLists: new SyntaxList<AttributeListSyntax>(),
                    modifiers: TokenList(),
                    type: PredefinedType(Token(SyntaxKind.ObjectKeyword)),
                    identifier: Identifier("this"),
                    @default: null
                )
            );
            g.Emit(OpCodes.Ldarg_0);
            g.Emit(OpCodes.Ldlen);
            for (int i = 0; i < paramsList.Count; ++ i) {
                ParameterSyntax v = paramsList[i];
                string vname = v.Identifier.ValueText;
                ExpressionSyntax? initExpr = v.Default != null
                    ? v.Default.Value
                    : null;
                LocalBuilder builder = g.DeclareLocal(typeof(object));
                FieldBuilder builder2 = cl.DefineField(
                    fieldName: NinaConstsProviderUtil.IL_CLOSURECLASS_FIELD_PREFIX
                        + Guid.NewGuid().ToString("N"),
                    type: typeof(object),
                    attributes: FieldAttributes.Public | FieldAttributes.Static
                );
                locals[vname] = builder;
                fields[vname] = builder2;
                g.Emit(OpCodes.Dup);
                g.Emit(OpCodes.Ldc_I4, i);
                Label label1 = g.DefineLabel();
                g.Emit(OpCodes.Ble, label1);
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldc_I4, i);
                g.Emit(OpCodes.Ldelem_Ref);
                Label label2 = g.DefineLabel();
                g.Emit(OpCodes.Br, label2);
                g.MarkLabel(label1);
                if (initExpr != null) {
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: g,
                        _expr: initExpr,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _closure_builder: _closure_builder,
                        _fields: _fields,
                        _field_consts: _field_consts,
                        _locals: locals,
                        _local_consts: local_consts
                    );
                }
                else {
                    g.Emit(OpCodes.Ldnull);
                }
                g.MarkLabel(label2);
                g.Emit(OpCodes.Dup);
                g.Emit(OpCodes.Stloc, builder);
                g.Emit(OpCodes.Stsfld, builder2);
            }
            g.Emit(OpCodes.Pop);

            compile_block(
                _mb: _mb,
                _cl: _cl,
                _g: g,
                _block: lambda.Block !,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: cl,
                _fields: fields,
                _field_consts: field_consts,
                _locals: locals,
                _local_consts: local_consts
            );
            
            cl.CreateType();
            _g.Emit(OpCodes.Ldnull);
            _g.Emit(OpCodes.Ldftn, mb);
            _g.Emit(
                OpCodes.Newobj,
                typeof(Func<object[], object>).GetConstructors()[0]
            );
            _g.Emit(OpCodes.Dup);
            _g.Emit(OpCodes.Stsfld, fb_self);
        }
        else {
            NinaError.error("unexpected error.", 844249);
        }
    }
    public static void compile_block(
            ModuleBuilder _mb, TypeBuilder _cl, ILGenerator _g, BlockSyntax _block,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, FieldInfo> _fields,
            Dictionary<string, FieldInfo> _field_consts,
            Dictionary<string, LocalBuilder> _locals,
            Dictionary<string, LocalBuilder> _local_consts,
            Label? _label_break = null, Label? _label_continue = null,
            TypeBuilder? _closure_builder = null) {
        List<StatementSyntax> stms = _block.Statements.ToList();
        Dictionary<string, FieldInfo> fields
            = new Dictionary<string, FieldInfo>(_fields);
        Dictionary<string, FieldInfo> field_consts
            = new Dictionary<string, FieldInfo>(_field_consts);
        for (int i = 0; i < stms.Count; ++ i) {
            StatementSyntax v = stms[i];
            compile_stm(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _stm: v,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: fields,
                _field_consts: field_consts,
                _locals: _locals,
                _local_consts: _local_consts,
                _label_break: _label_break,
                _label_continue: _label_continue
            );
        }
    }
    public static void compile_stm(
            ModuleBuilder _mb, TypeBuilder _cl, ILGenerator _g, StatementSyntax _stm,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, FieldInfo> _fields,
            Dictionary<string, FieldInfo> _field_consts,
            Dictionary<string, LocalBuilder> _locals,
            Dictionary<string, LocalBuilder> _local_consts,
            Label? _label_break = null, Label? _label_continue = null,
            TypeBuilder? _closure_builder = null) {
        if (_stm is ExpressionStatementSyntax expr) {
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: expr.Expression,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: _fields,
                _field_consts: _field_consts,
                _locals: _locals,
                _local_consts: _local_consts
            );
            _g.Emit(OpCodes.Pop);
        }
        else if (_stm is LocalDeclarationStatementSyntax vars) {
            List<VariableDeclaratorSyntax> list
                = vars.Declaration.Variables.ToList();
            var dic = vars.IsConst ? _field_consts : _fields;
            for (int i = 0; i < list.Count; ++ i) {
                VariableDeclaratorSyntax v = list[i];
                string id = v.Identifier.ValueText;
                FieldBuilder builder = _closure_builder!.DefineField(
                    fieldName: NinaConstsProviderUtil.IL_CLOSURECLASS_FIELD_PREFIX
                        + Guid.NewGuid().ToString("N"),
                    type: typeof(object),
                    attributes: FieldAttributes.Public | FieldAttributes.Static
                );
                dic[id] = builder;
                if (v.Initializer != null) {
                    ExpressionSyntax initExpr = v.Initializer.Value;
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: initExpr,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _closure_builder: _closure_builder,
                        _fields: _fields,
                        _field_consts: _field_consts,
                        _locals: _locals,
                        _local_consts: _local_consts
                    );
                    _g.Emit(OpCodes.Stsfld, builder);
                }
            }
        }
        else if (_stm is IfStatementSyntax ifs) {
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: ifs.Condition,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: _fields,
                _field_consts: _field_consts,
                _locals: _locals,
                _local_consts: _local_consts
            );
            Label label_else = _g.DefineLabel();
            _g.Emit(OpCodes.Brfalse, label_else);
            BlockSyntax? if_block = ifs.Statement as BlockSyntax;
            if (if_block == null) {
                NinaError.error("unexpected error.", 292481);
            }
            compile_block(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _block: if_block !,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: _fields,
                _field_consts: _field_consts,
                _locals: _locals,
                _local_consts: _local_consts,
                _label_break: _label_break,
                _label_continue: _label_continue
            );
            Label label_end = _g.DefineLabel();
            _g.Emit(OpCodes.Br, label_end);
            _g.MarkLabel(label_else);
            BlockSyntax? else_block
                = ifs.Else != null
                    ? ifs.Else.Statement as BlockSyntax
                    : null;
            if (else_block != null) {
                compile_block(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _block: else_block !,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _closure_builder: _closure_builder,
                    _fields: _fields,
                    _field_consts: _field_consts,
                    _locals: _locals,
                    _local_consts: _local_consts,
                    _label_break: _label_break,
                    _label_continue: _label_continue
                );
            }
            _g.MarkLabel(label_end);
        }
        else if (_stm is WhileStatementSyntax whiles) {
            Label label_while = _g.DefineLabel();
            Label label_end = _g.DefineLabel();
            _g.MarkLabel(label_while);
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: whiles.Condition,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: _fields,
                _field_consts: _field_consts,
                _locals: _locals,
                _local_consts: _local_consts
            );
            _g.Emit(OpCodes.Brfalse, label_end);
            BlockSyntax? while_block = whiles.Statement as BlockSyntax;
            if (while_block == null) {
                NinaError.error("unexpected error.", 195812);
            }
            compile_block(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _block: while_block !,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: _fields,
                _field_consts: _field_consts,
                _locals: _locals,
                _local_consts: _local_consts,
                _label_break: label_end,
                _label_continue: label_while
            );
            _g.Emit(OpCodes.Br, label_while);
            _g.MarkLabel(label_end);
        }
        else if (_stm is ReturnStatementSyntax returns) {
            if (returns.Expression != null) {
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: returns.Expression,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _closure_builder: _closure_builder,
                    _fields: _fields,
                    _field_consts: _field_consts,
                    _locals: _locals,
                    _local_consts: _local_consts
                );
            }
            else {
                _g.Emit(OpCodes.Ldnull);
            }
            _g.Emit(OpCodes.Ret);
        }
        else if (_stm is BreakStatementSyntax) {
            if (_label_break != null)
                _g.Emit(OpCodes.Br, (Label) _label_break !);
            else
                NinaError.error("unexpected error.", 231187);
        }
        else if (_stm is ContinueStatementSyntax) {
            if (_label_continue != null)
                _g.Emit(OpCodes.Br, (Label) _label_continue !);
            else
                NinaError.error("unexpected error.", 481929);
        }
        else {
            NinaError.error("unexpected error.", 121055);
        }
    }
    public static void compile_method(
            ModuleBuilder _mb, TypeBuilder _cl, MethodBuilder _builder,
            ParameterListSyntax _params, BlockSyntax _block,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts) {
        ILGenerator g = _builder.GetILGenerator();
        Dictionary<string, LocalBuilder> locals
            = new Dictionary<string, LocalBuilder>();
        Dictionary<string, LocalBuilder> local_consts
            = new Dictionary<string, LocalBuilder>();

        compile_block(
            _mb: _mb,
            _cl: _cl,
            _g: g,
            _block: _block,
            _globs: _globs,
            _glob_consts: _glob_consts,
            _closure_builder: null,
            _fields:
                new Dictionary<string, FieldInfo>(),
            _field_consts:
                new Dictionary<string, FieldInfo>(),
            _locals:
                new Dictionary<string, LocalBuilder>(),
            _local_consts:
                new Dictionary<string, LocalBuilder>()
        );
        g.Emit(OpCodes.Ret);
    }
    public static void init_globs(
            TypeBuilder _tb, ILGenerator _g,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts) {
        MethodInfo[] mtds = typeof(NinaAPI).GetMethods(
            BindingFlags.Public | BindingFlags.Static
        );
        for (int i = 0; i < mtds.Length; ++ i) {
            MethodInfo v = mtds[i];
            Type tp = v.GetType();
            FieldBuilder fb = _tb.DefineField(
                fieldName: NinaConstsProviderUtil.IL_BUILTIN_ID_PREFIX
                    + Guid.NewGuid().ToString("N"),
                type: typeof(Func<object[], object>),
                attributes: FieldAttributes.Public | FieldAttributes.Static
            );
            MethodBuilder mb = _tb.DefineMethod(
                name: NinaConstsProviderUtil.IL_BUILTIN_ID_PREFIX
                    + Guid.NewGuid().ToString("N"),
                attributes: MethodAttributes.Public | MethodAttributes.Static,
                callingConvention: CallingConventions.Standard,
                returnType: typeof(object),
                parameterTypes: new [] { typeof(object[]) }
            );
            ILGenerator mg = mb.GetILGenerator();
            ParameterInfo[] plist = v.GetParameters();
            List<Type> types = new List<Type>();
            mg.Emit(OpCodes.Ldarg_0);
            mg.Emit(OpCodes.Ldlen);
            mg.Emit(OpCodes.Ldc_I4, plist.Length + 1);
            Label label = mg.DefineLabel();
            mg.Emit(OpCodes.Beq, label);
            mg.Emit(
                OpCodes.Ldstr,
                "you must line up your arguments " +
                "when calling built-in functions."
            );
            mg.Emit(OpCodes.Ldc_I4, 294012);
            mg.Emit(OpCodes.Call, typeof(NinaAPIUtil).GetMethod("error") !);
            mg.MarkLabel(label);
            for (int j = 0; j < plist.Length; ++ j) {
                ParameterInfo w = plist[j];
                types.Add(w.ParameterType);
                mg.Emit(OpCodes.Ldarg_0);
                mg.Emit(OpCodes.Ldc_I4, j + 1);
                mg.Emit(OpCodes.Ldelem_Ref);
            }
            mg.EmitCall(
                opcode: OpCodes.Call,
                methodInfo: v,
                optionalParameterTypes: null
            );
            mg.Emit(OpCodes.Ret);
            _g.Emit(OpCodes.Ldnull);
            _g.Emit(OpCodes.Ldftn, mb);
            _g.Emit(OpCodes.Newobj,
                typeof(Func<object[], object>).GetConstructors()[0]);
            _g.Emit(OpCodes.Stsfld, fb);
            _glob_consts[NinaConstsProviderUtil.CSHARP_ID_PREFIX + v.Name]
                = fb;
        }

        FieldInfo[] flds = typeof(NinaAPI).GetFields(
            BindingFlags.Public | BindingFlags.Static
        );
        for (int i = 0; i < flds.Length; ++ i) {
            FieldInfo v = flds[i];
            _glob_consts[NinaConstsProviderUtil.CSHARP_ID_PREFIX + v.Name]
                = v;
        }
    }
    public static Type? compile(ClassDeclarationSyntax _cl) {
        AssemblyName an = new AssemblyName("NinaRuntime");
        AssemblyBuilder ab
            = AssemblyBuilder.DefineDynamicAssembly(an,
                AssemblyBuilderAccess.RunAndCollect);
        ModuleBuilder mb =
            ab.DefineDynamicModule(an.Name !);
        TypeBuilder tb
            = mb.DefineType(
                name: _cl.Identifier.ValueText,
                attr: TypeAttributes.Public | TypeAttributes.Abstract
            );
        
        List<MemberDeclarationSyntax> members = _cl.Members.ToList();
        Dictionary<string, FieldInfo> globs
            = new Dictionary<string, FieldInfo>();
        Dictionary<string, FieldInfo> glob_consts
            = new Dictionary<string, FieldInfo>();
        (MethodBuilder, MethodDeclarationSyntax)? main = null;
        for (int i = 0; i < members.Count; ++ i) {
            MemberDeclarationSyntax v = members[i];
            if (v is MethodDeclarationSyntax m) {
                MethodBuilder builder = tb.DefineMethod(
                    name: m.Identifier.ValueText,
                    attributes:
                        MethodAttributes.Public | MethodAttributes.Static
                );
                main = (builder, m);
            }
            else if (v is FieldDeclarationSyntax f) {
                List<VariableDeclaratorSyntax> vs
                    = f.Declaration.Variables.ToList();
                bool isConst
                    = f.Modifiers.Where(
                        token => token.IsKind(SyntaxKind.ConstKeyword)
                    ).Count() > 0;
                var dic = isConst ? glob_consts : globs;
                for (int j = 0; j < vs.Count; ++ j) {
                    VariableDeclaratorSyntax w = vs[j];
                    string idname = w.Identifier.ValueText;
                    FieldBuilder builder = tb.DefineField(
                        fieldName: idname,
                        type: typeof(object),
                        attributes:
                            FieldAttributes.Public | FieldAttributes.Static
                    );
                    dic[idname] = builder;
                }
            }
            else {
                NinaError.error("unexpected error.", 518393);
            }
        }
        var (mainBuilder, mainDecl) = main!.Value;
        init_globs(
            _tb: tb,
            _g: mainBuilder.GetILGenerator(),
            _globs: globs,
            _glob_consts: glob_consts
        );
        compile_method(
            _mb: mb,
            _cl: tb,
            _builder: mainBuilder,
            _params: mainDecl.ParameterList,
            _block: mainDecl.Body !,
            _globs: globs,
            _glob_consts: glob_consts
        );

        return tb.CreateType();
    }
}