using System.Data;
using Test_3TierAPI.Infrastructure.DataBase;
using Test_3TierAPI.Models.API;

namespace Test_3TierAPI.Repositories
{
    public class DataRepository
    {
        private readonly DatabaseTransactionManager _dbManager;

        public DataRepository(DatabaseTransactionManager dbManager)
        {
            _dbManager = dbManager;
        }

        public async Task<DataTable> GetDataTable(RequestDTO<object> request)
        {
            return await _dbManager.GetDataTableAsync("02", request.ProcedureName, CommandType.StoredProcedure, request.Data);
        }
    }
}
