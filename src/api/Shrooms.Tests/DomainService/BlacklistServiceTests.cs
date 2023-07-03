﻿using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Exceptions;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models;
using System;
using System.Threading.Tasks;
using Shrooms.Tests.Extensions;
using System.Collections.Generic;
using System.Data.Entity;
using Shrooms.Domain.Services.BlacklistUsers;
using Shrooms.Contracts.DataTransferObjects.BlacklistUsers;
using Shrooms.Contracts.Enums;
using Shrooms.Domain.Services.Permissions;
using Shrooms.Contracts.Constants;
using Shrooms.Domain.ServiceValidators.Validators.BlacklistStates;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class BlacklistServiceTests
    {
        private ISystemClock _systemClock;
        private IBlacklistValidator _validator;
        private IPermissionService _permissionService;

        private DbSet<BlacklistUser> _blacklistUsersDbSet;

        private BlacklistService _blacklistService;

        [SetUp]
        public void TestInitializer()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _blacklistUsersDbSet = uow.MockDbSetForAsync(new List<BlacklistUser>());

            uow.GetDbSet<BlacklistUser>()
                .Returns(_blacklistUsersDbSet);

            _systemClock = Substitute.For<ISystemClock>();
            _systemClock.UtcNow.Returns(DateTime.UtcNow);

            _validator = Substitute.For<IBlacklistValidator>();
            _permissionService = Substitute.For<IPermissionService>();

            _blacklistService = new BlacklistService(uow, _validator, _systemClock, _permissionService);
        }

        [Test]
        public void CreateAsync_WhenUserIsAlreadyBlacklisted_ThrowsValidationException()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto();
            var args = new CreateBlacklistUserDto();

            _validator
                .CheckIfUserIsAlreadyBlacklistedAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistService.CreateAsync(args, userOrg));
        }

        [Test]
        public void CreateAsync_WhenUserDoesNotExist_ThrowsValidationException()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto();
            var args = new CreateBlacklistUserDto();

            _validator
                .CheckIfUserExistsAsync(Arg.Any<string>(), Arg.Any<UserAndOrganizationDto>())
                .Throws(new ValidationException(0));

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistService.CreateAsync(args, userOrg));
        }

        [Test]
        public async Task CreateAsync_WithValidValues_CreatesBlacklistUser()
        {
            // Arrange
            var userOrg = new UserAndOrganizationDto();
            var args = new CreateBlacklistUserDto
            {
                UserId = "Id",
                EndDate = DateTime.UtcNow,
                Reason = "Reason"
            };

            // Act
            await _blacklistService.CreateAsync(args, userOrg);

            // Assert
            _blacklistUsersDbSet.Received(1).Add(Arg.Any<BlacklistUser>());
        }

        [Test]
        public void CancelAsync_BlacklistEntryDoesNotExists_ThrowsValidationException()
        {
            // Arrange
            const string userId = "Id";
            var blacklistEntryToDelete = new BlacklistUser
            {
                UserId = userId,
                Reason = string.Empty,
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(new List<BlacklistUser> { blacklistEntryToDelete });
            _validator
                .When(validator => validator.CheckIfBlacklistUserExists(Arg.Any<BlacklistUser>()))
                .Do(_ => throw new ValidationException(0));

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistService.CancelAsync(userId, userOrg));
        }

        [Test]
        public void CancelAsync_WhenAllBlacklistEntriesAreExpired_ThrowsValidationException()
        {
            // Arrange
            const string userId = "Id";
            var blacklistEntryToDelete = new BlacklistUser
            {
                UserId = userId,
                Reason = string.Empty,
                EndDate = DateTime.UtcNow.AddDays(-10),
                OrganizationId = 1,
                Status = BlacklistStatus.Expired
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(new List<BlacklistUser> { blacklistEntryToDelete });
            _validator
                .When(validator => validator.CheckIfBlacklistUserExists(Arg.Is((BlacklistUser)null)))
                .Do(_ => throw new ValidationException(0));

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistService.CancelAsync(userId, userOrg));
        }

        [Test]
        public async Task CancelAsync_WithValidValues_CancelsBlacklistEntry()
        {
            // Arrange
            const string userId = "Id";
            var blacklistEntryToCancel = new BlacklistUser
            {
                UserId = userId,
                Reason = string.Empty,
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1,
                Status = BlacklistStatus.Active
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(new List<BlacklistUser> { blacklistEntryToCancel });

            // Act
            await _blacklistService.CancelAsync(userId, userOrg);

            // Assert
            Assert.AreEqual(BlacklistStatus.Canceled, blacklistEntryToCancel.Status);
        }

        [Test]
        public async Task GetAsync_WhenMoreThanOneBlacklistUserEntryIsPresent_FindsActiveBlacklistEntry()
        {
            // Arrange
            const string userId = "Id";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var shouldFindThis = new BlacklistUser
            {
                UserId = userId,
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1,
                Status = BlacklistStatus.Active,
                CreatedByUser = new ApplicationUser(),
                ModifiedByUser = new ApplicationUser()
            };

            var blacklistUsers = new List<BlacklistUser>
            {
                shouldFindThis,
                new()
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                },
                new()
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                }
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Act
            var result = await _blacklistService.GetAsync(userId, userOrg);

            // Assert
            Assert.AreEqual(shouldFindThis.UserId, result.UserId);
        }

        [Test]
        public async Task GetAsync_WhenAllBlacklistEntriesAreExpired_ReturnsNull()
        {
            // Arrange
            const string userId = "Id";
            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var shouldFindThis = new BlacklistUser
            {
                UserId = userId,
                EndDate = DateTime.UtcNow.AddDays(-10),
                OrganizationId = 1,
                Status = BlacklistStatus.Expired
            };

            var blacklistUsers = new List<BlacklistUser>
            {
                shouldFindThis,
                new()
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                },
                new()
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                }
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Act
            var result = await _blacklistService.GetAsync(userId, userOrg);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void UpdateAsync_WhenAllBlacklistEntriesAreExpired_ThrowsValidationException()
        {
            // Arrange
            const string userId = "Id";

            var updateDto = new UpdateBlacklistUserDto
            {
                UserId = userId,
                Reason = "Reason",
                EndDate = DateTime.UtcNow.AddYears(10)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var blacklistUsers = new List<BlacklistUser>
            {
                new()
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                },
                new()
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                },
                new()
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                }
            };

            _validator
               .When(validator => validator.CheckIfBlacklistUserExists(Arg.Is((BlacklistUser)null)))
               .Do(_ => throw new ValidationException(0));

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistService.UpdateAsync(updateDto, userOrg));
        }

        [Test]
        public void UpdateAsync_WhenBlacklistEntryIsNotFound_ThrowsValidationException()
        {
            // Arrange
            const string userId = "Id";

            var updateDto = new UpdateBlacklistUserDto
            {
                UserId = userId,
                Reason = "Reason",
                EndDate = DateTime.UtcNow.AddYears(10)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            // ReSharper disable once CollectionNeverUpdated.Local
            var blacklistUsers = new List<BlacklistUser>();

            _validator
               .When(validator => validator.CheckIfBlacklistUserExists(Arg.Is((BlacklistUser)null)))
               .Do(_ => throw new ValidationException(0));

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistService.UpdateAsync(updateDto, userOrg));
        }

        [Test]
        public async Task UpdateAsync_WithValidValues_UpdatesBlacklistEntry()
        {
            // Arrange
            const string userId = "Id";

            var updateDto = new UpdateBlacklistUserDto
            {
                UserId = userId,
                Reason = "Reason",
                EndDate = DateTime.UtcNow.AddYears(10)
            };

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var updatedBlacklistEntry = new BlacklistUser
            {
                UserId = userId,
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1,
                Status = BlacklistStatus.Active
            };

            var blacklistUsers = new List<BlacklistUser>
            {
                updatedBlacklistEntry,
                new()
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                },
                new()
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Canceled
                }
            };

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Act
            await _blacklistService.UpdateAsync(updateDto, userOrg);

            // Assert
            Assert.AreEqual(updateDto.UserId, updatedBlacklistEntry.UserId);
            Assert.AreEqual(updateDto.Reason, updatedBlacklistEntry.Reason);
            Assert.AreEqual(updateDto.EndDate, updatedBlacklistEntry.EndDate);
        }

        [Test]
        public void TryFindActiveBlacklistUserEntry_WhenMoreThanOneBlacklistEntryIsPresent_FindsActiveBlacklistEntry()
        {
            // Arrange
            var foundEntry = new BlacklistUser
            {
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1,
                Status = BlacklistStatus.Active,
                ModifiedByUser = new ApplicationUser(),
                CreatedByUser = new ApplicationUser()
            };

            var blacklistUsers = new List<BlacklistUser>
            {
                foundEntry,
                new()
                {
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                },
                new()
                {
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Canceled
                }
            };

            // Act
            _blacklistService.TryFindActiveBlacklistUserEntry(blacklistUsers, out var result);

            // Assert
            Assert.AreEqual(foundEntry.UserId, result.UserId);
            Assert.AreEqual(foundEntry.Reason, result.Reason);
            Assert.AreEqual(foundEntry.EndDate, result.EndDate);
        }

        [Test]
        public void TryFindActiveBlacklistUserEntry_WhenMoreThanOneBlacklistEntryIsPresent_ReturnsTrue()
        {
            // Arrange
            var foundEntry = new BlacklistUser
            {
                EndDate = DateTime.UtcNow.AddDays(10),
                OrganizationId = 1,
                Status = BlacklistStatus.Active,
                ModifiedByUser = new ApplicationUser(),
                CreatedByUser = new ApplicationUser()
            };

            var blacklistUsers = new List<BlacklistUser>
            {
                foundEntry,
                new()
                {
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                },
                new()
                {
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Canceled
                }
            };

            // Act
            var result = _blacklistService.TryFindActiveBlacklistUserEntry(blacklistUsers, out _);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void TryFindActiveBlacklistUserEntry_WhenBlacklistEntryIsNotFound_ReturnsFalse()
        {
            // Arrange
            var blacklistUsers = new List<BlacklistUser>();

            // Act
            var result = _blacklistService.TryFindActiveBlacklistUserEntry(blacklistUsers, out _);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TryFindActiveBlacklistUserEntry_WhenBlacklistEntryIsNotFound_SetsResultToNull()
        {
            // Arrange
            var blacklistUsers = new List<BlacklistUser>();

            // Act
            _blacklistService.TryFindActiveBlacklistUserEntry(blacklistUsers, out var result);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void TryFindActiveBlacklistUserEntry_WhenAllBlacklistEntriesAreExpired_ReturnsFalse()
        {
            // Arrange
            var blacklistUsers = new List<BlacklistUser>
            {
                new()
                {
                    EndDate = DateTime.UtcNow.AddDays(-20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Canceled
                },
                new()
                {
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                },
                new()
                {
                    EndDate = DateTime.UtcNow.AddYears(-20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                }
            };

            // Act
            var result = _blacklistService.TryFindActiveBlacklistUserEntry(blacklistUsers, out _);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TryFindActiveBlacklistUserEntry_WhenAllBlacklistEntriesAreExpired_SetsResultToNull()
        {
            // Arrange
            var blacklistUsers = new List<BlacklistUser>
            {
                new()
                {
                    EndDate = DateTime.UtcNow.AddDays(-20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Canceled
                },
                new()
                {
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                },
                new()
                {
                    EndDate = DateTime.UtcNow.AddYears(-20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired
                }
            };

            // Act
            _blacklistService.TryFindActiveBlacklistUserEntry(blacklistUsers, out var result);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllExceptActiveAsync_ValidValues_DoesNotReturnActiveBlacklistEntries()
        {
            // Arrange
            const string userId = "Id";

            var userOrg = new UserAndOrganizationDto
            {
                OrganizationId = 1
            };

            var blacklistUsers = new List<BlacklistUser>
            {
                new()
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Active,
                    ModifiedByUser = new ApplicationUser(),
                    CreatedByUser = new ApplicationUser()
                },
                new()
                {
                    UserId = userId,
                    EndDate = DateTime.UtcNow.AddDays(-10),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Expired,
                    ModifiedByUser = new ApplicationUser(),
                    CreatedByUser = new ApplicationUser()
                },
                new()
                {
                    UserId = "Id2",
                    EndDate = DateTime.UtcNow.AddYears(20),
                    OrganizationId = 1,
                    Status = BlacklistStatus.Canceled,
                    ModifiedByUser = new ApplicationUser(),
                    CreatedByUser = new ApplicationUser()
                }
            };

            _permissionService
              .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Is(BasicPermissions.Blacklist))
              .Returns(true);

            _blacklistUsersDbSet.SetDbSetDataForAsync(blacklistUsers);

            // Act
            var result = await _blacklistService.GetAllExceptActiveAsync(userId, userOrg);

            // Assert
            Assert.That(result, Is.All.Matches<BlacklistUserDto>(entry => entry.Status != BlacklistStatus.Active));
        }

        [Test]
        public void GetAllExceptActiveAsync_WhenUserDoesNotHaveBlacklistBasicPermissionButRequestUserAndUserIdMatches_DoesNotThrow()
        {
            // Arrange
            const string userId = "Same id";

            var userOrg = new UserAndOrganizationDto
            {
                UserId = userId
            };

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Is(BasicPermissions.Blacklist))
                .Returns(false);

            // Assert
            Assert.DoesNotThrowAsync(async () => await _blacklistService.GetAllExceptActiveAsync(userId, userOrg));
        }

        [Test]
        public void GetAllExceptActiveAsync_WhenUserHasBlacklistBasicPermissionButRequestUserAndUserIdDoesNotMatch_DoesNotThrow()
        {
            // Arrange
            const string userId = "Id";

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "Random id"
            };

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Is(BasicPermissions.Blacklist))
                .Returns(true);

            // Assert
            Assert.DoesNotThrowAsync(async () => await _blacklistService.GetAllExceptActiveAsync(userId, userOrg));
        }

        [Test]
        public void GetAllExceptActiveAsync_WhenUserHasBlacklistBasicPermissionAndRequestUserAndUserIdMatches_DoesNotThrow()
        {
            // Arrange
            const string userId = "Same id";

            var userOrg = new UserAndOrganizationDto
            {
                UserId = userId
            };

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Is(BasicPermissions.Blacklist))
                .Returns(true);

            // Assert
            Assert.DoesNotThrowAsync(async () => await _blacklistService.GetAllExceptActiveAsync(userId, userOrg));
        }

        [Test]
        public void GetAllExceptActiveAsync_WhenUserDoesNotHaveBlacklistBasicPermissionAndRequestUserAndUserIdDoesNotMatch_ThrowsValidationException()
        {
            // Arrange
            const string userId = "Id";

            var userOrg = new UserAndOrganizationDto
            {
                UserId = "Random id"
            };

            _permissionService
                .UserHasPermissionAsync(Arg.Any<UserAndOrganizationDto>(), Arg.Is(BasicPermissions.Blacklist))
                .Returns(false);

            // Assert
            Assert.ThrowsAsync<ValidationException>(async () => await _blacklistService.GetAllExceptActiveAsync(userId, userOrg));
        }
    }
}
