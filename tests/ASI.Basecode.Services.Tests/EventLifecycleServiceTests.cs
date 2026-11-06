using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using ASI.Basecode.Resources;
using ASI.Basecode.Resources.Constants;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ASI.Basecode.Services.Tests;

public class EventLifecycleServiceTests
{
    private static readonly IMapper Mapper = new MapperConfiguration(config =>
    {
        config.CreateMap<EventViewModel, Event>();
        config.CreateMap<Event, EventViewModel>();
        config.CreateMap<EventListItem, EventViewModel>();
    }).CreateMapper();

    private static readonly DateTime Start = new(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Deadline = new(2026, 3, 9, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CreateDraftStoresADraftWithTheActorAndInjectedClockAndWritesNoAudit()
    {
        var clock = new FixedClock { Now = new DateTimeOffset(2026, 3, 1, 2, 0, 0, TimeSpan.Zero) };
        var harness = new Harness(clock);
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);

        var id = await harness.Service.CreateDraftAsync(
            Model("  Orientation  ", eventType.TypeId, venue.VenueId, limit: 30, description: "  Welcome  "),
            TestContext.Current.CancellationToken);

        var created = Assert.Single(harness.Events.Items);
        Assert.Equal(id, created.EventId);
        Assert.Equal("Orientation", created.Title);
        Assert.Equal("Welcome", created.Description);
        Assert.Equal(EventStatusCode.Draft, created.EventStatusId);
        Assert.Equal(7, created.CreatedBy);
        Assert.Equal(clock.Now.UtcDateTime, created.CreatedAt);
        Assert.Equal(DateTimeKind.Utc, created.CreatedAt.Kind);
        Assert.Empty(harness.Audit.Entries);
    }

    [Fact]
    public async Task BlankDescriptionIsStoredAsNull()
    {
        var harness = new Harness();
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);

        await harness.Service.CreateDraftAsync(
            Model("Orientation", eventType.TypeId, venue.VenueId, description: "   "),
            TestContext.Current.CancellationToken);

        Assert.Null(Assert.Single(harness.Events.Items).Description);
    }

    [Fact]
    public async Task InactiveEventTypeIsInvalidOnTypeIdAndWritesNothing()
    {
        var harness = new Harness();
        var eventType = harness.AddType("Retired", isActive: false);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);

        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.CreateDraftAsync(
            Model("Orientation", eventType.TypeId, venue.VenueId), TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Invalid, error.Outcome);
        Assert.Equal(nameof(EventViewModel.TypeId), error.Field);
        Assert.Empty(harness.Events.Items);
        Assert.Empty(harness.Audit.Entries);
    }

    [Fact]
    public async Task MissingEventTypeIsInvalidOnTypeId()
    {
        var harness = new Harness();
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);

        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.CreateDraftAsync(
            Model("Orientation", 999, venue.VenueId), TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Invalid, error.Outcome);
        Assert.Equal(nameof(EventViewModel.TypeId), error.Field);
        Assert.Empty(harness.Events.Items);
    }

    [Fact]
    public async Task InactiveVenueIsInvalidOnVenueIdAndWritesNothing()
    {
        var harness = new Harness();
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Closed room", capacity: 30, isActive: false);

        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.CreateDraftAsync(
            Model("Orientation", eventType.TypeId, venue.VenueId), TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Invalid, error.Outcome);
        Assert.Equal(nameof(EventViewModel.VenueId), error.Field);
        Assert.Empty(harness.Events.Items);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(31)]
    public async Task ParticipantLimitOutsideOneToVenueCapacityIsInvalidAndWritesNothing(int limit)
    {
        var harness = new Harness();
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);

        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.CreateDraftAsync(
            Model("Orientation", eventType.TypeId, venue.VenueId, limit: limit), TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Invalid, error.Outcome);
        Assert.Equal(nameof(EventViewModel.ParticipantLimit), error.Field);
        Assert.Empty(harness.Events.Items);
    }

    [Fact]
    public async Task DeadlineAfterStartIsInvalidOnRegistrationDeadlineAndWritesNothing()
    {
        var harness = new Harness();
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);
        var model = Model("Orientation", eventType.TypeId, venue.VenueId);
        model.RegistrationDeadline = Start.AddMinutes(1);

        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.CreateDraftAsync(
            model, TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Invalid, error.Outcome);
        Assert.Equal(nameof(EventViewModel.RegistrationDeadline), error.Field);
        Assert.Empty(harness.Events.Items);
    }

    [Fact]
    public async Task DeadlineEqualToStartIsAccepted()
    {
        var harness = new Harness();
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);
        var model = Model("Orientation", eventType.TypeId, venue.VenueId);
        model.RegistrationDeadline = Start;

        await harness.Service.CreateDraftAsync(model, TestContext.Current.CancellationToken);

        Assert.Equal(Start, Assert.Single(harness.Events.Items).RegistrationDeadline);
    }

    [Fact]
    public async Task EditingADraftUpdatesTheRowAndWritesOneEventUpdatedEntryWithOldAndNewValues()
    {
        var harness = new Harness();
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);
        var id = await harness.Service.CreateDraftAsync(
            Model("Orientation", eventType.TypeId, venue.VenueId), TestContext.Current.CancellationToken);

        var edited = Model("Orientation renamed", eventType.TypeId, venue.VenueId, limit: 12);
        edited.EventId = id;
        await harness.Service.UpdateDraftAsync(edited, TestContext.Current.CancellationToken);

        var row = Assert.Single(harness.Events.Items);
        Assert.Equal("Orientation renamed", row.Title);
        Assert.Equal(12, row.ParticipantLimit);
        Assert.Equal(7, row.UpdatedBy);
        Assert.NotNull(row.UpdatedAt);

        var entry = Assert.Single(harness.Audit.Entries);
        Assert.Equal(AuditActions.EventUpdated, entry.Action);
        Assert.Equal("events", entry.EntityType);
        Assert.Equal(id, entry.EntityId);
        Assert.Contains("\"Title\":\"Orientation\"", Json(entry.OldValues));
        Assert.Contains("\"Title\":\"Orientation renamed\"", Json(entry.NewValues));
        Assert.Contains("\"TypeName\":\"Workshop\"", Json(entry.OldValues));
        Assert.Contains("\"VenueName\":\"Room A\"", Json(entry.NewValues));
        Assert.Contains("\"ParticipantLimit\":12", Json(entry.NewValues));
        Assert.Contains("\"Status\":1", Json(entry.OldValues));
    }

    [Theory]
    [InlineData(EventStatusCode.Published)]
    [InlineData(EventStatusCode.Cancelled)]
    [InlineData(EventStatusCode.Completed)]
    public async Task EditingANonDraftIsInvalidAndChangesNothing(EventStatusCode status)
    {
        var harness = new Harness();
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);
        var id = await harness.Service.CreateDraftAsync(
            Model("Orientation", eventType.TypeId, venue.VenueId), TestContext.Current.CancellationToken);
        var row = Assert.Single(harness.Events.Items);
        row.EventStatusId = status;

        var edited = Model("Should not apply", eventType.TypeId, venue.VenueId);
        edited.EventId = id;
        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.UpdateDraftAsync(
            edited, TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Invalid, error.Outcome);
        Assert.Equal("Orientation", row.Title);
        Assert.Empty(harness.Audit.Entries);
    }

    [Fact]
    public async Task EditingAMissingEventIsNotFound()
    {
        var harness = new Harness();
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);
        var model = Model("Orientation", eventType.TypeId, venue.VenueId);
        model.EventId = 404;

        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.UpdateDraftAsync(
            model, TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.NotFound, error.Outcome);
        Assert.Empty(harness.Audit.Entries);
    }

    [Fact]
    public async Task AnonymousActorIsForbiddenAndWritesNothing()
    {
        var harness = new Harness(actor: new FixedActor(0, isAuthenticated: false));
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);

        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.CreateDraftAsync(
            Model("Orientation", eventType.TypeId, venue.VenueId), TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Forbidden, error.Outcome);
        Assert.Empty(harness.Events.Items);
        Assert.Empty(harness.Audit.Entries);
    }

    [Fact]
    public async Task TwentySixEventsPageInStartThenIdOrder()
    {
        var harness = new Harness();
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);
        for (var index = 1; index <= 26; index++)
        {
            // Two rows share a start so the event_id tie-break decides their order.
            var startsAt = Start.AddHours(index <= 25 ? index : 25);
            harness.Events.Items.Add(new Event
            {
                EventId = index,
                Title = $"Event {index:D2}",
                TypeId = eventType.TypeId,
                VenueId = venue.VenueId,
                StartsAt = startsAt,
                RegistrationDeadline = startsAt.AddHours(-1),
                ParticipantLimit = 10,
                EventStatusId = EventStatusCode.Draft,
                CreatedBy = 7,
                CreatedAt = Start
            });
        }

        var first = await harness.Service.GetPageAsync(1, TestContext.Current.CancellationToken);
        Assert.Equal(26, first.TotalCount);
        Assert.Equal(2, first.TotalPages);
        Assert.Equal(1, first.PageNumber);
        Assert.Equal(25, first.Items.Count);
        Assert.Equal(Enumerable.Range(1, 25).Select(index => $"Event {index:D2}"), first.Items.Select(item => item.Title));
        Assert.Equal("Workshop", first.Items[0].TypeName);
        Assert.Equal("Room A", first.Items[0].VenueName);

        var second = await harness.Service.GetPageAsync(2, TestContext.Current.CancellationToken);
        Assert.Equal(2, second.PageNumber);
        Assert.Single(second.Items);
        Assert.Equal("Event 26", second.Items[0].Title);

        var pastEnd = await harness.Service.GetPageAsync(999, TestContext.Current.CancellationToken);
        Assert.Equal(2, pastEnd.PageNumber);
        Assert.Single(pastEnd.Items);
        Assert.Equal("Event 26", pastEnd.Items[0].Title);

        var zero = await harness.Service.GetPageAsync(0, TestContext.Current.CancellationToken);
        Assert.Equal(1, zero.PageNumber);
        Assert.Equal(25, zero.Items.Count);

        var missing = await harness.Service.GetPageAsync(null, TestContext.Current.CancellationToken);
        Assert.Equal(1, missing.PageNumber);
        Assert.Equal(25, missing.Items.Count);
    }

    [Fact]
    public async Task EmptyListShowsPageOneOfZero()
    {
        var harness = new Harness();

        var page = await harness.Service.GetPageAsync(999, TestContext.Current.CancellationToken);

        Assert.Equal(1, page.PageNumber);
        Assert.Equal(0, page.TotalCount);
        Assert.Equal(0, page.TotalPages);
        Assert.Empty(page.Items);
    }

    [Fact]
    public async Task FormOptionsOfferOnlyActiveReferences()
    {
        var harness = new Harness();
        harness.AddType("Active type", isActive: true);
        harness.AddType("Inactive type", isActive: false);
        harness.AddVenue("Active room", capacity: 20, isActive: true);
        harness.AddVenue("Inactive room", capacity: 20, isActive: false);

        var options = await harness.Service.GetFormOptionsAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "Active type" }, options.EventTypes.Select(option => option.TypeName));
        Assert.Equal(new[] { "Active room" }, options.Venues.Select(option => option.RoomName));
        Assert.Equal(20, options.Venues[0].Capacity);
    }

    [Fact]
    public async Task GetEventReturnsNullForAMissingRow()
    {
        var harness = new Harness();

        Assert.Null(await harness.Service.GetEventAsync(404, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task PublishLocksReferencesPublishesAndWritesOneAuditEntry()
    {
        var clock = new FixedClock { Now = new DateTimeOffset(2026, 3, 1, 2, 0, 0, TimeSpan.Zero) };
        var harness = new Harness(clock);
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", capacity: 30, isActive: true);
        var id = await harness.Service.CreateDraftAsync(Model("Orientation", eventType.TypeId, venue.VenueId), TestContext.Current.CancellationToken);
        var row = Assert.Single(harness.Events.Items);
        row.ImagePath = "uploads/events/0123456789abcdef0123456789abcdef.jpg";
        harness.Locks.Clear();

        await harness.Service.PublishAsync(id, TestContext.Current.CancellationToken);

        Assert.Equal(new[] { "type", "venue", "event" }, harness.Locks);
        Assert.Equal(EventStatusCode.Published, row.EventStatusId);
        Assert.Equal(7, row.PublishedBy);
        Assert.Equal(clock.Now.UtcDateTime, row.PublishedAt);
        Assert.Equal(7, row.UpdatedBy);
        var audit = Assert.Single(harness.Audit.Entries);
        Assert.Equal(AuditActions.EventPublished, audit.Action);
        Assert.Equal(id, audit.EntityId);
    }

    [Theory]
    [InlineData(EventStatusCode.Published, true, true, true, 10, 1)]
    [InlineData(EventStatusCode.Completed, true, true, true, 10, 1)]
    [InlineData(EventStatusCode.Cancelled, true, true, true, 10, 1)]
    [InlineData(EventStatusCode.Draft, false, true, true, 10, 1)]
    [InlineData(EventStatusCode.Draft, true, false, true, 10, 1)]
    [InlineData(EventStatusCode.Draft, true, true, false, 10, 1)]
    [InlineData(EventStatusCode.Draft, true, true, true, 31, 1)]
    [InlineData(EventStatusCode.Draft, true, true, true, 10, 0)]
    [InlineData(EventStatusCode.Draft, true, true, true, 10, -1)]
    public async Task PublishRefusalLeavesStatusAndAuditUnchanged(
        EventStatusCode status, bool activeType, bool activeVenue, bool hasCover, int limit, int deadlineOffsetSeconds)
    {
        var clock = new FixedClock { Now = new DateTimeOffset(2026, 3, 1, 2, 0, 0, TimeSpan.Zero) };
        var harness = new Harness(clock);
        var eventType = harness.AddType("Workshop", isActive: true);
        var venue = harness.AddVenue("Room A", 30, isActive: true);
        var id = await harness.Service.CreateDraftAsync(Model("Orientation", eventType.TypeId, venue.VenueId), TestContext.Current.CancellationToken);
        var row = Assert.Single(harness.Events.Items);
        row.EventStatusId = status;
        row.ImagePath = hasCover ? "uploads/events/0123456789abcdef0123456789abcdef.jpg" : null;
        row.ParticipantLimit = limit;
        eventType.IsActive = activeType;
        venue.IsActive = activeVenue;
        row.RegistrationDeadline = clock.Now.UtcDateTime.AddSeconds(deadlineOffsetSeconds);

        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.PublishAsync(id, TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Invalid, error.Outcome);
        Assert.Equal(status, row.EventStatusId);
        Assert.Empty(harness.Audit.Entries);
    }

    [Fact]
    public async Task PublishMissingEventIsNotFoundAndChangedReferencesAreStaleWithoutWrites()
    {
        var missing = new Harness();
        var missingError = await Assert.ThrowsAsync<DomainException>(() => missing.Service.PublishAsync(404, TestContext.Current.CancellationToken));
        Assert.Equal(OutcomeCode.NotFound, missingError.Outcome);

        var clock = new FixedClock();
        var harness = new Harness(clock);
        var eventType = harness.AddType("Workshop", true);
        var venue = harness.AddVenue("Room A", 30, true);
        var id = await harness.Service.CreateDraftAsync(Model("Orientation", eventType.TypeId, venue.VenueId), TestContext.Current.CancellationToken);
        var row = Assert.Single(harness.Events.Items);
        row.ImagePath = "uploads/events/0123456789abcdef0123456789abcdef.jpg";
        harness.Events.BeforeLock = item => item.VenueId++;

        var stale = await Assert.ThrowsAsync<DomainException>(() => harness.Service.PublishAsync(id, TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Stale, stale.Outcome);
        Assert.Equal(EventStatusCode.Draft, row.EventStatusId);
        Assert.Empty(harness.Audit.Entries);
    }

    [Theory]
    [InlineData(0, false, RoleCodes.Officer)]
    [InlineData(7, true, RoleCodes.Member)]
    [InlineData(0, true, RoleCodes.Officer)]
    public async Task PublishRejectsAnonymousMemberAndInvalidActors(int userId, bool authenticated, string role)
    {
        var harness = new Harness(actor: new FixedActor(userId, authenticated, role));

        var error = await Assert.ThrowsAsync<DomainException>(() => harness.Service.PublishAsync(1, TestContext.Current.CancellationToken));

        Assert.Equal(OutcomeCode.Forbidden, error.Outcome);
        Assert.Empty(harness.Audit.Entries);
    }

    private static EventViewModel Model(
        string title,
        int typeId,
        int venueId,
        int limit = 10,
        string description = "Description") => new()
        {
            Title = title,
            Description = description,
            TypeId = typeId,
            VenueId = venueId,
            StartsAt = Start,
            RegistrationDeadline = Deadline,
            ParticipantLimit = limit
        };

    private static string Json(IAuditValues values) => JsonSerializer.Serialize(values, values.GetType());

    private sealed class Harness
    {
        public Harness(TimeProvider? clock = null, ICurrentActor? actor = null)
        {
            Events = new MemoryEvents(this);
            EventTypes = new MemoryEventTypes(this);
            Venues = new MemoryVenues(this);
            Audit = new RecordingAudit();
            Service = new EventLifecycleService(
                Events,
                EventTypes,
                Venues,
                new ImmediateUnitOfWork(),
                Mapper,
                clock ?? TimeProvider.System,
                Audit,
                actor ?? new FixedActor(7),
                new UnusedPrivateFileStore(),
                NullLogger<EventLifecycleService>.Instance);
        }

        public MemoryEvents Events { get; }
        public MemoryEventTypes EventTypes { get; }
        public MemoryVenues Venues { get; }
        public RecordingAudit Audit { get; }
        public EventLifecycleService Service { get; }
        public List<string> Locks { get; } = [];

        public EventType AddType(string name, bool isActive)
        {
            var eventType = new EventType
            {
                TypeId = EventTypes.Items.Count + 1,
                TypeName = name,
                Description = name,
                IsActive = isActive,
                CreatedBy = 7,
                CreatedAt = Start
            };
            EventTypes.Items.Add(eventType);
            Events.TypeNames[eventType.TypeId] = name;
            return eventType;
        }

        public Venue AddVenue(string roomName, int capacity, bool isActive)
        {
            var venue = new Venue
            {
                VenueId = Venues.Items.Count + 1,
                RoomName = roomName,
                Capacity = capacity,
                ImagePath = "uploads/venues/test.jpg",
                IsActive = isActive,
                CreatedBy = 7,
                CreatedAt = Start
            };
            Venues.Items.Add(venue);
            Events.VenueNames[venue.VenueId] = roomName;
            return venue;
        }
    }

    private sealed class FixedClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 3, 1, 2, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class FixedActor(int userId, bool isAuthenticated = true, string role = RoleCodes.Officer) : ICurrentActor
    {
        public bool IsAuthenticated => isAuthenticated;
        public int UserId => userId;
        public string Role => role;
    }

    private sealed class RecordingAudit : IActivityLogWriter
    {
        public List<AuditEntry> Entries { get; } = [];

        public void Append(AuditEntry entry) => Entries.Add(entry);
    }

    private sealed class MemoryEvents(Harness harness) : IEventRepository
    {
        public List<Event> Items { get; } = [];
        public Dictionary<int, string> TypeNames { get; } = [];
        public Dictionary<int, string> VenueNames { get; } = [];
        private int nextId = 1;
        public Action<Event>? BeforeLock { get; set; }

        public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(Items.Count);

        public Task<IReadOnlyList<EventListItem>> GetPageAsync(int skip, int take, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<EventListItem> page = Items
                .OrderBy(item => item.StartsAt)
                .ThenBy(item => item.EventId)
                .Skip(skip)
                .Take(take)
                .Select(item => new EventListItem
                {
                    EventId = item.EventId,
                    Title = item.Title,
                    TypeName = TypeNames.GetValueOrDefault(item.TypeId),
                    VenueName = VenueNames.GetValueOrDefault(item.VenueId),
                    StartsAt = item.StartsAt,
                    RegistrationDeadline = item.RegistrationDeadline,
                    ParticipantLimit = item.ParticipantLimit,
                    EventStatusId = item.EventStatusId
                })
                .ToList();
            return Task.FromResult(page);
        }

        public Task<Event> GetEventAsync(int eventId, CancellationToken cancellationToken = default)
        {
            var item = Items.FirstOrDefault(item => item.EventId == eventId);
            return Task.FromResult<Event>(item == null ? null! : new Event
            {
                EventId = item.EventId,
                TypeId = item.TypeId,
                VenueId = item.VenueId,
                EventStatusId = item.EventStatusId,
                ImagePath = item.ImagePath
            });
        }

        public Task<Event> LockEventAsync(int eventId, CancellationToken cancellationToken = default)
        {
            harness.Locks.Add("event");
            var item = Items.FirstOrDefault(item => item.EventId == eventId);
            if (item != null) BeforeLock?.Invoke(item);
            return Task.FromResult<Event>(item!);
        }

        public Task AddEventAsync(Event eventItem, CancellationToken cancellationToken = default)
        {
            eventItem.EventId = nextId++;
            Items.Add(eventItem);
            return Task.CompletedTask;
        }

        public Task UpdateEventAsync(Event eventItem, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class MemoryEventTypes(Harness harness) : IEventTypeRepository
    {
        public List<EventType> Items { get; } = [];

        private EventType? Find(int typeId) => Items.FirstOrDefault(item => item.TypeId == typeId);

        public Task<IReadOnlyList<EventType>> GetActiveEventTypesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EventType>>(Items.Where(item => item.IsActive).OrderBy(item => item.TypeName).ThenBy(item => item.TypeId).ToList());

        public Task<int> CountEventTypesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Items.Count);

        public Task<IReadOnlyList<EventType>> GetEventTypesPageAsync(int offset, int pageSize, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EventType>>(Items.Skip(offset).Take(pageSize).ToList());

        public EventType GetEventType(int typeId) => Find(typeId)!;

        public Task<EventType> LockEventTypeAsync(int typeId, CancellationToken cancellationToken = default)
        {
            harness.Locks.Add("type");
            return Task.FromResult(Find(typeId)!);
        }

        public Task<int> CountLinkedEventsAsync(int typeId, CancellationToken cancellationToken = default) => Task.FromResult(0);

        public Task AddEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default)
        {
            Items.Add(eventType);
            return Task.CompletedTask;
        }

        public Task UpdateEventTypeAsync(EventType eventType, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public bool EventTypeExists(int typeId) => Find(typeId) != null;

        public bool TypeNameExists(string typeName, int? excludeId = null) =>
            Items.Any(item => item.TypeName == typeName && (!excludeId.HasValue || item.TypeId != excludeId.Value));
    }

    private sealed class MemoryVenues(Harness harness) : IVenueRepository
    {
        public List<Venue> Items { get; } = [];

        private Venue? Find(int venueId) => Items.FirstOrDefault(item => item.VenueId == venueId);


        public Task<IReadOnlyList<Venue>> GetActiveVenuesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Venue>>(Items.Where(item => item.IsActive).OrderBy(item => item.RoomName).ThenBy(item => item.VenueId).ToList());

        public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(Items.Count);

        public Task<IReadOnlyList<Venue>> GetPageAsync(int skip, int take, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Venue>>(Items.Skip(skip).Take(take).ToList());

        public Venue GetVenue(int venueId) => Find(venueId)!;

        public Task<Venue> LockVenueAsync(int venueId, CancellationToken cancellationToken = default)
        {
            harness.Locks.Add("venue");
            return Task.FromResult(Find(venueId)!);
        }

        public Task<int> CountLinkedEventsAsync(int venueId, CancellationToken cancellationToken = default) => Task.FromResult(0);

        public Task<bool> HasEventAboveCapacityAsync(int venueId, int capacity, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task AddVenueAsync(Venue venue, CancellationToken cancellationToken = default)
        {
            Items.Add(venue);
            return Task.CompletedTask;
        }

        public Task UpdateVenueAsync(Venue venue, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public bool VenueExists(int venueId) => Find(venueId) != null;
    }

    private sealed class ImmediateUnitOfWork : IUnitOfWork
    {
        public DbContext Database => throw new NotSupportedException();
        public void SaveChanges() { }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken cancellationToken = default) => work(cancellationToken);
        public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default) => work(cancellationToken);
    }

    private sealed class UnusedPrivateFileStore : IPrivateFileStore
    {
        public Task<string> SaveAsync(PrivateFileArea area, Stream content, string extension, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public string CreateTemporaryPath(string extension) => throw new NotSupportedException();
        public Task<string> PromoteAsync(string temporaryPath, PrivateFileArea area, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public void Delete(string relativePath) => throw new NotSupportedException();
    }
}
