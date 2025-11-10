//using Microsoft.AspNetCore.Mvc;
//using Test_3TierAPI.CustomAnnotation;
//using Test_3TierAPI.CustomAttribute;
//using Test_3TierAPI.CustomAttribute.ValidAttribute;
//using Test_3TierAPI.Models.API;
//using Test_3TierAPI.Services.공통;

//namespace Test_3TierAPI.Controllers
//{
//    [ApiController]
//    [Route("api/frmTRCOM00001")]
//    [RequireData(true)]         // 데이터 검증 여부 확인을 위한 커스텀 어노테이션
//    [FieldLength("B코드", maxLength: 10)]        // object안에 b코드가 무조건 있어야 하고, 그 길이는 10자리 이하여야함
//    [FieldEqual("B코드", targetValue: "SH")]    // object안에 b코드가 SH여야함, 위 길이제한과 함께 동시에 체크되어야 함
//    [FieldRequire("Price")]
//    [FieldRequire("ProductId")]
//    [FieldLength("ProductId", minLength: 3)]
//    //[RequiredField("address")]
//    public class FrmTRCOM00001Controller : ControllerBase
//    {
//        private readonly ILogger<FrmTRCOM00001Controller> _logger;

//        private readonly FrmTRCOM00001Service _frmTRCOM00001Service;

//        public FrmTRCOM00001Controller(ILogger<FrmTRCOM00001Controller> logger, FrmTRCOM00001Service frmTRCOM00001Service)
//        {
//            _logger = logger;
//            _frmTRCOM00001Service = frmTRCOM00001Service;
//        }

//        [HttpPost("looupBtn")]
//        [FieldEqual("B코드", targetValue:"JS")]
//        [FieldRequire("age")]
//        [FieldLength("ProductId", isRequired: false)]
//        [RemoveField("ProductId")]
//        [SkipFieldValid]
//        public async Task<IActionResult> LooupBtn([FromBody] RequestDTO<object> data)
//        {
//            var response = await _frmTRCOM00001Service.LooupBtn(data);

//            // controller단에서는 그저 DB 조회결과만 반환,
//            // ResponseDTO 생성은 ApiResponseFilter에서 처리
//            return Ok(response);
//        }

//        [HttpPost("looupBtn2")] // 얘는 컨트롤러에서 요구한 필드와 제약조건 모두가 다 필요함
//        [FieldRange("B코드", minValue:3, maxValue:10)]    // 컨트롤러의 제약조건을 모두 통과하고 또 object안에 b코드가 3~10자리여야함
//        [SkipFieldValid]
//        public async Task<IActionResult> LooupBtn2([FromBody] RequestDTO<object> data)
//        {
//            var response = await _frmTRCOM00001Service.LooupBtn2(data);

//            // controller단에서는 그저 DB 조회결과만 반환,
//            // ResponseDTO 생성은 ApiResponseFilter에서 처리
//            return Ok(response);
//        }

//        [HttpPost("getProcedureTest")]
        
//        [FieldEqual("B코드", targetValue: "JS")]    // object안에 b코드가 JS여야함,
//        [SkipFieldValid]
//        public async Task<IActionResult> GetProcedureTest([FromBody] RequestDTO<object> requestDto)
//        {
//            var response = await _frmTRCOM00001Service.GetProcedureTest(requestDto);
//            // controller단에서는 그저 DB 조회결과만 반환,
//            // ResponseDTO 생성은 ApiResponseFilter에서 처리
//            return Ok(response);
//        }
//    }
//}
