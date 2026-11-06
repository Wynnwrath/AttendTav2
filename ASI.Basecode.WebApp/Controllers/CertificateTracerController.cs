using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers;

[Authorize(Policy = Policies.Management)]
[AutoValidateAntiforgeryToken]
[Route("CertificateTracer")]
public sealed class CertificateTracerController : ControllerBase<CertificateTracerController>
{
    private readonly ICertificateTracerService tracer;
    private readonly IWebHostEnvironment environment;

    public CertificateTracerController(
        IHttpContextAccessor httpContextAccessor,
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        IMapper mapper,
        ICertificateTracerService tracer,
        IWebHostEnvironment environment)
        : base(httpContextAccessor, loggerFactory, configuration, mapper)
    {
        this.tracer = tracer;
        this.environment = environment;
    }

    [HttpGet("")]
    public IActionResult Index() => environment.IsDevelopment() ? View() : NotFound();

    [HttpPost("Trace")]
    public async Task<IActionResult> Trace(int registrationId, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        if (registrationId <= 0)
        {
            return BadRequest();
        }

        try
        {
            var pdf = await tracer.CreateSamplePdfDownloadAsync(registrationId, cancellationToken);
            return pdf is null ? NotFound() : File(pdf, "application/pdf");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            _logger.LogError("Development certificate PDF tracer failed.");
            return Problem(statusCode: StatusCodes.Status500InternalServerError, title: "The sample PDF could not be created.");
        }
    }
}
