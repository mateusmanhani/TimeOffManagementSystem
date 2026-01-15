using MAG.TOF.Application.Interfaces;
using MAG.TOF.Domain.Entities;
using MAG.TOF.Domain.Enums;
using MAG.TOF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MAG.TOF.Infrastructure.Repositories
{
    public class RequestRepository : IRequestRepository
    {

        private readonly TofDbContext _context;

        public RequestRepository(TofDbContext context)
        {
            _context = context;
        }

        public async Task AddRequestAsync(Request request, CancellationToken cancellationToken)
        {
            //  Add entity to the context
            await _context.Requests.AddAsync(request);

            //  Save changes to the database
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRequestAsync(int id, CancellationToken cancellationToken)
        {
            //  Find the entity by id
            var request = await _context.Requests.FindAsync(id);

            // If request exists, delete it
            if (request != null)
            {
                _context.Requests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Request?> GetRequestByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _context.Requests
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateRequestAsync(Request request, CancellationToken cancellationToken)
        {
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();
        }

        public async Task<Request?> HasOverlappingRequestsAsync(int usrId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            // Find the first overlapping request for this user
            return await _context.Requests
                .Where(r => r.UserId == usrId &&
                               r.StartDate <= endDate &&
                               r.EndDate >= startDate)
                .OrderBy(r => r.StartDate) // Get earliest overlapping request
                .FirstOrDefaultAsync();
        }

        public async Task<List<Request>> GetRequestsAsync(
            int? managerId = null,
            RequestStatus? status = null,
            int? userId = null,
            DateTime? from = null,
            DateTime? to = null,
            int? page = null, int?
            pageSize = null,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Request> q = _context.Requests.AsQueryable();

            if (managerId.HasValue) q = q.Where(r => r.ManagerId == managerId.Value);
            if (status.HasValue) q = q.Where(r => r.Status == status.Value);
            if (userId.HasValue) q = q.Where(r => r.UserId == userId.Value);
            if (from.HasValue) q = q.Where(r => r.StartDate >= from.Value);
            if (to.HasValue) q = q.Where(r => r.EndDate <= to.Value);

            // optional paging
            if (page.HasValue && pageSize.HasValue)
            {
                q = q.OrderByDescending(r => r.StartDate)
                     .Skip((page.Value - 1) * pageSize.Value)
                     .Take(pageSize.Value);
            }
            else
            {
                q = q.OrderByDescending(r => r.StartDate).AsNoTracking();
            }

            return await q.ToListAsync(cancellationToken);
        }
    }
}
