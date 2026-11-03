using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IVenueService
    {
        List<VenueViewModel> GetVenues();
        VenueViewModel GetVenue(int id);
        void AddVenue(VenueViewModel model, int actorUserId);
        void UpdateVenue(VenueViewModel model, int actorUserId);
        void ToggleActive(int id, int actorUserId);
    }
}
