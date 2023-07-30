using AIaaS.Application.Common.Models;
using AIaaS.Application.Interfaces;
using AIaaS.WebAPI.ExtensionMethods;
using Ardalis.Result;
using Microsoft.Extensions.Logging;

namespace AIaaS.Application.Services
{
    public class WorkflowTemplateService : IWorkflowTemplateService
    {
        private readonly ILogger<WorkflowTemplateService> _logger;

        public WorkflowTemplateService(ILogger<WorkflowTemplateService> logger)
        {
            _logger = logger;
        }

        public Result<string?> GetWorkflowSampleTemplate()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Common", "WorkflowTemplates", "WorkflowSampleTemplate.json");
            return GetWorkflowTemplate(filePath);
        }

        public Result<string?> GetWorkflowTemplate(string filePath, bool skipPreprocessing = false)
        {
            try
            {
                var template = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(template))
                {
                    return Result.Error($"Template '{filePath}' is empty");
                }

                if (skipPreprocessing)
                {
                    return template;
                }

                var workflowDto = new WorkflowDto();
                workflowDto.UpdateSerializedRoot(template);
                var workflowGraphDto = workflowDto.GetDeserializedWorkflowGraph();
                if (workflowGraphDto is null)
                {
                    return Result.Error("Deserialized workflow graph is null");
                }

                var nodes = workflowGraphDto.Root.ToList(generateGuidIfNotExist: true);
                if (!nodes.Any())
                {
                    return Result.Error("There are no nodes on the workflow");
                }

                workflowDto.UpdateSerializedRootFromGraph(workflowGraphDto);

                if (string.IsNullOrEmpty(workflowDto.Root))
                {
                    return Result.Error("Serialized workflow graph is null");
                }

                return Result.Success(workflowDto.Root);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error when trying to get sample workflow template";
                _logger.LogError(ex, errorMessage);
                return Result.Error(errorMessage);
            }
        }
    }
}
