using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using static Azure.Core.HttpHeader;

namespace Test_3TierAPI.Infrastructure.DataBase
{
    public class DatabaseTransactionManager : IAsyncDisposable
    {
        private readonly DBConnectionFactory _connectionFactory;
        private DbConnection? _connection;
        private DbTransaction? _transaction;
        private bool _isCommitted;
        private int _commandTimeout = 30; // 기본 타임아웃 (초 단위)

        public DatabaseTransactionManager(DBConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        /// <summary>
        /// CommandTimeout 설정 (초 단위)
        /// 기본 값 : 30초
        /// </summary>
        /// <param name="timeoutSeconds"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SetCommandTimeout(int timeoutSeconds)
        {
            if (timeoutSeconds <= 0)
                throw new ArgumentException("CommandTimeout 값은 0보다 커야 합니다.");
            _commandTimeout = timeoutSeconds;
        }

        /// <summary>
        /// 비동기로 트랜잭션 시작
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task BeginTransactionAsync(string dbType)
        {
            if (_transaction != null)
                throw new InvalidOperationException("이미 열린 트랜잭션이 있습니다. Commit 또는 Rollback을 먼저 호출하세요.");

            try
            {
                _connection = await _connectionFactory.GetConnectionAsync(dbType);
                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                _transaction = await _connection.BeginTransactionAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("트랜잭션 시작 중 오류가 발생했습니다.", ex);
            }
        }

        /// <summary>
        /// 비동기로 쿼리 실행 후 결과 반환 (SELECT)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbType"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<T> ExecuteQueryAsync<T>(string dbType, string query, object parameters = null)
        {
            try
            {
                await using var connection = await _connectionFactory.GetConnectionAsync(dbType);

                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = _commandTimeout;

                AddParameters(command, parameters);

                if (typeof(T) == typeof(DataTable))
                {
                    var dataTable = new DataTable();
                    await using var reader = await command.ExecuteReaderAsync();
                    dataTable.Load(reader);
                    return (T)(object)dataTable;
                }
                else
                {
                    var result = await command.ExecuteScalarAsync();
                    return result == null || result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"쿼리 실행 중 오류가 발생했습니다: {query}", ex);
            }
        }

        /// <summary>
        /// 비동기로 Stored Procedure 실행 후 DataTable 반환 (SELECT)
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<DataTable> GetDataTableAsync(string dbType, string storedProcedure, object parameters = null)
        {
            try
            {
                await using var connection = await _connectionFactory.GetConnectionAsync(dbType);

                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = storedProcedure;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = _commandTimeout;

                AddParameters(command, parameters);

                using var reader = await command.ExecuteReaderAsync();
                var dataTable = new DataTable();
                dataTable.Load(reader);

                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception($"Stored Procedure '{storedProcedure}' 실행 중 오류가 발생했습니다.", ex);
            }
        }

        /// <summary>
        /// 비동기로 Stored Procedure 실행 후 영향 받은 행 수 반환 (INSERT, UPDATE, DELETE)
        /// Transaction을 활용하는 객체는 dbType을 BeginTransactionAsync()로 설정한 DB 타입으로 사용
        /// 즉 함수 사용 전 꼭 BeginTransactionAsync()를 호출해야 함
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<int> ExecuteNonQueryAsync(string storedProcedure, object parameters = null)
        {
            if (_transaction == null)
                throw new InvalidOperationException("트랜잭션이 시작되지 않았습니다.");

            try
            {
                using var command = _connection.CreateCommand();
                command.Transaction = _transaction;
                command.CommandText = storedProcedure;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = _commandTimeout;

                AddParameters(command, parameters);
                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Stored Procedure '{storedProcedure}' 실행 중 오류가 발생했습니다.", ex);
            }
        }

        /// <summary>
        /// 비동기로 Stored Procedure 실행 후 Output Parameter 값을 반환 (OUTPUT)
        /// transaction을 활용하는 객체는 dbType을 BeginTransactionAsync()로 설정한 DB 타입으로 사용
        /// 즉 함수 사용 전 꼭 BeginTransactionAsync()를 호출해야 함
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <param name="parameters"></param>
        /// <param name="outputParamName"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<object> ExecuteWithOutputParamAsync(string storedProcedure, object parameters, string outputParamName)
        {
            if (_transaction == null)
                throw new InvalidOperationException("트랜잭션이 시작되지 않았습니다.");

            try
            {
                using var command = _connection.CreateCommand();
                command.Transaction = _transaction;
                command.CommandText = storedProcedure;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = _commandTimeout;

                AddParameters(command, parameters);

                var outputParam = command.CreateParameter();
                outputParam.ParameterName = outputParamName;
                outputParam.Direction = ParameterDirection.Output;
                outputParam.DbType = DbType.String;
                command.Parameters.Add(outputParam);

                await command.ExecuteNonQueryAsync();
                return outputParam.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Stored Procedure '{storedProcedure}' 실행 중 Output Parameter 처리 오류가 발생했습니다.", ex);
            }
        }

        #region 비동기 아웃풋 반환 함수 사용 예제
        // 위 비동기 아웃풋 반환 함수를 사용하는 예제
        //public async Task<string> 온라인주문정보_상태처리_SetAsync(DataTable 연계대상데이터Table, string 작업구분)
        //{
        //    using var transactionManager = new DatabaseTransactionManager(_connectionFactory);

        //    await transactionManager.BeginTransactionAsync("MSSQL"); // 트랜잭션 시작

        //    var parameters = new Dictionary<string, object>
        //        {
        //            { "@전송대상데이터", 연계대상데이터Table },
        //            { "@B코드", Common.PermitInfo.B코드 },
        //            { "@센터코드", Common.PermitInfo.센터코드 },
        //            { "@등록자", Common.PermitInfo.사원이름 },
        //            { "@작업구분", 작업구분 },
        //            { "@대상건수", 연계대상데이터Table.Rows.Count }
        //        };

        //    object result = await transactionManager.ExecuteWithOutputParamAsync(
        //        "usp_SWE_온라인주문정보_상태처리_Set",
        //        parameters,
        //        "@작업로우수"
        //    );

        //    await transactionManager.CommitAsync(); // 트랜잭션 커밋

        //    return result?.ToString();
        //}
        #endregion

        /// <summary>
        /// 비동기로 트랜잭션 커밋
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task CommitAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    _isCommitted = true;
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("트랜잭션 커밋 중 오류가 발생했습니다.", ex);
            }
        }

        /// <summary>
        /// 비동기로 트랜잭션 롤백
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("트랜잭션 롤백 중 오류가 발생했습니다.", ex);
            }
        }

        /// <summary>
        /// 비동기로 리소스 정리
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!_isCommitted && _transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
                if (_connection != null)
                {
                    await _connection.CloseAsync();
                    await _connection.DisposeAsync();
                    _connection = null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("리소스 정리 중 오류가 발생했습니다.", ex);
            }
        }

        private static void AddParameters(DbCommand command, object parameters)
        {
            if (parameters == null) return;

            if (parameters is Dictionary<string, object> paramDict)
            {
                foreach (var kvp in paramDict)
                {
                    var dbParam = command.CreateParameter();
                    dbParam.ParameterName = "@" + kvp.Key;
                    dbParam.Value = kvp.Value ?? DBNull.Value;
                    command.Parameters.Add(dbParam);
                }
            }
            else
            {
                foreach (var param in parameters.GetType().GetProperties())
                {
                    var dbParam = command.CreateParameter();
                    dbParam.ParameterName = "@" + param.Name;
                    dbParam.Value = param.GetValue(parameters) ?? DBNull.Value;
                    command.Parameters.Add(dbParam);
                }
            }
        }
    }
}
