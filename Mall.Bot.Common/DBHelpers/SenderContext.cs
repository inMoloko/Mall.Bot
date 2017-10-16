using Mall.Bot.Common.DBHelpers.Models;
using System.Data.Entity;
using System.Linq;

namespace Mall.Bot.Common.DBHelpers
{
    public class SenderContext : DbContext
    {
        public SenderContext(string connStringName) : base(connStringName)
        {
            Database.SetInitializer<SenderContext>(null);
        }

        public DbSet<BotMessage> Message{ get; set; }
        public DbSet<Models.Common.BotUser> BotUser { get; set; }

        public void UndoChanges()
        {
            var changedEntries = this.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
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
