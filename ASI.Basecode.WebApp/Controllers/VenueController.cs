using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using ASI.Basecode.WebApp.Storage;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "admin,officer")]
    [AutoValidateAntiforgeryToken]
    public class VenueController : ControllerBase<VenueController>
    {
        private readonly IVenueService _venueService;
        private readonly IVenueImageStorage _venueImageStorage;

        public VenueController(IHttpContextAccessor httpContextAccessor,
                               ILoggerFactory loggerFactory,
                                IConfiguration configuration,
                                IMapper mapper,
                                IVenueService venueService,
                                IVenueImageStorage venueImageStorage) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _venueService = venueService;
            _venueImageStorage = venueImageStorage;
        }

        public IActionResult Index()
        {
            var venues = _venueService.GetVenues();
            return View(venues);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new VenueViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(VenueViewModel model, IFormFile photo)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string savedImagePath = null;
            try
            {
                var actorUserId = GetActorUserId();
                if (actorUserId <= 0)
                {
                    throw new InvalidDataException("A valid authenticated user is required to create a venue.");
                }

                savedImagePath = await _venueImageStorage.SaveAsync(photo);
                model.ImagePath = savedImagePath;
                _venueService.AddVenue(model, actorUserId);
                TempData["SuccessMessage"] = "Venue added successfully.";
                return RedirectToAction("Index");
            }
            catch (InvalidDataException ex)
            {
                if (!string.IsNullOrWhiteSpace(savedImagePath))
                {
                    TryDeleteVenueImage(savedImagePath);
                }
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(savedImagePath))
                {
                    TryDeleteVenueImage(savedImagePath);
                }
                throw;
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = _venueService.GetVenue(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VenueViewModel model, IFormFile photo)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string newImagePath = null;
            var databaseUpdateSucceeded = false;
            var existing = _venueService.GetVenue(model.VenueId);
            if (existing == null)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var actorUserId = GetActorUserId();
                if (actorUserId <= 0)
                {
                    throw new InvalidDataException("A valid authenticated user is required to update a venue.");
                }

                model.ImagePath = existing.ImagePath;
                if (photo != null && photo.Length > 0)
                {
                    newImagePath = await _venueImageStorage.SaveAsync(photo);
                    model.ImagePath = newImagePath;
                }

                _venueService.UpdateVenue(model, actorUserId);
                databaseUpdateSucceeded = true;
                if (!string.IsNullOrWhiteSpace(newImagePath)
                    && !string.Equals(existing.ImagePath, newImagePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteVenueImage(existing.ImagePath);
                }
                TempData["SuccessMessage"] = "Venue updated successfully.";
                return RedirectToAction("Index");
            }
            catch (InvalidDataException ex)
            {
                if (!databaseUpdateSucceeded && !string.IsNullOrWhiteSpace(newImagePath))
                {
                    TryDeleteVenueImage(newImagePath);
                }
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
            catch
            {
                if (!databaseUpdateSucceeded && !string.IsNullOrWhiteSpace(newImagePath))
                {
                    TryDeleteVenueImage(newImagePath);
                }
                throw;
            }
        }

        [HttpPost]
        public IActionResult ToggleActive(int id)
        {
            try
            {
                _venueService.ToggleActive(id, GetActorUserId());
                TempData["SuccessMessage"] = "Venue status updated.";
            }
            catch (InvalidDataException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        private void TryDeleteVenueImage(string relativePath)
        {
            try
            {
                _venueImageStorage.Delete(relativePath);
            }
            catch (System.Exception ex) when (ex is IOException
                                               || ex is System.UnauthorizedAccessException
                                               || ex is InvalidDataException)
            {
                _logger.LogWarning(ex, "Could not clean up venue image {ImagePath}.", relativePath);
            }
        }
    }
}
