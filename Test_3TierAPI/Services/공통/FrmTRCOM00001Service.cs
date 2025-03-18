using Microsoft.AspNetCore.Mvc;
using System.Data;
using Test_3TierAPI.Models.API;
using Test_3TierAPI.Repositories;

namespace Test_3TierAPI.Services.공통
{
    public class FrmTRCOM00001Service
    {
        private readonly ILogger<FrmTRCOM00001Service> _logger;
        private readonly TestRepository _testRepository;    
        public FrmTRCOM00001Service(ILogger<FrmTRCOM00001Service> logger, TestRepository testRepository)
        {
            _logger = logger;
            _testRepository = testRepository;
        }

        // 서비스에서는 DB 조회결과만 반환
        public async Task<object> LooupBtn(RequestDTO<object> data)
        {
            DataTable table = await _testRepository.Test();
            // 예제 데이터 (단일 객체)
            //var responseData = new Dictionary<string, object>
            //{
            //    { "회원사코드", "1001" },
            //    { "그룹코드", "A001" },
            //    { "사용자명", "홍길동" },
            //    { "권한", "관리자" }
            //};

            return table;
        }


        public async Task<object> LooupBtn2(RequestDTO<object> data)
        {
            DataTable table = await _testRepository.Test2();
            return table;
        }

        public async Task<object> GetProcedureTest(RequestDTO<object> data)
        {
            DataTable table = await _testRepository.GetProcedureTest();
            return table;
        }
    }
}
