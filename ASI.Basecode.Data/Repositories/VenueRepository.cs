using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class VenueRepository : BaseRepository, IVenueRepository
    {
        public VenueRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<IReadOnlyList<Venue>> GetActiveVenuesAsync(CancellationToken cancellationToken = default)
        {
            return await GetDbSet<Venue>()
                .AsNoTracking()
                .Where(venue => venue.IsActive)
                .OrderBy(venue => venue.RoomName)
                .ThenBy(venue => venue.VenueId)
                .ToListAsync(cancellationToken);
        }

        public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
            GetDbSet<Venue>().AsNoTracking().CountAsync(cancellationToken);

        public async Task<IReadOnlyList<Venue>> GetPageAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip));
            if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

            return await GetDbSet<Venue>()
                .AsNoTracking()
                .OrderBy(venue => venue.RoomName)
                .ThenBy(venue => venue.VenueId)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public Venue GetVenue(int venueId)
        {
            return this.GetDbSet<Venue>().FirstOrDefault(v => v.VenueId == venueId);
        }

        // Event writers must acquire this same venue lock before changing competing limits.
        public async Task<Venue> LockVenueAsync(int venueId, CancellationToken cancellationToken = default)
        {
            if (Context.Database.CurrentTransaction == null) throw new InvalidOperationException("A transaction is required to lock a venue.");
            var row = await GetDbSet<Venue>().FromSqlInterpolated($"SELECT * FROM dbo.venues WITH (UPDLOCK, HOLDLOCK, ROWLOCK) WHERE venue_id = {venueId}")
                .AsNoTracking().SingleOrDefaultAsync(cancellationToken);
            var tracked = GetDbSet<Venue>().Local.FirstOrDefault(v => v.VenueId == venueId);
            if (row == null) { if (tracked != null) Context.Entry(tracked).State = EntityState.Detached; return null; }
            if (tracked == null) { GetDbSet<Venue>().Attach(row); return row; }
            Context.Entry(tracked).CurrentValues.SetValues(row);
            Context.Entry(tracked).OriginalValues.SetValues(row);
            Context.Entry(tracked).State = EntityState.Unchanged;
            return tracked;
        }

        public Task<bool> HasEventAboveCapacityAsync(int venueId, int capacity, CancellationToken cancellationToken = default) =>
            GetDbSet<Event>().AnyAsync(e => e.VenueId == venueId && e.ParticipantLimit > capacity &&
                (e.EventStatusId == EventStatusCode.Draft || e.EventStatusId == EventStatusCode.Published), cancellationToken);

        // Deactivation keeps every existing reference, so the audit counts all linked events.
        public Task<int> CountLinkedEventsAsync(int venueId, CancellationToken cancellationToken = default) =>
            GetDbSet<Event>().AsNoTracking().CountAsync(e => e.VenueId == venueId, cancellationToken);

        public Task AddVenueAsync(Venue venue, CancellationToken cancellationToken = default)
        {
            this.GetDbSet<Venue>().Add(venue);
            return Task.CompletedTask;
        }

        public Task UpdateVenueAsync(Venue venue, CancellationToken cancellationToken = default)
        {
            this.SetEntityState(venue, EntityState.Modified);
            return Task.CompletedTask;
        }

        public bool VenueExists(int venueId)
        {
            return this.GetDbSet<Venue>().Any(v => v.VenueId == venueId);
        }
    }
}
