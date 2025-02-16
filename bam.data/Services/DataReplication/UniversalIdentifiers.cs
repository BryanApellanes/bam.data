using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bam.Services.DataReplication
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UniversalIdentifiers
    {
        Uuid,
        Cuid,
        CKey // composite key
    }
}
