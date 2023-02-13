using AIaaS.WebAPI.Models;

namespace AIaaS.WebAPI.Services;

public interface IMessageService
{
    Message GetPublicMessage();
    Message GetProtectedMessage();
    Message GetAdminMessage();
}
