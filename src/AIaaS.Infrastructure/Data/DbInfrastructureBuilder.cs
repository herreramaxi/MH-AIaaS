//using Microsoft.AspNetCore.Builder;

//namespace AIaaS.Infrastructure.Data
//{
//    public static class DbInfrastructureBuilder
//    {
//        public static void CreateDbIfNotExists(this WebApplication host)
//        {
//            using (var scope = host.Services.CreateScope())
//            {
//                var services = scope.ServiceProvider;
//                try
//                {
//                    var context = services.GetRequiredService<EfContext>();
//                    DbInitializer.Initialize(context);
//                }
//                catch (Exception ex)
//                {
//                    var logger = services.GetRequiredService<ILogger<Program>>();
//                    logger.LogError(ex, "An error occurred creating the DB.");
//                }
//            }
//        }
//    }
//}
