using AIaaS.WebAPI.Models.Dtos;

namespace AIaaS.WebAPI.Interfaces;

public interface IMessageService
{
    MessageDto GetPublicMessage();
    MessageDto GetProtectedMessage();
    MessageDto GetAdminMessage();
}
