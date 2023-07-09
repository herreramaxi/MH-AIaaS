using AIaaS.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.WebAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(EfContext context)
        {
            //context.Database.EnsureCreated();
            context.Database.Migrate();
            //// Look for any students.
            //if (context.Users.Any())
            //{
            //    return;   // DB has been seeded
            //}

            //var users = new User[]
            //{
            //new User{Email="herreramaxi@gmail.com"},
            //new User{Email="user1@mh-aiaas.com"},
            //new User{Email="user2@mh-aiaas.com"},
            //new User{Email="user3@mh-aiaas.com"},
            //};
            //foreach (User u in users)
            //{
            //    context.Users.Add(u);
            //}
            //context.SaveChanges();
        }
    }
}
