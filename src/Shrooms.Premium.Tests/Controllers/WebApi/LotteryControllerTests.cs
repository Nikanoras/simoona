﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.ViewModels;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using Shrooms.Premium.Domain.Services.Args;
using Shrooms.Premium.Domain.Services.Lotteries;
using Shrooms.Premium.Presentation.Api.Controllers.Lotteries;
using Shrooms.Premium.Presentation.WebViewModels.Lotteries;
using X.PagedList;

namespace Shrooms.Premium.Tests.Controllers.WebApi
{
    [TestFixture]
    public class LotteryControllerTests
    {
        private LotteryController _lotteryController;

        private IMapper _mapper;
        private ILotteryService _lotteryService;
        private ILotteryExportService _lotteryExportService;

        [SetUp]
        public void TestInitializers()
        {
            _mapper = Substitute.For<IMapper>();
            _lotteryService = Substitute.For<ILotteryService>();
            _lotteryExportService = Substitute.For<ILotteryExportService>();

            _lotteryController = new LotteryController(_mapper, _lotteryService, _lotteryExportService);

            _lotteryController.ControllerContext = Substitute.For<HttpControllerContext>();
            _lotteryController.Request = new HttpRequestMessage();
            _lotteryController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            _lotteryController.Request.SetConfiguration(new HttpConfiguration());
            _lotteryController.RequestContext.Principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("OrganizationId", "1")
            }));
        }

        [Test]
        public void GetAllLotteries_Should_Return_Ok_With_IEnumerable_Of_LotteryDetails_ViewModel()
        {
            // Arrange
            _mapper.Map<IEnumerable<LotteryDetailsDTO>, IEnumerable<LotteryDetailsViewModel>>(LotteryDetailsDTO)
                .Returns(LotteryDetailsViewModel);

            _lotteryService.GetLotteries(UserAndOrganizationArg).Returns(LotteryDetailsDTO);

            // Act
            var response = _lotteryController.GetAllLotteries();

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<IEnumerable<LotteryDetailsViewModel>>>(response);
            _lotteryService.Received(1).GetLotteries(UserAndOrganizationArg);
        }

        [Test]
        public async Task GetLottery_Should_Return_Ok()
        {
            // Arrange
            var lotteryViewModel = new LotteryDetailsViewModel
            {
                Id = 2,
                Status = 1,
                Title = "Hello"
            };
            var lotteryDTO = new LotteryDetailsDTO
            {
                Id = 2,
                Status = 1,
                Title = "Hello"
            };

            _mapper.Map<LotteryDetailsDTO, LotteryDetailsViewModel>(lotteryDTO).Returns(lotteryViewModel);

            _lotteryService.GetLotteryDetailsAsync(2, UserAndOrganizationArg).Returns(lotteryDTO);

            // Act
            var response = await _lotteryController.GetLottery(2);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<LotteryDetailsViewModel>>(response);
            await _lotteryService.Received(1).GetLotteryDetailsAsync(2, UserAndOrganizationArg);
        }

        [Test]
        public async Task GetLottery_Should_Return_Unprocessable_Entity_Error()
        {
            // Arrange
            _lotteryService.GetLotteryDetailsAsync(3000, UserAndOrganizationArg).Returns((LotteryDetailsDTO)null);

            // Act
            var response = await _lotteryController.GetLottery(3000);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NegotiatedContentResult<string>>(response);
            await _lotteryService.Received(1).GetLotteryDetailsAsync(3000, UserAndOrganizationArg);
        }

        [Test]
        public void Abort_Should_Return_Ok()
        {
            // Arrange
            _lotteryService
                .AbortLottery(2, UserAndOrganizationArg)
                .Returns(true);

            // Act
            var response = _lotteryController.Abort(2);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            _lotteryService.Received(1).AbortLottery(2, UserAndOrganizationArg);
        }

        [Test]
        public void Abort_Should_Return_Unprocessable_Entity_Error()
        {
            // Arrange
            _lotteryService.AbortLottery(5, UserAndOrganizationArg).Returns(false);

            // Act
            var response = _lotteryController.Abort(5);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NegotiatedContentResult<string>>(response);
            _lotteryService.Received(1).AbortLottery(5, UserAndOrganizationArg);
        }

        [Test]
        public async Task CreateLottery_Should_Return_Invalid_Model_State()
        {
            // Arrange
            var lotteryViewModel = new CreateLotteryViewModel
            {
                Title = "test"
            };
            var lotteryDTO = new LotteryDTO
            {
                Title = "test"
            };
            _mapper.Map<CreateLotteryViewModel, LotteryDTO>(lotteryViewModel)
                .Returns(lotteryDTO);

            // Act
            _lotteryController.ModelState.AddModelError("model", "error");

            var response = await _lotteryController.CreateLottery(lotteryViewModel);

            // Assert
            Assert.IsInstanceOf<InvalidModelStateResult>(response);
            await _lotteryService.DidNotReceive().CreateLottery(lotteryDTO);
        }

        [Test]
        public async Task CreateLottery_Should_Return_Ok()
        {
            // Arrange
            var lotteryViewModel = new CreateLotteryViewModel
            {
                Title = "test"
            };
            var lotteryDTO = new LotteryDTO
            {
                Title = "test"
            };
            _mapper.Map<CreateLotteryViewModel, LotteryDTO>(lotteryViewModel)
                .Returns(lotteryDTO);

            // Act
            var response = await _lotteryController.CreateLottery(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).CreateLottery(lotteryDTO);
        }

        [Test]
        public async Task CreateLottery_Should_Return_Bad_Request()
        {
            // Arrange
            var lotteryViewModel = new CreateLotteryViewModel
            {
                Title = "test"
            };
            var lotteryDTO = new LotteryDTO
            {
                Title = "test"
            };
            _mapper.Map<CreateLotteryViewModel, LotteryDTO>(lotteryViewModel)
                .Returns(lotteryDTO);
            _lotteryService.CreateLottery(lotteryDTO).Throws(new LotteryException("Exception"));

            // Act
            var response = await _lotteryController.CreateLottery(lotteryViewModel);

            // Assert
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).CreateLottery(lotteryDTO);
        }

        [Test]
        public async Task BuyLotteryTicket_Should_Return_Ok()
        {
            // Arrange
            var ticketViewModel = new BuyLotteryTicketViewModel
            {
                LotteryId = 1,
                Tickets = 5
            };
            var ticketDTO = new BuyLotteryTicketDTO
            {
                LotteryId = 1,
                Tickets = 5
            };
            _mapper.Map<BuyLotteryTicketViewModel, BuyLotteryTicketDTO>(ticketViewModel)
                .Returns(ticketDTO);

            // Act
            var response = await _lotteryController.BuyLotteryTicket(ticketViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).BuyLotteryTicketAsync(ticketDTO, UserAndOrganizationArg);
        }

        [Test]
        public async Task BuyLotteryTicket_Should_Return_Bad_Request()
        {
            // Arrange
            var ticketViewModel = new BuyLotteryTicketViewModel
            {
                LotteryId = 1,
                Tickets = 5
            };
            var ticketDTOModel = new BuyLotteryTicketDTO
            {
                LotteryId = 1,
                Tickets = 5
            };
            _mapper.Map<BuyLotteryTicketViewModel, BuyLotteryTicketDTO>(ticketViewModel)
                .Returns(ticketDTOModel);

            _lotteryService.BuyLotteryTicketAsync(ticketDTOModel, UserAndOrganizationArg).Throws(new LotteryException("Exception"));

            // Act
            var response = await _lotteryController.BuyLotteryTicket(ticketViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).BuyLotteryTicketAsync(ticketDTOModel, UserAndOrganizationArg);
        }

        [Test]
        public async Task GetPagedLotteries_Should_Return_Ok()
        {
            // Arrange
            var args = new GetPagedLotteriesArgs
            {
                Filter = "",
                PageNumber = 1,
                PageSize = 10,
                UserOrg = _userAndOrganization
            };

            var pagedListAsync = await LotteryDetailsDTO.ToPagedListAsync(args.PageNumber, args.PageSize);
            _lotteryService.GetPagedLotteriesAsync(args).Returns(pagedListAsync);

            // Act
            var response = await _lotteryController.GetPagedLotteries("", 1, 10);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<PagedViewModel<LotteryDetailsDTO>>>(response);
            await _lotteryService.Received(1).GetPagedLotteriesAsync(Arg.Any<GetPagedLotteriesArgs>());
        }

        [Test]
        public void RefundParticipants_Should_Return_Ok()
        {
            // Arrange

            // Act
            var response = _lotteryController.RefundParticipants(1337);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            _lotteryService.Received(1).RefundParticipants(1337, UserAndOrganizationArg);
        }

        [Test]
        public async Task FinishLottery_Should_Return_Ok()
        {
            // Arrange

            // Act
            var response = await _lotteryController.FinishLottery(37);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            await _lotteryService.Received(1).FinishLotteryAsync(37, UserAndOrganizationArg);
        }

        [Test]
        public async Task FinishLottery_Should_Return_Bad_Request()
        {
            // Arrange
            _lotteryService.FinishLotteryAsync(37, UserAndOrganizationArg).Throws(new LotteryException("Exception"));

            // Act
            var response = await _lotteryController.FinishLottery(37);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            await _lotteryService.Received(1).FinishLotteryAsync(37, UserAndOrganizationArg);
        }

        [Test]
        public void UpdateDrafted_Should_Return_Ok()
        {
            // Arrange
            var lotteryViewModel = new EditDraftedLotteryViewModel
            {
                Id = 31,
                Title = "Hello"
            };

            var lotteryDto = new LotteryDTO
            {
                Id = 31,
                Title = "Hello"
            };
            _mapper.Map<EditDraftedLotteryViewModel, LotteryDTO>(lotteryViewModel)
                .Returns(lotteryDto);

            // Act
            var response = _lotteryController.UpdateDrafted(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            _lotteryService.Received(1).EditDraftedLotteryAsync(lotteryDto);
        }

        [Test]
        public void UpdateDrafted_Should_Return_Bad_Request()
        {
            // Arrange
            var lotteryViewModel = new EditDraftedLotteryViewModel
            {
                Id = 31,
                Title = "Hello"
            };

            var lotteryDto = new LotteryDTO
            {
                Id = 31,
                Title = "Hello"
            };
            _mapper.Map<EditDraftedLotteryViewModel, LotteryDTO>(lotteryViewModel)
                .Returns(lotteryDto);

            _lotteryService.When(x => x.EditDraftedLotteryAsync(lotteryDto))
                .Do(_ => throw new LotteryException("Exception"));

            // Act
            var response = _lotteryController.UpdateDrafted(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            _lotteryService.Received(1).EditDraftedLotteryAsync(lotteryDto);
        }

        [Test]
        public void UpdateStarted_Should_Return_Ok()
        {
            // Arrange
            var lotteryViewModel = new EditStartedLotteryViewModel
            {
                Id = 31
            };

            var lotteryDto = new EditStartedLotteryDTO
            {
                Id = 31
            };
            _mapper.Map<EditStartedLotteryViewModel, EditStartedLotteryDTO>(lotteryViewModel)
                .Returns(lotteryDto);

            // Act
            var response = _lotteryController.UpdateStarted(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkResult>(response);
            _lotteryService.Received(1).EditStartedLotteryAsync(lotteryDto);
        }

        [Test]
        public void UpdateStarted_Should_Return_Bad_Request()
        {
            // Arrange
            var lotteryViewModel = new EditStartedLotteryViewModel
            {
                Id = 31
            };

            var lotteryDto = new EditStartedLotteryDTO
            {
                Id = 31
            };
            _mapper.Map<EditStartedLotteryViewModel, EditStartedLotteryDTO>(lotteryViewModel)
                .Returns(lotteryDto);
            _lotteryService.When(x => x.EditStartedLotteryAsync(lotteryDto))
                .Do(_ => throw new LotteryException("Exception"));

            // Act
            var response = _lotteryController.UpdateStarted(lotteryViewModel);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(response);
            _lotteryService.Received(1).EditStartedLotteryAsync(lotteryDto);
        }

        [Test]
        public async Task LotteryStats_Should_Return_Ok()
        {
            // Arrange
            var lotteryStats = new LotteryStatsDTO
            {
                KudosSpent = 60,
                TicketsSold = 30,
                TotalParticipants = 15
            };

            _lotteryService.GetLotteryStatsAsync(13, UserAndOrganizationArg).Returns(lotteryStats);

            // Act
            var response = await _lotteryController.LotteryStats(13);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<OkNegotiatedContentResult<LotteryStatsDTO>>(response);
            await _lotteryService.Received(1).GetLotteryStatsAsync(13, UserAndOrganizationArg);
        }

        [Test]
        public async Task LotteryStats_Should_Return_Unprocessable_Entity_Error()
        {
            // Arrange
            _lotteryService.GetLotteryStatsAsync(13, UserAndOrganizationArg).Returns((LotteryStatsDTO)null);

            // Act
            var response = await _lotteryController.LotteryStats(13);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsInstanceOf<NegotiatedContentResult<string>>(response);
            await _lotteryService.Received(1).GetLotteryStatsAsync(13, UserAndOrganizationArg);
        }

        private IEnumerable<LotteryDetailsDTO> LotteryDetailsDTO => new List<LotteryDetailsDTO>
        {
            new LotteryDetailsDTO { Id = 1, Status = 2, EndDate = DateTime.Now.AddDays(2), Title = "Monitor", EntryFee = -5 },
            new LotteryDetailsDTO { Id = 2, Status = 2, EndDate = DateTime.Now.AddDays(-5), Title = "Computer", EntryFee = 2 },
            new LotteryDetailsDTO { Id = 3, Status = 3, EndDate = DateTime.Now.AddDays(4), Title = "Table", EntryFee = 2 },
            new LotteryDetailsDTO { Id = 4, Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", EntryFee = 5 },
            new LotteryDetailsDTO { Id = 5, Status = 3, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", EntryFee = 5 },
            new LotteryDetailsDTO { Id = 6, Status = 4, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", EntryFee = 5 },
            new LotteryDetailsDTO { Id = 7, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10000 kudos", EntryFee = 5 },
            new LotteryDetailsDTO { Id = 8, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10 000 kudos", EntryFee = 5 }
        };

        private IEnumerable<LotteryDetailsViewModel> LotteryDetailsViewModel => new List<LotteryDetailsViewModel>
        {
            new LotteryDetailsViewModel { Id = 1, Status = 2, EndDate = DateTime.Now.AddDays(2), Title = "Monitor", EntryFee = -5 },
            new LotteryDetailsViewModel { Id = 2, Status = 2, EndDate = DateTime.Now.AddDays(-5), Title = "Computer", EntryFee = 2 },
            new LotteryDetailsViewModel { Id = 3, Status = 3, EndDate = DateTime.Now.AddDays(4), Title = "Table", EntryFee = 2 },
            new LotteryDetailsViewModel { Id = 4, Status = 2, EndDate = DateTime.Now.AddDays(5), Title = "1000 kudos", EntryFee = 5 },
            new LotteryDetailsViewModel { Id = 5, Status = 3, EndDate = DateTime.Now.AddDays(5), Title = "100 kudos", EntryFee = 5 },
            new LotteryDetailsViewModel { Id = 6, Status = 4, EndDate = DateTime.Now.AddDays(5), Title = "10 kudos", EntryFee = 5 },
            new LotteryDetailsViewModel { Id = 7, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10000 kudos", EntryFee = 5 },
            new LotteryDetailsViewModel { Id = 8, Status = 1, EndDate = DateTime.Now.AddDays(5), Title = "10 000 kudos", EntryFee = 5 }
        };

        private readonly UserAndOrganizationDTO _userAndOrganization = new UserAndOrganizationDTO
        {
            OrganizationId = 1,
            UserId = "1"
        };

        private UserAndOrganizationDTO UserAndOrganizationArg => Arg.Is<UserAndOrganizationDTO>(o => o.UserId == _userAndOrganization.UserId && o.OrganizationId == _userAndOrganization.OrganizationId);
    }
}
