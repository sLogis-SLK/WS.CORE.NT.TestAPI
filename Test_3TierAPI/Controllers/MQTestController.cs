using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using Test_3TierAPI.CustomAttribute;
using Test_3TierAPI.Helpers;
using Test_3TierAPI.Infrastructure.DataBase;
using Test_3TierAPI.MassTransit.MQMessage;
using SLK.NT.Common.Model;
using Test_3TierAPI.Models.NTLogin;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Test_3TierAPI.Controllers
{
    [ApiController]
    [Route("api/mqtest")]
    [RequireData(false)]
    public class MQTestController : ControllerBase
    {
        private readonly ILogger<MQTestController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly DatabaseTransactionManager _dbManager;
        private readonly IHttpClientFactory _httpClientFactory; // HttpClient 대신 Factory 사용

        public MQTestController(
            ILogger<MQTestController> logger,
            IPublishEndpoint publishEndpoint,
            DatabaseTransactionManager databaseTransactionManager,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _dbManager = databaseTransactionManager;
            _httpClientFactory = httpClientFactory; // Factory 저장
        }

        //[HttpPost("getjobidstatus")]
        //public async Task<IActionResult> GetJobIdStatus([FromBody] string jobId)
        //{
        //    try
        //    {
        //        var encodedJobId = Uri.EscapeDataString(jobId);
        //        var requestUri = $"/api/orchestration/retrievestatus/jobid/{encodedJobId}";

        //        using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");
        //        using var response = await httpClient.GetAsync(requestUri);

        //        var contentString = await response.Content.ReadAsStringAsync();
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jobModel = JsonConvert.DeserializeObject<Job_Model>(contentString);

        //            if (jobModel == null)
        //            {
        //                _logger.LogWarning($"Job with ID {jobId} not found or deserialization failed.");
        //                throw new Exception("Job not found or deserialization failed");
        //            }

        //            return Ok(jobModel);
        //        }
        //        else
        //        {
        //            _logger.LogWarning($"Failed to retrieve job status for ID {jobId}. Status code: {response.StatusCode}");
        //            throw new HttpRequestException($"Failed to retrieve job status. Status code: {response.StatusCode}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while retrieving job status");
        //        throw new HttpRequestException("Failed to retrieve job status", ex);
        //    }
        //}

        //[HttpPost("getchunkidstatus")]
        //public async Task<IActionResult> GetChunkIdStatus([FromBody] string chunkId)
        //{
        //    try
        //    {
        //        var encodedChunkId = Uri.EscapeDataString(chunkId);
        //        var requestUri = $"/api/orchestration/retrievestatus/chunkid/{encodedChunkId}";
        //        using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");
        //        using var response = await httpClient.GetAsync(requestUri);
        //        var contentString = await response.Content.ReadAsStringAsync();
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jobChunkModel = JsonConvert.DeserializeObject<JobChunk_Model>(contentString);
        //            if (jobChunkModel == null)
        //            {
        //                _logger.LogWarning($"Chunk with ID {chunkId} not found or deserialization failed.");
        //                throw new Exception("Chunk not found or deserialization failed");
        //            }
        //            return Ok(jobChunkModel);
        //        }
        //        else
        //        {
        //            _logger.LogWarning($"Failed to retrieve chunk status for ID {chunkId}. Status code: {response.StatusCode}");
        //            throw new HttpRequestException($"Failed to retrieve chunk status. Status code: {response.StatusCode}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while retrieving chunk status");
        //        throw new HttpRequestException("Failed to retrieve chunk status", ex);
        //    }
        //}

        //[HttpPost("productsreqbulk")]
        //public async Task<IActionResult> ProductsReqBulk([FromBody] string message)
        //{
        //    // HttpClient를 using으로 관리하여 적절한 리소스 해제 보장
        //    using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");

        //    try
        //    {
        //        string filePath = "C:\\Users\\User\\OneDrive - (주)에스엘케이\\TECH팀 - General\\99. 기타\\풀필먼트 벤치마킹\\테스트 자료\\ProductsReqJson.txt";

        //        List<ProductsReqList_NT> orderList = ProductsJsonConverter.LoadAndConvert(filePath);

        //        //var dto = new DTO_APIGateway<List<Model_Online_Order_Master>>
        //        //{
        //        //    RequestId = Guid.NewGuid().ToString(),
        //        //    TimeAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        //        //    _requestData = orderList
        //        //};

        //        string jsonData = JsonConvert.SerializeObject(orderList);
        //        var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

        //        // HttpClient 호출 시 예외 처리 추가
        //        //HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/test/onlineorderbulk", jsonContent);
        //        HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/nttest/productreqbatch", jsonContent);      // Hangfire에서 호출하는 API

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            _logger.LogWarning($"API Gateway returned error status: {response.StatusCode}");
        //        }

        //        string result = await response.Content.ReadAsStringAsync();
        //        return Ok(result);
        //    }
        //    catch (HttpRequestException httpEx)
        //    {
        //        _logger.LogError(httpEx, "HTTP request failed in TestAPIGateway");
        //        return StatusCode(502, "External API communication failed");
        //    }
        //    catch (TaskCanceledException timeoutEx)
        //    {
        //        _logger.LogError(timeoutEx, "Request timeout in TestAPIGateway");
        //        return StatusCode(408, "Request timeout");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred in TestAPIGateway");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        [HttpPost("productsreq")]
        public async Task<IActionResult> ProductsReq([FromBody] string message)
        {
            // HttpClient를 using으로 관리하여 적절한 리소스 해제 보장
            using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");

            try
            {
                string filePath = "C:\\Users\\User\\OneDrive - (주)에스엘케이\\TECH팀 - General\\99. 기타\\풀필먼트 벤치마킹\\테스트 자료\\ProductsReqJson.txt";

                List<ProductsReqList_NT> orderList = ProductsJsonConverter.LoadAndConvert(filePath);

                //var dto = new DTO_APIGateway<List<Model_Online_Order_Master>>
                //{
                //    RequestId = Guid.NewGuid().ToString(),
                //    TimeAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    _requestData = orderList
                //};

                // 토큰 확인용 로그
                var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                _logger.LogInformation("Current auth header: {Auth}", authHeader?[..30] ?? "NONE");

                string jsonData = JsonConvert.SerializeObject(orderList);
                var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // HttpClient 호출 시 예외 처리 추가
                //HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/test/onlineorderbulk", jsonContent);
                HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/nt/productsreq/bypass", jsonContent);      // Hangfire에서 호출하는 API

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API Gateway returned error status: {response.StatusCode}");
                }

                string result = await response.Content.ReadAsStringAsync();
                return Ok(result);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed in TestAPIGateway");
                return StatusCode(502, "External API communication failed");
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Request timeout in TestAPIGateway");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TestAPIGateway");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("newonlineorder")]
        public async Task<IActionResult> NewOnlineOrder([FromBody] string message)
        {
            // HttpClient를 using으로 관리하여 적절한 리소스 해제 보장
            using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");

            try
            {
                //int randomNumber = new Random().Next(1, 1000) * 10; // 1부터 9999까지의 랜덤 숫자 생성
                int randomNumber = 20;

                string query = $@"
                            SELECT TOP({randomNumber}) *
                            FROM 온라인_주문마스터 WITH (NOLOCK)
                            WHERE 센터코드 = 'TLK09'
                            AND B코드 = 'HB'
                            AND 작업일자 BETWEEN '20250101' AND '20250131'";

                DataTable returnTable = await _dbManager.GetDataTableAsync("02", query, CommandType.Text, null);
                List<OnlineOrderMaster_Model> orderList = DataTableMapper.MapToList<OnlineOrderMaster_Model>(returnTable);

                //var dto = new DTO_APIGateway<List<Model_Online_Order_Master>>
                //{
                //    RequestId = Guid.NewGuid().ToString(),
                //    TimeAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    _requestData = orderList
                //};

                string jsonData = JsonConvert.SerializeObject(orderList);
                var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // HttpClient 호출 시 예외 처리 추가
                //HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/test/onlineorderbulk", jsonContent);
                HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/nt/onlineorderdev/batch/identifier", jsonContent);      // Hangfire에서 호출하는 API

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API Gateway returned error status: {response.StatusCode}");
                }

                string result = await response.Content.ReadAsStringAsync();
                return Ok(result);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed in TestAPIGateway");
                return StatusCode(502, "External API communication failed");
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Request timeout in TestAPIGateway");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TestAPIGateway");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("newonlineorderchunkcount")]
        public async Task<IActionResult> NewOnlineOrderChunkCount([FromBody] string message)
        {
            // HttpClient를 using으로 관리하여 적절한 리소스 해제 보장
            using var httpClient = _httpClientFactory.CreateClient("TestAPIGateway");

            try
            {
                //int randomNumber = new Random().Next(1, 1000) * 10; // 1부터 9999까지의 랜덤 숫자 생성
                int randomNumber = 1000;

                string query = $@"
                            SELECT TOP({randomNumber}) *
                            FROM 온라인_주문마스터 WITH (NOLOCK)
                            WHERE 센터코드 = 'TLK09'
                            AND B코드 = 'HB'
                            AND 작업일자 BETWEEN '20250101' AND '20250131'";

                DataTable returnTable = await _dbManager.GetDataTableAsync("02", query, CommandType.Text, null);
                List<OnlineOrderMaster_Model> orderList = DataTableMapper.MapToList<OnlineOrderMaster_Model>(returnTable);

                //var dto = new DTO_APIGateway<List<Model_Online_Order_Master>>
                //{
                //    RequestId = Guid.NewGuid().ToString(),
                //    TimeAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                //    _requestData = orderList
                //};

                string jsonData = JsonConvert.SerializeObject(orderList);
                var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // HttpClient 호출 시 예외 처리 추가
                //HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/test/onlineorderbulk", jsonContent);
                HttpResponseMessage response = await httpClient.PostAsync("/api/orchestration/nt/onlineorderdev/batch/count", jsonContent);      // Hangfire에서 호출하는 API

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API Gateway returned error status: {response.StatusCode}");
                }

                string result = await response.Content.ReadAsStringAsync();
                return Ok(result);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request failed in TestAPIGateway");
                return StatusCode(502, "External API communication failed");
            }
            catch (TaskCanceledException timeoutEx)
            {
                _logger.LogError(timeoutEx, "Request timeout in TestAPIGateway");
                return StatusCode(408, "Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TestAPIGateway");
                return StatusCode(500, "Internal server error");
            }
        }

        // ========================================================================================
        // 1) GET Job by JobId
        // ========================================================================================
        [HttpPost("job")]
        public async Task<IActionResult> GetJob([FromBody] string jobId)
        {
            try
            {
                var encodedJobId = Uri.EscapeDataString(jobId);
                var requestUri = $"/api/orchestration/nt/status/job/{encodedJobId}";

                using var client = _httpClientFactory.CreateClient("TestAPIGateway");
                using var response = await client.GetAsync(requestUri);

                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving job");
                return StatusCode(500, "Internal server error");
            }
        }

        // ========================================================================================
        // 2) GET Chunk by ChunkId
        // ========================================================================================
        [HttpPost("chunk")]
        public async Task<IActionResult> GetChunk([FromBody] string chunkId)
        {
            try
            {
                var encodedChunkId = Uri.EscapeDataString(chunkId);
                var requestUri = $"/api/orchestration/nt/status/chunk/{encodedChunkId}";

                using var client = _httpClientFactory.CreateClient("TestAPIGateway");
                using var response = await client.GetAsync(requestUri);

                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chunk");
                return StatusCode(500, "Internal server error");
            }
        }

        // ========================================================================================
        // 3) GET Chunks by JobId
        // ========================================================================================
        [HttpPost("chunks/job")]
        public async Task<IActionResult> GetChunksByJob([FromBody] string jobId)
        {
            try
            {
                var encodedJobId = Uri.EscapeDataString(jobId);
                var requestUri = $"/api/orchestration/nt/status/job/{encodedJobId}/chunks";

                using var client = _httpClientFactory.CreateClient("TestAPIGateway");
                using var response = await client.GetAsync(requestUri);

                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving chunk list by job");
                return StatusCode(500, "Internal server error");
            }
        }

        //// ========================================================================================
        //// 4) GET Chunk by JobId + Identifier
        //// ========================================================================================
        //public class JobIdentifierRequest
        //{
        //    public string JobId { get; set; } = default!;
        //    public string Identifier { get; set; } = default!;
        //}

        //[HttpPost("chunk-by-identifier")]
        //public async Task<IActionResult> GetChunkByJobAndIdentifier([FromBody] JobIdentifierRequest req)
        //{
        //    try
        //    {
        //        var encodedIdentifier = Uri.EscapeDataString(req.Identifier);
        //        var encodedJobId = Uri.EscapeDataString(req.JobId);

        //        var requestUri =
        //            $"/api/orchestration/nt/status/chunk?jobId={encodedJobId}&identifier={encodedIdentifier}";

        //        using var client = _httpClientFactory.CreateClient("TestAPIGateway");
        //        using var response = await client.GetAsync(requestUri);

        //        var content = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, content);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving chunk by jobId + identifier");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //// ========================================================================================
        //// 5) DomainType + Identifier 검색
        //// ========================================================================================
        //public class DomainSearchRequest
        //{
        //    public string DomainType { get; set; } = default!;
        //    public string Identifier { get; set; } = default!;
        //}

        //[HttpPost("chunks/search/domain")]
        //public async Task<IActionResult> SearchChunksByDomain([FromBody] DomainSearchRequest req)
        //{
        //    try
        //    {
        //        var requestUri =
        //            $"/api/orchestration/nt/status/chunks/search/domain" +
        //            $"?domainType={Uri.EscapeDataString(req.DomainType)}" +
        //            $"&identifier={Uri.EscapeDataString(req.Identifier)}";

        //        using var client = _httpClientFactory.CreateClient("TestAPIGateway");
        //        using var response = await client.GetAsync(requestUri);

        //        var content = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, content);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error searching chunks by domain");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //// ========================================================================================
        //// 6) Identifier 검색
        //// ========================================================================================
        //[HttpPost("chunks/search/identifier")]
        //public async Task<IActionResult> SearchChunksByIdentifier([FromBody] string identifier)
        //{
        //    try
        //    {
        //        var requestUri =
        //            $"/api/orchestration/nt/status/chunks/search/identifier" +
        //            $"?identifier={Uri.EscapeDataString(identifier)}";

        //        using var client = _httpClientFactory.CreateClient("TestAPIGateway");
        //        using var response = await client.GetAsync(requestUri);

        //        var content = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, content);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error searching chunks by identifier");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //// ========================================================================================
        //// 7) Job + Status 검색
        //// ========================================================================================
        //public class JobStatusRequest
        //{
        //    public string JobId { get; set; } = default!;
        //    public string Status { get; set; } = default!;
        //}

        //[HttpPost("chunks/search/job-status")]
        //public async Task<IActionResult> SearchChunksByJobStatus([FromBody] JobStatusRequest req)
        //{
        //    try
        //    {
        //        var requestUri =
        //            $"/api/orchestration/nt/status/chunks/search/job-status" +
        //            $"?jobId={Uri.EscapeDataString(req.JobId)}" +
        //            $"&status={Uri.EscapeDataString(req.Status)}";

        //        using var client = _httpClientFactory.CreateClient("TestAPIGateway");
        //        using var response = await client.GetAsync(requestUri);

        //        var content = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, content);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error searching chunks by job + status");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        //// ========================================================================================
        //// 8) Status + CreatedAt 검색
        //// ========================================================================================
        //public class StatusCreatedRequest
        //{
        //    public string Status { get; set; } = default!;
        //    public DateTime CreatedFrom { get; set; }
        //    public DateTime CreatedTo { get; set; }
        //}

        //[HttpPost("chunks/search/status-created")]
        //public async Task<IActionResult> SearchChunksByStatusCreated([FromBody] StatusCreatedRequest req)
        //{
        //    try
        //    {
        //        var requestUri =
        //            $"/api/orchestration/nt/status/chunks/search/status-created" +
        //            $"?status={Uri.EscapeDataString(req.Status)}" +
        //            $"&createdFrom={HttpUtility.UrlEncode(req.CreatedFrom.ToString("o"))}" +
        //            $"&createdTo={HttpUtility.UrlEncode(req.CreatedTo.ToString("o"))}";

        //        using var client = _httpClientFactory.CreateClient("TestAPIGateway");
        //        using var response = await client.GetAsync(requestUri);

        //        var content = await response.Content.ReadAsStringAsync();
        //        return StatusCode((int)response.StatusCode, content);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error searching chunks by status + createdAt");
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        [HttpPost("login-test")]
        [AllowAnonymous]
        public async Task<IActionResult> NTLoginTest([FromBody] NTLoginTestRequest request)
        {
            using var client = _httpClientFactory.CreateClient("TestAPIGateway");

            var requestUri = "/api/orchestration/api/ntauth/login";

            var payload = new
            {
                userName = request.UserName,
                password = request.Password
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync(requestUri, content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, body);
            }

            var loginResponse = JsonConvert.DeserializeObject<NTLoginResponse>(body);

            if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.AccessToken))
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    "Invalid login response from Auth API");
            }

            return Ok(new
            {
                accessToken = loginResponse.AccessToken,
                tokenType = "Bearer",
                expiresAtUtc = loginResponse.ExpiresAtUtc,
                user = new
                {
                    id = loginResponse.UserId,
                    userName = loginResponse.UserName,
                    roles = loginResponse.Roles,
                    permissions = loginResponse.Permissions
                }
            });
        }


        [HttpPost("register-test")]
        public async Task<IActionResult> NTRegisterTest([FromBody] NTRegisterTestRequest request)
        {
            using var client = _httpClientFactory.CreateClient("TestAPIGateway");

            try
            {
                // ===============================
                // Auth API register endpoint
                // ===============================
                var requestUri = "/api/orchestration/api/ntauth/register";

                var registerPayload = new
                {
                    userName = request.UserName, // 의미: Email
                    password = request.Password
                };

                var json = JsonConvert.SerializeObject(registerPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // ===============================
                // Call Auth API
                // ===============================
                using var response = await client.PostAsync(requestUri, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "NTRegisterTest failed. StatusCode={StatusCode}, Body={Body}",
                        response.StatusCode,
                        responseBody);

                    return StatusCode((int)response.StatusCode, responseBody);
                }

                _logger.LogInformation(
                    "NTRegisterTest success for UserName={UserName}",
                    request.UserName);

                return Ok("Register test succeeded.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during NTRegisterTest");
                return StatusCode(502, "Auth API communication failed");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout occurred during NTRegisterTest");
                return StatusCode(408, "Auth API request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during NTRegisterTest");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}