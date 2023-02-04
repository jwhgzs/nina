using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace Nina;

public class NinaDataArray: List<object> {
    public Dictionary<int, bool> my_consts
        = new Dictionary<int, bool>();
    public NinaDataArray(): base() {}
    public NinaDataArray(List<object> _src)
            : base(_src) {}
}
public class NinaDataObject: Dictionary<string, object> {
    public Dictionary<string, bool> my_consts
        = new Dictionary<string, bool>();
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
            NinaError.error("invalid convert to numeric.", 123901);
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
            NinaError.error("invalid conversion to numeric.", 412910);
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
        else if (_o.GetType().Name.StartsWith(
                NinaConstsProviderUtil.IL_CLOSURECLASS_ID_PREFIX))
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
    public static object opAdd(object _lhs, object _rhs) {
        if (_lhs is string a || _rhs is string b)
            return toString(_lhs) + toString(_rhs);
        else
            return toNumber(_lhs) + toNumber(_rhs);
    }
    public static object opSub(object _lhs, object _rhs) {
        return toNumber(_lhs) - toNumber(_rhs);
    }
    public static object opPos(object _o) {
        return toNumber(_o);
    }
    public static object opNeg(object _o) {
        return - toNumber(_o);
    }
    public static object opMut(object _lhs, object _rhs) {
        return toNumber(_lhs) * toNumber(_rhs);
    }
    public static object opDiv(object _lhs, object _rhs) {
        return toNumber(_lhs) / toNumber(_rhs);
    }
    public static object opRem(object _lhs, object _rhs) {
        return toNumber(_lhs) % toNumber(_rhs);
    }
    public static object opPow(object _lhs, object _rhs) {
        return Math.Pow(toNumber(_lhs), toNumber(_rhs));
    }
    public static object opLNot(object _o) {
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
    public static object opLEqu(object _lhs, object _rhs) {
        return opLEqu_bool(_lhs, _rhs);
    }
    public static object opLNEqu(object _lhs, object _rhs) {
        return ! opLEqu_bool(_lhs, _rhs);
    }
    public static object opMore(object _lhs, object _rhs) {
        return toNumber(_lhs) > toNumber(_rhs);
    }
    public static object opLess(object _lhs, object _rhs) {
        return toNumber(_lhs) < toNumber(_rhs);
    }
    public static object opMoreE(object _lhs, object _rhs) {
        return toNumber(_lhs) >= toNumber(_rhs);
    }
    public static object opLessE(object _lhs, object _rhs) {
        return toNumber(_lhs) <= toNumber(_rhs);
    }
    public static object opLAnd(object _lhs, object _rhs) {
        return ! toBool(_lhs) ? _lhs : _rhs;
    }
    public static object opLOr(object _lhs, object _rhs) {
        return toBool(_lhs) ? _lhs : _rhs;
    }
    public static object opNot(object _o) {
        return ~ (int) toNumber(_o);
    }
    public static object opAnd(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) & (int) toNumber(_rhs));
    }
    public static object opOr(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) | (int) toNumber(_rhs));
    }
    public static object opXOr(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) ^ (int) toNumber(_rhs));
    }
    public static object opSftL(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) << (int) toNumber(_rhs));
    }
    public static object opSftR(object _lhs, object _rhs) {
        return (double) ((int) toNumber(_lhs) >> (int) toNumber(_rhs));
    }
    public static object opTypeof(object _o) {
        return toTypeDesc(_o);
    }
    
    public static object member_get(object _obj, object _key) {
        if (_obj is NinaDataArray arr) {
            int key = (int) toNumber(_key);
            return key >= 0 && key <= arr.Count - 1
                ? arr[key]
                : null !;
        }
        else if (_obj is NinaDataObject obj) {
            string key = toString(_key);
            return obj.ContainsKey(key) ? obj[key] : null !;
        }
        else {
            NinaError.error("invalid target for member access operation.", 626768);
        }
        return null !;
    }
    public static object member_init(
            object _obj, object _key, object _val, int _isConst) {
        if (_obj is NinaDataArray arr) {
            int key = (int) toNumber(_key);
            if (arr.my_consts.ContainsKey(key)) {
                NinaError.error(
                    "invalid assignment to constant member.",
                    594943);
            }
            arr.EnsureCapacity(key + 1);
            while (arr.Count < key + 1)
                arr.Add(null !);
            arr[key] = _val;
            if (_isConst != 0)
                arr.my_consts[key] = true;
        }
        else if (_obj is NinaDataObject obj) {
            string key = toString(_key);
            if (obj.my_consts.ContainsKey(key)) {
                NinaError.error(
                    "invalid assignment to a constant member.",
                    794922);
            }
            obj[key] = _val;
            if (_isConst != 0)
                obj.my_consts[key] = true;
        }
        else {
            NinaError.error("invalid target to access member.", 426694);
        }
        return _val;
    }
    public static object member_set(
            object _obj, object _key, object _val) {
        return member_init(_obj, _key, _val, 0);
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
                MethodBase? nmtd = v.GetMethod();
                string nss = nmtd != null
                    ? NinaCompilerUtil.snapshot_method(_file, nmtd)
                    : "";
                if (pos_table.ContainsKey(nss)) {
                    if (matched >= _n) {
                        ss = nss;
                        offset = v.GetILOffset();
                        byte[] a = nmtd!.GetMethodBody()!.GetILAsByteArray()!;
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
    public static object func_invoke(dynamic _func, object[] _params) {
        Type tp = _func.GetType();
        if (! tp.Name.StartsWith(NinaConstsProviderUtil.IL_CLOSURECLASS_ID_PREFIX))
            NinaError.error("invalid function to call.", 163091);
        return _func(_params);
    }
}

public static class NinaAPI {
    public static object @null = null !;
    public static object @true = true;
    public static object @false = false;
    public static object math_PI = Math.PI;
    public static object math_E = Math.E;
    public static object eval(object _code, object _arg) {
        string code = NinaAPIUtil.toString(_code);
        try {
            return NinaCore.execute(
                "[Dynamic Codes: " + Guid.NewGuid().ToString("N") + "]",
                code, _arg
            ) !;
        }
        catch (TargetInvocationException ex) {
            NinaError.error(
                "error when evaluating:\n"
                    + NinaError.trim_header(ex.InnerException!.Message),
                352982);
        }
        return null !;
    }
    public static object console_print(object _data) {
        string v = NinaAPIUtil.toString(_data);
        Console.Write(v);
        return null !;
    }
    public static object console_printf(object _data) {
        string v = NinaAPIUtil.toString(_data);
        Console.WriteLine(v);
        return null !;
    }
    public static object console_read() {
        return Console.ReadLine() !;
    }
    public static object console_exit() {
        Environment.Exit(- 2);
        return null !;
    }
    public static object string_at(object _str, object _i) {
        string str = NinaAPIUtil.toString(_str);
        int i = (int) NinaAPIUtil.toNumber(_i);
        try {
            return str[i].ToString();
        }
        catch {
            return null !;
        }
    }
    public static object string_sub(object _str, object _i, object _n) {
        string str = NinaAPIUtil.toString(_str);
        int i = (int) NinaAPIUtil.toNumber(_i);
        int n = (int) NinaAPIUtil.toNumber(_n);
        try {
            return str.Substring(i, n);
        }
        catch {
            return null !;
        }
    }
    public static object string_sub_to_tail(object _str, object _i) {
        string str = NinaAPIUtil.toString(_str);
        int i = (int) NinaAPIUtil.toNumber(_i);
        try {
            return str.Substring(i);
        }
        catch {
            return null !;
        }
    }
    public static object string_split(object _str, object _sub) {
        string str = NinaAPIUtil.toString(_str);
        string sub = NinaAPIUtil.toString(_sub);
        try {
            return new NinaDataArray(
                str.Split(sub).Select(v => (object) v).ToList()
            );
        }
        catch {
            return null !;
        }
    }
    public static object string_split_count(object _str, object _sub, object _n) {
        string str = NinaAPIUtil.toString(_str);
        string sub = NinaAPIUtil.toString(_sub);
        int n = (int) NinaAPIUtil.toNumber(_n);
        try {
            return new NinaDataArray(
                str.Split(sub, n).Select(v => (object) v).ToList()
            );
        }
        catch {
            return null !;
        }
    }
    public static object string_to_array(object _str) {
        string str = NinaAPIUtil.toString(_str);
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
    public static object string_from_array(object _arr) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null)
            return null !;
        string ret = "";
        for (int i = 0; i < arr.Count; ++ i) {
            ret += NinaAPIUtil.toString(arr[i]);
        }
        return ret;
    }
    public static object string_join(object _arr, object _sub) {
        NinaDataArray? arr = _arr as NinaDataArray;
        string sub = NinaAPIUtil.toString(_sub);
        if (arr == null)
            return null !;
        string ret = "";
        for (int i = 0; i < arr.Count; ++ i) {
            if (i > 0)
                ret += sub;
            ret += NinaAPIUtil.toString(arr[i]);
        }
        return ret;
    }
    public static object string_replace(object _str, object _sub, object _rep) {
        string str = NinaAPIUtil.toString(_str);
        string sub = NinaAPIUtil.toString(_sub);
        string rep = NinaAPIUtil.toString(_rep);
        return str.Replace(sub, rep);
    }
    public static object string_replace_count(
            object _str, object _sub, object _rep, object _n) {
        string str = NinaAPIUtil.toString(_str);
        string sub = NinaAPIUtil.toString(_sub);
        string rep = NinaAPIUtil.toString(_rep);
        int n = (int) NinaAPIUtil.toNumber(_n);
        string[] arr = str.Split(sub, n + 1);
        return string.Join(rep, arr);
    }
    public static object string_upper(object _str) {
        string str = NinaAPIUtil.toString(_str);
        return str.ToUpper();
    }
    public static object string_lower(object _str) {
        string str = NinaAPIUtil.toString(_str);
        return str.ToLower();
    }
    public static object string_length(object _str) {
        string str = NinaAPIUtil.toString(_str);
        return (double) str.Length;
    }
    public static object array_length(object _arr) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("invalid array to operate.", 796923);
        }
        return (double) arr!.Count;
    }
    public static object array_append(object _arr, object _item) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("invalid array to operate.", 120312);
        }
        arr!.Add(_item);
        return true;
    }
    public static object array_insert(object _arr, object _n, object _item) {
        NinaDataArray? arr = _arr as NinaDataArray;
        int n = (int) NinaAPIUtil.toNumber(_n);
        if (arr == null) {
            NinaError.error("invalid array to operate.", 102914);
        }
        try {
            arr!.Insert(n, _item);
            return true;
        }
        catch {
            return false;
        }
    }
    public static object array_pop(object _arr) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("invalid array to operate.", 593192);
        }
        try {
            arr!.RemoveAt(arr.Count - 1);
            return true;
        }
        catch {
            return false;
        }
    }
    public static object array_remove(object _arr, object _n) {
        NinaDataArray? arr = _arr as NinaDataArray;
        int n = (int) NinaAPIUtil.toNumber(_n);
        if (arr == null) {
            NinaError.error("invalid array to operate.", 605912);
        }
        try {
            arr!.RemoveAt(n);
            return true;
        }
        catch {
            return false;
        }
    }
    public static object array_clear(object _arr) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("invalid array to operate.", 491021);
        }
        arr!.Clear();
        return true;
    }
    public static object array_find(object _arr, object _item) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("invalid array to operate.", 701901);
        }
        for (int i = 0; i < arr!.Count; ++ i) {
            if (NinaAPIUtil.opLEqu_bool(arr![i], _item)) {
                return (double) i;
            }
        }
        return - 1d;
    }
    public static object array_find_last(object _arr, object _item) {
        NinaDataArray? arr = _arr as NinaDataArray;
        if (arr == null) {
            NinaError.error("invalid array to operate.", 249931);
        }
        for (int i = arr!.Count - 1; i >= 0; -- i) {
            if (NinaAPIUtil.opLEqu_bool(arr![i], _item)) {
                return (double) i;
            }
        }
        return - 1d;
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
            NinaError.error("invalid array to operate.", 120491);
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
    public static object array_from_json(object _json) {
        string json = NinaAPIUtil.toString(_json);
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
    public static object object_length(object _obj) {
        NinaDataObject? obj = _obj as NinaDataObject;
        if (obj == null) {
            NinaError.error("invalid object to operate.", 796923);
        }
        return (double) obj!.Count;
    }
    public static object object_has(object _obj, object _key) {
        NinaDataObject? obj = _obj as NinaDataObject;
        string key = NinaAPIUtil.toString(_key);
        if (obj == null) {
            NinaError.error("invalid object to operate.", 123091);
        }
        return obj!.ContainsKey(key);
    }
    public static object object_find(object _obj, object _item) {
        NinaDataObject? obj = _obj as NinaDataObject;
        string item = NinaAPIUtil.toString(_item);
        if (obj == null) {
            NinaError.error("invalid object to operate.", 190341);
        }
        for (int i = 0; i < obj!.Count; ++ i) {
            var (k, v) = obj.ElementAt(i);
            if (NinaAPIUtil.opLEqu_bool(v, item)) {
                return k;
            }
        }
        return null !;
    }
    public static object object_find_last(object _obj, object _item) {
        NinaDataObject? obj = _obj as NinaDataObject;
        string item = NinaAPIUtil.toString(_item);
        if (obj == null) {
            NinaError.error("invalid object to operate.", 765402);
        }
        for (int i = obj!.Count - 1; i >= 0; ++ i) {
            var (k, v) = obj.ElementAt(i);
            if (NinaAPIUtil.opLEqu_bool(v, item)) {
                return k;
            }
        }
        return null !;
    }
    public static object object_remove(object _obj, object _key) {
        NinaDataObject? obj = _obj as NinaDataObject;
        string key = NinaAPIUtil.toString(_key);
        if (obj == null) {
            NinaError.error("invalid object to operate.", 790422);
        }
        return obj!.Remove(key);
    }
    public static object object_clear(object _obj) {
        NinaDataObject? obj = _obj as NinaDataObject;
        if (obj == null) {
            NinaError.error("invalid object to operate.", 234012);
        }
        obj!.Clear();
        return true;
    }
    public static object object_to_json(object _obj) {
        NinaDataObject? obj = _obj as NinaDataObject;
        if (obj == null) {
            NinaError.error("invalid object to operate.", 102941);
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
    public static object object_from_json(object _json) {
        string json = NinaAPIUtil.toString(_json);
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
    public static object time_now() {
        TimeSpan ts
            = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);
        return ts.TotalSeconds;
    }
    public static object time_to_string(object _time) {
        double s = NinaAPIUtil.toNumber(_time);
        DateTime now
            = DateTimeOffset.FromUnixTimeSeconds((long) s)
                .LocalDateTime;
        return now.ToString();
    }
    public static object time_to_object(object _time) {
        double s = NinaAPIUtil.toNumber(_time);
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
    public static object time_from_string(object _str) {
        bool ok = DateTime.TryParse(
            NinaAPIUtil.toString(_str), out DateTime ret);
        if (ok) {
            TimeSpan ts
                = ret.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0);
            return ts.TotalSeconds;
        }
        else {
            return null !;
        }
    }
    private static Random random_gener = new Random();
    public static object random_raw() {
        return random_gener.NextDouble();
    }
    public static object random_range(object _min, object _max) {
        double min = NinaAPIUtil.toNumber(_min);
        double max = NinaAPIUtil.toNumber(_max);
        double d = random_gener.NextDouble();
        return min + d * (max - min);
    }
    public static object math_floor(object _n) {
        double n = NinaAPIUtil.toNumber(_n);
        return Math.Floor(n);
    }
    public static object math_ceil(object _n) {
        double n = NinaAPIUtil.toNumber(_n);
        return Math.Ceiling(n);
    }
    public static object math_round(object _n) {
        double n = NinaAPIUtil.toNumber(_n);
        return Math.Round(n);
    }
    public static object math_round_digit(object _n, object _d) {
        double n = NinaAPIUtil.toNumber(_n);
        double d = NinaAPIUtil.toNumber(_d);
        return Math.Round(n, (int) d);
    }
    public static object math_sin(object _a) {
        return Math.Sin(NinaAPIUtil.toNumber(_a));
    }
    public static object math_cos(object _a) {
        return Math.Cos(NinaAPIUtil.toNumber(_a));
    }
    public static object math_tan(object _a) {
        return Math.Tan(NinaAPIUtil.toNumber(_a));
    }
    public static object math_asin(object _a) {
        return Math.Asin(NinaAPIUtil.toNumber(_a));
    }
    public static object math_acos(object _a) {
        return Math.Acos(NinaAPIUtil.toNumber(_a));
    }
    public static object math_atan(object _a) {
        return Math.Atan(NinaAPIUtil.toNumber(_a));
    }
    public static object math_sqrt(object _a) {
        return Math.Sqrt(NinaAPIUtil.toNumber(_a));
    }
    public static object math_abs(object _a) {
        return Math.Abs(NinaAPIUtil.toNumber(_a));
    }
    public static object math_max(object _a, object _b) {
        return Math.Max(
            NinaAPIUtil.toNumber(_a), NinaAPIUtil.toNumber(_b)
        );
    }
    public static object math_min(object _a, object _b) {
        return Math.Min(
            NinaAPIUtil.toNumber(_a), NinaAPIUtil.toNumber(_b)
        );
    }
    public static object math_log(object _a, object _b) {
        return Math.Log(
            NinaAPIUtil.toNumber(_a), NinaAPIUtil.toNumber(_b)
        );
    }
}