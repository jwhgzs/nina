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
            NinaASTIdentifierExpression _id, ILGenerator _g,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, FieldInfo> _fields,
            Dictionary<string, FieldInfo> _field_consts,
            Dictionary<string, LocalBuilder> _locals,
            Dictionary<string, LocalBuilder> _local_consts,
            bool _isSetting = false,
            TypeBuilder? _closure_builder = null) {
        string idname = _id.name;
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
            ModuleBuilder _mb, TypeBuilder _cl, ILGenerator _g,
            ANinaASTExpression _expr,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, FieldInfo> _fields,
            Dictionary<string, FieldInfo> _field_consts,
            Dictionary<string, LocalBuilder> _locals,
            Dictionary<string, LocalBuilder> _local_consts,
            TypeBuilder? _closure_builder = null) {
        if (_expr is NinaASTLiteralExpression lit) {
            if (lit.type == NinaCodeBlockType.Number)
                _g.Emit(OpCodes.Ldc_R8, (double) lit.val_d !);
            else if (lit.type == NinaCodeBlockType.String)
                _g.Emit(OpCodes.Ldstr, lit.val_s !);
            else
                NinaError.error("unexpected error.", 120591);
        }
        else if (_expr is NinaASTIdentifierExpression id) {
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
        else if (_expr is NinaASTBinaryExpression binary) {
            if (binary.type == NinaOperatorType.LOr
                    || binary.type == NinaOperatorType.LAnd) {
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: binary.expr_l,
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
                    binary.type == NinaOperatorType.LOr
                        ? OpCodes.Brtrue
                        : OpCodes.Brfalse,
                    label
                );
                _g.Emit(OpCodes.Pop);
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: binary.expr_r,
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
            else if (binary.type == NinaOperatorType.Arr) {
                NinaASTSuperListExpression plist_raw
                    = (binary.expr_l as NinaASTSuperListExpression) !;
                List<(string, ANinaASTExpression?)> plist
                    = plist_raw.list;
                plist.Insert(
                    0,
                    (
                        "this", null
                    )
                );
                NinaASTBlockExpression block
                    = (binary.expr_r as NinaASTBlockExpression) !;

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

                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Ldlen);
                for (int i = 0; i < plist.Count; ++ i) {
                    var (vname, init) = plist[i];
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
                    if (init != null) {
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: g,
                            _expr: init,
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
                    _block: block,
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
            else if (binary.type == NinaOperatorType.Equ) {
                ANinaASTExpression l = binary.expr_l;
                ANinaASTExpression r = binary.expr_r;
                if (l is NinaASTBinaryExpression tmp
                        && tmp.type == NinaOperatorType.MBraL) {
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: tmp.expr_l,
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
                        _expr: tmp.expr_r,
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
                else if (l is NinaASTIdentifierExpression nid) {
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
            else if (binary.type == NinaOperatorType.MBraL) {
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: binary.expr_l,
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
                    _expr: binary.expr_r,
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
            else if (binary.type == NinaOperatorType.BraL) {
                NinaASTIdentifierExpression? nid
                    = binary.expr_l as NinaASTIdentifierExpression;
                MethodInfo? inner = nid != null
                    ? compile_innerFunc(nid.name)
                    : null;
                bool isInner = inner != null;
                NinaASTListExpression args_raw
                    = (binary.expr_r as NinaASTListExpression) !;
                List<ANinaASTExpression> args
                    = args_raw.list;
                if (! isInner) {
                    LocalBuilder tmp = _g.DeclareLocal(typeof(object));
                    if (nid == null
                            && binary.expr_l is NinaASTBinaryExpression tmp2
                            && tmp2.type == NinaOperatorType.MBraL) {
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: _g,
                            _expr: tmp2.expr_l,
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
                            _expr: tmp2.expr_r,
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
                            _expr: binary.expr_l,
                            _globs: _globs,
                            _glob_consts: _glob_consts,
                            _closure_builder: _closure_builder,
                            _fields: _fields,
                            _field_consts: _field_consts,
                            _locals: _locals,
                            _local_consts: _local_consts
                        );
                        if (nid != null && nid.name == "self") {
                            compile_expr(
                                _mb: _mb,
                                _cl: _cl,
                                _g: _g,
                                _expr: new NinaASTIdentifierExpression("this"),
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
                    if (args.Count > 0 && args[0].has_annos(
                            NinaConstsProviderUtil.CSHARP_ANNO_SPECIALARG)) {
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: _g,
                            _expr: args[0],
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
                        ANinaASTExpression v = args[i];
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
                        ANinaASTExpression v = args[i];
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
            else {
                NinaError.error("unexpected error.", 220291);
            }
        }
        else if (_expr is NinaASTObjectExpression objcr) {
            _g.Emit(
                OpCodes.Newobj,
                objcr.isArray
                    ? typeof(NinaDataArray).GetConstructor(new Type[0]) !
                    : typeof(NinaDataObject).GetConstructor(new Type[0]) !
            );
            NinaASTBlockExpression? block = objcr.block;
            NinaASTListExpression? list = objcr.list;
            if (block != null) {
                for (int i = 0; i < block.stms.Count; ++ i) {
                    NinaASTVarStatement v
                        = (block.stms[i] as NinaASTVarStatement) !;
                    List<(string, ANinaASTExpression?)> w = v.vars.list;
                    for (int j = 0; j < w.Count; ++ j) {
                        var (key_raw, val) = w[j];
                        if (val == null)
                            continue;
                        _g.Emit(OpCodes.Dup);
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: _g,
                            _expr: new NinaASTIdentifierExpression(
                                key_raw
                            ),
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
                        if (v.isConst)
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
        else {
            NinaError.error("unexpected error.", 844249);
        }
    }
    public static void compile_block(
            ModuleBuilder _mb, TypeBuilder _cl, ILGenerator _g,
            NinaASTBlockExpression _block,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, FieldInfo> _fields,
            Dictionary<string, FieldInfo> _field_consts,
            Dictionary<string, LocalBuilder> _locals,
            Dictionary<string, LocalBuilder> _local_consts,
            Label? _label_break = null, Label? _label_continue = null,
            TypeBuilder? _closure_builder = null) {
        List<ANinaASTStatement> stms = _block.stms;
        Dictionary<string, FieldInfo> fields
            = new Dictionary<string, FieldInfo>(_fields);
        Dictionary<string, FieldInfo> field_consts
            = new Dictionary<string, FieldInfo>(_field_consts);
        for (int i = 0; i < stms.Count; ++ i) {
            ANinaASTStatement v = stms[i];
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
            ModuleBuilder _mb, TypeBuilder _cl, ILGenerator _g,
            ANinaASTStatement _stm,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, FieldInfo> _fields,
            Dictionary<string, FieldInfo> _field_consts,
            Dictionary<string, LocalBuilder> _locals,
            Dictionary<string, LocalBuilder> _local_consts,
            Label? _label_break = null, Label? _label_continue = null,
            TypeBuilder? _closure_builder = null) {
        if (_stm is NinaASTExpressionStatement expr) {
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: expr.expr !,
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
        else if (_stm is NinaASTVarStatement vars) {
            List<(string, ANinaASTExpression?)> list
                = vars.vars.list;
            var dic = ! vars.isGlobal
                ? (vars.isConst ? _field_consts : _fields)
                : (vars.isConst ? _glob_consts : _globs);
            for (int i = 0; i < list.Count; ++ i) {
                var (id, init) = list[i];
                FieldBuilder builder = (
                    ! vars.isGlobal
                        ? _closure_builder !
                        : _cl
                ).DefineField(
                    fieldName: NinaConstsProviderUtil.IL_CLOSURECLASS_FIELD_PREFIX
                        + Guid.NewGuid().ToString("N"),
                    type: typeof(object),
                    attributes: FieldAttributes.Public | FieldAttributes.Static
                );
                dic[id] = builder;
                if (init != null) {
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: init,
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
        else if (_stm is NinaASTIfStatement ifs) {
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: ifs.expr !,
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
            compile_block(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _block: ifs.block !,
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
            if (ifs.block_else != null) {
                compile_block(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _block: ifs.block_else !,
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
        else if (_stm is NinaASTWhileStatement whiles) {
            Label label_while = _g.DefineLabel();
            Label label_end = _g.DefineLabel();
            _g.MarkLabel(label_while);
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: whiles.expr !,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _closure_builder: _closure_builder,
                _fields: _fields,
                _field_consts: _field_consts,
                _locals: _locals,
                _local_consts: _local_consts
            );
            _g.Emit(OpCodes.Brfalse, label_end);
            compile_block(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _block: whiles.block !,
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
        else if (_stm is NinaASTWordStatement words) {
            if (words.type == NinaKeywordType.Return) {
                if (words.expr != null) {
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: words.expr,
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
            else if (words.type == NinaKeywordType.Break) {
                _g.Emit(OpCodes.Br, (Label) _label_break !);
            }
            else if (words.type == NinaKeywordType.Continue) {
                _g.Emit(OpCodes.Br, (Label) _label_continue !);
            }
            else {
                NinaError.error("unexpected error.", 694817);
            }
        }
        else {
            NinaError.error("unexpected error.", 121055);
        }
    }
    public static void compile_main(
            ModuleBuilder _mb, TypeBuilder _cl, MethodBuilder _builder,
            NinaASTBlockExpression _block,
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
    public static Type? compile(NinaASTBlockExpression _block) {
        AssemblyName an = new AssemblyName("NinaRuntime");
        AssemblyBuilder ab
            = AssemblyBuilder.DefineDynamicAssembly(
                name: an,
                access: AssemblyBuilderAccess.RunAndCollect
            );
        ModuleBuilder mb =
            ab.DefineDynamicModule(an.Name !);
        TypeBuilder tb
            = mb.DefineType(
                name: "NinaEntry",
                attr: TypeAttributes.Public | TypeAttributes.Abstract
            );
        MethodBuilder mtdb = tb.DefineMethod(
            name: "Main",
            attributes: MethodAttributes.Public | MethodAttributes.Static
        );
        
        Dictionary<string, FieldInfo> globs
            = new Dictionary<string, FieldInfo>();
        Dictionary<string, FieldInfo> glob_consts
            = new Dictionary<string, FieldInfo>();
        init_globs(
            _tb: tb,
            _g: mtdb.GetILGenerator(),
            _globs: globs,
            _glob_consts: glob_consts
        );
        compile_main(
            _mb: mb,
            _cl: tb,
            _builder: mtdb,
            _block: _block,
            _globs: globs,
            _glob_consts: glob_consts
        );

        return tb.CreateType();
    }
}