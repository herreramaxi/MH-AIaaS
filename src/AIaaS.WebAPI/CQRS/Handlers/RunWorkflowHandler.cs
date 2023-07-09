using AIaaS.WebAPI.CQRS.Commands;
using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.ExtensionMethods;
using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models;
using AIaaS.WebAPI.Models.Dtos;
using Ardalis.Result;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using System.Text.Json;

namespace AIaaS.WebAPI.CQRS.Handlers
{
    public class RunWorkflowHandler : BaseWorkflowHandler, IRequestHandler<RunWorkflowCommand, Result<WorkflowDto>>
    {
        private EfContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public RunWorkflowHandler(IEnumerable<IWorkflowOperator> workflowOperators
            , EfContext dbContext, IMapper mapper, ILogger<RunWorkflowHandler> logger) : base(workflowOperators,  dbContext)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<WorkflowDto>> Handle(RunWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var workflowDto = request.WorkflowDto;
                var workflow = await _dbContext.Workflows
                    .Include(w => w.WorkflowDataViews)
                    .FirstOrDefaultAsync(w => w.Id == workflowDto.Id);

                if (workflow is null)
                {
                    return Result.NotFound("Workflow not found");
                }

                var context = new WorkflowContext()
                {
                    MLContext = new MLContext(seed: 0),
                    Workflow = workflow,
                    RunWorkflow = true
                };

                var result = await this.Run(workflowDto, context);

                if (!result.IsSuccess)
                {
                    return result;
                }

                workflow.Data = result.Value.Root;

                _dbContext.Workflows.Update(workflow);
                await _dbContext.SaveChangesAsync();

                var mapped = _mapper.Map<Workflow, WorkflowDto>(workflow);

                return Result.Success(mapped);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when running workflow", ex.Message);

                return Result.Error($"Error when running workflow: {ex.Message}");
            }

        }
    }
}
