using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces;

public interface IRegistrationRepository
{
    Task<bool> IsConfirmedAsync(int registrationId, CancellationToken cancellationToken = default);
}
