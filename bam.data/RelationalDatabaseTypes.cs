using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bam.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RelationalDatabaseTypes
    {
        SQLite,
        MsSql,
        MySql,
        Postgres
    }
}