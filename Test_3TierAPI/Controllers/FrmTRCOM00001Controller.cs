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

            // controller단에서는 그저 DB 조회결과만 반환,
            // ResponseDTO 생성은 ApiResponseFilter에서 처리
            return Ok(response);
        }

        [HttpPost("looupBtn2")]
        public async Task<IActionResult> LooupBtn2([FromBody] RequestDTO<object> data)
        {
            var response = await _frmTRCOM00001Service.LooupBtn2(data);

            // controller단에서는 그저 DB 조회결과만 반환,
            // ResponseDTO 생성은 ApiResponseFilter에서 처리
            return Ok(response);
        }

        [HttpPost("getProcedureTest")]
        public async Task<IActionResult> GetProcedureTest([FromBody] RequestDTO<object> data)
        {
            var response = await _frmTRCOM00001Service.GetProcedureTest(data);
            // controller단에서는 그저 DB 조회결과만 반환,
            // ResponseDTO 생성은 ApiResponseFilter에서 처리
            return Ok(response);
        }
    }
}
