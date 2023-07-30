using Ardalis.Result;

namespace AIaaS.Application.Interfaces
{
    public interface IWorkflowTemplateService
    {
        Result<string?> GetWorkflowSampleTemplate();
        Result<string?> GetWorkflowTemplate(string filePath, bool skipPreprocessing = false);
    }
}
