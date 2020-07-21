using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ComplexUnitOfWork.Model;

using Microsoft.EntityFrameworkCore;

namespace ComplexUnitOfWork.UnitOfWork.Database
{
    public class SampleContext : DbContext, IUnitOfWorkComponent
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Constants.ConnectionString);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Sample>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Sample>().Property(p => p.Id).HasMaxLength(100);

            modelBuilder.Entity<Locale>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Locale>().Property(p => p.Id).HasMaxLength(5);
            modelBuilder.Entity<Locale>().HasData(new List<Locale>()
            {
                new Locale {Id = "en-GB"},
                new Locale {Id = "hu-HU"}
            });
        }

        public DbSet<Sample> Samples => Set<Sample>();
        public DbSet<Locale> Locales => Set<Locale>();

        public bool HasChanges => ChangeTracker.HasChanges();

        public Task CompleteAsync()
        {
            return SaveChangesAsync();
        }

        public void Rollback()
        {
            var changedEntries = ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch(entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }
        }
    }
}
