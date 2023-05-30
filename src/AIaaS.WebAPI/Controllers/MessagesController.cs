using AIaaS.WebAPI.Interfaces;
using AIaaS.WebAPI.Models.Dtos;
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
    [AllowAnonymous]
    public ActionResult<MessageDto> GetPublicMessage()
    {
        var id = User.Identity;
        return _messageService.GetPublicMessage();
    }

    [HttpGet("protected")]
    public ActionResult<MessageDto> GetProtectedMessage()
    {
        var ct = ClaimTypes.Role;
        var isPresent = User.FindFirst(ct);
        var claims = User.Claims;
        return _messageService.GetProtectedMessage();
    }

    [HttpGet("admin")]  
    [Authorize(Policy = "Administrator")]
    public ActionResult<MessageDto> GetAdminMessage()
    {
        var surname= ClaimTypes.Surname;
        var givenname= ClaimTypes.GivenName;
        var email = ClaimTypes.Email;
        return _messageService.GetAdminMessage();
    }

    [HttpGet("administrator")]
    //[Authorize(Roles = "Administrator")]
    [Authorize(Policy = "Administrator")]
    public ActionResult<MessageDto> GetAdminRoleMessage()
    {
        return _messageService.GetProtectedMessage();
    }
}
