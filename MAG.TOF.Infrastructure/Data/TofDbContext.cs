using MAG.TOF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAG.TOF.Infrastructure.Data
{
    public class TofDbContext : DbContext
    {
        public TofDbContext(DbContextOptions<TofDbContext> options) : base(options)
        {
        }

        public DbSet<Request> Requests { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Requests");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.DepartmentId).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.TotalBusinessDays).IsRequired();
                entity.Property(e => e.ManagerId).IsRequired();
                entity.Property(e => e.ManagerComment).HasMaxLength(1000);
                entity.Property(e => e.StatusId).IsRequired();
            });
        }
    }
}
