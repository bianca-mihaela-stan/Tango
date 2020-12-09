using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Tango.Models
{
    public class AppContext : DbContext
    {
        public AppContext() : base("DBConnectionString") {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppContext,
        Tango.Migrations.Configuration>("DBConnectionString"));

        }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Notification> Notifications { get; set; }

    }
}