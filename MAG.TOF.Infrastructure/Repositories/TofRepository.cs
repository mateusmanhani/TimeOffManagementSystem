using MAG.TOF.Application.Interfaces;
using MAG.TOF.Domain.Entities;
using MAG.TOF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Infrastructure.Repositories
{
    public class TofRepository : ITofRepository
    {

        private readonly TofDbContext _context;

        public TofRepository(TofDbContext context)
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
    }
}
