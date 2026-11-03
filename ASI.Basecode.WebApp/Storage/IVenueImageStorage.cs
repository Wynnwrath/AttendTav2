using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Storage
{
    public interface IVenueImageStorage
    {
        Task<string> SaveAsync(IFormFile image);
        void Delete(string relativePath);
    }
}
