using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class EventTypeService : IEventTypeService
    {
        private readonly IEventTypeRepository _repository;
        private readonly IMapper _mapper;

        public EventTypeService(IEventTypeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public List<EventTypeViewModel> GetEventTypes()
        {
            var eventTypes = _repository.GetEventTypes().OrderBy(t => t.TypeName).ToList();
            return _mapper.Map<List<EventTypeViewModel>>(eventTypes);
        }

        public EventTypeViewModel GetEventType(int id)
        {
            var eventType = _repository.GetEventType(id);
            return eventType == null ? null : _mapper.Map<EventTypeViewModel>(eventType);
        }

        public void AddEventType(EventTypeViewModel model, int actorUserId)
        {
            if (actorUserId <= 0)
            {
                throw new InvalidDataException("A valid authenticated user is required to create an event type.");
            }

            var normalizedTypeName = model.TypeName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedTypeName))
            {
                throw new InvalidDataException("Type name is required.");
            }

            if (_repository.TypeNameExists(normalizedTypeName))
            {
                throw new InvalidDataException("Event type name already exists.");
            }

            var eventType = new EventType();
            eventType.TypeName = normalizedTypeName;
            eventType.Description = model.Description;
            eventType.IsActive = true;
            eventType.CreatedBy = actorUserId;
            eventType.CreatedAt = DateTime.UtcNow;
            _repository.AddEventType(eventType);
        }

        public void UpdateEventType(EventTypeViewModel model, int actorUserId)
        {
            if (actorUserId <= 0)
            {
                throw new InvalidDataException("A valid authenticated user is required to update an event type.");
            }

            var normalizedTypeName = model.TypeName?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedTypeName))
            {
                throw new InvalidDataException("Type name is required.");
            }

            var eventType = _repository.GetEventType(model.TypeId);
            if (eventType == null)
            {
                throw new InvalidDataException("Event type not found.");
            }

            if (_repository.TypeNameExists(normalizedTypeName, model.TypeId))
            {
                throw new InvalidDataException("Event type name already exists.");
            }

            eventType.TypeName = normalizedTypeName;
            eventType.Description = model.Description;
            eventType.UpdatedBy = actorUserId;
            eventType.UpdatedAt = DateTime.UtcNow;
            _repository.UpdateEventType(eventType);
        }

        public void ToggleActive(int id, int actorUserId)
        {
            if (actorUserId <= 0)
            {
                throw new InvalidDataException("A valid authenticated user is required to update an event type.");
            }

            var eventType = _repository.GetEventType(id);
            if (eventType == null)
            {
                throw new InvalidDataException("Event type not found.");
            }

            eventType.IsActive = !eventType.IsActive;
            eventType.UpdatedBy = actorUserId;
            eventType.UpdatedAt = DateTime.UtcNow;
            _repository.UpdateEventType(eventType);
        }
    }
}
