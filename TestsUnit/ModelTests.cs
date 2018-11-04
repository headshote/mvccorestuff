using MvcCoreAuth.Models.University;
using System;
using Xunit;

namespace Tests
{
    public class ModelTests
    {
        private readonly Person person;

        public ModelTests()
        {
            person = new Student();
        }

        [Fact]
        public void FullNameTest()
        {
            person.FirstMidName = "Someguy";
            person.LastName = "McGuy";

            Assert.Equal("McGuy, Someguy", person.FullName);
        }
    }
}
