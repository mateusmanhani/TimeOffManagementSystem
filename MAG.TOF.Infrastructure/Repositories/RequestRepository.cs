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

        public async Task AddRequestAsync(Request request)
        {
            //  Add entity to the context
            await _context.Requests.AddAsync(request);

            //  Save changes to the database
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRequestAsync(int id)
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

        public async Task<List<Request>> GetAllRequestsAsync()
        {
            return await _context.Requests
                .OrderByDescending(r => r.StartDate) // Most recent first
                .ToListAsync();
        }

        public async Task<Request?> GetRequestByIdAsync(int id)
        {
            return await _context.Requests
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateRequestAsync(Request request)
        {
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();
        }

        // todo review repository architecture 1 per table or what?
        // Rename repository to RequestRepository?
        public async Task<Request?> HasOverlappingRequestsAsync(int usrId, DateTime startDate, DateTime endDate)
        {
            // Find the first overlapping request for this user
            return await _context.Requests
                .Where(r => r.UserId == usrId &&
                               r.StartDate <= endDate &&
                               r.EndDate >= startDate)
                .OrderBy(r => r.StartDate) // Get earliest overlapping request
                .FirstOrDefaultAsync();
        }

        public async Task<List<Request>> GetRequestsByUserIdAsync(int userId)
        {
            return await _context.Requests
                .Where( r => r.UserId == userId)
                .OrderByDescending(r => r.StartDate) // Most recent first
                .ToListAsync();
        }

        // todo - implement pagination | Manual or PaginatedResult class?
        public async Task<List<Request>> GetPendingRequestsByManagerId(int loggedUserId)
        {
            return await _context.Requests
                .Where(r => r.ManagerId == loggedUserId 
                && r.Status == RequestStatus.Pending)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
           
        }
    }
}
