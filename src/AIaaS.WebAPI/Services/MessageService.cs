using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models.Dtos;

namespace AIaaS.WebAPI.Services;

public class MessageService : IMessageService
{
    public MessageDto GetAdminMessage()
    {
        return new MessageDto { text = "This is an admin message2." };
    }

    public MessageDto GetProtectedMessage()
    {
        return new MessageDto { text = "This is a protected message2." };
    }

    public MessageDto GetPublicMessage()
    {
        return new MessageDto { text = "This is a public message2." };
    }
}
