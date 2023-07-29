using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows.Notifications;
using AIaaS.Application.Interfaces;
using AIaaS.Application.SignalR;
using AIaaS.Application.Specifications.Workflows;
using AIaaS.Domain.Entities;
using AIaaS.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AIaaS.WebAPI.SignalR
{
    public class WorkflowNodeRunHistoryChangeHandler : INotificationHandler<WorkflowNodeRunHistoryChangeNotification>
    {
        private readonly IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowClient> _context;
        private readonly IRepository<Workflow> _workflowRepository;
        private readonly IRepository<WorkflowRunHistory> _workflowRunHistoryRepository;
        private readonly IWorkflowRunHistoryContext _workflowRunHistoryContext;
        private readonly IMapper _mapper;

        public WorkflowNodeRunHistoryChangeHandler(
            IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowClient> context,
            IRepository<WorkflowRunHistory> workflowRunHistoryRepository,
            IWorkflowRunHistoryContext workflowRunHistoryContext,
            IRepository<Workflow> workflowRepository,
            IMapper mapper)
        {
            _workflowRunHistoryRepository = workflowRunHistoryRepository;
            _workflowRunHistoryContext = workflowRunHistoryContext;
            _workflowRepository = workflowRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task Handle(WorkflowNodeRunHistoryChangeNotification notification, CancellationToken cancellationToken)
        {        
            var mapped = _mapper.Map<WorkflowNodeRunHistoryDto>(notification.WorkflowNodeRunHistory);
            var message = WebSocketMessage.CreateMessage(mapped);
            await _context.Clients.All.ReceiveWorkflowUpdate(message);
        }
    }
}
