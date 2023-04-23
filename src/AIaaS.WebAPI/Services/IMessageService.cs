using AIaaS.WebAPI.Models.Dtos;

namespace AIaaS.WebAPI.Services;

public interface IMessageService
{
    MessageDto GetPublicMessage();
    MessageDto GetProtectedMessage();
    MessageDto GetAdminMessage();
}
