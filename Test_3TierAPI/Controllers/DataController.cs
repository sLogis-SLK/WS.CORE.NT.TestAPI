using Microsoft.AspNetCore.Mvc;
using System.Data;
using Test_3TierAPI.CustomAttribute;
using Test_3TierAPI.CustomAttribute.ValidAttribute;
using Test_3TierAPI.Models.API;
using Test_3TierAPI.Services;

namespace Test_3TierAPI.Controllers
{
    [ApiController]
    [Route("api/data")]
    [RequireData(true)]
    public class DataController : ControllerBase
    {
        private readonly DataService _dataService;

        public DataController(DataService dataService)
        {
            _dataService = dataService;
        }


        [HttpPost("getdatatable")]
        public async Task<IActionResult> GetDataTable([FromBody] RequestDTO<object> data)
        {
            // 프로시저 이름 단위로 적절한 서비스 바인딩
            
            DataTable table = await _dataService.GetDataTable(data);

            // 프로시저 이름 단위로 valid 체크를 하자


            return Ok(table);
        }
    }
}
