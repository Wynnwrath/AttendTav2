using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IEventTypeService
    {
        List<EventTypeViewModel> GetEventTypes();
        EventTypeViewModel GetEventType(int id);
        void AddEventType(EventTypeViewModel model, int actorUserId);
        void UpdateEventType(EventTypeViewModel model, int actorUserId);
        void ToggleActive(int id, int actorUserId);
    }
}
