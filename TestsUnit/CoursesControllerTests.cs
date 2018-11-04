using Moq;
using MvcCoreAuth.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MvcCoreAuth.Controllers;
using MvcCoreAuth.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcCoreAuth.Models.University;

namespace Tests
{
    public class CoursesControllerTests
    {
        private readonly DbContextOptionsBuilder<ApplicationDbContext> optionsBulder;

        public CoursesControllerTests()
        {
            optionsBulder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase();
        }

        private void FillDb()
        {
            using (var mockContext = new ApplicationDbContext(optionsBulder.Options))
            {
                mockContext.Database.EnsureDeleted();

                mockContext.Courses.Add(new Course
                {
                    CourseID = 6666,
                    CourseAssignments = new List<CourseAssignment>() { },
                    Credits = 4,
                    Department = new Department(),
                    DepartmentID = 1,
                    Enrollments = new List<Enrollment>() { },
                    Title = "Some shitty course"
                });
                mockContext.Courses.Add(new Course
                {
                    CourseID = 4666,
                    CourseAssignments = new List<CourseAssignment>() {
                        new CourseAssignment{ }
                    },
                    Credits = 4,
                    Department = new Department(),
                    DepartmentID = 1,
                    Enrollments = new List<Enrollment>() { },
                    Title = "Some other shitty course"
                });
                mockContext.Courses.Add(new Course
                {
                    CourseID = 666,
                    CourseAssignments = new List<CourseAssignment>() {
                        new CourseAssignment{ }
                    },
                    Credits = 4,
                    Department = new Department(),
                    DepartmentID = 1,
                    Enrollments = new List<Enrollment>() {
                        new Enrollment{ },
                        new Enrollment{ }
                    },
                    Title = "Even more shitty course"
                });
                mockContext.Courses.Add(new Course
                {
                    CourseID = 5666,
                    CourseAssignments = new List<CourseAssignment>() {
                        new CourseAssignment{ },
                        new CourseAssignment{ },
                        new CourseAssignment{ }
                    },
                    Credits = 4,
                    Department = new Department(),
                    DepartmentID = 1,
                    Enrollments = new List<Enrollment>() {
                        new Enrollment{ },
                        new Enrollment{ },
                        new Enrollment{ }
                    },
                    Title = "Actually NOT shitty course"
                });

                mockContext.SaveChanges();
            }
        }

        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfCourses()
        {
            using (var mockContext = new ApplicationDbContext(optionsBulder.Options))
            {
                FillDb();

                var controller = new CoursesController(mockContext);

                // Act
                var result = await controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var models = Assert.IsAssignableFrom<ICollection<Course>>(viewResult.ViewData.Model);

                Assert.Equal(4, models.Count);
            }
        }

        [Fact]
        public async Task IndexPost_ReturnsARedirect_WhenModelStateIsValid()
        {
            using (var mockContext = new ApplicationDbContext(optionsBulder.Options))
            {
                FillDb();

                var controller = new CoursesController(mockContext);

                // Act
                var result = await controller.DeleteConfirmed(666);

                // Assert
                var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Null(redirectToActionResult.ControllerName);
                Assert.Equal("Index", redirectToActionResult.ActionName);
            }
        }

        [Fact]
        public async Task ForDetails_ReturnsNotFoundObjectResultForNonexistentContact()
        {
            using (var mockContext = new ApplicationDbContext(optionsBulder.Options))
            {
                FillDb();

                var controller = new CoursesController(mockContext);

                // Act
                var result = await controller.Details(69);

                // Assert
                var actionResult = Assert.IsType<NotFoundResult>(result);
            }
        }
    }
}
