using AIaaS.Application.Common.Models;
using AIaaS.Application.Features.Workflows.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace AIaaS.Application.SignalR
{
    public class SignalRWorkflowRunHistoryHub : Hub<IWorkflowClient>
    {
        public override async Task OnConnectedAsync()
        {
            //await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");

            await Clients.All.ReceiveMessage($"{Context.ConnectionId} has joined");
        }
    }

    public interface IWorkflowClient
    {
        //Task ReceiveWorkflowRunHistoryUpdate(WorkflowRunHistoryDto workflowRunHistory);
        //Task ReceiveWorkflowNodeRunHistoryUpdate(WorkflowNodeRunHistoryDto workflowNodeRunHistoryDto);
        //Task ReceiveWorkflowNodeValidateUpdate(WorkflowNodeValidateChangeDto workflowNodeValidateChangeDto);
        Task ReceiveWorkflowUpdate(WebSocketMessage message);
        Task ReceiveMessage(string message);
    }
}
