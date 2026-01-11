using MAG.TOF.Domain.Entities;

namespace MAG.TOF.Application.Interfaces
{
    public interface IRequestRepository
    {
        Task <List<Request>> GetAllRequestsAsync();
        Task<Request?> GetRequestByIdAsync(int id);
        Task AddRequestAsync(Request request);
        Task UpdateRequestAsync(Request request);
        Task DeleteRequestAsync(int id);

        Task <List<Request>> GetRequestsByUserIdAsync(int userId);

        Task<Request?> HasOverlappingRequestsAsync(int userId, DateTime startDate, DateTime endDate);
        Task<List<Request>> GetPendingRequestsByManagerId(int loggedUserId);
    }
}
