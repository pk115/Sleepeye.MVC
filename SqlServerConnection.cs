using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sleepeye.MVC
{
    public  class SqlServerConnection
    {

      public static  string ConnectionString = ConfigurationSettings.AppSettings["SQLConnect"].ToString();

        public static SqlConnection con;
        public static void OpenConection()
        {
            con = new SqlConnection(ConnectionString);
            con.Open();
        }
        public static void CloseConnection()
        {

            con.Close();
        }
        public static void ExecuteQueries(string Query_)
        {
            OpenConection();
            SqlCommand cmd = new SqlCommand(Query_, con);
            cmd.ExecuteNonQuery();
            CloseConnection();
        }
        public static SqlDataReader DataReader(string Query_)
        {
            OpenConection();
            SqlCommand cmd = new SqlCommand(Query_, con);
            SqlDataReader dr = cmd.ExecuteReader();
            CloseConnection();
            return dr;

        }
        public static DataTable ConvertDataTable(string Query_)
        {

            OpenConection();
            SqlDataAdapter dr = new SqlDataAdapter(Query_, ConnectionString);
            DataSet ds = new DataSet();
            dr.Fill(ds);
            CloseConnection();
            return ds.Tables[0];

        }
        public static string ConvertDateTimeToString(string dateTime)
        {
            var date = string.Empty;
            if (dateTime != "")
            {
                date = DateTime.ParseExact(dateTime, "dd/MM/yyyy hh:mm:ss", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
            }

            return date;

        }
        public static void ExecuteQueriesCommandText(SqlCommand cmd)
        {
            OpenConection();
            cmd.Connection = con;
            cmd.ExecuteNonQuery();
            CloseConnection();
        }
        public static DataTable GetDataTableCommandText(SqlCommand cmd)
        {
            OpenConection();
            SqlDataAdapter da = new SqlDataAdapter();
            cmd.Connection = con;
            da.SelectCommand = cmd;

            DataSet ds = new DataSet();
            da.Fill(ds);
            CloseConnection();
            return ds.Tables[0];
        }

        /// <summary>
        /// แปลง Data Table เป็น Json
        /// Convert DataTable To JSON
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string Convert_DataTableToJSON_With_StringBuilder(DataTable dt)
        {
            var JSON_String = new StringBuilder();
            if (dt.Rows.Count > 0)
            {
                JSON_String.Append("[");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    JSON_String.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        if (j < dt.Columns.Count - 1)
                        {
                            JSON_String.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + "\"" + dt.Rows[i][j].ToString() + "\",");
                        }
                        else if (j == dt.Columns.Count - 1)
                        {
                            JSON_String.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":" + "\"" + dt.Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == dt.Rows.Count - 1)
                    {
                        JSON_String.Append("}");
                    }
                    else
                    {
                        JSON_String.Append("},");
                    }
                }
                JSON_String.Append("]");
            }
            return JSON_String.ToString();
        }

        private static string[] SqlServerTypes = { "bigint", "binary", "bit", "char", "date", "datetime", "datetime2", "datetimeoffset", "decimal", "filestream", "float", "geography", "geometry", "hierarchyid", "image", "int", "money", "nchar", "ntext", "numeric", "nvarchar", "real", "rowversion", "smalldatetime", "smallint", "smallmoney", "sql_variant", "text", "time", "timestamp", "tinyint", "uniqueidentifier", "varbinary", "varchar", "xml" };
        //private static Type[] CSharpTypes = { typeof(long), typeof(byte[]), typeof(bool), typeof(char), typeof(string), typeof(string), typeof(string), typeof(string)
        //        , typeof(decimal) ,typeof(byte[]) ,typeof(double) ,typeof(string) ,typeof(string) ,typeof(string),typeof(byte[]) ,typeof(int) ,typeof(decimal) ,typeof(string),typeof(string) 
        //        ,typeof(decimal) ,typeof(string) ,typeof(Single),typeof(byte[]) ,typeof(string) ,typeof(short) ,typeof(decimal) ,typeof(object),typeof(string) ,typeof(string) ,typeof(byte[]) ,typeof(byte) ,typeof(Guid),typeof(byte[]),typeof(string),typeof(string)  };

        private static Type[] CSharpTypes = { typeof(long), typeof(byte[]), typeof(bool), typeof(char), typeof(DateTime), typeof(DateTime), typeof(DateTime), typeof(DateTimeOffset)
                , typeof(decimal) ,typeof(byte[]) ,typeof(double) ,typeof(string) ,typeof(string) ,typeof(string),typeof(byte[]) ,typeof(int) ,typeof(decimal) ,typeof(string),typeof(string)
                ,typeof(decimal) ,typeof(string) ,typeof(Single),typeof(byte[]) ,typeof(DateTime) ,typeof(short) ,typeof(decimal) ,typeof(object),typeof(string) ,typeof(TimeSpan) ,typeof(byte[]) ,typeof(byte) ,typeof(Guid),typeof(byte[]),typeof(string),typeof(string)  };
        /// <summary>
        /// แปลง Data type จาก sql เป็น c#
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type ConvertSqlServerFormatToCSharp(string typeName)
        {
            var index = Array.IndexOf(SqlServerTypes, typeName);

            return index > -1
                ? CSharpTypes[index]
                : typeof(string);
        }

        /// <summary>
        /// แปลง Data type จาก c# เป็น sql
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static string ConvertCSharpFormatToSqlServer(string typeName)
        {
            var index = Array.IndexOf(CSharpTypes, typeName);

            return index > -1
                ? SqlServerTypes[index]
                : null;
        }
    }
}
