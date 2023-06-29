﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Excel;
using NSubstitute;
using NUnit.Framework;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Infrastructure.ExcelGenerator;
using Shrooms.Tests.Extensions;

namespace Shrooms.Tests.DomainService
{
    public class KudosExportServiceTests
    {
        private IUnitOfWork2 _uow;
        private DbSet<KudosLog> _kudosDbSet;
        private DbSet<ApplicationUser> _usersDbSet;
        private IKudosExportService _kudosExportService;
        private ExcelBuilderFactory _excelBuilder;

        [SetUp]
        public void TestInitializer()
        {
            _uow = Substitute.For<IUnitOfWork2>();
            _kudosDbSet = Substitute.For<DbSet<KudosLog>, IQueryable<KudosLog>, IDbAsyncEnumerable<KudosLog>>();
            _kudosDbSet.SetDbSetDataForAsync(MockKudos());
            _uow.GetDbSet<KudosLog>().Returns(_kudosDbSet);

            _usersDbSet = Substitute.For<DbSet<ApplicationUser>, IQueryable<ApplicationUser>, IDbAsyncEnumerable<ApplicationUser>>();
            _usersDbSet.SetDbSetDataForAsync(MockUsers());
            _uow.GetDbSet<ApplicationUser>().Returns(_usersDbSet);

            _excelBuilder = new ExcelBuilderFactory();

            _kudosExportService = new KudosExportService(
                _uow,
                _excelBuilder);
        }

        [Test]
        public async Task Kudos_Should_Return_Excel_File()
        {
            var filter = new KudosLogsFilterDto
            {
                OrganizationId = 2,
                SearchUserId = null,
                Status = BusinessLayerConstants.KudosStatusAllFilter,
                SortBy = "Created",
                SortOrder = "desc"
            };

            var content = await _kudosExportService.ExportToExcelAsync(filter);
            var bytes = await content.ReadAsByteArrayAsync();

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(bytes)))
            {
                excelReader.IsFirstRowAsColumnNames = true;
                var excelData = excelReader.AsDataSet();
                var excelRows = excelData.Tables[0].Rows;
                var excelColumns = excelData.Tables[0].Columns;

                Assert.AreEqual(Resources.Models.Kudos.Kudos.ExportColumnSender, excelColumns[0].ColumnName);
                Assert.AreEqual(Resources.Models.Kudos.Kudos.ExportColumnReceiver, excelColumns[1].ColumnName);
                Assert.AreEqual("name surname", excelRows[0].ItemArray[0]);
                Assert.AreEqual("name surname", excelRows[0].ItemArray[1]);
                Assert.AreEqual("name2 surname2", excelRows[1].ItemArray[0]);
                Assert.AreEqual("name surname", excelRows[1].ItemArray[1]);
                Assert.AreEqual(4, excelRows.Count);

                excelReader.Close();
            }
        }

        [Test]
        public async Task Kudos_Should_Return_Filtered_Excel_File()
        {
            var filter = new KudosLogsFilterDto
            {
                OrganizationId = 2,
                SearchUserId = "testUserId3",
                Status = BusinessLayerConstants.KudosStatusAllFilter,
                SortBy = "Created",
                SortOrder = "desc"
            };

            var content = await _kudosExportService.ExportToExcelAsync(filter);
            var bytes = await content.ReadAsByteArrayAsync();

            using (var excelReader = ExcelReaderFactory.CreateOpenXmlReader(new MemoryStream(bytes)))
            {
                excelReader.IsFirstRowAsColumnNames = true;
                var excelData = excelReader.AsDataSet();
                var excelRows = excelData.Tables[0].Rows;

                Assert.AreEqual("name5 surname5", excelRows[0].ItemArray[0]);
                Assert.AreEqual("name3 surname3", excelRows[0].ItemArray[1]);
                Assert.AreEqual(1, excelRows.Count);

                excelReader.Close();
            }
        }

        private IQueryable<KudosLog> MockKudos()
        {
            return new List<KudosLog>
            {
                new()
                {
                    Status = KudosStatus.Pending,
                    Id = 1,
                    EmployeeId = "testUserId",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId"
                },
                new()
                {
                    Status = KudosStatus.Pending,
                    Id = 2,
                    EmployeeId = "testUserId",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId2"
                },
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 3,
                    EmployeeId = "testUserId",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name",
                        LastName = "surname"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId"
                },
                new()
                {
                    Status = KudosStatus.Approved,
                    Id = 4,
                    EmployeeId = "testUserId3",
                    Employee = new ApplicationUser
                    {
                        Id = "testUserId",
                        FirstName = "name3",
                        LastName = "surname3"
                    },
                    OrganizationId = 2,
                    CreatedBy = "testUserId5"
                }
            }.AsQueryable();
        }

        private IQueryable<ApplicationUser> MockUsers()
        {
            return new List<ApplicationUser>
            {
                new()
                {
                    Id = "testUserId",
                    FirstName = "name",
                    LastName = "surname"
                },
                new()
                {
                    Id = "testUserId2",
                    FirstName = "name2",
                    LastName = "surname2"
                },
                new()
                {
                    Id = "testUserId3",
                    FirstName = "name3",
                    LastName = "surname3"
                },
                new()
                {
                    Id = "testUserId4",
                    FirstName = "name4",
                    LastName = "surname4"
                },
                new()
                {
                    Id = "testUserId5",
                    FirstName = "name5",
                    LastName = "surname5"
                }
            }.AsQueryable();
        }
    }
}
