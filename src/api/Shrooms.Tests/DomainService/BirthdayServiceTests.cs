﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Birthday;
using Shrooms.Domain.Services.Roles;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    public class BirthdayServiceTests
    {
        private IBirthdayService _birthdayService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            uow.MockDbSetForAsync(MockUsers());

            var roleService = Substitute.For<IRoleService>();

            var newRoleId = Guid.NewGuid().ToString();
            roleService.GetRoleIdByNameAsync(Roles.NewUser).Returns(newRoleId);
            roleService.ExcludeUsersWithRole(newRoleId).ReturnsForAnyArgs(x => true);

            _birthdayService = new BirthdayService(uow, roleService);
        }

        private static IQueryable<ApplicationUser> MockUsers()
        {
            return new List<ApplicationUser>
            {
                new()
                {
                    Id = "testUserId",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(1993, 11, 30)
                },
                new()
                {
                    Id = "testUserId2",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(1988, 12, 05)
                },
                new()
                {
                    Id = "testUserId3",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(2000, 12, 06)
                },
                new()
                {
                    Id = "testUserId4",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(2015, 11, 28)
                },
                new()
                {
                    Id = "testUserId5",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(2015, 11, 29)
                },
                new()
                {
                    Id = "testUserId6",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(1930, 12, 11)
                },
                new()
                {
                    Id = "christmasBirthdayUser",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(1985, 12, 27)
                },
                new()
                {
                    Id = "newYearBirthdayUser",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(1985, 01, 01)
                },
                new()
                {
                    Id = "februaryBirthdayUser1",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(2012, 02, 29)
                },
                new()
                {
                    Id = "februaryBirthdayUser2",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(1985, 02, 28)
                },
                new()
                {
                    Id = "februaryBirthdayUser3",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(1985, 03, 04)
                },

                new()
                {
                    Id = "endOfYearBirthdayUser1",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(1985, 12, 31)
                },
                new()
                {
                    Id = "endOfYearBirthdayUser2",
                    FirstName = "Name",
                    LastName = "Surname",
                    BirthDay = new DateTime(1985, 12, 30)
                }
            }.AsQueryable();
        }

        [Test]
        public async Task Should_Return_If_Get_This_Week_Birthdays_Returns_Wrong_Users_1()
        {
            var time = new DateTime(2015, 11, 29);
            var birthdays = (await _birthdayService.GetWeeklyBirthdaysAsync(time)).ToArray();

            Assert.AreEqual(3, birthdays.Length);
            Assert.AreEqual("testUserId2", birthdays[0].Id);
            Assert.AreEqual("Saturday", birthdays[0].DayOfWeek);
            Assert.AreEqual("2015-12-05", birthdays[0].DateString);
        }

        [Test]
        public async Task Should_Return_If_Get_This_Week_Birthdays_Returns_Wrong_Users_2()
        {
            var time = new DateTime(2015, 12, 05);
            var birthdays = (await _birthdayService.GetWeeklyBirthdaysAsync(time)).ToArray();

            Assert.AreEqual(2, birthdays.Length);
            Assert.AreEqual("testUserId3", birthdays[1].Id);
            Assert.AreEqual("Sunday", birthdays[1].DayOfWeek);
            Assert.AreEqual("2015-12-06", birthdays[1].DateString);
        }

        [Test]
        public async Task Should_Return_Correct_Year_In_DateString()
        {
            var time = new DateTime(2015, 12, 28);
            var birthdays = (await _birthdayService.GetWeeklyBirthdaysAsync(time)).ToArray();

            Assert.AreEqual(4, birthdays.Length);
            Assert.AreEqual("christmasBirthdayUser", birthdays[3].Id);
            Assert.AreEqual("2015-12-27", birthdays[3].DateString);
            Assert.AreEqual("Sunday", birthdays[3].DayOfWeek);
            Assert.AreEqual("newYearBirthdayUser", birthdays[0].Id);
            Assert.AreEqual("2016-01-01", birthdays[0].DateString);
            Assert.AreEqual("Friday", birthdays[0].DayOfWeek);
        }

        [Test]
        public async Task Should_Return_Feb_29_Birthdays()
        {
            var time = new DateTime(2017, 02, 28);
            var birthdays = (await _birthdayService.GetWeeklyBirthdaysAsync(time)).ToArray();

            Assert.AreEqual(3, birthdays.Length);
            Assert.AreEqual("februaryBirthdayUser3", birthdays[0].Id);
            Assert.AreEqual("februaryBirthdayUser1", birthdays[1].Id);
            Assert.AreEqual("2017-03-01", birthdays[1].DateString);
            Assert.AreEqual("februaryBirthdayUser2", birthdays[2].Id);
        }

        [Test]
        public async Task Should_Handle_Leaping_Year_Correctly()
        {
            var time = new DateTime(2016, 02, 28);
            var birthdays = (await _birthdayService.GetWeeklyBirthdaysAsync(time)).ToArray();

            Assert.AreEqual(3, birthdays.Length);
            Assert.AreEqual("februaryBirthdayUser3", birthdays[0].Id);
            Assert.AreEqual("februaryBirthdayUser1", birthdays[1].Id);
            Assert.AreEqual("2016-02-29", birthdays[1].DateString);
            Assert.AreEqual("februaryBirthdayUser2", birthdays[2].Id);
        }

        [Test]
        public async Task Should_Correctly_Sort_Birthdays_At_End_Of_Year()
        {
            var time = new DateTime(2020, 12, 28);
            var birthdays = (await _birthdayService.GetWeeklyBirthdaysAsync(time)).ToArray();

            Assert.AreEqual(4, birthdays.Length);
            Assert.AreEqual("newYearBirthdayUser", birthdays[0].Id);
            Assert.AreEqual("endOfYearBirthdayUser1", birthdays[1].Id);
            Assert.AreEqual("endOfYearBirthdayUser2", birthdays[2].Id);
        }
    }
}
