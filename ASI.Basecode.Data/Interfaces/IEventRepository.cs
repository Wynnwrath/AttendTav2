using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IEventRepository
    {
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<EventListItem>> GetPageAsync(int skip, int take, CancellationToken cancellationToken = default);
        Task<Event> GetEventAsync(int eventId, CancellationToken cancellationToken = default);
        Task<Event> LockEventAsync(int eventId, CancellationToken cancellationToken = default);
        Task AddEventAsync(Event eventItem, CancellationToken cancellationToken = default);
        Task UpdateEventAsync(Event eventItem, CancellationToken cancellationToken = default);
    }
}
