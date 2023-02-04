using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nina;

public class NinaJSONObjectConverter : JsonConverter<object> {
    public override object? Read(
            ref Utf8JsonReader _reader, Type _type,
            JsonSerializerOptions _options) {
        JsonTokenType tp = _reader.TokenType;
        switch (tp) {
            case JsonTokenType.Number:
                if (_reader.TryGetInt32(out int i))
                    return i;
                if (_reader.TryGetInt64(out long l))
                    return l;
                if (_reader.TryGetDouble(out double d))
                    return d;
                return null;
            case JsonTokenType.String:
                return _reader.GetString();
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.StartArray:
                NinaDataArray ret = new NinaDataArray();
                while (_reader.Read()) {
                    switch (_reader.TokenType) {
                        case JsonTokenType.EndArray:
                            return ret;
                        default:
                            ret.Add(
                                Read(ref _reader, typeof(object), _options) !
                            );
                            break;
                    }
                }
                NinaError.error("无效的 JSON 格式.", 120391);
                break;
            case JsonTokenType.StartObject:
                NinaDataObject ret2 = new NinaDataObject();
                while (_reader.Read()) {
                    switch (_reader.TokenType) {
                        case JsonTokenType.EndObject:
                            return ret2;
                        case JsonTokenType.PropertyName:
                            string key = _reader.GetString() !;
                            _reader.Read();
                            ret2[key]
                                = Read(
                                    ref _reader, typeof(object), _options
                                ) !;
                            break;
                        default:
                            NinaError.error("无效的 JSON 格式.", 193210);
                            break;
                    }
                }
                NinaError.error("无效的 JSON 格式.", 120391);
                break;
            default:
                NinaError.error("无效的 JSON 格式.", 192312);
                break;
        }
        return null;
    }
    public override void Write(
            Utf8JsonWriter _writer, object _value,
            JsonSerializerOptions _options) {
        if (_value == null)
            _writer.WriteNullValue();
        else if (_value is double d)
            _writer.WriteNumberValue(d);
        else if (_value is string s)
            _writer.WriteStringValue(s);
        else if (_value is bool b)
            _writer.WriteBooleanValue(b);
        else if (_value is int i)
            _writer.WriteNumberValue(i);
        else if (_value is long l)
            _writer.WriteNumberValue(l);
        else if (_value is NinaDataArray arr) {
            _writer.WriteStartArray();
            for (int j = 0; j < arr.Count; ++ j) {
                Write(_writer, arr[j], _options);
            }
            _writer.WriteEndArray();
        }
        else if (_value is NinaDataObject obj) {
            _writer.WriteStartObject();
            for (int j = 0; j < obj.Count; ++ j) {
                var (key, val) = obj.ElementAt(j);
                _writer.WritePropertyName(key);
                Write(_writer, val, _options);
            }
            _writer.WriteEndObject();
        }
        else {
            _writer.WriteStringValue(
                NinaAPIUtil.toTypeDesc(_value)
            );
        }
    }
}