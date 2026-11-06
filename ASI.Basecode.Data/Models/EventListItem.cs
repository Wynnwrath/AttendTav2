using System;
using ASI.Basecode.Data.Models.Lookups;

namespace ASI.Basecode.Data.Models;

/// <summary>Read projection for the Management event list.</summary>
public sealed class EventListItem
{
    public int EventId { get; set; }
    public string Title { get; set; }
    public string TypeName { get; set; }
    public string VenueName { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public int ParticipantLimit { get; set; }
    public EventStatusCode EventStatusId { get; set; }
}
