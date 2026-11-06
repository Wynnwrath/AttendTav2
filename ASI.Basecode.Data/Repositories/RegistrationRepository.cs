using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Models.Lookups;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories;

public sealed class RegistrationRepository : BaseRepository, IRegistrationRepository
{
    public RegistrationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<bool> IsConfirmedAsync(int registrationId, CancellationToken cancellationToken = default) =>
        GetDbSet<Registration>().AnyAsync(
            registration => registration.RegistrationId == registrationId
                && registration.RegistrationStatusId == RegistrationStatusCode.Confirmed,
            cancellationToken);
}
