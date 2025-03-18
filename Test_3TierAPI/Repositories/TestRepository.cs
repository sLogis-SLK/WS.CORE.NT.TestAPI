using System.Data;
using Test_3TierAPI.Infrastructure.DataBase;

namespace Test_3TierAPI.Repositories
{
    public class TestRepository
    {
        private readonly DatabaseTransactionManager _dbManager;

        public TestRepository(DatabaseTransactionManager dbManager)
        {
            _dbManager = dbManager;
        }

        // 원래는 RequestDTO 받아야 함
        public async Task<DataTable> Test()
        {
            string procedureName = "usp_온라인_처리이력_Get";
            object param = new
            {
                자체주문번호 = "1N22503100301550",
                중요전달사항만 = 1,
                조회구분자 = 0
            };

            DataTable response = await _dbManager.GetDataTableAsync("02", procedureName, param);

            // C#에서 @"" 사용 시 여러 줄 문자열을 깔끔하게 작성 가능.
            string query = @"
                SELECT * 
                FROM 온라인_처리이력 WITH (NOLOCK)
                WHERE 자체주문번호 = '1N22503100301550'
                ORDER BY 등록일시";

            DataTable response2 = await _dbManager.ExecuteQueryAsync<DataTable>("02", query);

            return response2;
        }

        public async Task<DataTable> Test2()
        {
            string query = @"
                SELECT *
                FROM 온라인_반품상세 가 WITH (NOLOCK)
                WHERE 가.등록일시 BETWEEN '20250316' AND '20250318'";
            DataTable response = await _dbManager.ExecuteQueryAsync<DataTable>("02", query);
            return response;
        }

        public async Task<DataTable> GetProcedureTest()
        {
            string procedureName = "usp_온라인_반품처리_물류용_Get";

            object param = new
            {
                B코드 = "SH",
                조회시작일 = "20250317",
                조회종료일 = "20250318",
                상태구분 = "%",
                승인요청여부 = "%",
                승인수신여부 = "%",
                조회구분자 = 0
            };

            DataTable response = await _dbManager.GetDataTableAsync("02", procedureName, param);
            return response;
        }
    }
}
