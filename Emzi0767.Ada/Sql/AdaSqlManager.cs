using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emzi0767.Ada.Config;
using Npgsql;
using NpgsqlTypes;

namespace Emzi0767.Ada.Sql
{
    public sealed class AdaSqlManager
    {
        private AdaPostgresConfiguration Configuration { get; set; }
        private string ConnectionString { get; set; }

        private static Dictionary<Type, NpgsqlDbType> TypeMappings { get; set; }

        public AdaSqlManager(AdaPostgresConfiguration config)
        {
            this.Configuration = config;
            this.ConnectionString = string.Concat("Server='", config.Hostname, "';Port=", config.Port, ";Database='", config.Database, "';User Id='", config.Username, "';Password='", config.Password, "';");
        }

        static AdaSqlManager()
        {
            TypeMappings = new Dictionary<Type, NpgsqlDbType>
            {
                [typeof(bool)] = NpgsqlDbType.Boolean,

                [typeof(float)] = NpgsqlDbType.Real,
                [typeof(double)] = NpgsqlDbType.Double,
                [typeof(decimal)] = NpgsqlDbType.Numeric,

                [typeof(short)] = NpgsqlDbType.Smallint,
                [typeof(short[])] = NpgsqlDbType.Array | NpgsqlDbType.Smallint,
                [typeof(int)] = NpgsqlDbType.Integer,
                [typeof(int[])] = NpgsqlDbType.Array | NpgsqlDbType.Integer,
                [typeof(long)] = NpgsqlDbType.Bigint,

                [typeof(object)] = NpgsqlDbType.Circle,
                [typeof(string)] = NpgsqlDbType.Varchar,
                [typeof(string[])] = NpgsqlDbType.Array | NpgsqlDbType.Varchar,
                [typeof(byte[])] = NpgsqlDbType.Bytea,
                [typeof(Array)] = NpgsqlDbType.Array,

                [typeof(Guid)] = NpgsqlDbType.Uuid,
                [typeof(DateTime)] = NpgsqlDbType.TimestampTZ,
                [typeof(TimeSpan)] = NpgsqlDbType.Interval,
                [typeof(System.Net.IPAddress)] = NpgsqlDbType.Inet
            };

            //TypeMappings[typeof(System.Xml.XmlDocument)] = NpgsqlDbType.Xml;
        }

        public async Task<IEnumerable<IDictionary<string, object>>> QueryAsync(string query, IEnumerable<NpgsqlParameter> parameters)
        {
            using (var conn = new NpgsqlConnection(this.ConnectionString))
            {
                await conn.OpenAsync();

                using (var tran = conn.BeginTransaction())
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Transaction = tran;

                    foreach (var param in parameters)
                        cmd.Parameters.Add(param);

                    cmd.Prepare();

                    var lst = new List<Dictionary<string, object>>();
                    using (var rdr = await cmd.ExecuteReaderAsync())
                        while (await rdr.ReadAsync())
                            lst.Add(Enumerable
                                .Range(0, rdr.FieldCount)
                                .ToDictionary(rdr.GetName, rdr.GetValue));

                    await tran.CommitAsync();

                    return lst.AsEnumerable();
                }
            }
        }

        public async Task QueryNonReaderAsync(string query, IEnumerable<NpgsqlParameter> parameters, IEnumerable<IDictionary<string, object>> param_values)
        {
            using (var conn = new NpgsqlConnection(this.ConnectionString))
            {
                await conn.OpenAsync();

                using (var tran = conn.BeginTransaction())
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Transaction = tran;

                    if (parameters.Any())
                    {
                        foreach (var param in parameters)
                            cmd.Parameters.Add(param);

                        cmd.Prepare();

                        foreach (var dict in param_values)
                        {
                            foreach (var kvp in dict)
                                cmd.Parameters[kvp.Key].Value = kvp.Value;

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    else
                        await cmd.ExecuteNonQueryAsync();

                    await tran.CommitAsync();
                }
            }
        }

        public NpgsqlDbType GetTypeFromObject(object o)
        {
            return TypeMappings[o.GetType()];
        }
    }
}
