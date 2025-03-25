using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using Azure.Core;

namespace Test_3TierAPI.Infrastructure.DataBase
{
    /// <summary>
    /// 데이터베이스 트랜잭션 관리자 클래스
    /// DI 관리와, 비동기 처리를 위해 DB 연결 및 트랜잭션 클래스를 새로 만듬
    /// connection은 DI를 통해 Singleton으로 관리되는 DBConnectionFactory로부터 가져옴
    /// </summary>
    public class DatabaseTransactionManager : IAsyncDisposable
    {
        private readonly DBConnectionFactory _connectionFactory;
        private DbConnection? _connection;
        private DbTransaction? _transaction;    // Update 함수에서만 사용
        private bool _isCommitted;
        private int _commandTimeout;

        public DatabaseTransactionManager(DBConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));   // 어차피 DI 관리
            _isCommitted = false;
            _commandTimeout = 30;   // 기본 타임아웃 시간 (30초)
        }

        /// <summary>
        /// CommandTimeout 설정 (초) : 기본값 30초
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
        /// 비동기 트랜잭션 시작. 이미 트랜잭션이 열려있으면 예외 발생
        /// Update() 함수 실행 전 필수 실행
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
        /// 비동기 단일 데이터 조회
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbType"></param>
        /// <param name="query"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<T?> GetSingleDataAsync<T>(string dbType, string query, CommandType commandType, object parameters = null)
        {
            EnsureParam(commandType, parameters);
            
            try
            {
                _connection = await _connectionFactory.GetConnectionAsync(dbType);
                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                using var command = await CreateCommandAsync(query, commandType, parameters, false);
                object? result = await command.ExecuteScalarAsync();
                return result == null || result == DBNull.Value ? default : (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                throw new Exception($"쿼리 실행 중 오류가 발생했습니다: {query}", ex);
            }
        }

        /// <summary>
        /// 비동기 데이터 테이블 조회
        /// </summary>
        /// <param name="dbType"></param>
        /// <param name="query"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<DataTable> GetDataTableAsync(string dbType, string query, CommandType commandType, object parameters = null)
        {
            EnsureParam(commandType, parameters);

            try
            {
                _connection = await _connectionFactory.GetConnectionAsync(dbType);
                if (_connection.State != ConnectionState.Open)
                    await _connection.OpenAsync();

                using var command = await CreateCommandAsync(query, commandType, parameters, false);
                await using var reader = await command.ExecuteReaderAsync();

                var dataTable = new DataTable();
                dataTable.Load(reader);

                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception($"쿼리 실행 중 오류가 발생했습니다: {query}", ex);
            }
        }

        /// <summary>
        /// 비동기 UPDATE, INSERT, DELETE 실행
        /// Transaction 필요
        /// BeginTransaction() 함수 실행 후 사용!!
        /// </summary>
        /// <param name="queryOrProcedure"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<int> ExecuteNonQueryAsync(string queryOrProcedure, CommandType commandType, object parameters = null)
        {
            EnsureTransaction();
            EnsureParam(commandType, parameters);

            try
            {
                using var command = await CreateCommandAsync(queryOrProcedure, commandType, parameters, true);
                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"쿼리 실행 중 오류가 발생했습니다: {queryOrProcedure}", ex);
            }
        }

        /// <summary>
        /// 비동기 UPDATE, INSERT, DELETE 실행 (Output Parameter 포함)
        /// Transaction 필요
        /// BeginTransaction() 함수 실행 후 사용!!
        /// </summary>
        /// <param name="queryOrProcedure"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <param name="outputParamName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<object?> ExecuteWithOutputParamAsync(string queryOrProcedure, CommandType commandType, object parameters, string outputParamName)
        {
            EnsureTransaction();
            EnsureParam(commandType, parameters);
            try
            {
                using var command = await CreateCommandAsync(queryOrProcedure, commandType, parameters, true);

                if (string.IsNullOrEmpty(outputParamName))
                    throw new ArgumentException("Output Parameter 이름이 올바르지 않습니다.", nameof(outputParamName));

                var outputParam = command.CreateParameter();
                outputParam.ParameterName = outputParamName;
                outputParam.Direction = ParameterDirection.Output;
                outputParam.DbType = DbType.String;
                command.Parameters.Add(outputParam);

                await command.ExecuteNonQueryAsync();

                return command.Parameters.Contains(outputParamName) ? command.Parameters[outputParamName].Value : DBNull.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"쿼리 실행 중 Output Parameter 처리 오류가 발생했습니다: {queryOrProcedure}", ex);
            }
        }

        /// <summary>
        /// 비동기 트랜잭션 커밋
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
        /// 비동기 트랜잭션 롤백
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
        /// 비동기 리소스 정리
        /// IAsyncDisposable에 의해 관리되므로, 명시적으로 사용할 필요 없음
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

        /// <summary>
        /// 익명 클래스 또는 Dictionary를 통해 받은 값을 프로시저에 맞게 파라미터로 변환
        /// key: 파라미터명, value: 파라미터값
        /// key에 자동으로 @를 붙여 파라미터로 사용
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameters"></param>
        //private static void AddParameters(DbCommand command, object parameters)
        //{
        //    if (parameters == null) return;

        //    if (parameters is Dictionary<string, object> paramDict)
        //    {
        //        foreach (KeyValuePair<string, object> kvp in paramDict)
        //        {
        //            DbParameter dbParam = command.CreateParameter();
        //            dbParam.ParameterName = "@" + kvp.Key;
        //            dbParam.Value = kvp.Value ?? DBNull.Value;
        //            command.Parameters.Add(dbParam);
        //        }
        //    }
        //    else
        //    {
        //        foreach (PropertyInfo? param in parameters.GetType().GetProperties())
        //        {
        //            DbParameter dbParam = command.CreateParameter();
        //            dbParam.ParameterName = "@" + param.Name;
        //            dbParam.Value = param.GetValue(parameters) ?? DBNull.Value;
        //            command.Parameters.Add(dbParam);
        //        }
        //    }
        //}

        //private static void AddParameters(DbCommand command, object parameters)
        //{
        //    if (parameters == null) return;

        //    // JObject 타입 처리 추가
        //    if (parameters is JObject jo)
        //    {
        //        foreach (var property in jo.Properties())
        //        {
        //            DbParameter dbParam = command.CreateParameter();
        //            dbParam.ParameterName = "@" + property.Name;
        //            dbParam.Value = property.Value.ToObject<object>() ?? DBNull.Value;
        //            command.Parameters.Add(dbParam);
        //        }
        //        return; // JObject 처리 후 종료
        //    }

        //    // Dictionary<string, object> 타입 체크
        //    if (parameters is Dictionary<string, object> paramDict)
        //    {
        //        foreach (KeyValuePair<string, object> kvp in paramDict)
        //        {
        //            DbParameter dbParam = command.CreateParameter();
        //            dbParam.ParameterName = "@" + kvp.Key;
        //            dbParam.Value = kvp.Value ?? DBNull.Value;
        //            command.Parameters.Add(dbParam);
        //        }
        //    }
        //    // IDictionary 인터페이스를 구현한 경우
        //    else if (parameters is IDictionary dictionary)
        //    {
        //        foreach (DictionaryEntry entry in dictionary)
        //        {
        //            DbParameter dbParam = command.CreateParameter();
        //            dbParam.ParameterName = "@" + entry.Key.ToString();
        //            dbParam.Value = entry.Value ?? DBNull.Value;
        //            command.Parameters.Add(dbParam);
        //        }
        //    }
        //    // dynamic 접근 시도
        //    else
        //    {
        //        try
        //        {
        //            // dynamic으로 변환하여 처리 시도
        //            dynamic dynamicParams = parameters;
        //            JObject jObject = JObject.FromObject(dynamicParams);

        //            foreach (var property in jObject.Properties())
        //            {
        //                DbParameter dbParam = command.CreateParameter();
        //                dbParam.ParameterName = "@" + property.Name;
        //                dbParam.Value = property.Value.ToObject<object>() ?? DBNull.Value;
        //                command.Parameters.Add(dbParam);
        //            }
        //            return; // dynamic 처리 성공하면 여기서 종료
        //        }
        //        catch (Exception ex)
        //        {
        //            // dynamic 접근 실패 시 JSON 변환 시도
        //            try
        //            {
        //                string json = JsonConvert.SerializeObject(parameters);
        //                var convertedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        //                if (convertedDict != null)
        //                {
        //                    foreach (KeyValuePair<string, object> kvp in convertedDict)
        //                    {
        //                        DbParameter dbParam = command.CreateParameter();
        //                        dbParam.ParameterName = "@" + kvp.Key;
        //                        dbParam.Value = kvp.Value ?? DBNull.Value;
        //                        command.Parameters.Add(dbParam);
        //                    }
        //                    return; // JSON 변환 성공하면 여기서 종료
        //                }
        //            }
        //            catch
        //            {
        //                // JSON 변환도 실패하면 리플렉션으로 폴백
        //            }

        //            // 기존 리플렉션 방식으로 폴백
        //            foreach (PropertyInfo? prop in parameters.GetType().GetProperties())
        //            {
        //                // 시스템 내부 속성이나 컬렉션 타입은 건너뛰기
        //                if (prop.PropertyType.Namespace?.StartsWith("System.Collections") == true)
        //                    continue;

        //                DbParameter dbParam = command.CreateParameter();
        //                dbParam.ParameterName = "@" + prop.Name;
        //                dbParam.Value = prop.GetValue(parameters) ?? DBNull.Value;
        //                command.Parameters.Add(dbParam);
        //            }
        //        }
        //    }
        //}
        private static void AddParameters(DbCommand command, object parameters)
        {
            if (parameters == null) return;

            // JObject 타입 처리
            if (parameters is JObject jObject)
            {
                AddParametersFromJObject(command, jObject);
                return;
            }

            // Dictionary<string, object> 타입 처리
            if (parameters is Dictionary<string, object> paramDict)
            {
                AddParametersFromDictionary(command, paramDict);
                return;
            }

            // IDictionary 인터페이스 처리
            if (parameters is IDictionary dictionary)
            {
                AddParametersFromIDictionary(command, dictionary);
                return;
            }

            // 위 타입들이 아닌 경우, 다양한 변환 시도
            try
            {
                // dynamic으로 처리 시도
                dynamic dynamicParams = parameters;
                JObject jObj = JObject.FromObject(dynamicParams);
                AddParametersFromJObject(command, jObj);
            }
            catch
            {
                try
                {
                    // JSON 직렬화/역직렬화를 통한 변환 시도
                    string json = JsonConvert.SerializeObject(parameters);
                    var convertedDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (convertedDict != null)
                    {
                        AddParametersFromDictionary(command, convertedDict);
                    }
                    else
                    {
                        // 모든 변환 실패 시 리플렉션 사용
                        AddParametersUsingReflection(command, parameters);
                    }
                }
                catch
                {
                    // JSON 변환 실패 시 리플렉션 사용
                    AddParametersUsingReflection(command, parameters);
                }
            }
        }

        // JObject에서 파라미터 추가
        private static void AddParametersFromJObject(DbCommand command, JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                AddParameter(command, property.Name, property.Value.ToObject<object>());
            }
        }

        // Dictionary에서 파라미터 추가
        private static void AddParametersFromDictionary(DbCommand command, Dictionary<string, object> paramDict)
        {
            foreach (var kvp in paramDict)
            {
                AddParameter(command, kvp.Key, kvp.Value);
            }
        }

        // IDictionary에서 파라미터 추가
        private static void AddParametersFromIDictionary(DbCommand command, IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                AddParameter(command, entry.Key.ToString(), entry.Value);
            }
        }

        // 리플렉션을 사용하여 파라미터 추가
        private static void AddParametersUsingReflection(DbCommand command, object parameters)
        {
            foreach (PropertyInfo prop in parameters.GetType().GetProperties())
            {
                // 시스템 내부 속성이나 컬렉션 타입은 건너뛰기
                if (prop.PropertyType.Namespace?.StartsWith("System.Collections") == true)
                    continue;

                AddParameter(command, prop.Name, prop.GetValue(parameters));
            }
        }

        // 단일 파라미터 추가 헬퍼 메서드
        private static void AddParameter(DbCommand command, string name, object value)
        {
            DbParameter dbParam = command.CreateParameter();
            dbParam.ParameterName = "@" + name;
            dbParam.Value = value ?? DBNull.Value;
            command.Parameters.Add(dbParam);
        }

        /// <summary>
        /// Command 객체 생성
        /// </summary>
        /// <param name="queryOrProcedure"></param>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <param name="useTransaction"></param>
        /// <returns></returns>
        private async Task<DbCommand> CreateCommandAsync(string queryOrProcedure, CommandType commandType, object parameters, bool useTransaction)
        {
            var command = _connection?.CreateCommand();
            command.CommandText = queryOrProcedure;
            command.CommandType = commandType;
            command.CommandTimeout = _commandTimeout;

            if (useTransaction)
            {
                EnsureTransaction();
                command.Transaction = _transaction;
            }

            AddParameters(command, parameters);
            return command;
        }

        /// <summary>
        /// Update 함수 실행 전 트랜잭션이 열려있는지 확인 없으면 예외 발생
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void EnsureTransaction()
        {
            if (_transaction == null)
                throw new InvalidOperationException("트랜잭션이 시작되지 않았습니다.");
        }

        /// <summary>
        /// Command Type이 Procedure일 때, 파라미터가 없으면 예외 발생
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="parameters"></param>
        /// <exception cref="Exception"></exception>
        private void EnsureParam(CommandType commandType, object parameters)
        {
            if (commandType == CommandType.StoredProcedure && parameters == null)
                throw new Exception("저장 프로시저는 파라미터가 필요합니다.");
        }
    }
}
