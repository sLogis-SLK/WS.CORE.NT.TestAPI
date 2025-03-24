using System.Data;
using Test_3TierAPI.Models.API;
using Test_3TierAPI.Repositories;

namespace Test_3TierAPI.Services
{
    public class DataService
    {
        private readonly ILogger<DataService> _logger;
        private readonly DataRepository _dataRepository;

        public DataService(ILogger<DataService> logger, DataRepository dataRepository)
        {
            _logger = logger;
            _dataRepository = dataRepository;
        }

        public async Task<DataTable> GetDataTable(RequestDTO<object> data)
        {
            return await _dataRepository.GetDataTable(data);
        }
    }    
}
