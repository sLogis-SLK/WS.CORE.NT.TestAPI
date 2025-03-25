using System.Diagnostics;

namespace Test_3TierAPI.Models.API
{
    /// <summary>
    /// MetaDTO
    /// API 요청에 대한 메타 정보를 담는 DTO
    /// 추후 Mac Address 등 추가 필요할지도
    /// Release 환경에서는 MetaDTO는 함께 반환되지 않음
    /// </summary>
    public class MetaDTO
    {
        public string? JobUUID { get; set; } // 작업 UUID
        public string? ExecutionTime { get; set; } // API 처리 시간 (ms)
        public string? Procedurename { get; set; } // 프로시저 이름
        public int TableCount { get; set; } // 응답 데이터 개수
        public DateTime? ServerTimeStamp { get; set; } // 응답 생성 시간 (UTC)
        public DateTime SWRequestTimestamp { get; set; } // 소프트웨어 요청 시각
        public string? RequestIP { get; set; } // 요청 IP 주소
        public string? Requester { get; set; } // 요청자(소프트웨어 사용자 이름)
        public string? RequestURL { get; set; } // 요청 URL
        public string? TraceId { get; set; } // 분산 트랜잭션 추적 ID
        public int StatusCode { get; set; } // 오류 코드(에러 발생 시)
        public string? ErrorDetail { get; set; } // 오류 메시지(에러 발생 시)
        public int? RateLimitRemaining { get; set; } // 남은 요청 횟수
        public int? RateLimitMax { get; set; } // 최대 요청 횟수
        public string? CacheStatus { get; set; } // 캐시 상태 ("HIT" or "MISS")
        public string? DataSource { get; set; } // 데이터 소스 (DB, Cache, etc.)
    }
}
