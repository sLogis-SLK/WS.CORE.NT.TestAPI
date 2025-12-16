using Microsoft.AspNetCore.Mvc;

namespace SLK.NT.Common.Model
{
    public class NTErrorResponse : ProblemDetails
    {
        public Guid? JobId { get; set; }
        public string? DomainType { get; set; }
        
    }
}
