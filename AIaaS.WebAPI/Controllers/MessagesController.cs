using AIaaS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIaaS.WebAPI.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("public")]
    public ActionResult<Message> GetPublicMessage()
    {
        var id = User.Identity;
        return _messageService.GetPublicMessage();
    }

    [HttpGet("protected")]
    [Authorize]
    public ActionResult<Message> GetProtectedMessage()
    {
        var ct = ClaimTypes.Role;
        var isPresent = User.FindFirst(ct);
        var claims = User.Claims;
        return _messageService.GetProtectedMessage();
    }

    [HttpGet("admin")]
    //[Authorize]
    [Authorize(Roles = "Administrator")]
    public ActionResult<Message> GetAdminMessage()
    {
        return _messageService.GetAdminMessage();
    }

    [HttpGet("administrator")]
    [Authorize(Roles = "Administrator")]
    public ActionResult<Message> GetAdminRoleMessage()
    {
        return _messageService.GetProtectedMessage();
    }
}
