using System;
using MvcCoreAuth.Data;
using MvcCoreAuth.Data.University;

namespace TestsIntegration
{
    internal class Utilities
    {
        internal static void InitializeDbForTests(ApplicationDbContext db)
        {
            DbInitializer.Initialize(db);
        }
    }
}