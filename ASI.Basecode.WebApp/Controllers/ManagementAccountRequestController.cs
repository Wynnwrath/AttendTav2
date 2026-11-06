using System.Threading;
using System.Threading.Tasks;
using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.WebApp.Controllers;

/// <summary>
/// Management-only decisions for pending account requests. Queue presentation is owned by its caller.
/// </summary>
[Authorize(Policy = Policies.Management)]
[AutoValidateAntiforgeryToken]
public sealed class ManagementAccountRequestController : ControllerBase<ManagementAccountRequestController>
{
    private readonly IAccountRequestService _accountRequestService;

    public ManagementAccountRequestController(
        IHttpContextAccessor httpContextAccessor,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        IMapper mapper,
        IAccountRequestService accountRequestService)
        : base(httpContextAccessor, loggerFactory, configuration, mapper)
    {
        _accountRequestService = accountRequestService;
    }

    /// <summary>Approves a pending request and returns its decision outcome.</summary>
    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            await _accountRequestService.ApproveAsync(id, GetActorUserId(), HttpContext.RequestAborted);
            return Ok(new { outcome = "Approved", message = "Account request approved." });
        }
        catch (DomainException exception)
        {
            return HandleDecisionException(exception);
        }
    }

    /// <summary>Rejects a pending request with a required reason.</summary>
    [HttpPost]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        try
        {
            await _accountRequestService.RejectAsync(id, reason, GetActorUserId(), HttpContext.RequestAborted);
            return Ok(new { outcome = "Rejected", message = "Account request rejected." });
        }
        catch (DomainException exception)
        {
            return HandleDecisionException(exception);
        }
    }

    private IActionResult HandleDecisionException(DomainException exception)
    {
        if (IsAjaxRequest)
        {
            return HandleDomainException(exception, model: null);
        }

        // These command endpoints have no page of their own; return the same outcome envelope
        // used by ControllerBase for AJAX callers when invoked by a form/API client.
        var statusCode = exception.Outcome switch
        {
            OutcomeCode.Invalid => StatusCodes.Status400BadRequest,
            OutcomeCode.Forbidden => StatusCodes.Status403Forbidden,
            OutcomeCode.NotFound => StatusCodes.Status404NotFound,
            OutcomeCode.FileFailure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status409Conflict
        };

        return StatusCode(statusCode, new { outcome = exception.Outcome.ToString(), message = exception.Message });
    }
}
