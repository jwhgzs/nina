using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Reflection;
using System.Diagnostics;

namespace Nina;

public class NinaDataArray: List<object> {
    public NinaDataArray(): base() {}
    public NinaDataArray(List<object> _src)
            : base(_src) {}
}
public class NinaDataObject: Dictionary<string, object> {
    public HashSet<string> my_consts = new HashSet<string>();
    public NinaDataObject(): base() {}
    public NinaDataObject(Dictionary<string, object> _src)
            : base(_src) {}
}

public static class NinaAPIUtil {
    public static bool toBool(object _o) {
        if (_o == null)
            return false;
        else if (_o is bool b)
            return b;
        else if (_o is double d)
            return d != 0;
        else if (_o is string s)
            return s.Length > 0;
        else if (_o is int i)
            return i != 0;
        else if (_o is long l)
            return l != 0;
        else
            NinaError.error("无法进行到 Number 的类型转换.", 123901);
        return false;
    }
    public static double toNumber(object _o) {
        if (_o == null)
            return 0;
        else if (_o is double d)
            return d;
        else if (_o is bool b)
            return b ? 1 : 0;
        else if (_o is string s && double.TryParse(s, out double sd))
            return sd;
        else if (_o is int i)
            return i;
        else if (_o is long l)
            return l;
        else
            NinaError.error("无法进行到 Number 的类型转换.", 412910);
        return 0;
    }
    public static double toNumberS(object _o) {
        if (_o is double d)
            return d;
        else if (_o is bool b)
            return b ? 1 : 0;
        else if (_o is int i)
            return i;
        else if (_o is long l)
            return l;
        else
            NinaError.error("无法进行到 Number 的类型转换.", 546892);
        return 0;
    }
    public static string toTypeDesc(object _o) {
        if (_o == null)
            return "[Null]";
        else if (_o is double || _o is bool
                || _o is int || _o is long)
            return "[Number]";
        else if (_o is string)
            return "[String]";
        else if (_o is NinaDataArray)
            return "[Array]";
        else if (_o is NinaDataObject o)
            return o.ContainsKey("type")
                ? "[Object: "
                    + NinaAPIUtil.toString(o["type"])
                    + "]"
                : "[Object]";
        else if (_o is Delegate)
            return "[Function]";
        else
            return "[Unknown]";
    }
    public static string toString(object _o) {
        if (_o == null)
            return "[Null]";
        else if (_o is string s)
            return s;
        else if (_o is double d)
            return d.ToString();
        else if (_o is bool b)
            return b ? "1" : "0";
        else if (_o is int i)
            return i.ToString();
        else if (_o is long l)
            return l.ToString();
        else
            return toTypeDesc(_o);
    }
    public static string toStringS(object _o) {
        if (_o is string s)
            return s;
        else
            NinaError.error("无法进行到 String 的类型转换.", 149012);
        return "";
    }

    public static object opAdd(object _lhs, object _rhs) {
        if (_lhs is string a || _rhs is string b)
            return toString(_lhs) + toString(_rhs);
        else
            return toNumber(_lhs) + toNumber(_rhs);
    }
    public static object opAddS(object _lhs, object _rhs) {
        if (_lhs is string a && _rhs is string b)
            return a + b;
        else
            return toNumberS(_lhs) + toNumberS(_rhs);
    }
    public static object opSub(object _lhs, object _rhs) {
        return toNumber(_lhs) - toNumber(_rhs);
    }
    public static object opSubS(object _lhs, object _rhs) {
        return toNumberS(_lhs) - toNumberS(_rhs);
    }
    public static object opPos(object _o) {
        return toNumber(_o);
    }
    public static object opPosS(object _o) {
        return toNumberS(_o);
    }
    public static object opNeg(object _o) {
        return - toNumber(_o);
    }
    public static object opNegS(object _o) {
        return - toNumberS(_o);
    }
    public static object opMut(object _lhs, object _rhs) {
        return toNumber(_lhs) * toNumber(_rhs);
    }
    public static object opMutS(object _lhs, object _rhs) {
        return toNumberS(_lhs) * toNumberS(_rhs);
    }
    public static object opDiv(object _lhs, object _rhs) {
        return toNumber(_lhs) / toNumber(_rhs);
    }
    public static object opDivS(object _lhs, object _rhs) {
        return toNumberS(_lhs) / toNumberS(_rhs);
    }
    public static object opRem(object _lhs, object _rhs) {
        return toNumber(_lhs) % toNumber(_rhs);
    }
    public static object opRemS(object _lhs, object _rhs) {
        return toNumberS(_lhs) % toNumberS(_rhs);
    }
    public static object opPow(object _lhs, object _rhs) {
        return Math.Pow(toNumber(_lhs), toNumber(_rhs));
    }
    public static object opPowS(object _lhs, object _rhs) {
        return Math.Pow(toNumberS(_lhs), toNumberS(_rhs));
    }
    public static object opLNot(object _o) {
        return ! toBool(_o);
    }
    public static object opLNotS(object _o) {
        return ! toBool(_o);
    }
    public static bool opLEqu_bool(object _lhs, object _rhs) {
        if (_lhs == _rhs)
            return true;
        if (_lhs is double d1 && _rhs is double d2)
            return d1 == d2;
        if (_lhs is string || _rhs is string)
            return toString(_lhs) == toString(_rhs);
        return false;
    }
    public static bool opLEquS_bool(object _lhs, object _rhs) {
        if (_lhs == _rhs)
            return true;
        if (_lhs is double d1 && _rhs is double d2)
            return d1 == d2;
        if (_lhs is string s1 && _rhs is string s2)
            return s1 == s2;
        return false;
    }
    public static object opLEqu(object _lhs, object _rhs) {
        return opLEqu_bool(_lhs, _rhs);
    }
    public static object opLEquS(object _lhs, object _rhs) {
        return opLEquS_bool(_lhs, _rhs);
    }
    public static object opLNEqu(object _lhs, object _rhs) {
        return ! opLEqu_bool(_lhs, _rhs);
    }
    public static object opLNEquS(object _lhs, object _rhs) {
        return ! opLEquS_bool(_lhs, _rhs);
    }
    public static object opMore(object _lhs, object _rhs) {
        return toNumber(_lhs) > toNumber(_rhs);
    }
    public static object opMoreS(object _lhs, object _rhs) {
        return toNumberS(_lhs) > toNumberS(_rhs);
    }
    public static object opLess(object _lhs, object _rhs) {
        return toNumber(_lhs) < toNumber(_rhs);
    }
    public static object opLessS(object _lhs, object _rhs) {
        return toNumberS(_lhs) < toNumberS(_rhs);
    }
    public static object opMoreE(object _lhs, object _rhs) {
        return toNumber(_lhs) >= toNumber(_rhs);
    }
    public static object opMoreES(object _lhs, object _rhs) {
        return toNumberS(_lhs) >= toNumberS(_rhs);
    }
    public static object opLessE(object _lhs, object _rhs) {
        return toNumber(_lhs) <= toNumber(_rhs);
    }
    public static object opLessES(object _lhs, object _rhs) {
        return toNumberS(_lhs) <= toNumberS(_rhs);
    }
    public static object opLAnd(object _lhs, object _rhs) {
        return ! toBool(_lhs) ? _lhs : _rhs;
    }
    public static object opLOr(object _lhs, object _rhs) {
        return toBool(_lhs) ? _lhs : _rhs;
    }
    public static object opNot(object _o) {
        return (double) (~ (int) toNumber(_o));
    }
    public static object opNotS(object _o) {
        return (double) (~ (int) toNumberS(_o));
    }
    public static object opAnd(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) & (int) toNumber(_rhs));
    }
    public static object opAndS(object _lhs, object _rhs) {
        return (double) ((int) toNumberS(_lhs) & (int) toNumberS(_rhs));
    }
    public static object opOr(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) | (int) toNumber(_rhs));
    }
    public static object opOrS(object _lhs, object _rhs) {
        return (double) ((int) toNumberS(_lhs) | (int) toNumberS(_rhs));
    }
    public static object opXOr(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) ^ (int) toNumber(_rhs));
    }
    public static object opXOrS(object _lhs, object _rhs) {
        return (double) ((int) toNumberS(_lhs) ^ (int) toNumberS(_rhs));
    }
    public static object opSftL(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) << (int) toNumber(_rhs));
    }
    public static object opSftLS(object _lhs, object _rhs) {
        return (double) ((int) toNumberS(_lhs) << (int) toNumberS(_rhs));
    }
    public static object opSftR(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) >> (int) toNumber(_rhs));
    }
    public static object opSftRS(object _lhs, object _rhs) {
        return (double) ((int) toNumberS(_lhs) >> (int) toNumberS(_rhs));
    }
    public static object opTypeof(object _o) {
        return toTypeDesc(_o);
    }
    public static object member_get(
            object _obj, object _key, bool _isStrict) {
        if (_obj is NinaDataArray arr) {
            int key = _isStrict
                ? (int) toNumberS(_key)
                : (int) toNumber(_key);
            return key >= 0 && key <= arr.Count - 1
                ? arr[key]
                : null !;
        }
        else if (_obj is NinaDataObject obj) {
            string key = _isStrict
                ? toStringS(_key)
                : toString(_key);
            return obj.ContainsKey(key) ? obj[key] : null !;
        }
        else {
            NinaError.error("成员访问的操作对象无效.", 626768);
        }
        return null !;
    }
    public static object member_init(
            object _obj, object _key, object _val, bool _isConst,
            bool _allowSetConst, bool _isStrict) {
        if (_obj is NinaDataArray arr) {
            int key = _isStrict
                ? (int) toNumberS(_key)
                : (int) toNumber(_key);
            arr.EnsureCapacity(key + 1);
            while (arr.Count < key + 1)
                arr.Add(null !);
            arr[key] = _val;
        }
        else if (_obj is NinaDataObject obj) {
            string key = _isStrict
                ? toStringS(_key)
                : toString(_key);
            if (obj.my_consts.Contains(key) && ! _allowSetConst) {
                NinaError.error(
                    "无法给常量成员重新赋值.",
                    794922
                );
            }
            obj[key] = _val;
            if (_isConst)
                obj.my_consts.Add(key);
        }
        else {
            NinaError.error("成员赋值的操作对象无效.", 426694);
        }
        return _val;
    }
    public static object member_set(
            object _obj, object _key, object _val, bool _isStrict) {
        return member_init(_obj, _key, _val, false, false, _isStrict);
    }

    public static void error(string _msg, int _uniqueCode) {
        NinaError.error(_msg, _uniqueCode);
    }
    public static void error_full(
            string _msg, int _uniqueCode,
            string _file, int _line, int _col) {
        NinaError.error(_msg, _uniqueCode, new NinaErrorPosition(
            _file, _line, _col
        ));
    }
    public static Dictionary<string, List<(int, NinaErrorPosition)>> pos_table
        = new Dictionary<string, List<(int, NinaErrorPosition)>>();
    public static string error_ex_report(Exception _ex, string _file) {
        StackTrace trace
            = new StackTrace(_ex, true);
        StackFrame[] frames = trace.GetFrames();
        
        List<NinaErrorPosition> posList = new List<NinaErrorPosition>();
        var doit = bool (int _n) => {
            string ss = "";
            int offset = - 1;
            int matched = 0;
            for (int i = 0; i < frames.Length; ++ i) {
                StackFrame v = frames[i];
                MethodBase nmtd = v.GetMethod() !;
                string nss = NinaCompilerUtil.snapshot_method(_file, nmtd);
                if (pos_table.ContainsKey(nss)) {
                    if (matched >= _n) {
                        ss = nss;
                        offset = v.GetILOffset();
                        byte[] a = nmtd.GetMethodBody()!.GetILAsByteArray()!;
                        NinaDebugger.read_ILCode(a, ref offset);
                        break;
                    }
                    else {
                        ++ matched;
                    }
                }
            }
            if (ss.Length == 0 || offset < 0) {
                return false;
            }

            List<(int, NinaErrorPosition)> table = pos_table[ss];
            for (int i = 0; i < table.Count; ++ i) {
                var (os, pos) = table.ElementAt(i);
                if (os >= offset) {
                    posList.Add(pos);
                    return true;
                }
            }

            return false;
        };

        for (int i = 0; doit(i); ++ i);
        return NinaError.gen_report(
            _ex.Message, - 1,
            posList.Count > 0
                ? posList
                : new List<NinaErrorPosition>()
        );
    }
    public static void error_ex(Exception _ex, string _file) {
        throw new Exception(
            error_ex_report(_ex, _file)
        );
    }
    private static object convert_ex_resolve_code(string _msg) {
        if (_msg.StartsWith(NinaError.header)) {
            _msg = _msg.Substring(NinaError.header.Length);
            if (_msg.StartsWith("(#")) {
                _msg = _msg.Substring("(#".Length);
                string code = "";
                int i = 0;
                while (char.IsNumber(_msg[i])) {
                    code += _msg[i ++];
                }
                if (code.Length > 0)
                    return int.Parse(code);
            }
        }
        return null !;
    }
    public static object convert_ex(Exception _ex, string _file) {
        string msg = error_ex_report(_ex, _file);
        return new NinaDataObject() {
            ["type"] = "NinaException",
            ["message"] = msg,
            ["code"] = convert_ex_resolve_code(msg)
        };
    }
}

public static class NinaAPI {
    public static object @null = null !;
    public static object 空 = @null;
    public static object @true = true;
    public static object 真 = @true;
    public static object @false = false;
    public static object 假 = @false;
    public static object math_PI = Math.PI;
    public static object 数学_PI = math_PI;
    public static object math_E = Math.E;
    public static object 数学_E = math_E;

    public static object number(object _data) {
        return NinaAPIUtil.toNumber(_data);
    }
    public static object 到数字(object _data) {
        return number(_data);
    }
    public static object @string(object _data) {
        return NinaAPIUtil.toString(_data);
    }
    public static object 到字符串(object _data) {
        return @string(_data);
    }
    public static object eval(object _code, object _arg) {
        string code = NinaAPIUtil.toStringS(_code);
        try {
            return NinaCore.execute(
                "[动态代码: " + Guid.NewGuid().ToString("N") + "]",
                code, _arg
            ) !;
        }
        catch (TargetInvocationException ex) {
            NinaError.error(
                "在执行动态代码时出现错误:\n"
                    + NinaError.trim_header(ex.InnerException!.Message),
                352982
            );
        }
        return null !;
    }
    public static object 执行(object _code, object _arg) {
        return eval(_code, _arg);
    }
    public static object @throw(object _msg) {
        NinaError.error("用户自定义的异常：\n" + _msg, - 1);
        return null !;
    }
    public static object 抛出(object _msg) {
        return @throw(_msg);
    }

    public static object console_print(object _data) {
        string v = NinaAPIUtil.toString(_data);
        Console.Write(v);
        return null !;
    }
    public static object 输出(object _data) {
        return console_print(_data);
    }
    public static object console_printf(object _data) {
        string v = NinaAPIUtil.toString(_data);
        Console.WriteLine(v);
        return null !;
    }
    public static object 输出换行(object _data) {
        return console_printf(_data);
    }
    public static object console_read() {
        return Console.ReadLine() !;
    }
    public static object 输入() {
        return console_read();
    }
    public static object console_exit() {
        Environment.Exit(- 2);
        return null !;
    }
    public static object 退出() {
        return console_exit();
    }

    public static object string_at(object _str, object _i) {
        string str = NinaAPIUtil.toStringS(_str);
        int i = (int) NinaAPIUtil.toNumberS(_i);
        try {
            return str[i].ToString();
        }
        catch {
            return null !;
        }
    }
    public static object 字符串_取字符(object _str, object _i) {
        return string_at(_str, _i);
    }
    public static object string_sub(object _str, object _i, object _n) {
        string str = NinaAPIUtil.toStringS(_str);
        int i = (int) NinaAPIUtil.toNumberS(_i);
        int n = (int) NinaAPIUtil.toNumberS(_n);
        try {
            return str.Substring(i, n);
        }
        catch {
            return null !;
        }
    }
    public static object 字符串_截取(object _str, object _i, object _n) {
        return string_sub(_str, _i, _n);
    }
    public static object string_sub_to_tail(object _str, object _i) {
        string str = NinaAPIUtil.toStringS(_str);
        int i = (int) NinaAPIUtil.toNumberS(_i);
        try {
            return str.Substring(i);
        }
        catch {
            return null !;
        }
    }
    public static object 字符串_截取到末尾(object _str, object _i) {
        return string_sub_to_tail(_str, _i);
    }
    public static object string_split(object _str, object _sub) {
        string str = NinaAPIUtil.toStringS(_str);
        string sub = NinaAPIUtil.toStringS(_sub);
        try {
            return new NinaDataArray(
                str.Split(sub).Select(v => (object) v).ToList()
            );
        }
        catch {
            return null !;
        }
    }
    public static object 字符串_分割(object _str, object _sub) {
        return string_split(_str, _sub);
    }
    public static object string_split_count(object _str, object _sub, object _n) {
        string str = NinaAPIUtil.toStringS(_str);
        string sub = NinaAPIUtil.toStringS(_sub);
        int n = (int) NinaAPIUtil.toNumberS(_n);
        try {
            return new NinaDataArray(
                str.Split(sub, n).Select(v => (object) v).ToList()
            );
        }
        catch {
            return null !;
        }
    }
    public static object 字符串_按次数分割(object _str, object _sub, object _n) {
        return string_split_count(_str, _sub, _n);
    }
    public static object string_to_array(object _str) {
        string str = NinaAPIUtil.toStringS(_str);
        try {
            return
                new NinaDataArray(
                    str.Select(v => (object) v.ToString()).ToList()
                );
        }
        catch {
            return null !;
        }
    }
    public static object 字符串_字符串到数组(object _str) {
        return string_to_array(_str);
    }
    public static object string_from_array(object _arr) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null)
            return null !;
        string ret = "";
        for (int i = 0; i < arr.Count; ++ i) {
            ret += NinaAPIUtil.toStringS(arr[i]);
        }
        return ret;
    }
    public static object 字符串_数组到字符串(object _arr) {
        return string_from_array(_arr);
    }
    public static object string_from_array_join(object _arr, object _sub) {
        NinaDataArray? arr = _arr as NinaDataArray;
        string sub = NinaAPIUtil.toStringS(_sub);
        if (arr == null)
            return null !;
        string ret = "";
        for (int i = 0; i < arr.Count; ++ i) {
            if (i > 0)
                ret += sub;
            ret += NinaAPIUtil.toStringS(arr[i]);
        }
        return ret;
    }
    public static object 字符串_连接数组到字符串(
            object _str, object _sub, object _rep) {
        return string_replace(_str, _sub, _rep);
    }
    public static object string_replace(object _str, object _sub, object _rep) {
        string str = NinaAPIUtil.toStringS(_str);
        string sub = NinaAPIUtil.toStringS(_sub);
        string rep = NinaAPIUtil.toStringS(_rep);
        return str.Replace(sub, rep);
    }
    public static object 字符串_替换(object _str, object _sub, object _rep) {
        return string_replace(_str, _sub, _rep);
    }
    public static object string_replace_count(
            object _str, object _sub, object _rep, object _n) {
        string str = NinaAPIUtil.toStringS(_str);
        string sub = NinaAPIUtil.toStringS(_sub);
        string rep = NinaAPIUtil.toStringS(_rep);
        int n = (int) NinaAPIUtil.toNumberS(_n);
        string[] arr = str.Split(sub, n + 1);
        return string.Join(rep, arr);
    }
    public static object 字符串_按次数替换(
            object _str, object _sub, object _rep, object _n) {
        return string_replace_count(_str, _sub, _rep, _n);
    }
    public static object string_upper(object _str) {
        string str = NinaAPIUtil.toStringS(_str);
        return str.ToUpper();
    }
    public static object 字符串_到大写(object _str) {
        return string_upper(_str);
    }
    public static object string_lower(object _str) {
        string str = NinaAPIUtil.toStringS(_str);
        return str.ToLower();
    }
    public static object 字符串_到小写(object _str) {
        return string_lower(_str);
    }
    public static object string_length(object _str) {
        string str = NinaAPIUtil.toStringS(_str);
        return (double) str.Length;
    }
    public static object 字符串_取长度(object _str) {
        return string_length(_str);
    }

    public static object array_length(object _arr) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("操作的数组无效.", 796923);
        }
        return (double) arr!.Count;
    }
    public static object 数组_取长度(object _arr) {
        return array_length(_arr);
    }
    public static object array_append(object _arr, object _item) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("操作的数组无效.", 120312);
        }
        arr!.Add(_item);
        return true;
    }
    public static object 数组_添加项(object _arr, object _item) {
        return array_append(_arr, _item);
    }
    public static object array_insert(object _arr, object _n, object _item) {
        NinaDataArray? arr = _arr as NinaDataArray;
        int n = (int) NinaAPIUtil.toNumberS(_n);
        if (arr == null) {
            NinaError.error("操作的数组无效.", 102914);
        }
        try {
            arr!.Insert(n, _item);
            return true;
        }
        catch {
            return false;
        }
    }
    public static object 数组_插入项(object _arr, object _n, object _item) {
        return array_insert(_arr, _n, _item);
    }
    public static object array_pop(object _arr) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("操作的数组无效.", 593192);
        }
        try {
            arr!.RemoveAt(arr.Count - 1);
            return true;
        }
        catch {
            return false;
        }
    }
    public static object 数组_移除末尾项(object _arr) {
        return array_pop(_arr);
    }
    public static object array_remove(object _arr, object _n) {
        NinaDataArray? arr = _arr as NinaDataArray;
        int n = (int) NinaAPIUtil.toNumberS(_n);
        if (arr == null) {
            NinaError.error("操作的数组无效.", 605912);
        }
        try {
            arr!.RemoveAt(n);
            return true;
        }
        catch {
            return false;
        }
    }
    public static object 数组_移除项(object _arr, object _n) {
        return array_remove(_arr, _n);
    }
    public static object array_clear(object _arr) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("操作的数组无效.", 491021);
        }
        arr!.Clear();
        return true;
    }
    public static object 数组_清空(object _arr) {
        return array_clear(_arr);
    }
    public static object array_find(object _arr, object _item) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("操作的数组无效.", 701901);
        }
        for (int i = 0; i < arr!.Count; ++ i) {
            if (NinaAPIUtil.opLEquS_bool(arr![i], _item)) {
                return (double) i;
            }
        }
        return - 1d;
    }
    public static object 数组_查找值(object _arr, object _item) {
        return array_find(_arr, _item);
    }
    public static object array_find_last(object _arr, object _item) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("操作的数组无效.", 249931);
        }
        for (int i = arr!.Count - 1; i >= 0; -- i) {
            if (NinaAPIUtil.opLEquS_bool(arr![i], _item)) {
                return (double) i;
            }
        }
        return - 1d;
    }
    public static object 数组_反向查找值(object _arr, object _item) {
        return array_find_last(_arr, _item);
    }
    private static JsonSerializerOptions json_option
            = new JsonSerializerOptions() {
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = {
            new NinaJSONObjectConverter()
        }
    };
    public static object array_to_json(object _arr) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("操作的数组无效.", 120491);
        }
        try {
            return
                JsonSerializer.Serialize(
                    arr, json_option
                );
        }
        catch {
            return null !;
        }
    }
    public static object 数组_数组到JSON(object _arr) {
        return array_to_json(_arr);
    }
    public static object array_from_json(object _json) {
        string json = NinaAPIUtil.toStringS(_json);
        try {
            return
                new NinaDataArray(
                    JsonSerializer.Deserialize<List<object>>(
                        json,
                        json_option
                    ) !
                );
        }
        catch {
            return null !;
        }
    }
    public static object 数组_JSON到数组(object _json) {
        return array_from_json(_json);
    }

    public static object object_length(object _obj) {
        NinaDataObject? obj = _obj as NinaDataObject;
        if (obj == null) {
            NinaError.error("操作的对象无效.", 796923);
        }
        return (double) obj!.Count;
    }
    public static object 对象_取长度(object _obj) {
        return object_length(_obj);
    }
    public static object object_has(object _obj, object _key) {
        NinaDataObject? obj = _obj as NinaDataObject;
        string key = NinaAPIUtil.toStringS(_key);
        if (obj == null) {
            NinaError.error("操作的对象无效.", 123091);
        }
        return obj!.ContainsKey(key);
    }
    public static object 对象_是否存在键(object _obj, object _key) {
        return object_has(_obj, _key);
    }
    public static object object_find(object _obj, object _item) {
        NinaDataObject? obj = _obj as NinaDataObject;
        if (obj == null) {
            NinaError.error("操作的对象无效.", 190341);
        }
        for (int i = 0; i < obj!.Count; ++ i) {
            var (k, v) = obj.ElementAt(i);
            if (NinaAPIUtil.opLEquS_bool(v, _item)) {
                return k;
            }
        }
        return null !;
    }
    public static object 对象_查找值(object _obj, object _item) {
        return object_find(_obj, _item);
    }
    public static object object_find_last(object _obj, object _item) {
        NinaDataObject? obj = _obj as NinaDataObject;
        if (obj == null) {
            NinaError.error("操作的对象无效.", 765402);
        }
        for (int i = obj!.Count - 1; i >= 0; ++ i) {
            var (k, v) = obj.ElementAt(i);
            if (NinaAPIUtil.opLEquS_bool(v, _item)) {
                return k;
            }
        }
        return null !;
    }
    public static object 对象_反向查找值(object _obj, object _item) {
        return object_find_last(_obj, _item);
    }
    public static object object_remove(object _obj, object _key) {
        NinaDataObject? obj = _obj as NinaDataObject;
        string key = NinaAPIUtil.toStringS(_key);
        if (obj == null) {
            NinaError.error("操作的对象无效.", 790422);
        }
        return obj!.Remove(key) && obj!.my_consts.Remove(key);
    }
    public static object 对象_移除项(object _obj, object _key) {
        return object_remove(_obj, _key);
    }
    public static object object_clear(object _obj) {
        NinaDataObject? obj = _obj as NinaDataObject;
        if (obj == null) {
            NinaError.error("操作的对象无效.", 234012);
        }
        obj!.Clear();
        obj!.my_consts.Clear();
        return true;
    }
    public static object 对象_清空(object _obj) {
        return object_clear(_obj);
    }
    public static object object_to_JSON(object _obj) {
        NinaDataObject? obj = _obj as NinaDataObject;
        if (obj == null) {
            NinaError.error("操作的对象无效.", 102941);
        }
        try {
            return
                JsonSerializer.Serialize(
                    obj, json_option
                );
        }
        catch {
            return null !;
        }
    }
    public static object 对象_对象到JSON(object _obj) {
        return object_to_JSON(_obj);
    }
    public static object object_from_JSON(object _json) {
        string json = NinaAPIUtil.toStringS(_json);
        try {
            return
                new NinaDataObject(
                    JsonSerializer.Deserialize<Dictionary<string, object>>(
                        json,
                        json_option
                    ) !
                );
        }
        catch {
            return null !;
        }
    }
    public static object 对象_JSON到对象(object _json) {
        return object_from_JSON(_json);
    }

    public static object time_now() {
        TimeSpan ts
            = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);
        return ts.TotalSeconds;
    }
    public static object 时间_取当前时间戳() {
        return time_now();
    }
    public static object time_to_string(object _time) {
        double s = NinaAPIUtil.toNumberS(_time);
        DateTime now
            = DateTimeOffset.FromUnixTimeSeconds((long) s)
                .LocalDateTime;
        return now.ToString();
    }
    public static object 时间_时间戳到字符串(object _time) {
        return time_to_string(_time);
    }
    public static object time_to_object(object _time) {
        double s = NinaAPIUtil.toNumberS(_time);
        DateTime now
            = DateTimeOffset.FromUnixTimeSeconds((long) s)
                .LocalDateTime;
        return new NinaDataObject {
            ["type"] = "NinaTime",
            ["ts"] = s,
            ["y"] = (double) now.Year,
            ["m"] = (double) now.Month,
            ["d"] = (double) now.Day,
            ["d_w"] = (double) now.DayOfWeek,
            ["d_y"] = (double) now.DayOfYear,
            ["h"] = (double) now.Hour,
            ["m"] = (double) now.Minute,
            ["s"] = (double) now.Second,
            ["ms"] = (double) now.Millisecond
        };
    }
    public static object 时间_时间戳到时间对象(object _time) {
        return time_to_object(_time);
    }
    public static object time_from_string(object _str) {
        bool ok = DateTime.TryParse(
            NinaAPIUtil.toStringS(_str), out DateTime ret);
        if (ok) {
            TimeSpan ts
                = ret.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);
            return ts.TotalSeconds;
        }
        else {
            return null !;
        }
    }
    public static object 时间_字符串到时间戳(object _str) {
        return time_from_string(_str);
    }

    private static Random random_gener = new Random();
    public static object random_raw() {
        return random_gener.NextDouble();
    }
    public static object 随机数_原始值() {
        return random_raw();
    }
    public static object random_range(object _min, object _max) {
        double min = NinaAPIUtil.toNumberS(_min);
        double max = NinaAPIUtil.toNumberS(_max);
        double d = random_gener.NextDouble();
        return min + d * (max - min);
    }
    public static object 随机数_有范围(object _min, object _max) {
        return random_range(_min, _max);
    }

    public static object math_floor(object _n) {
        double n = NinaAPIUtil.toNumberS(_n);
        return Math.Floor(n);
    }
    public static object 数学_向下舍入(object _n) {
        return math_floor(_n);
    }
    public static object math_ceil(object _n) {
        double n = NinaAPIUtil.toNumberS(_n);
        return Math.Ceiling(n);
    }
    public static object 数学_向上舍入(object _n) {
        return math_ceil(_n);
    }
    public static object math_round(object _n) {
        double n = NinaAPIUtil.toNumberS(_n);
        return Math.Round(n);
    }
    public static object 数学_四舍五入(object _n) {
        return math_round(_n);
    }
    public static object math_round_digit(object _n, object _d) {
        double n = NinaAPIUtil.toNumberS(_n);
        double d = NinaAPIUtil.toNumberS(_d);
        return Math.Round(n, (int) d);
    }
    public static object 数学_按位数四舍五入(object _n, object _d) {
        return math_round_digit(_n, _d);
    }
    public static object math_sin(object _a) {
        return Math.Sin(NinaAPIUtil.toNumberS(_a));
    }
    public static object 数学_正弦(object _a) {
        return math_sin(_a);
    }
    public static object math_cos(object _a) {
        return Math.Cos(NinaAPIUtil.toNumberS(_a));
    }
    public static object 数学_余弦(object _a) {
        return math_cos(_a);
    }
    public static object math_tan(object _a) {
        return Math.Tan(NinaAPIUtil.toNumberS(_a));
    }
    public static object 数学_正切(object _a) {
        return math_tan(_a);
    }
    public static object math_asin(object _a) {
        return Math.Asin(NinaAPIUtil.toNumberS(_a));
    }
    public static object 数学_反正弦(object _a) {
        return math_asin(_a);
    }
    public static object math_acos(object _a) {
        return Math.Acos(NinaAPIUtil.toNumberS(_a));
    }
    public static object 数学_反余弦(object _a) {
        return math_acos(_a);
    }
    public static object math_atan(object _a) {
        return Math.Atan(NinaAPIUtil.toNumberS(_a));
    }
    public static object 数学_反正切(object _a) {
        return math_atan(_a);
    }
    public static object math_sqrt(object _a) {
        return Math.Sqrt(NinaAPIUtil.toNumberS(_a));
    }
    public static object 数学_平方根(object _a) {
        return math_sqrt(_a);
    }
    public static object math_abs(object _a) {
        return Math.Abs(NinaAPIUtil.toNumberS(_a));
    }
    public static object 数学_绝对值(object _a) {
        return math_abs(_a);
    }
    public static object math_max(object _a, object _b) {
        return Math.Max(
            NinaAPIUtil.toNumberS(_a), NinaAPIUtil.toNumberS(_b)
        );
    }
    public static object 数学_最大(object _a, object _b) {
        return math_max(_a, _b);
    }
    public static object math_min(object _a, object _b) {
        return Math.Min(
            NinaAPIUtil.toNumberS(_a), NinaAPIUtil.toNumberS(_b)
        );
    }
    public static object 数学_最小(object _a, object _b) {
        return math_min(_a, _b);
    }
    public static object math_log(object _a, object _b) {
        return Math.Log(
            NinaAPIUtil.toNumberS(_a), NinaAPIUtil.toNumberS(_b)
        );
    }
    public static object 数学_对数(object _a, object _b) {
        return math_log(_a, _b);
    }
}