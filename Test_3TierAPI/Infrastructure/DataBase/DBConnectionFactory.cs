
using System.Data.Common;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Data.SqlClient;

namespace Test_3TierAPI.Infrastructure.DataBase
{
    /// <summary>
    /// DB Connection을 생성하는 Factory 클래스
    /// DI를 통해 Singleton으로 사용
    /// </summary>
    public class DBConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DBConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 특정 DB 유형(strType)에 따라 적절한 DB Connection을 반환 (비동기)
        /// </summary>
        public async Task<DbConnection> GetConnectionAsync(string strType)
        {
            string connectionString = GetConnectionString(strType);
            DbConnection connection = CreateDbConnection(strType, connectionString);

            // await connection.OpenAsync(); // 비동기 연결
            return connection;
        }

        /// <summary>
        /// 특정 DB 유형(strType)에 따라 적절한 ConnectionString을 가져옴
        /// </summary>
        private string GetConnectionString(string strType)
        {
            return strType switch
            {
                "01" => _configuration.GetConnectionString("OracleDB"),
                "02" => _configuration.GetConnectionString("SqlServerDB"),
                "03" => _configuration.GetConnectionString("InformixDB"),
                _ => throw new Exception("Not Defined Database Type")
            };
        }

        /// <summary>
        /// 특정 DB 유형(strType)에 따라 적절한 DbConnection을 생성
        /// </summary>
        private DbConnection CreateDbConnection(string strType, string connectionString)
        {
            return strType switch
            {
                "01" => new OracleConnection(connectionString),
                "02" => new SqlConnection(connectionString),
                //"03" => new IfxConnection(connectionString),
                _ => throw new Exception("Not Defined Database Type")
            };
        }
    }
}
