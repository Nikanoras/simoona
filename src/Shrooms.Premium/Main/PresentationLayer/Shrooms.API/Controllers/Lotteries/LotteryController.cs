﻿using AutoMapper;
using PagedList;
using Shrooms.API.Controllers;
using Shrooms.API.Filters;
using Shrooms.Constants.Authorization.Permissions;
using Shrooms.Constants.WebApi;
using Shrooms.DataTransferObjects.Models.Lotteries;
using Shrooms.Domain.Services.Lotteries;
using Shrooms.DomainExceptions.Exceptions.Lotteries;
using Shrooms.WebViewModels.Models;
using Shrooms.WebViewModels.Models.Lotteries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace Shrooms.API.Controllers.Lotteries
{
    [Authorize]
    [RoutePrefix("Lottery")]
    public class LotteryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ILotteryService _lotteryService;

        public LotteryController(IMapper mapper, ILotteryService lotteryService)
        {
            _mapper = mapper;
            _lotteryService = lotteryService;
        }

        [Route("All")]
        public IHttpActionResult GetAllLotteries()
        {
            var lotteriesDTO = _lotteryService.GetLotteries(GetUserAndOrganization());

            var result = _mapper.Map<IEnumerable<LotteryDetailsDTO>, IEnumerable<LotteryDetailsViewModel>>(lotteriesDTO);

            return Ok(result);
        }
        [HttpGet]
        [Route("Paged")]
        [PermissionAuthorize(Permission = AdministrationPermissions.Lottery)]
        public PagedViewModel<LotteryDetailsDTO> GetPagedLotteries(string filter = "", int page = 1, int pageSize = ConstWebApi.DefaultPageSize)
        {
            var pagedLotteries = _lotteryService.GetPagedLotteries(filter, page);

            var pagedModel = new PagedViewModel<LotteryDetailsDTO>
            {
                PagedList = pagedLotteries,
                PageCount = pagedLotteries.PageCount,
                ItemCount = pagedLotteries.TotalItemCount,
                PageSize = pageSize
            };

            return pagedModel;
        }

        [HttpGet]
        [Route("Details")]
        public IHttpActionResult GetLottery(int id)
        {
            try
            {
                var lotteryDTO = _lotteryService.GetLotteryDetails(id, GetUserAndOrganization());

                var lotteryViewModel = _mapper.Map<LotteryDetailsDTO, LotteryDetailsViewModel>(lotteryDTO);

                return Ok(lotteryViewModel);

            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }

           
        }

        [HttpPost]
        [Route("Create")]
        [InvalidateCacheOutput("Get", typeof(LotteryWidgetController))]
        public async Task<IHttpActionResult> CreateLottery(CreateLotteryViewModel lotteryViewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createLotteryDTO = _mapper.Map<CreateLotteryViewModel, CreateLotteryDTO>(lotteryViewModel);
            SetOrganizationAndUser(createLotteryDTO);
            try
            {
                var createLottery = await _lotteryService.CreateLottery(createLotteryDTO);
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }

        [HttpPost]
        [Route("Enter")]
        public async Task<IHttpActionResult> BuyLotteryTicket(BuyLotteryTicketViewModel lotteryTickets)
        {
            try
            {
                var buyLotterTicketDTO = _mapper.Map<BuyLotteryTicketViewModel, BuyLotteryTicketDTO>(lotteryTickets);

                await _lotteryService.BuyLotteryTicketAsync(buyLotterTicketDTO, GetUserAndOrganization());

                return Ok();

            }
            catch (LotteryException ex)
            {

                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("Delete")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                _lotteryService.RemoveLottery(id, GetUserAndOrganization());
                return Ok();
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        [Route("UpdateDrafted")]
        public IHttpActionResult UpdateDrafted(EditDraftedLotteryViewModel editLotteryViewModel)
        {
            try
            {
                var editDraftedLotteryDTO = _mapper.Map<EditDraftedLotteryViewModel, EditDraftedLotteryDTO>(editLotteryViewModel);
                SetOrganizationAndUser(editDraftedLotteryDTO);
                _lotteryService.EditDraftedLottery(editDraftedLotteryDTO);

                return Ok();
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Route("UpdateStarted")]
        public IHttpActionResult UpdateStarted(EditStartedLotteryViewModel editLotteryViewModel)
        {
            try
            {
                var editStartedLotteryDTO = _mapper.Map<EditStartedLotteryViewModel, EditStartedLotteryDTO>(editLotteryViewModel);
                SetOrganizationAndUser(editStartedLotteryDTO);
                _lotteryService.EditStartedLottery(editStartedLotteryDTO);
                return Ok();
            }
            catch (LotteryException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPatch]
        [Route("Finish")]
        public async Task<IHttpActionResult> FinishLottery(int id)
        {
            try
            {
                await _lotteryService.FinishLotteryAsync(id);

                return Ok();

            }
            catch (LotteryException ex)
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [Route("{id}/Stats")]
        public IHttpActionResult LotteryStats(int id)
        {
            try
            {
                var lotteryStats = _lotteryService.GetLotteryStats(id);

                return Ok(lotteryStats);
            }
            catch (LotteryException ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}
