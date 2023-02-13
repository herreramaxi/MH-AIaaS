using AIaaS.WebAPI.Models;

namespace AIaaS.WebAPI.Services;

public class MessageService : IMessageService
{
    public Message GetAdminMessage()
    {
        return new Message { text = "This is an admin message2." };
    }

    public Message GetProtectedMessage()
    {
        return new Message { text = "This is a protected message2." };
    }

    public Message GetPublicMessage()
    {
        return new Message { text = "This is a public message2." };
    }
}
