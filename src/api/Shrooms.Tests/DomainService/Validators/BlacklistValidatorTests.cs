﻿using NSubstitute;
using NUnit.Framework;
using Shrooms.DataLayer.EntityModels.Models;
using System;
using System.Data.Entity;
using Shrooms.Tests.Extensions;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistStates;

namespace Shrooms.Tests.DomainService.Validators
{
    [TestFixture]
    public class BlacklistValidatorTests
    {
        private DbSet<ApplicationUser> _usersDbSet;
        private DbSet<BlacklistUser> _blacklistUsersDbSet;

        private BlacklistValidator _validator;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();

            _usersDbSet = uow.MockDbSetForAsync<ApplicationUser>();
            _blacklistUsersDbSet = uow.MockDbSetForAsync<BlacklistUser>();

            uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);
            uow.GetDbSet<BlacklistUser>().Returns(_blacklistUsersDbSet);

            _validator = new BlacklistValidator(uow);
        }

        [Test]
        public void CheckIfBlacklistStateExists_WhenBlacklistEntryIsNull_ThrowsValidationException()
        {
            Assert.Throws<ValidationException>(() => _validator.CheckIfBlacklistUserExists(null));
        }

        [Test]
        public void CheckIfBlacklistStateExists_WhenBlacklistEntryIsNotNull_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _validator.CheckIfBlacklistUserExists(new BlacklistUser()));
        }

        [Test]
        public void CheckIfUserExistsAsync_WhenUserIsFound_DoesNotThrow()
        {
            // Arrange
            const string userId = "Id";
            var users = new List<ApplicationUser>
            {
                new()
                {
                    Id = userId,
                    OrganizationId = 1
                }
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _usersDbSet.SetDbSetDataForAsync(users);

            // Assert
            Assert.DoesNotThrowAsync(async () => await _validator.CheckIfUserExistsAsync(userId, userOrg));
        }

        [Test]
        public void CheckIfUserExistsAsync_WhenUserIsNotFound_ThrowsValidationException()
        {
            // Arrange
            var userId = "Id";
            var users = new List<ApplicationUser>
            {
                new()
                {
                    Id = "Other id",
                    OrganizationId = 1
                }
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _usersDbSet.SetDbSetDataForAsync(users);

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _validator.CheckIfUserExistsAsync(userId, userOrg));
        }

        [Test]
        public void CheckIfUserIsAlreadyBlacklistedAsync_WhenBlacklistEntryIsFound_ThrowsValidationException()
        {
            // Arrange
            const string userId = "Id";

            var blacklistUsers = new List<BlacklistUser>
            {
                new()
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    OrganizationId = 1
                }
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _validator.CheckIfUserIsAlreadyBlacklistedAsync(userId, userOrg));
        }

        [Test]
        public void CheckIfUserIsAlreadyBlacklistedAsync_WhenBlacklistEntriesAreExpired_DoesNotThrow()
        {
            // Arrange
            const string userId = "Id";

            var blacklistUsers = new List<BlacklistUser>
            {
                new()
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddYears(-1),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                }
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Assert
            Assert.DoesNotThrowAsync(async () => await _validator.CheckIfUserIsAlreadyBlacklistedAsync(userId, userOrg));
        }

        [Test]
        public void CheckIfUserIsAlreadyBlacklistedAsync_WhenBlacklistEntryIsNotFound_DoesNotThrow()
        {
            // Arrange
            const string userId = "Id";

            // ReSharper disable once CollectionNeverUpdated.Local
            var blacklistUsers = new List<BlacklistUser>();

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Assert
            Assert.DoesNotThrowAsync(async () => await _validator.CheckIfUserIsAlreadyBlacklistedAsync(userId, userOrg));
        }
    }
}
