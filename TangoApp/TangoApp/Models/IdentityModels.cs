using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TangoApp.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
        //am adaugat o colectie cu notificariile care se creeaza in urma unei actiunii
        //facute de userul curent
        [InverseProperty("UserSend")]
        public virtual ICollection<Notification> NotificationSend { get; set; }
        [InverseProperty("UserReceive")]
        public virtual ICollection<Notification> NotificationReceive { get; set; }
        //adaugam colectiile corespunzatoare modelului FriendShip
        [InverseProperty("User1")]
        public virtual ICollection<Friendship> FriendshipSend { get; set; }
        [InverseProperty("User2")]
        public virtual ICollection<Friendship> FriendshipReceive { get; set; }

        public bool UserStatus { get; set; }

    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext,
            TangoApp.Migrations.Configuration>("DefaultConnection"));
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Friendship> Friendships { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

    }
}