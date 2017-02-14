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
            TypeMappings = new Dictionary<Type, NpgsqlDbType>();
            TypeMappings[typeof(bool)] = NpgsqlDbType.Boolean;

            TypeMappings[typeof(float)] = NpgsqlDbType.Real;
            TypeMappings[typeof(double)] = NpgsqlDbType.Double;
            TypeMappings[typeof(decimal)] = NpgsqlDbType.Numeric;

            TypeMappings[typeof(short)] = NpgsqlDbType.Smallint;
            TypeMappings[typeof(short[])] = NpgsqlDbType.Array | NpgsqlDbType.Smallint;
            TypeMappings[typeof(int)] = NpgsqlDbType.Integer;
            TypeMappings[typeof(int[])] = NpgsqlDbType.Array | NpgsqlDbType.Integer;
            TypeMappings[typeof(long)] = NpgsqlDbType.Bigint;

            TypeMappings[typeof(object)] = NpgsqlDbType.Circle;
            TypeMappings[typeof(string)] = NpgsqlDbType.Varchar;
            TypeMappings[typeof(string[])] = NpgsqlDbType.Array | NpgsqlDbType.Varchar;
            TypeMappings[typeof(byte[])] = NpgsqlDbType.Bytea;
            TypeMappings[typeof(Array)] = NpgsqlDbType.Array;

            TypeMappings[typeof(Guid)] = NpgsqlDbType.Uuid;
            TypeMappings[typeof(DateTime)] = NpgsqlDbType.TimestampTZ;
            TypeMappings[typeof(TimeSpan)] = NpgsqlDbType.Interval;
            TypeMappings[typeof(System.Net.IPAddress)] = NpgsqlDbType.Inet;

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
