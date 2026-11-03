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
    public class VenueService : IVenueService
    {
        private readonly IVenueRepository _repository;
        private readonly IMapper _mapper;

        public VenueService(IVenueRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public List<VenueViewModel> GetVenues()
        {
            var venues = _repository.GetVenues().OrderBy(v => v.RoomName).ToList();
            return _mapper.Map<List<VenueViewModel>>(venues);
        }

        public VenueViewModel GetVenue(int id)
        {
            var venue = _repository.GetVenue(id);
            return venue == null ? null : _mapper.Map<VenueViewModel>(venue);
        }

        public void AddVenue(VenueViewModel model, int actorUserId)
        {
            if (actorUserId <= 0)
            {
                throw new InvalidDataException("A valid authenticated user is required to create a venue.");
            }

            if (string.IsNullOrWhiteSpace(model.ImagePath))
            {
                throw new InvalidDataException("A venue photo is required.");
            }

            var venue = new Venue();
            venue.RoomName = model.RoomName?.Trim();
            venue.Capacity = model.Capacity;
            venue.Location = model.Location;
            venue.AvailabilityNotes = model.AvailabilityNotes;
            venue.ContactPerson = model.ContactPerson;
            venue.ImagePath = model.ImagePath;
            venue.IsActive = true;
            venue.CreatedBy = actorUserId;
            venue.CreatedAt = DateTime.UtcNow;
            _repository.AddVenue(venue);
        }

        public void UpdateVenue(VenueViewModel model, int actorUserId)
        {
            if (actorUserId <= 0)
            {
                throw new InvalidDataException("A valid authenticated user is required to update a venue.");
            }

            var venue = _repository.GetVenue(model.VenueId);
            if (venue == null)
            {
                throw new InvalidDataException("Venue not found.");
            }

            if (string.IsNullOrWhiteSpace(model.ImagePath))
            {
                throw new InvalidDataException("A venue photo is required.");
            }

            venue.RoomName = model.RoomName?.Trim();
            venue.Capacity = model.Capacity;
            venue.Location = model.Location;
            venue.AvailabilityNotes = model.AvailabilityNotes;
            venue.ContactPerson = model.ContactPerson;
            venue.ImagePath = model.ImagePath;
            venue.UpdatedBy = actorUserId;
            venue.UpdatedAt = DateTime.UtcNow;
            _repository.UpdateVenue(venue);
        }

        public void ToggleActive(int id, int actorUserId)
        {
            if (actorUserId <= 0)
            {
                throw new InvalidDataException("A valid authenticated user is required to update a venue.");
            }

            var venue = _repository.GetVenue(id);
            if (venue == null)
            {
                throw new InvalidDataException("Venue not found.");
            }

            venue.IsActive = !venue.IsActive;
            venue.UpdatedBy = actorUserId;
            venue.UpdatedAt = DateTime.UtcNow;
            _repository.UpdateVenue(venue);
        }
    }
}
