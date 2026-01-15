using MAG.TOF.Domain.Entities;
using MAG.TOF.Domain.Enums;

namespace MAG.TOF.Application.Interfaces
{
    public interface IRequestRepository
    {
        Task<List<Request>> GetRequestsAsync(
            int? managerId = null,
            RequestStatus? status = null,
            int? userId = null,
            DateTime? from = null,
            DateTime? to = null,
            int? page = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default);
        Task<Request?> GetRequestByIdAsync(int id, CancellationToken cancellationToken);
        Task AddRequestAsync(Request request, CancellationToken cancellationToken);
        Task UpdateRequestAsync(Request request, CancellationToken cancellationToken);
        Task DeleteRequestAsync(int id, CancellationToken cancellationToken);

        public Task<Request?> HasOverlappingRequestsAsync(int usrId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    }
}
