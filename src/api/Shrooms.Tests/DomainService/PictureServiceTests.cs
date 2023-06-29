﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Picture;
using Shrooms.Infrastructure.Storage;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    [TestFixture]
    public class PictureServiceTests
    {
        private IPictureService _pictureService;
        private DbSet<Organization> _organizationsDbSet;

        [SetUp]
        public void Init()
        {
            var uow = Substitute.For<IUnitOfWork2>();
            _organizationsDbSet = uow.MockDbSetForAsync<Organization>();

            var azureStorage = Substitute.For<IStorage>();
            _pictureService = new PictureService(azureStorage, uow);
        }

        [Test]
        public async Task UploadFromStream_ShouldReturnCorrectName_WhenJpg()
        {
            // Arrange
            _organizationsDbSet.SetDbSetDataForAsync(new List<Organization>
            {
                new() { Id = 2, ShortName = "pictures" }
            }.AsQueryable());

            // Act
            var result = await _pictureService.UploadFromStreamAsync(null, null, "test.jpg", 2);

            // Assert
            Assert.That(result, Does.EndWith(".jpg"));
        }

        [Test]
        public async Task UploadFromStream_ShouldReturnCorrectName_WhenPng()
        {
            // Arrange
            _organizationsDbSet.SetDbSetDataForAsync(new List<Organization>
            {
                new() { Id = 2, ShortName = "pictures" }
            }.AsQueryable());

            // Act
            var result = await _pictureService.UploadFromStreamAsync(null, null, "test.png", 2);

            // Assert
            Assert.That(result, Does.EndWith(".png"));
        }

        [Test]
        public async Task UploadFromStream_ShouldReturnCorrectName_WhenGif()
        {
            // Arrange
            _organizationsDbSet.SetDbSetDataForAsync(new List<Organization>
            {
                new() { Id = 2, ShortName = "pictures" }
            }.AsQueryable());

            // Act
            var result = await _pictureService.UploadFromStreamAsync(null, null, "test.gif", 2);

            // Assert
            Assert.That(result, Does.EndWith(".gif"));
        }
    }
}
