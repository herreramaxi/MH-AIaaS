using AIaaS.Application.Common.Models;
using AIaaS.Domain.Entities;
using AIaaS.WebAPI.CQRS.Handlers;
using AIaaS.WebAPI.Interfaces;
using Ardalis.Result;
using AutoMapper;
using CleanArchitecture.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace AIaaS.Application.Workflows.Commands.RunWorkflow
{
    public class RunWorkflowCommand : IRequest<Result<WorkflowDto>>
    {
        public RunWorkflowCommand(WorkflowDto workflowDto)
        {
            WorkflowDto = workflowDto;
        }

        public WorkflowDto WorkflowDto { get; }
    }

    public class RunWorkflowHandler : BaseWorkflowHandler, IRequestHandler<RunWorkflowCommand, Result<WorkflowDto>>
    {
        private IApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<RunWorkflowHandler> _logger;

        public RunWorkflowHandler(IEnumerable<IWorkflowOperator> workflowOperators
            , IApplicationDbContext dbContext, IMapper mapper, ILogger<RunWorkflowHandler> logger) : base(workflowOperators, dbContext)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<WorkflowDto>> Handle(RunWorkflowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //TODO: fix this, it is too heavy
                var workflow = await _dbContext.Workflows
                    .Include(x => x.MLModel)
                    .ThenInclude(x => x.ModelMetrics)
                    .Include(x => x.WorkflowDataViews)
                    .FirstOrDefaultAsync(w => w.Id == request.WorkflowDto.Id);

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

                var result = await this.Run(request.WorkflowDto, context);

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
