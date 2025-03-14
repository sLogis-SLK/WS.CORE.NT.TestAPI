using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Test_3TierAPI.CustomAnnotation;
using Test_3TierAPI.Models.API;
using Test_3TierAPI.Services.공통;

namespace Test_3TierAPI.Controllers
{
    [ApiController]
    [Route("api/frmTRCOM00001")]
    [RequireData(true)]         // 데이터 검증 여부 확인을 위한 커스텀 어노테이션
    public class FrmTRCOM00001Controller : ControllerBase
    {
        private readonly ILogger<FrmTRCOM00001Controller> _logger;

        private readonly FrmTRCOM00001Service _frmTRCOM00001Service;

        public FrmTRCOM00001Controller(ILogger<FrmTRCOM00001Controller> logger, FrmTRCOM00001Service frmTRCOM00001Service)
        {
            _logger = logger;
            _frmTRCOM00001Service = frmTRCOM00001Service;
        }

        [HttpPost("looupBtn")]
        public async Task<IActionResult> LooupBtn([FromBody] RequestDTO<object> data)
        {
            var response = await _frmTRCOM00001Service.LooupBtn(data);

            var _meta = HttpContext.Items.TryGetValue("MetaDTO", out object? metaObj) && metaObj is MetaDTO meta
                ? meta
                : null;


            response.Meta = _meta;
            response.StatusCode = response.Meta.StatusCode = HttpContext.Response.StatusCode;

            // statusCode에 따라 적절한 HTTP 응답 반환
            return response.StatusCode switch
            {
                200 => Ok(response), // 정상 응답
                400 => BadRequest(response), // 잘못된 요청
                401 => Unauthorized(response), // 인증 오류
                403 => Forbid(), // 접근 금지
                404 => NotFound(response), // 데이터 없음
                500 => StatusCode(500, response), // 서버 오류
                _ => StatusCode(response.StatusCode, response) // 기타 상태 코드
            };
        }
    }
}
