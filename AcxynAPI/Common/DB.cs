using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AcxynAPI.Common
{
    public class DB
    {
        //insert or update
        public static void Upsert(string sql, Dictionary<string, object> parameters, string cs)
        {
            using (var conn = new MySqlConnection(cs))
            {
                conn.Open();
                var cmd = new MySqlCommand(string.Empty, conn);

                foreach (var p in parameters)
                {
                    cmd.Parameters.AddWithValue(p.Key, p.Value);
                }

                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        //return inserted id
        public static int InsertReturningId(string sql, Dictionary<string, object> parameters, string cs)
        {
            using (var conn = new MySqlConnection(cs))
            {
                conn.Open();
                var cmd = new MySqlCommand(string.Empty, conn);

                foreach (var p in parameters)
                {
                    cmd.Parameters.AddWithValue(p.Key, p.Value);
                }

                cmd.CommandText = sql;
                int returningId = Convert.ToInt32(cmd.ExecuteScalar().ToString());
                conn.Close();

                return returningId;
            }
        }

        public static DataTable Select(string sql, Dictionary<string, object> parameters, string cs)
        {
            using (var conn = new MySqlConnection(cs))
            {
                conn.Open();
                var cmd = new MySqlCommand(string.Empty, conn);

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        cmd.Parameters.AddWithValue(p.Key, p.Value);
                    }

                }
                cmd.CommandText = sql;
                MySqlDataReader reader = cmd.ExecuteReader();
                DataTable queryDT = new DataTable("DT");
                if (reader.HasRows)
                {
                    queryDT.Load(reader);
                }
                conn.Close();
                return queryDT;
            }
        }

        public static void TransactionUpsert(string sql, Dictionary<string, object> parameters, string cs)
        {
            using (var conn = new MySqlConnection(cs))
            {
                conn.Open();
                var cmd = new MySqlCommand(string.Empty, conn);
                MySqlTransaction trans;

                foreach (var p in parameters)
                {
                    cmd.Parameters.AddWithValue(p.Key, p.Value);
                }

                trans = conn.BeginTransaction();
                cmd.Transaction = trans;

                try
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }
}
