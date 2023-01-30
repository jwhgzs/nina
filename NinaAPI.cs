namespace Nina;

public class NinaDataArray: List<object> {
    public Dictionary<int, bool> my_consts
        = new Dictionary<int, bool>();
}
public class NinaDataObject: Dictionary<string, object> {
    public Dictionary<string, bool> my_consts
        = new Dictionary<string, bool>();
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
        else
            NinaError.error("invalid conversion to numeric.", 412910);
        return 0;
    }
    public static string toTypeDesc(object _o) {
        if (_o == null)
            return "[Null]";
        else if (_o is double || _o is bool)
            return "[Number]";
        else if (_o is string)
            return "[String]";
        else if (_o is NinaDataArray)
            return "[NinaDataArray]";
        else if (_o is NinaDataObject)
            return "[NinaDataObject]";
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
                    "invalid assignment to constant member.",
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
}

public static class NinaAPI {
    public static object @null = null !;
    public static object @true = true;
    public static object @false = false;
    public static object math_PI = Math.PI;
    public static object math_E = Math.E;
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