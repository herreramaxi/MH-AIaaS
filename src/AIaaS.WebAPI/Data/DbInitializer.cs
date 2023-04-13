using AIaaS.WebAPI.Models;

namespace AIaaS.WebAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AIaaSContext context)
        {
            context.Database.EnsureCreated();

            // Look for any students.
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var users = new User[]
            {
            new User{Email="admin@mh-aiaas.com"},
            new User{Email="user1@mh-aiaas.com"},
            new User{Email="user2@mh-aiaas.com"},
            new User{Email="user3@mh-aiaas.com"},
            };
            foreach (User u in users)
            {
                context.Users.Add(u);
            }
            context.SaveChanges();
        }
    }
}
