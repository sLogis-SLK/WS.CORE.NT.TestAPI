using LinqToDB.SchemaProvider;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using Test_3TierAPI.Infrastructure.DataBase;
using Test_3TierAPI.Models.DataBaseValid;

namespace Test_3TierAPI.Infrastructure.DataBaseValid
{
    /// <summary>
    /// 프로시저 파라미터 메타데이터 캐시 클래스
    /// 시스템 카탈로그에서 프로시저 파라미터 정보를 조회하고 캐싱
    /// modify_date 기반으로 캐시 갱신 판단
    /// </summary>
    public class ProcedureMetadataCache
    {
        private readonly IMemoryCache _cache;   // 추후 RateLimitMiddleware에 있는 캐시 관리랑 엮어서 하나의 서비스로 빼자.
        private readonly DatabaseTransactionManager _dbManager;
        private readonly ILogger<ProcedureMetadataCache> _logger;
        private readonly TimeSpan _fallbackCacheDuration = TimeSpan.FromDays(7);

        public ProcedureMetadataCache(IMemoryCache cache, DatabaseTransactionManager dbManager, ILogger<ProcedureMetadataCache> logger)
        {
            _cache = cache;
            _dbManager = dbManager;
            _logger = logger;
        }

        /// <summary>
        /// 프로시저 정보를 가져옴 (캐시 우선, 없거나 변경되었으면 DB 조회)
        /// 수정일자 확인 불가시 항상 DB에서 새로 로드하여 무결성 보장
        /// </summary>
        /// <param name="procedureName">프로시저 이름</param>
        /// <param name="dbType">데이터베이스 타입 (기본값: "02")</param>
        /// <returns>프로시저 정보 객체</returns>
        public async Task<DBProcedureInfo> GetProcedureInfoAsync(string procedureName, string dbType = "02")
        {
            string cacheKey = $"ProcInfo:{procedureName}";

            try
            {
                // 1. 현재 프로시저에 수정일자 조회
                DateTime? currentModifyDate = await GetProcedureLastModifiedDateAsync(procedureName, dbType);

                // 2. 수정일자 확인 가능 & 캐시가 있고 & 캐시가 최신인 경우에만 캐시 사용
                if (currentModifyDate.HasValue &&
                   _cache.TryGetValue(cacheKey, out DBProcedureInfo procInfo) &&
                   currentModifyDate <= procInfo.LastModified)
                {
                    _logger.LogDebug($"프로시저 '{procedureName}'의 캐시 사용 (수정 없음)");
                    procInfo.Source = "Cache";
                    return procInfo;
                }

                // 3. 그 외 모든 경우 - db에서 새로 로드
                // - 캐시 없음
                // - 수정일자 확인 불가
                // - 프로시저가 수정됨
                _logger.LogInformation(
                    currentModifyDate.HasValue
                        ? $"프로시저 '{procedureName}' 정보 새로 로드 (수정 또는 캐시 없음)"
                        : $"프로시저 '{procedureName}' 정보 새로 로드 (수정일자 확인 불가)");

                var newProcInfo = await LoadProcedureInfoAsync(procedureName, dbType);
                newProcInfo.Source = "Database";

                // 4. 캐시 업데이트
                _cache.Set(cacheKey, newProcInfo, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _fallbackCacheDuration    // 캐시 만료일 설정
                });

                return newProcInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"프로시저 '{procedureName}' 정보 조회 중 오류 발생");

                // 오류 발생 시 캐시에 있으면 캐시 사용 (일시적 DB 오류 대비)
                if (_cache.TryGetValue(cacheKey, out DBProcedureInfo cachedInfo))
                {
                    _logger.LogWarning($"DB 오류 발생. 일시적 대응으로 프로시저 '{procedureName}'의 캐시 사용");
                    cachedInfo.Source = "CacheFallback";
                    return cachedInfo;
                }

                return null;    // 추후 어떻게 처리할지 고민
            }
        }

        /// <summary>
        /// 프로시저 파라미터 캐시 수동 갱신
        /// </summary>
        public async Task<DBProcedureInfo> RefreshProcedureInfoAsync(string procedureName, string dbType)
        {
            string cacheKey = $"ProcInfo:{procedureName}";

            try
            {
                var procInfo = await LoadProcedureInfoAsync(procedureName, dbType);
                procInfo.Source = "ManualRefresh";

                _cache.Set(cacheKey, procInfo);
                _logger.LogInformation($"프로시저 '{procedureName}' 캐시 수동 갱신 완료");

                return procInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"프로시저 '{procedureName}' 캐시 수동 갱신 중 오류 발생");
                throw;
            }
        }

        /// <summary>
        /// 프로시저의 마지막 수정 날짜 조회
        /// </summary>
        private async Task<DateTime?> GetProcedureLastModifiedDateAsync(string procedureName, string dbType)
        {
            string query = @"
                SELECT modify_date 
                FROM sys.objects 
                WHERE name = @procedureName AND type = 'P'";

            try
            {
                DataTable result = await _dbManager.GetDataTableAsync(dbType, query, CommandType.Text, new { procedureName });

                if(result.Rows.Count > 0 && result.Rows[0]["modify_date"] != DBNull.Value)
                {
                    return Convert.ToDateTime(result.Rows[0]["modify_date"]);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"프로시저 '{procedureName}' 수정일자 조회 중 오류 발생");
                return null;
            }
        }

        /// <summary>
        /// 프로시저 정보 전체 로드 (파라미터 및 메타데이터)
        /// </summary>
        private async Task<DBProcedureInfo> LoadProcedureInfoAsync(string procedureName, string dbType)
        {
            var procInfo = new DBProcedureInfo
            {
                Name = procedureName,
                CachedTime = DateTime.Now,
                Parameters = await LoadParametersFromDatabaseAsync(procedureName, dbType)
            };

            // 프로시저 수정일자 로드
            DateTime? modifyDate = await GetProcedureLastModifiedDateAsync(procedureName, dbType);

            // 수정일자를 확인할 수 있는 경우에만 설정, 아니면 매우 과거 시간을 설정하여 
            // 다음 요청 시 수정일자를 확인할 수 있게 되면 무조건 갱신되도록 함
            procInfo.LastModified = modifyDate ?? DateTime.MinValue;

            return procInfo;
        }

        /// <summary>
        /// SQL Server 시스템 카탈로그에서 프로시저 파라미터 정보 로드
        /// </summary>
        private async Task<List<DBProcedureParameterInfo>> LoadParametersFromDatabaseAsync(string procedureName, string dbType)
        {
            string query = @"
                SELECT 
                    p.name AS ParameterName, 
                    t.name AS DataType,
                    p.max_length AS MaxLength,
                    p.precision AS Precision,
                    p.scale AS Scale,
                    p.has_default_value AS HasDefaultValue,
                    p.is_output AS IsOutput
                FROM sys.parameters p
                JOIN sys.types t ON p.system_type_id = t.system_type_id
                WHERE object_id = OBJECT_ID(@procedureName)
                ORDER BY p.parameter_id";

            DataTable result = await _dbManager.GetDataTableAsync(
                dbType,
                query,
                CommandType.Text,
                new { procedureName });

            var parameters = new List<DBProcedureParameterInfo>();

            foreach (DataRow row in result.Rows)
            {
                parameters.Add(new DBProcedureParameterInfo
                {
                    Name = row["ParameterName"].ToString(),
                    DataType = row["DataType"].ToString(),
                    MaxLength = row["MaxLength"] != DBNull.Value ? Convert.ToInt32(row["MaxLength"]) : null,
                    Precision = row["Precision"] != DBNull.Value ? Convert.ToInt32(row["Precision"]) : null,
                    Scale = row["Scale"] != DBNull.Value ? Convert.ToInt32(row["Scale"]) : null,
                    HasDefaultValue = Convert.ToBoolean(row["HasDefaultValue"]),
                    IsOutput = Convert.ToBoolean(row["IsOutput"])
                });
            }

            return parameters;
        }
    }
}
