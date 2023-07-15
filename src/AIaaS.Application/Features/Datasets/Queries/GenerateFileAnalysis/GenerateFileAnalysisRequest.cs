using AIaaS.Application.Common.Models.Dtos;
using AIaaS.Application.Features.Datasets.Queries.GenerateFileAnalysis;
using Ardalis.Result;
using MediatR;

namespace AIaaS.Application.Features.Datasets.Queries.GenerateFilePreview
{
    public class GenerateFileAnalysisRequest: IRequest<Result<FileAnalysisDto>>
    {
        public GenerateFileAnalysisRequest(GenerateFileAnalysisParameter generateFileAnalysisParameter)
        {
            GenerateFileAnalysisParameter = generateFileAnalysisParameter;
        }

        public GenerateFileAnalysisParameter GenerateFileAnalysisParameter { get; }
    }
}
