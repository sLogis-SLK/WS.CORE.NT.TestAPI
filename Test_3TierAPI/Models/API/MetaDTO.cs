using System.Diagnostics;

namespace Test_3TierAPI.Models.API
{
    public class MetaDTO
    {
        public string? JobUUID { get; set; } // 작업 UUID
        public string? ExecutionTime { get; set; } // API 처리 시간 (ms)
        public string? ServerTimeStamp { get; set; } // 응답 생성 시간 (UTC)
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
