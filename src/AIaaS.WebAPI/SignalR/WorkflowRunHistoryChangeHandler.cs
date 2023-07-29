﻿using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows.Notifications;
using AIaaS.Application.SignalR;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace AIaaS.WebAPI.SignalR
{
    public class WorkflowRunHistoryChangeHandler : INotificationHandler<WorkflowRunHistoryChangeNotification>
    {
        private readonly IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowClient> _context; 
        private readonly IMapper _mapper;

        public WorkflowRunHistoryChangeHandler(
            IHubContext<SignalRWorkflowRunHistoryHub, IWorkflowClient> context,
            IMapper mapper)
        {
            
            _context = context;
            _mapper = mapper;
        }

        public async Task Handle(WorkflowRunHistoryChangeNotification notification, CancellationToken cancellationToken)
        {       
            var mapped = _mapper.Map<WorkflowRunHistoryDto>(notification.WorkflowRunHistory);
            var message = WebSocketMessage.CreateMessage(mapped);
            await _context.Clients.All.ReceiveWorkflowUpdate(message);
        }
    }
}
