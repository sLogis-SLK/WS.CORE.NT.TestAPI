using Microsoft.AspNetCore.Mvc;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Services.공통
{
    public class FrmTRCOM00001Service
    {
        private readonly ILogger<FrmTRCOM00001Service> _logger;
        public FrmTRCOM00001Service(ILogger<FrmTRCOM00001Service> logger)
        {
            _logger = logger;
        }

        public async Task<ResponseDTO<Dictionary<string, object>>> LooupBtn(RequestDTO<object> data)
        {
            // 예제 데이터 (단일 객체)
            var responseData = new Dictionary<string, object>
            {
                { "회원사코드", "1001" },
                { "그룹코드", "A001" },
                { "사용자명", "홍길동" },
                { "권한", "관리자" }
            };

            // ResponseDTO 생성
            var responseDto = new ResponseDTO<Dictionary<string, object>>
            {
                JobUUID = Guid.NewGuid().ToString(),
                Success = true,
                StatusCode = 200,                               // DB또는 다른 API 호출 결과에 따른 StatusCode 할당 필요
                Message = "요청이 정상적으로 처리되었습니다.",
                Data = responseData
            };

            return responseDto;
        }
    }
}
