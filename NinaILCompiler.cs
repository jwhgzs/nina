using System.Reflection;
using System.Reflection.Emit;

namespace Nina;

static class NinaILCompiler {
    private static void postable(
            Dictionary<string, List<(int, NinaErrorPosition)>> _pos_table,
            string _ss, ILGenerator _g, ANinaAST _node) {
        if (! _pos_table.ContainsKey(_ss))
            _pos_table[_ss] = new List<(int, NinaErrorPosition)>();
        _pos_table[_ss].Add(
            (_g.ILOffset, _node.pos)
        );
    }
    public static MethodInfo? compile_innerFunc(string _func) {
        if (_func.StartsWith(NinaConstsProviderUtil.NINA_ID_PREFIX)) {
            string rname = NinaCompilerUtil.unformat_identifier(_func);
            if (NinaAPI.chinese_table.ContainsKey(rname))
                rname = NinaAPI.chinese_table[rname];
            return typeof(NinaAPI).GetMethod(
                rname, BindingFlags.Public | BindingFlags.Static
            );
        }
        return null;
    }
    public static void compile_identifier(
            NinaASTIdentifierExpression _id, ILGenerator _g,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, (FieldInfo, int)> _outs,
            Dictionary<string, (FieldInfo, int)> _out_consts,
            List<string> _ins, List<string> _in_consts,
            Dictionary<string, List<(int, NinaErrorPosition)>> _pos_table,
            string _ss, bool _isSetting = false) {
        string idname = _id.name;
        if (NinaCodeBlockUtil.specialIdentifiers_chineseTable
                .ContainsKey(idname)) {
            idname = NinaCodeBlockUtil.specialIdentifiers_chineseTable[idname];
        }
        if (! _isSetting) {
            if (_ins.Contains(idname)) {
                _g.Emit(OpCodes.Ldloc_0);
                _g.Emit(OpCodes.Ldc_I4, _ins.LastIndexOf(idname));
                _g.Emit(OpCodes.Call,
                    typeof(List<object>).GetProperty("Item")!.GetGetMethod() !);
            }
            else if (_in_consts.Contains(idname)) {
                _g.Emit(OpCodes.Ldloc_1);
                _g.Emit(OpCodes.Ldc_I4, _in_consts.LastIndexOf(idname));
                _g.Emit(OpCodes.Call,
                    typeof(List<object>).GetProperty("Item")!.GetGetMethod() !);
            }
            else if (_outs.TryGetValue(idname,
                    out (FieldInfo, int) from_outs)) {
                _g.Emit(OpCodes.Ldsfld, from_outs.Item1);
                _g.Emit(OpCodes.Ldc_I4, from_outs.Item2);
                _g.Emit(
                    OpCodes.Call,
                    typeof(List<object>).GetProperty("Item")
                        !.GetGetMethod() !
                );
            }
            else if (_out_consts.TryGetValue(idname,
                    out (FieldInfo, int) from_out_consts)) {
                _g.Emit(OpCodes.Ldsfld, from_out_consts.Item1);
                _g.Emit(OpCodes.Ldc_I4, from_out_consts.Item2);
                _g.Emit(
                    OpCodes.Call,
                    typeof(List<object>).GetProperty("Item")
                        !.GetGetMethod() !
                );
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
                NinaError.error("未定义的变量.", 997023, _id.pos);
            }
        }
        else {
            if (_ins.Contains(idname)) {
                LocalBuilder tmp = _g.DeclareLocal(typeof(object));
                _g.Emit(OpCodes.Stloc, tmp);
                _g.Emit(OpCodes.Ldloc_0);
                _g.Emit(OpCodes.Ldc_I4, _ins.LastIndexOf(idname));
                _g.Emit(OpCodes.Ldloc, tmp);
                _g.Emit(OpCodes.Call,
                    typeof(List<object>).GetProperty("Item")!.GetSetMethod() !);
            }
            else if (_in_consts.Contains(idname)) {
                NinaError.error(
                    "无法为常量重新赋值.",
                    139012, _id.pos
                );
            }
            else if (_outs.TryGetValue(idname,
                    out (FieldInfo, int) from_outs)) {
                LocalBuilder tmp = _g.DeclareLocal(typeof(object));
                _g.Emit(OpCodes.Stloc, tmp);
                _g.Emit(OpCodes.Ldsfld, from_outs.Item1);
                _g.Emit(OpCodes.Ldc_I4, from_outs.Item2);
                _g.Emit(OpCodes.Ldloc, tmp);
                _g.Emit(
                    OpCodes.Call,
                    typeof(List<object>).GetProperty("Item")
                        !.GetSetMethod() !
                );
            }
            else if (_out_consts.TryGetValue(idname, out _)) {
                NinaError.error(
                    "无法为常量重新赋值.",
                    491293, _id.pos
                );
            }
            else if (_globs.TryGetValue(idname, out FieldInfo? from_glob)) {
                _g.Emit(OpCodes.Stsfld, from_glob);
            }
            else if (_glob_consts.TryGetValue(idname, out _)) {
                NinaError.error(
                    "无法为常量重新赋值.",
                    493929, _id.pos
                );
            }
            else {
                NinaError.error("未定义的变量.", 194161, _id.pos);
            }
        }
        
        postable(_pos_table, _ss, _g, _id);
    }
    public static void compile_expr(
            ModuleBuilder _mb, TypeBuilder _cl, ILGenerator _g,
            ANinaASTExpression _expr,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, (FieldInfo, int)> _outs,
            Dictionary<string, (FieldInfo, int)> _out_consts,
            List<string> _ins, List<string> _in_consts,
            Dictionary<string, List<(int, NinaErrorPosition)>> _pos_table,
            string _ss) {
        if (_expr is NinaASTLiteralExpression lit) {
            if (lit.type == NinaCodeBlockType.Number) {
                _g.Emit(OpCodes.Ldc_R8, (double) lit.val_d !);
                _g.Emit(OpCodes.Box, typeof(double));
            }
            else if (lit.type == NinaCodeBlockType.String) {
                _g.Emit(OpCodes.Ldstr, lit.val_s !);
            }
            else if (lit.type == NinaCodeBlockType.None) {
                _g.Emit(OpCodes.Ldnull);
            }
            else {
                NinaError.error("莫名其妙的错误.", 120591);
            }
        }
        else if (_expr is NinaASTIdentifierExpression id) {
            compile_identifier(
                _id: id,
                _g: _g,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _ins: _ins,
                _in_consts: _in_consts,
                _outs: _outs,
                _out_consts: _out_consts,
                _pos_table: _pos_table,
                _ss: _ss
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
                    _ins: _ins,
                    _in_consts: _in_consts,
                    _outs: _outs,
                    _out_consts: _out_consts,
                    _pos_table: _pos_table,
                    _ss: _ss
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
                    _ins: _ins,
                    _in_consts: _in_consts,
                    _outs: _outs,
                    _out_consts: _out_consts,
                    _pos_table: _pos_table,
                    _ss: _ss
                );
                _g.MarkLabel(label);
            }
            else if (binary.type == NinaOperatorType.Arr) {
                NinaASTSuperListExpression plist_raw
                    = (binary.expr_l as NinaASTSuperListExpression) !;
                List<(string, ANinaASTExpression?)> plist
                    = plist_raw.list;
                plist.Insert(
                    0, ("this", null)
                );
                NinaASTBlockExpression block
                    = (binary.expr_r as NinaASTBlockExpression) !;

                TypeBuilder cl
                    = _mb.DefineType(
                        name: NinaConstsProviderUtil.IL_CLOSURECLASS_ID_PREFIX
                            + Guid.NewGuid().ToString("N"),
                        attr: TypeAttributes.Public | TypeAttributes.Abstract
                    );
                MethodBuilder mb = cl.DefineMethod(
                    name: "func",
                    attributes: MethodAttributes.Public | MethodAttributes.Static,
                    callingConvention: CallingConventions.Standard,
                    returnType: typeof(object),
                    parameterTypes: new [] { typeof(List<object>) }
                );
                string ss = NinaCompilerUtil.snapshot_method(
                    block.pos.file, mb
                );

                var genField = () => {
                    return cl.DefineField(
                        fieldName: NinaConstsProviderUtil.IL_BUILTIN_ID_PREFIX
                            + Guid.NewGuid().ToString("N"),
                        type: typeof(List<object>),
                        attributes: FieldAttributes.Public | FieldAttributes.Static
                    );
                };
                Dictionary<string, (FieldInfo, int)> outs
                    = new Dictionary<string, (FieldInfo, int)>(_outs);
                Dictionary<string, (FieldInfo, int)> out_consts
                    = new Dictionary<string, (FieldInfo, int)>(_out_consts);
                Dictionary<FieldInfo, FieldInfo> outs_handled
                    = new Dictionary<FieldInfo, FieldInfo>();
                Dictionary<FieldInfo, FieldInfo> out_consts_handled
                    = new Dictionary<FieldInfo, FieldInfo>();
                List<string> ins = new List<string>();
                List<string> in_consts = new List<string>();
                for (int i = 0; i < _outs.Count; ++ i) {
                    var (vname, (fld, fid)) = _outs.ElementAt(i);
                    if (! outs_handled.ContainsKey(fld)) {
                        outs_handled[fld] = genField();
                    }
                    outs[vname] = (outs_handled[fld], fid);
                }
                for (int i = 0; i < _out_consts.Count; ++ i) {
                    var (vname, (fld, fid)) = _out_consts.ElementAt(i);
                    if (! out_consts_handled.ContainsKey(fld)) {
                        out_consts_handled[fld] = genField();
                    }
                    out_consts[vname] = (out_consts_handled[fld], fid);
                }
                FieldBuilder env_out1 = genField();
                FieldBuilder env_out2 = genField();
                for (int i = 0; i < _ins.Count; ++ i) {
                    string vname = _ins[i];
                    outs[vname] = (env_out1, i);
                }
                for (int i = 0; i < _in_consts.Count; ++ i) {
                    string vname = _in_consts[i];
                    out_consts[vname] = (env_out2, i);
                }

                ILGenerator g = mb.GetILGenerator();
                LocalBuilder loc_ins = g.DeclareLocal(typeof(List<object>));
                LocalBuilder loc_in_consts = g.DeclareLocal(typeof(List<object>));
                int plistCount = plist.Count;
                g.Emit(OpCodes.Ldarg_0);
                g.Emit(OpCodes.Stloc_0);
                LocalBuilder cCount = g.DeclareLocal(typeof(int));
                g.Emit(OpCodes.Ldloc_0);
                g.Emit(OpCodes.Call,
                    typeof(List<object>).GetProperty("Count")!.GetGetMethod() !);
                g.Emit(OpCodes.Stloc, cCount);
                LocalBuilder dCount = g.DeclareLocal(typeof(int));
                g.Emit(OpCodes.Ldloc, cCount);
                g.Emit(OpCodes.Ldc_I4, plistCount);
                g.Emit(OpCodes.Sub);
                g.Emit(OpCodes.Stloc, dCount);
                g.Emit(OpCodes.Ldloc, dCount);
                g.Emit(OpCodes.Ldc_I4_0);
                Label tmp = g.DefineLabel();
                g.Emit(OpCodes.Ble, tmp);
                g.Emit(OpCodes.Ldloc_0);
                g.Emit(OpCodes.Ldc_I4, plistCount);
                g.Emit(OpCodes.Ldloc, dCount);
                g.Emit(OpCodes.Call,
                    typeof(List<object>).GetMethod("RemoveRange") !);
                g.MarkLabel(tmp);
                g.Emit(OpCodes.Newobj,
                    typeof(List<object>).GetConstructor(new Type[0]) !);
                g.Emit(OpCodes.Stloc_1);

                LocalBuilder returnReg = g.DeclareLocal(typeof(object));
                Label returnLabel = g.DefineLabel();
                g.Emit(OpCodes.Ldloc_1);
                g.Emit(OpCodes.Ldnull);
                g.Emit(OpCodes.Ldftn, mb);
                g.Emit(
                    OpCodes.Newobj,
                    typeof(Func<List<object>, object>).GetConstructors()[0] !
                );
                g.Emit(
                    OpCodes.Call,
                    typeof(List<object>).GetMethod("Add") !
                );
                in_consts.Add("self");
                for (int i = 0; i < plistCount; ++ i) {
                    var (vname, init) = plist[i];
                    ins.Add(vname);
                    g.Emit(OpCodes.Ldloc, cCount);
                    g.Emit(OpCodes.Ldc_I4, i);
                    Label label = g.DefineLabel();
                    g.Emit(OpCodes.Bgt, label);
                    g.Emit(OpCodes.Ldloc_0);
                    if (init != null) {
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: g,
                            _expr: init,
                            _globs: _globs,
                            _glob_consts: _glob_consts,
                            _ins: ins,
                            _in_consts: in_consts,
                            _outs: outs,
                            _out_consts: out_consts,
                            _pos_table: _pos_table,
                            _ss: ss
                        );
                    }
                    else {
                        g.Emit(OpCodes.Ldnull);
                    }
                    g.Emit(
                        OpCodes.Call,
                        typeof(List<object>).GetMethod("Add") !
                    );
                    g.MarkLabel(label);
                }
                compile_block(
                    _mb: _mb,
                    _cl: _cl,
                    _g: g,
                    _block: block,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _ins: ins,
                    _in_consts: in_consts,
                    _outs: outs,
                    _out_consts: out_consts,
                    _returnReg: returnReg,
                    _returnLabel: returnLabel,
                    _pos_table: _pos_table,
                    _ss: ss
                );
                g.MarkLabel(returnLabel);
                g.Emit(OpCodes.Ldloc, returnReg);
                g.Emit(OpCodes.Ret);

                Type clTp = cl.CreateType() !;
                for (int i = 0; i < outs_handled.Count; ++ i) {
                    var (oldFld, newFld) = outs_handled.ElementAt(i);
                    _g.Emit(OpCodes.Ldsfld, oldFld);
                    _g.Emit(OpCodes.Stsfld, newFld);
                }
                for (int i = 0; i < out_consts_handled.Count; ++ i) {
                    var (oldFld, newFld) = out_consts_handled.ElementAt(i);
                    _g.Emit(OpCodes.Ldsfld, oldFld);
                    _g.Emit(OpCodes.Stsfld, newFld);
                }
                if (_ins.Count > 0) {
                    _g.Emit(OpCodes.Ldloc_0);
                    _g.Emit(OpCodes.Stsfld, env_out1);
                }
                if (_in_consts.Count > 0) {
                    _g.Emit(OpCodes.Ldloc_1);
                    _g.Emit(OpCodes.Stsfld, env_out2);
                }
                _g.Emit(OpCodes.Ldnull);
                _g.Emit(OpCodes.Ldftn, mb);
                _g.Emit(OpCodes.Newobj,
                    typeof(Func<List<object>, object>).GetConstructors()[0] !);
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
                        _ins: _ins,
                        _in_consts: _in_consts,
                        _outs: _outs,
                        _out_consts: _out_consts,
                        _pos_table: _pos_table,
                        _ss: _ss
                    );
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: tmp.expr_r,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _ins: _ins,
                        _in_consts: _in_consts,
                        _outs: _outs,
                        _out_consts: _out_consts,
                        _pos_table: _pos_table,
                        _ss: _ss
                    );
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: r,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _ins: _ins,
                        _in_consts: _in_consts,
                        _outs: _outs,
                        _out_consts: _out_consts,
                        _pos_table: _pos_table,
                        _ss: _ss
                    );
                    if (tmp.annos.Contains(
                            NinaConstsProviderUtil.NINA_ANNO_STRONGLY))
                        _g.Emit(OpCodes.Ldc_I4_1);
                    else
                        _g.Emit(OpCodes.Ldc_I4_0);
                    _g.Emit(
                        OpCodes.Call,
                        typeof(NinaAPIUtil).GetMethod("member_set") !
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
                        _ins: _ins,
                        _in_consts: _in_consts,
                        _outs: _outs,
                        _out_consts: _out_consts,
                        _pos_table: _pos_table,
                        _ss: _ss
                    );
                    _g.Emit(OpCodes.Dup);
                    compile_identifier(
                        _id: nid,
                        _g: _g,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _ins: _ins,
                        _in_consts: _in_consts,
                        _outs: _outs,
                        _out_consts: _out_consts,
                        _isSetting: true,
                        _pos_table: _pos_table,
                        _ss: _ss
                    );
                }
                else {
                    NinaError.error("赋值表达式的左手值无效.", 807500, binary.pos);
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
                    _ins: _ins,
                    _in_consts: _in_consts,
                    _outs: _outs,
                    _out_consts: _out_consts,
                    _pos_table: _pos_table,
                    _ss: _ss
                );
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: binary.expr_r,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _ins: _ins,
                    _in_consts: _in_consts,
                    _outs: _outs,
                    _out_consts: _out_consts,
                    _pos_table: _pos_table,
                    _ss: _ss
                );
                if (binary.annos.Contains(
                        NinaConstsProviderUtil.NINA_ANNO_STRONGLY))
                    _g.Emit(OpCodes.Ldc_I4_1);
                else
                    _g.Emit(OpCodes.Ldc_I4_0);
                _g.Emit(
                    OpCodes.Call,
                    typeof(NinaAPIUtil).GetMethod("member_get") !
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
                    = (binary.expr_r as NinaASTListExpression)
                        ?? new NinaASTListExpression(
                            new List<ANinaASTExpression>() {
                                binary.expr_r
                            },
                            binary.expr_r.pos
                        );
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
                            _ins: _ins,
                            _in_consts: _in_consts,
                            _outs: _outs,
                            _out_consts: _out_consts,
                            _pos_table: _pos_table,
                            _ss: _ss
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
                            _ins: _ins,
                            _in_consts: _in_consts,
                            _outs: _outs,
                            _out_consts: _out_consts,
                            _pos_table: _pos_table,
                            _ss: _ss
                        );
                        if (tmp2.annos.Contains(
                                NinaConstsProviderUtil.NINA_ANNO_STRONGLY))
                            _g.Emit(OpCodes.Ldc_I4_1);
                        else
                            _g.Emit(OpCodes.Ldc_I4_0);
                        _g.Emit(
                            OpCodes.Call,
                            typeof(NinaAPIUtil).GetMethod("member_get") !
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
                            _ins: _ins,
                            _in_consts: _in_consts,
                            _outs: _outs,
                            _out_consts: _out_consts,
                            _pos_table: _pos_table,
                            _ss: _ss
                        );
                        if (nid != null && nid.name == "self") {
                            compile_expr(
                                _mb: _mb,
                                _cl: _cl,
                                _g: _g,
                                _expr: new NinaASTIdentifierExpression(
                                    "this",
                                    binary.expr_l.pos
                                ),
                                _globs: _globs,
                                _glob_consts: _glob_consts,
                                _ins: _ins,
                                _in_consts: _in_consts,
                                _outs: _outs,
                                _out_consts: _out_consts,
                                _pos_table: _pos_table,
                                _ss: _ss
                            );
                        }
                        else {
                            _g.Emit(OpCodes.Ldnull);
                        }
                        _g.Emit(OpCodes.Stloc, tmp);
                    }
                    _g.Emit(OpCodes.Isinst, typeof(Func<List<object>, object>));
                    _g.Emit(OpCodes.Dup);
                    Label ok = _g.DefineLabel();
                    _g.Emit(OpCodes.Brtrue, ok);
                    _g.Emit(OpCodes.Ldstr, "无效的方法调用操作.");
                    _g.Emit(OpCodes.Ldc_I4, 759541);
                    _g.Emit(OpCodes.Call, typeof(NinaAPIUtil).GetMethod("error") !);
                    _g.MarkLabel(ok);
                    _g.Emit(OpCodes.Newobj,
                        typeof(List<object>).GetConstructor(new Type[0]) !);
                    _g.Emit(OpCodes.Dup);
                    _g.Emit(OpCodes.Ldc_I4,
                        args.Count * NinaConstsProviderUtil.IL_LOCALLIST_ENSURETIMES);
                    _g.Emit(OpCodes.Call,
                        typeof(List<object>).GetMethod("EnsureCapacity") !);
                    _g.Emit(OpCodes.Pop);
                    _g.Emit(OpCodes.Dup);
                    if (args.Count > 0 && args[0].annos.Contains(
                            NinaConstsProviderUtil.NINA_ANNO_SPECIALARG)) {
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: _g,
                            _expr: args[0],
                            _globs: _globs,
                            _glob_consts: _glob_consts,
                            _ins: _ins,
                            _in_consts: _in_consts,
                            _outs: _outs,
                            _out_consts: _out_consts,
                            _pos_table: _pos_table,
                            _ss: _ss
                        );
                        args.RemoveAt(0);
                    }
                    else {
                        _g.Emit(OpCodes.Ldloc, tmp);
                    }
                    _g.Emit(OpCodes.Call, typeof(List<object>).GetMethod("Add") !);
                    for (int i = 0; i < args.Count; ++ i) {
                        ANinaASTExpression v = args[i];
                        _g.Emit(OpCodes.Dup);
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: _g,
                            _expr: v,
                            _globs: _globs,
                            _glob_consts: _glob_consts,
                            _ins: _ins,
                            _in_consts: _in_consts,
                            _outs: _outs,
                            _out_consts: _out_consts,
                            _pos_table: _pos_table,
                            _ss: _ss
                        );
                        _g.Emit(OpCodes.Call,
                            typeof(List<object>).GetMethod("Add") !);
                    }
                    
                    _g.Emit(
                        OpCodes.Callvirt,
                        typeof(Func<List<object>, object>)
                            .GetMethod("Invoke") !
                    );
                }
                else {
                    if (args.Count != inner!.GetParameters().Count()) {
                        NinaError.error(
                            "调用内置函数时必须对齐实参.",
                            645668, binary.pos
                        );
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
                            _ins: _ins,
                            _in_consts: _in_consts,
                            _outs: _outs,
                            _out_consts: _out_consts,
                            _pos_table: _pos_table,
                            _ss: _ss
                        );
                    }
                    _g.Emit(OpCodes.Call, inner !);
                }
            }
            else {
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: binary.expr_l,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _ins: _ins,
                    _in_consts: _in_consts,
                    _outs: _outs,
                    _out_consts: _out_consts,
                    _pos_table: _pos_table,
                    _ss: _ss
                );
                compile_expr(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _expr: binary.expr_r,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _ins: _ins,
                    _in_consts: _in_consts,
                    _outs: _outs,
                    _out_consts: _out_consts,
                    _pos_table: _pos_table,
                    _ss: _ss
                );
                _g.Emit(
                    OpCodes.Call,
                    typeof(NinaAPIUtil).GetMethod(
                        NinaConstsProviderUtil.NINA_APIUTIL_OPERATOR_PREFIX
                            + binary.type.ToString()
                            + (
                                binary.annos.Contains(
                                    NinaConstsProviderUtil.NINA_ANNO_STRONGLY)
                                ? NinaConstsProviderUtil.NINA_APIUTIL_STRONGLY_SUFFIX
                                : ""
                            )
                    ) !
                );
            }
        }
        else if (_expr is NinaASTUnaryExpression unary) {
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: unary.expr,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _ins: _ins,
                _in_consts: _in_consts,
                _outs: _outs,
                _out_consts: _out_consts,
                _pos_table: _pos_table,
                _ss: _ss
            );
            _g.Emit(
                OpCodes.Call,
                typeof(NinaAPIUtil).GetMethod(
                    NinaConstsProviderUtil.NINA_APIUTIL_OPERATOR_PREFIX
                        + unary.type.ToString()
                        + (
                            unary.annos.Contains(
                                NinaConstsProviderUtil.NINA_ANNO_STRONGLY)
                            ? NinaConstsProviderUtil.NINA_APIUTIL_STRONGLY_SUFFIX
                            : ""
                        )
                ) !
            );
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
                            _expr: new NinaASTLiteralExpression(
                                key_raw,
                                v.pos
                            ),
                            _globs: _globs,
                            _glob_consts: _glob_consts,
                            _ins: _ins,
                            _in_consts: _in_consts,
                            _outs: _outs,
                            _out_consts: _out_consts,
                            _pos_table: _pos_table,
                            _ss: _ss
                        );
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: _g,
                            _expr: val,
                            _globs: _globs,
                            _glob_consts: _glob_consts,
                            _ins: _ins,
                            _in_consts: _in_consts,
                            _outs: _outs,
                            _out_consts: _out_consts,
                            _pos_table: _pos_table,
                            _ss: _ss
                        );
                        if (v.isConst)
                            _g.Emit(OpCodes.Ldc_I4_1);
                        else
                            _g.Emit(OpCodes.Ldc_I4_0);
                        _g.Emit(OpCodes.Ldc_I4_1);
                        _g.Emit(OpCodes.Ldc_I4_1);
                        _g.Emit(
                            OpCodes.Call,
                            typeof(NinaAPIUtil).GetMethod("member_init") !
                        );
                        _g.Emit(OpCodes.Pop);
                    }
                }
            }
            else if (list != null) {
                for (int i = 0; i < list.list.Count; ++ i) {
                    ANinaASTExpression v
                        = list.list[i] as ANinaASTExpression;
                    _g.Emit(OpCodes.Dup);
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: new NinaASTLiteralExpression(
                            i,
                            v.pos
                        ),
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _ins: _ins,
                        _in_consts: _in_consts,
                        _outs: _outs,
                        _out_consts: _out_consts,
                        _pos_table: _pos_table,
                        _ss: _ss
                    );
                    compile_expr(
                        _mb: _mb,
                        _cl: _cl,
                        _g: _g,
                        _expr: v,
                        _globs: _globs,
                        _glob_consts: _glob_consts,
                        _ins: _ins,
                        _in_consts: _in_consts,
                        _outs: _outs,
                        _out_consts: _out_consts,
                        _pos_table: _pos_table,
                        _ss: _ss
                    );
                    _g.Emit(OpCodes.Ldc_I4_0);
                    _g.Emit(OpCodes.Ldc_I4_0);
                    _g.Emit(OpCodes.Ldc_I4_1);
                    _g.Emit(
                        OpCodes.Call,
                        typeof(NinaAPIUtil).GetMethod("member_init") !
                    );
                    _g.Emit(OpCodes.Pop);
                }
            }
            else {
                NinaError.error("莫名其妙的错误.", 293012);
            }
        }
        else {
            NinaError.error("莫名其妙的错误.", 844249);
        }

        postable(_pos_table, _ss, _g, _expr);
    }
    public static void compile_block(
            ModuleBuilder _mb, TypeBuilder _cl, ILGenerator _g,
            NinaASTBlockExpression _block,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, (FieldInfo, int)> _outs,
            Dictionary<string, (FieldInfo, int)> _out_consts,
            List<string> _ins, List<string> _in_consts,
            Dictionary<string, List<(int, NinaErrorPosition)>> _pos_table,
            string _ss, LocalBuilder _returnReg, Label _returnLabel,
            Label? _label_break = null, Label? _label_continue = null) {
        List<ANinaASTStatement> stms = _block.stms;
        for (int i = 0; i < stms.Count; ++ i) {
            ANinaASTStatement v = stms[i];
            compile_stm(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _stm: v,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _ins: _ins,
                _in_consts: _in_consts,
                _outs: _outs,
                _out_consts: _out_consts,
                _returnReg: _returnReg,
                _returnLabel: _returnLabel,
                _label_break: _label_break,
                _label_continue: _label_continue,
                _pos_table: _pos_table,
                _ss: _ss
            );
            postable(_pos_table, _ss, _g, v);
        }
    }
    public static void compile_stm(
            ModuleBuilder _mb, TypeBuilder _cl, ILGenerator _g,
            ANinaASTStatement _stm,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, (FieldInfo, int)> _outs,
            Dictionary<string, (FieldInfo, int)> _out_consts,
            List<string> _ins, List<string> _in_consts,
            Dictionary<string, List<(int, NinaErrorPosition)>> _pos_table,
            string _ss, LocalBuilder _returnReg, Label _returnLabel,
            Label? _label_break = null, Label? _label_continue = null) {
        if (_stm is NinaASTExpressionStatement expr) {
            compile_expr(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _expr: expr.expr !,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _ins: _ins,
                _in_consts: _in_consts,
                _outs: _outs,
                _out_consts: _out_consts,
                _pos_table: _pos_table,
                _ss: _ss
            );
            _g.Emit(OpCodes.Pop);
        }
        else if (_stm is NinaASTVarStatement vars) {
            List<(string, ANinaASTExpression?)> list
                = vars.vars.list;
            for (int i = 0; i < list.Count; ++ i) {
                var (id, init) = list[i];
                var doit = () => {
                    if (init != null) {
                        compile_expr(
                            _mb: _mb,
                            _cl: _cl,
                            _g: _g,
                            _expr: init,
                            _globs: _globs,
                            _glob_consts: _glob_consts,
                            _ins: _ins,
                            _in_consts: _in_consts,
                            _outs: _outs,
                            _out_consts: _out_consts,
                            _pos_table: _pos_table,
                            _ss: _ss
                        );
                    }
                    else {
                        _g.Emit(OpCodes.Ldnull);
                    }
                };
                if (vars.isGlobal) {
                    FieldBuilder builder
                        = _cl.DefineField(
                            fieldName: NinaConstsProviderUtil.IL_CLOSURECLASS_FIELD_PREFIX
                                + Guid.NewGuid().ToString("N"),
                            type: typeof(object),
                            attributes: FieldAttributes.Public | FieldAttributes.Static
                        );
                    (vars.isConst ? _glob_consts : _globs)[id] = builder;
                    doit();
                    _g.Emit(OpCodes.Stsfld, builder);
                }
                else {
                    _g.Emit(OpCodes.Ldloc, vars.isConst ? 1 : 0);
                    (vars.isConst ? _in_consts : _ins).Add(id);
                    doit();
                    _g.Emit(
                        OpCodes.Call,
                        typeof(List<object>).GetMethod("Add") !
                    );
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
                _ins: _ins,
                _in_consts: _in_consts,
                _outs: _outs,
                _out_consts: _out_consts,
                _pos_table: _pos_table,
                _ss: _ss
            );
            _g.Emit(
                OpCodes.Call,
                typeof(NinaAPIUtil).GetMethod(
                    NinaConstsProviderUtil.NINA_APIUTIL_CONVERTION_PREFIX
                        + "Bool"
                ) !
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
                _ins: _ins,
                _in_consts: _in_consts,
                _outs: _outs,
                _out_consts: _out_consts,
                _returnReg: _returnReg,
                _returnLabel: _returnLabel,
                _label_break: _label_break,
                _label_continue: _label_continue,
                _pos_table: _pos_table,
                _ss: _ss
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
                    _ins: _ins,
                    _in_consts: _in_consts,
                    _outs: _outs,
                    _out_consts: _out_consts,
                    _returnReg: _returnReg,
                    _returnLabel: _returnLabel,
                    _label_break: _label_break,
                    _label_continue: _label_continue,
                    _pos_table: _pos_table,
                    _ss: _ss
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
                _ins: _ins,
                _in_consts: _in_consts,
                _outs: _outs,
                _out_consts: _out_consts,
                _pos_table: _pos_table,
                _ss: _ss
            );
            _g.Emit(
                OpCodes.Call,
                typeof(NinaAPIUtil).GetMethod(
                    NinaConstsProviderUtil.NINA_APIUTIL_CONVERTION_PREFIX
                        + "Bool"
                ) !
            );
            _g.Emit(OpCodes.Brfalse, label_end);
            compile_block(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _block: whiles.block !,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _ins: _ins,
                _in_consts: _in_consts,
                _outs: _outs,
                _out_consts: _out_consts,
                _returnReg: _returnReg,
                _returnLabel: _returnLabel,
                _label_break: label_end,
                _label_continue: label_while,
                _pos_table: _pos_table,
                _ss: _ss
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
                        _ins: _ins,
                        _in_consts: _in_consts,
                        _outs: _outs,
                        _out_consts: _out_consts,
                        _pos_table: _pos_table,
                        _ss: _ss
                    );
                }
                else {
                    _g.Emit(OpCodes.Ldnull);
                }
                if (words.annos.Contains(
                        NinaConstsProviderUtil.NINA_ANNO_SPECIALRETURN)) {
                    _g.Emit(OpCodes.Stloc, _returnReg);
                    _g.Emit(OpCodes.Leave, _returnLabel);
                }
                else {
                    _g.Emit(OpCodes.Ret);
                }
            }
            else if (words.type == NinaKeywordType.Break) {
                _g.Emit(OpCodes.Br, (Label) _label_break !);
            }
            else if (words.type == NinaKeywordType.Continue) {
                _g.Emit(OpCodes.Br, (Label) _label_continue !);
            }
            else {
                NinaError.error("莫名其妙的错误.", 694817);
            }
        }
        else if (_stm is NinaASTTryStatement trys) {
            _g.BeginExceptionBlock();
            compile_block(
                _mb: _mb,
                _cl: _cl,
                _g: _g,
                _block: trys.block !,
                _globs: _globs,
                _glob_consts: _glob_consts,
                _ins: _ins,
                _in_consts: _in_consts,
                _outs: _outs,
                _out_consts: _out_consts,
                _returnReg: _returnReg,
                _returnLabel: _returnLabel,
                _pos_table: _pos_table,
                _ss: _ss
            );
            _g.BeginCatchBlock(typeof(Exception));
            if (trys.block_catch != null) {
                FieldBuilder exReg
                    = _cl.DefineField(
                        fieldName: NinaConstsProviderUtil.IL_BUILTIN_ID_PREFIX
                            + Guid.NewGuid().ToString("N"),
                        type: typeof(object),
                        attributes: FieldAttributes.Public | FieldAttributes.Static
                    );
                _glob_consts["exception"] = exReg;
                _g.Emit(OpCodes.Ldstr, trys.pos.file);
                _g.Emit(OpCodes.Call, typeof(NinaAPIUtil).GetMethod("convert_ex") !);
                _g.Emit(OpCodes.Stsfld, exReg);
                compile_block(
                    _mb: _mb,
                    _cl: _cl,
                    _g: _g,
                    _block: trys.block_catch,
                    _globs: _globs,
                    _glob_consts: _glob_consts,
                    _ins: _ins,
                    _in_consts: _in_consts,
                    _outs: _outs,
                    _out_consts: _out_consts,
                    _returnReg: _returnReg,
                    _returnLabel: _returnLabel,
                    _pos_table: _pos_table,
                    _ss: _ss
                );
            }
            else {
                _g.Emit(OpCodes.Pop);
            }
            _g.BeginFinallyBlock();
            _g.Emit(OpCodes.Ldnull);
            _g.Emit(OpCodes.Stsfld, _glob_consts["exception"]);
            _g.EndExceptionBlock();
        }
        else {
            NinaError.error("莫名其妙的错误.", 121055);
        }

        postable(_pos_table, _ss, _g, _stm);
    }
    public static void init_apis(
            string _file, TypeBuilder _tb, MethodBuilder _mb, ILGenerator _g,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, List<(int, NinaErrorPosition)>> _pos_table) {
        MethodInfo[] mtds = typeof(NinaAPI).GetMethods(
            BindingFlags.Public | BindingFlags.Static
        );
        for (int i = 0; i < mtds.Length; ++ i) {
            MethodInfo v = mtds[i];
            Type tp = v.GetType();
            FieldBuilder fb = _tb.DefineField(
                fieldName: NinaConstsProviderUtil.IL_BUILTIN_ID_PREFIX
                    + Guid.NewGuid().ToString("N"),
                type: typeof(Func<List<object>, object>),
                attributes: FieldAttributes.Public | FieldAttributes.Static
            );
            MethodBuilder mb = _tb.DefineMethod(
                name: NinaConstsProviderUtil.IL_BUILTIN_ID_PREFIX
                    + Guid.NewGuid().ToString("N"),
                attributes: MethodAttributes.Public | MethodAttributes.Static,
                callingConvention: CallingConventions.Standard,
                returnType: typeof(object),
                parameterTypes: new [] { typeof(List<object>) }
            );
            ILGenerator mg = mb.GetILGenerator();
            ParameterInfo[] plist = v.GetParameters();
            List<Type> types = new List<Type>();
            mg.Emit(OpCodes.Ldarg_0);
            mg.Emit(OpCodes.Call,
                typeof(List<object>).GetProperty("Count")!.GetGetMethod() !);
            mg.Emit(OpCodes.Ldc_I4, plist.Length + 1);
            Label label = mg.DefineLabel();
            mg.Emit(OpCodes.Beq, label);
            mg.Emit(
                OpCodes.Ldstr,
                "调用内置函数时必须对齐实参."
            );
            mg.Emit(OpCodes.Ldc_I4, 294012);
            mg.Emit(OpCodes.Call, typeof(NinaAPIUtil).GetMethod("error") !);
            mg.MarkLabel(label);
            for (int j = 0; j < plist.Length; ++ j) {
                ParameterInfo w = plist[j];
                types.Add(w.ParameterType);
                mg.Emit(OpCodes.Ldarg_0);
                mg.Emit(OpCodes.Ldc_I4, j + 1);
                mg.Emit(OpCodes.Call,
                    typeof(List<object>).GetProperty("Item")!.GetGetMethod() !);
            }
            mg.Emit(OpCodes.Call, v);
            mg.Emit(OpCodes.Ret);
            _g.Emit(OpCodes.Ldnull);
            _g.Emit(OpCodes.Ldftn, mb);
            _g.Emit(OpCodes.Newobj,
                typeof(Func<List<object>, object>).GetConstructors()[0]);
            _g.Emit(OpCodes.Stsfld, fb);
            _glob_consts[NinaConstsProviderUtil.NINA_ID_PREFIX + v.Name]
                = _glob_consts[
                    NinaConstsProviderUtil.NINA_ID_PREFIX
                        + NinaAPI.chinese_table.First(a => a.Value == v.Name).Key]
                = fb;
            NinaASTPlaceholder ph
                = new NinaASTPlaceholder(
                    new NinaErrorPosition(
                        "[内置函数: " + v.Name + "]",
                        - 2, - 2
                    )
                );
            postable(
                _pos_table,
                NinaCompilerUtil.snapshot_method(_file, mb),
                mg, ph
            );
            postable(
                _pos_table,
                NinaCompilerUtil.snapshot_method(_file, _mb),
                _g, ph
            );
        }

        FieldInfo[] flds = typeof(NinaAPI).GetFields(
            BindingFlags.Public | BindingFlags.Static
        );
        for (int i = 0; i < flds.Length; ++ i) {
            FieldInfo v = flds[i];
            _glob_consts[NinaConstsProviderUtil.NINA_ID_PREFIX + v.Name]
                = _glob_consts[
                    NinaConstsProviderUtil.NINA_ID_PREFIX
                        + NinaAPI.chinese_table.First(a => a.Value == v.Name).Key]
                = v;
        }
    }
    public static void compile_main(
            ModuleBuilder _mb, TypeBuilder _cl, MethodBuilder _builder,
            ILGenerator _g, NinaASTBlockExpression _block,
            Dictionary<string, FieldInfo> _globs,
            Dictionary<string, FieldInfo> _glob_consts,
            Dictionary<string, List<(int, NinaErrorPosition)>> _pos_table) {
        Dictionary<string, LocalBuilder> outs
            = new Dictionary<string, LocalBuilder>();
        Dictionary<string, LocalBuilder> out_consts
            = new Dictionary<string, LocalBuilder>();
        
        LocalBuilder returnReg = _g.DeclareLocal(typeof(object));
        Label returnLabel = _g.DefineLabel();
        _g.BeginExceptionBlock();
        compile_block(
            _mb: _mb,
            _cl: _cl,
            _g: _g,
            _block: _block,
            _globs: _globs,
            _glob_consts: _glob_consts,
            _ins: new List<string>(),
            _in_consts: new List<string>(),
            _outs:
                new Dictionary<string, (FieldInfo, int)>(),
            _out_consts:
                new Dictionary<string, (FieldInfo, int)>(),
            _returnReg: returnReg,
            _returnLabel: returnLabel,
            _pos_table: _pos_table,
            _ss:
                NinaCompilerUtil.snapshot_method(_block.pos.file, _builder)
        );
        _g.BeginCatchBlock(typeof(Exception));
        _g.Emit(OpCodes.Ldstr, _block.pos.file);
        _g.Emit(OpCodes.Call, typeof(NinaAPIUtil).GetMethod("error_ex") !);
        _g.EndExceptionBlock();

        _g.MarkLabel(returnLabel);
        _g.Emit(OpCodes.Ldloc, returnReg);
        _g.Emit(OpCodes.Ret);
    }
    public static object? execute(NinaASTBlockExpression _block,
            object? _arg = null) {
        AssemblyBuilder ab
            = AssemblyBuilder.DefineDynamicAssembly(
                name: new AssemblyName(NinaConstsProviderUtil.IL_ASSEMBLY_ID),
                access: AssemblyBuilderAccess.RunAndCollect
            );
        ModuleBuilder mb =
            ab.DefineDynamicModule(NinaConstsProviderUtil.IL_MODULE_ID);
        TypeBuilder tb
            = mb.DefineType(
                name: NinaConstsProviderUtil.IL_ENTRYCLASS_ID,
                attr: TypeAttributes.Public | TypeAttributes.Abstract
            );
        Dictionary<string, List<(int, NinaErrorPosition)>> pos_table
            = new Dictionary<string, List<(int, NinaErrorPosition)>>();
        MethodBuilder mtdb = tb.DefineMethod(
            name: "Main",
            attributes: MethodAttributes.Public | MethodAttributes.Static,
            callingConvention: CallingConventions.Standard,
            returnType: typeof(object),
            parameterTypes: new [] { typeof(object) }
        );
        ILGenerator mg = mtdb.GetILGenerator();
        
        Dictionary<string, FieldInfo> globs
            = new Dictionary<string, FieldInfo>();
        Dictionary<string, FieldInfo> glob_consts
            = new Dictionary<string, FieldInfo>();
        var newBuiltinField = () => tb.DefineField(
            fieldName: NinaConstsProviderUtil.IL_BUILTIN_ID_PREFIX
                + Guid.NewGuid().ToString("N"),
            type: typeof(object),
            attributes: FieldAttributes.Public | FieldAttributes.Static
        );
        FieldBuilder defaultThis = newBuiltinField();
        FieldBuilder defaultSelf = newBuiltinField();
        FieldBuilder defaultArgument = newBuiltinField();
        FieldBuilder defaultException = newBuiltinField();
        mg.Emit(OpCodes.Ldarg_0);
        mg.Emit(OpCodes.Stsfld, defaultArgument);
        globs["this"] = defaultThis;
        glob_consts["self"] = defaultSelf;
        glob_consts["argument"] = defaultArgument;
        glob_consts["exception"] = defaultException;
        init_apis(
            _file: _block.pos.file,
            _tb: tb,
            _mb: mtdb,
            _g: mg,
            _globs: globs,
            _glob_consts: glob_consts,
            _pos_table: pos_table
        );
        
        compile_main(
            _mb: mb,
            _cl: tb,
            _builder: mtdb,
            _g: mg,
            _block: _block,
            _globs: globs,
            _glob_consts: glob_consts,
            _pos_table: pos_table
        );

        Type tp = tb.CreateType() !;
        MethodInfo mi = tp.GetMethod("Main") !;
        NinaAPIUtil.pos_table = NinaCompilerUtil.merge_dictionaries(
            NinaAPIUtil.pos_table, pos_table
        );
        return mi.Invoke(null, new [] { _arg });
    }
}