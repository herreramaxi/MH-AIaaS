using AIaaS.Domain.Enums;

namespace AIaaS.Application.Common.Models
{
    public class WebSocketMessage
    {
        public Guid MessageId { get; private set; }
        public WebSocketMessageType MessageType { get; private set; }
        public object? Payload { get; private set; }

        public static WebSocketMessage CreateMessage( WorkflowRunHistoryDto payload)
        {
            return new WebSocketMessage
            {
                MessageId = Guid.NewGuid(),
                MessageType = WebSocketMessageType.WorkflowRunHistory,
                Payload = payload
            };
        }

        public static WebSocketMessage CreateMessage(WorkflowNodeRunHistoryDto payload)
        {
            return new WebSocketMessage
            {
                MessageId = Guid.NewGuid(),
                MessageType = WebSocketMessageType.WorkflowNodeRunHistory,
                Payload = payload
            };
        }
    }
}
