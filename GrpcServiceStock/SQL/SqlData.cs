using Dapper;
using GrpcServiceStock.Common;
using GrpcServiceStock.Response;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace GrpcServiceStock.SQL
{
    public class SqlData
    {
        private static string _DefaultConnection = null;

        private static string DefaultConnection
        {
            get
            {
                if (_DefaultConnection == null)
                {
                    _DefaultConnection = ConfigurationHelper.DefaultConnection;
                }
                return _DefaultConnection;
            }
        }

        public static IDbConnection _db;

        public static void Int()
        {
            if (_db == null)
                _db = new SqlConnection(DefaultConnection);

            if (_db.State == ConnectionState.Closed || _db.State == null)
                _db.Open();
        }

        /// <summary>
        /// Lấy tất
        /// </summary>
        /// <returns></returns>
        public static List<SymbolQuote> GetAllPriceStock(DateTime date)
        {
            List<SymbolQuote> lst = new List<SymbolQuote>();

            try
            {
                string command = string.Format(@"SELECT [Symbol],[Date],[Open],[High],[Low],[Close],[Volume] FROM [PriceStockVn] WHERE [Date] >= '{0}' ORDER BY [Date],[Symbol]", date);
                using (IDataReader dataReader = _db.ExecuteReader(command))
                {
                    while (dataReader.Read())
                    {
                        SymbolQuote item = new SymbolQuote();
                        item.Symbol = dataReader["Symbol"].AsString();
                        item.Date = dataReader["Date"].AsDateTime();
                        item.Open = dataReader["Open"].AsInt();
                        item.High = dataReader["High"].AsInt();
                        item.Low = dataReader["Low"].AsInt();
                        item.Close = dataReader["Close"].AsInt();
                        item.Volume = dataReader["Volume"].AsLong();
                        lst.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                //FileHelper.WriteLog("CategoryRepository", ex);
            }
            return lst;
        }

        /// <summary>
        /// Lấy ngày có dữ liệu gần nhất
        /// </summary>
        /// <returns></returns>
        public static DateTime GetMaxDate()
        {
            var value = DateTime.MinValue;

            try
            {
                string command = "SELECT MAX([Date]) FROM [PriceStockVn]";
                string getValue = _db.ExecuteScalar<string>(command);
                if (getValue != null)
                {
                    value = DateTime.Parse(getValue);
                }
            }
            catch (Exception ex)
            {
                //FileHelper.WriteLog("CategoryRepository", ex);
            }
            return value;
        }

        public static void Insert(SymbolQuote quote)
        {
            try
            {
                // SQL Insert command
                string insertQuery = "INSERT INTO [PriceStockVn] ([Symbol],[Date],[Open],High,[Low],[Close],[Volume]) VALUES (@Symbol,@Date,@Open,@High,@Low,@Close,@Volume)";

                using (IDbCommand cmd = _db.CreateCommand())
                {
                    cmd.CommandText = insertQuery;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter("@Symbol", quote.Symbol));
                    cmd.Parameters.Add(new SqlParameter("@Date", quote.Date));
                    cmd.Parameters.Add(new SqlParameter("@Open", quote.Open));
                    cmd.Parameters.Add(new SqlParameter("@High", quote.High));
                    cmd.Parameters.Add(new SqlParameter("@Low", quote.Low));
                    cmd.Parameters.Add(new SqlParameter("@Close", quote.Close));
                    cmd.Parameters.Add(new SqlParameter("@Volume", quote.Volume));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
