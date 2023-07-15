using Microsoft.AspNetCore.Http;

namespace AIaaS.Application.Features.Datasets.Queries.GenerateFileAnalysis
{
    public class GenerateFileAnalysisParameter
    {
        public IFormFile? File { get; set; }
        public string? Delimiter { get; set; }
        public bool? MissingRealsAsNaNs { get; set; }
    }
}
