using AIaaS.Application.Common.Models;
using Microsoft.AspNetCore.SignalR;

namespace AIaaS.Application.SignalR
{
    public class SignalRWorkflowRunHistoryHub : Hub<IWorkflowCLient>
    {
        public override async Task OnConnectedAsync()
        {
            //await Clients.All.SendAsync("ReceiveMessage", $"{Context.ConnectionId} has joined");

            await Clients.All.ReceiveMessage($"{Context.ConnectionId} has joined");
        }
    }

    public interface IWorkflowCLient
    {
        Task ReceiveWorkflowRunHistoryUpdate(WorkflowRunHistoryDto workflowRunHistory);
        Task ReceiveMessage(string message);
    }
}
