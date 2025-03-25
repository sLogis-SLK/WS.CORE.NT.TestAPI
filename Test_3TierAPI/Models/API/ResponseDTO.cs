namespace Test_3TierAPI.Models.API
{
    /// <summary>
    /// ResponseDTO
    /// 응답  데이터를 담는 DTO
    /// Release 환경에서는 MetaDTO는 함께 반환되지 않고, 메시지 역시 간소화됨
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ResponseDTO<T>
    {
        public string? JobUUID { get; set; } // 작업 UUID
        public bool Success { get; set; } // 성공 여부
        public int StatusCode { get; set; } // HTTP 상태 코드
        public string? ProcedureName { get; set; } // 프로시저 이름
        public string? Message { get; set; } // 응답 메시지
        public T? Data { get; set; } // 응답 데이터
        public int TableCount { get; set; } // 응답 데이터 개수
        public MetaDTO? Meta { get; set; } // 응답 메타 정보
    }
}
