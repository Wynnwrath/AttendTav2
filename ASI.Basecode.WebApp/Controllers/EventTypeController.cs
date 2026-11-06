using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Policy = Policies.Management)]
    [AutoValidateAntiforgeryToken]
    public class EventTypeController : ControllerBase<EventTypeController>
    {
        private readonly IEventTypeService _eventTypeService;

        public EventTypeController(IHttpContextAccessor httpContextAccessor,
                                   ILoggerFactory loggerFactory,
                                   IConfiguration configuration,
                                   IMapper mapper,
                                   IEventTypeService eventTypeService) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _eventTypeService = eventTypeService;
        }

        [HttpGet]
        [Route("EventType")]
        [Route("EventType/Index")]
        public async Task<IActionResult> Index(int? page = null)
        {
            var eventTypes = await _eventTypeService.GetPageAsync(page, HttpContext.RequestAborted);
            return View(eventTypes);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new EventTypeViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(EventTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _eventTypeService.AddEventTypeAsync(model, HttpContext.RequestAborted);
                TempData["SuccessMessage"] = "Event type added successfully.";
                return RedirectToAction("Index");
            }
            catch (DomainException ex)
            {
                return HandleDomainException(ex, model);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _eventTypeService.GetEventType(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EventTypeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _eventTypeService.UpdateEventTypeAsync(model, HttpContext.RequestAborted);
                TempData["SuccessMessage"] = "Event type updated successfully.";
                return RedirectToAction("Index");
            }
            catch (DomainException ex)
            {
                return HandleDomainException(ex, model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            try
            {
                await _eventTypeService.ToggleActiveAsync(id, HttpContext.RequestAborted);
                TempData["SuccessMessage"] = "Event type status updated.";
            }
            catch (DomainException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
