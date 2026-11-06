using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;

namespace ASI.Basecode.Data.Queries;

public static class ParticipationFacts
{
    public static readonly Expression<Func<User, Member, bool>> EligibleParticipant =
        (user, member) => user.IsActive && member.MembershipStatusId == MembershipStatusCode.Active;

    public static readonly Expression<Func<Attendance, bool>> ValidAttendance =
        attendance => attendance.InvalidatedAt == null;

    public static readonly Expression<Func<Registration, bool>> SeatHeld =
        registration => registration.RegistrationStatusId == RegistrationStatusCode.Confirmed;

    public static IQueryable<int> EligibleParticipantMemberIds(
        IQueryable<User> users,
        IQueryable<Member> members)
    {
        var user = Expression.Parameter(typeof(User), "user");
        var member = Expression.Parameter(typeof(Member), "member");
        var predicate = Replace(EligibleParticipant.Body, new Dictionary<ParameterExpression, Expression>
        {
            [EligibleParticipant.Parameters[0]] = user,
            [EligibleParticipant.Parameters[1]] = member
        });
        var memberFilter = Expression.Lambda<Func<Member, bool>>(
            Expression.AndAlso(Expression.Equal(Expression.Property(member, nameof(Member.UserId)), Expression.Property(user, nameof(User.Id))), predicate), member);
        var collection = Expression.Lambda<Func<User, IEnumerable<Member>>>(members.Where(memberFilter).Expression, user);
        return users.SelectMany(collection, (outer, inner) => inner.MemberId);
    }

    public static IQueryable<Attendance> ValidAttendances(IQueryable<Attendance> attendance) =>
        attendance.Where(ValidAttendance);

    public static IQueryable<Registration> SeatHolders(IQueryable<Registration> registrations) =>
        registrations.Where(SeatHeld);

    public static IQueryable<Registration> WaitlistPriority(IQueryable<Registration> registrations, int eventId) =>
        registrations.Where(registration => registration.EventId == eventId
                                            && registration.RegistrationStatusId == RegistrationStatusCode.Waitlisted)
            .OrderBy(registration => registration.ReviewedAt)
            .ThenBy(registration => registration.RegistrationId);

    public static Expression<Func<Event, bool>> StartCutoffPassed(DateTime nowUtc) =>
        eventItem => nowUtc >= eventItem.StartsAt;

    public static Expression<Func<Event, bool>> SignupOpen(DateTime nowUtc) =>
        eventItem => eventItem.EventStatusId == EventStatusCode.Published && nowUtc < eventItem.RegistrationDeadline;

    public static Expression<Func<Event, EventType, Venue, bool>> Publishable(DateTime nowUtc) =>
        (eventItem, eventType, venue) => eventItem.EventStatusId == EventStatusCode.Draft
                                         && eventType.IsActive
                                         && venue.IsActive
                                         && eventItem.ImagePath != null
                                         && eventItem.ImagePath.Trim() != ""
                                         && eventItem.ParticipantLimit <= venue.Capacity
                                         && nowUtc < eventItem.RegistrationDeadline;

    public static IQueryable<int> PublishableEventIds(
        IQueryable<Event> events,
        IQueryable<EventType> eventTypes,
        IQueryable<Venue> venues,
        DateTime nowUtc) =>
        PublishableEventIdsCore(events, eventTypes, venues, nowUtc);

    private static IQueryable<int> PublishableEventIdsCore(
        IQueryable<Event> events,
        IQueryable<EventType> eventTypes,
        IQueryable<Venue> venues,
        DateTime nowUtc)
    {
        var eventItem = Expression.Parameter(typeof(Event), "eventItem");
        var eventType = Expression.Parameter(typeof(EventType), "eventType");
        var venue = Expression.Parameter(typeof(Venue), "venue");
        var publishable = Publishable(nowUtc);
        var predicate = Replace(publishable.Body, new Dictionary<ParameterExpression, Expression>
        {
            [publishable.Parameters[0]] = eventItem,
            [publishable.Parameters[1]] = eventType,
            [publishable.Parameters[2]] = venue
        });
        var venueFilter = Expression.Lambda<Func<Venue, bool>>(
            Expression.AndAlso(Expression.Equal(Expression.Property(venue, nameof(Venue.VenueId)), Expression.Property(eventItem, nameof(Event.VenueId))), predicate), venue);
        var venueIds = venues.Where(venueFilter).Select(Expression.Lambda<Func<Venue, int>>(Expression.Property(eventItem, nameof(Event.EventId)), venue));
        var eventTypeFilter = Expression.Lambda<Func<EventType, bool>>(
            Expression.Equal(Expression.Property(eventType, nameof(EventType.TypeId)), Expression.Property(eventItem, nameof(Event.TypeId))), eventType);
        var eventTypeIds = eventTypes.Where(eventTypeFilter).SelectMany(
            Expression.Lambda<Func<EventType, IEnumerable<int>>>(venueIds.Expression, eventType));
        return events.SelectMany(Expression.Lambda<Func<Event, IEnumerable<int>>>(eventTypeIds.Expression, eventItem));
    }

    public static IQueryable<Registration> NoShows(
        IQueryable<Registration> registrations,
        IQueryable<Event> events,
        IQueryable<Attendance> attendance)
    {
        var seatHolders = SeatHolders(registrations);
        var validAttendance = ValidAttendances(attendance);
        return
        from registration in seatHolders
        join eventItem in events on registration.EventId equals eventItem.EventId
        where eventItem.EventStatusId == EventStatusCode.Completed
              && !validAttendance.Any(row => row.RegistrationId == registration.RegistrationId)
        select registration;
    }

    public static IQueryable<decimal?> AttendanceRates(
        IQueryable<Event> events,
        IQueryable<Registration> registrations,
        IQueryable<Attendance> attendance)
    {
        var seatHolders = SeatHolders(registrations);
        var validAttendance = ValidAttendances(attendance);
        return
        from eventItem in events
        let confirmed = seatHolders.Count(registration => registration.EventId == eventItem.EventId)
        let validAttendees =
            (from registration in seatHolders
             join row in validAttendance on registration.RegistrationId equals row.RegistrationId
             where registration.EventId == eventItem.EventId
             select registration.RegistrationId).Distinct().Count()
        select confirmed == 0 ? (decimal?)null : (decimal)validAttendees / confirmed;
    }

    private static Expression Replace(Expression expression, IReadOnlyDictionary<ParameterExpression, Expression> replacements) =>
        new ParameterReplacementVisitor(replacements).Visit(expression)!;

    private sealed class ParameterReplacementVisitor(IReadOnlyDictionary<ParameterExpression, Expression> replacements) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            replacements.TryGetValue(node, out var replacement) ? replacement : base.VisitParameter(node);
    }
}
