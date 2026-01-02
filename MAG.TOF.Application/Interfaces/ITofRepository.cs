using MAG.TOF.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Application.Interfaces
{
    public interface ITofRepository
    {
        Task <List<Request>> GetAllRequestsAsync();
        Task<Request?> GetRequestByIdAsync(int id);
        Task AddRequestAsync(Request request);
        Task UpdateRequestAsync(Request request);
        Task DeleteRequestAsync(int id);
    }
}
