using AIaaS.WebAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIaaS.UnitTests
{
    public class TestDatabaseFixture
    {
        //private const string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=EFTestSample;Trusted_Connection=True";
        private const string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=mh-aiaas;Trusted_Connection=True;MultipleActiveResultSets=true";

        private static readonly object _lock = new();
        private static bool _databaseInitialized;

        public TestDatabaseFixture()
        {
            //lock (_lock)
            //{
            //    if (!_databaseInitialized)
            //    {
            //        using (var context = CreateContext())
            //        {
            //            context.Database.EnsureDeleted();
            //            context.Database.EnsureCreated();

            //            context.AddRange(
            //                new Blog { Name = "Blog1", Url = "http://blog1.com" },
            //                new Blog { Name = "Blog2", Url = "http://blog2.com" });
            //            context.SaveChanges();
            //        }

            //        _databaseInitialized = true;
            //    }
            //}
        }

        public EfContext CreateContext()
            => new EfContext(
                new DbContextOptionsBuilder<EfContext>()
                    .UseSqlServer(ConnectionString)
                    .Options, null);
    }
}
