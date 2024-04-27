using System.Text.Json;
using System.Text.Json.Serialization;

namespace JAC.Shared;

//https://stackoverflow.com/questions/72774698/system-text-json-serialization-doesnt-work-for-abstract-members/72775719#72775719
public class AbstractToConcreteConverter<TAbstract, TConcrete> : JsonConverter<TAbstract> where TConcrete : TAbstract
{
    static AbstractToConcreteConverter()
    {
        if (typeof(TAbstract) == typeof(TConcrete))
            throw new ArgumentException(string.Format("Identical type {0} used for both TAbstract and TConcrete", typeof(TConcrete)));
    }
    
    public override TAbstract? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize<TConcrete>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, TAbstract value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, (TConcrete)value!, options);
}