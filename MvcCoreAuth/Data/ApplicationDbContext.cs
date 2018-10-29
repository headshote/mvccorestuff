using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MvcCoreAuth.Areas.Identity.Data;
using MvcCoreAuth.Models;
using MvcCoreAuth.Models.University;

namespace MvcCoreAuth.Data
{
    public class ApplicationDbContext : IdentityDbContext<MvcCoreAuthUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Core Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Core Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<Course>().ToTable("Course");
            builder.Entity<Enrollment>().ToTable("Enrollment");
            builder.Entity<Student>().ToTable("Student");
        }

        public DbSet<MvcCoreAuth.Models.Contact> Contact { get; set; }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Student> Students { get; set; }
    }
}
