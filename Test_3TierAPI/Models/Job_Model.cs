namespace SLK_Model
{
    /// <summary>
    /// Jobs 테이블 엔티티
    /// </summary>
    public class Job_Model
    {
        public string JobId { get; set; }
        public string JobType { get; set; }
        public string RequestedUrl { get; set; }
        public string RequestedServer { get; set; }
        public string IdentifierType { get; set; }
        public int TotalCount { get; set; }
        public int TotalChunkCount { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// JobChunks 테이블 엔티티
    /// </summary>
    public class JobChunk_Model
    {
        public string ChunkId { get; set; }
        public string JobId { get; set; }
        public int TotalChunkCount { get; set; }
        public int ChunkIndex { get; set; }
        public string Identifier { get; set; }
        public int? ItemCount { get; set; }
        public int ProcessedItemCount { get; set; }
        public string Status { get; set; }
        public string HandlerName { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// JobFailures 테이블 엔티티
    /// </summary>
    public class JobFailure_Model
    {
        public string FailureId { get; set; }
        public string ChunkId { get; set; }
        public string JobId { get; set; }
        public string Identifier { get; set; }
        public string ErrorFrom { get; set; } // "Orchestration" or "Worker"
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; } // JSON
        public bool IsRetryable { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// JobProgress 뷰 엔티티
    /// </summary>
    public class Model_View_JobProgress
    {
        public string JobId { get; set; }
        public string JobType { get; set; }
        public string IdentifierType { get; set; }
        public string RequestedUrl { get; set; }
        public string RequestedServer { get; set; }
        public int TotalCount { get; set; }
        public int TotalChunkCount { get; set; }
        public int ProcessedCount { get; set; }
        public int FailedCount { get; set; }
        public string JobStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal ChunkProgressPercentage { get; set; }
        public decimal ItemProgressPercentage { get; set; }
        public int ActualChunkCount { get; set; }
        public int CompletedChunks { get; set; }
        public int FailedChunks { get; set; }
        public int ProcessingChunks { get; set; }
        public int PendingChunks { get; set; }
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public int TotalFailures { get; set; }
        public int RetryableFailures { get; set; }
        public decimal ProcessingRatePerMinute { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
    }
}
