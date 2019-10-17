﻿using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.EntityModels.Models.Lotteries;
using Shrooms.Infrastructure.Logger;

namespace Shrooms.Domain.Services.Lotteries
{
    public class LotteryAbortJob : ILotteryAbortJob
    {
        private readonly IParticipantService _participantService;

        private readonly IKudosService _kudosService;

        private readonly ILogger _logger;

        public LotteryAbortJob(IKudosService kudosService, IParticipantService participantService,
            ILogger logger)
        {
            _kudosService = kudosService;
            _participantService = participantService;
            _logger = logger;
        }

        public void RefundLottery(Lottery lottery, UserAndOrganizationDTO userOrg)
        {
            var usersToRefund = _participantService.GetParticipantsToRefund(lottery.Id);

            foreach (var user in usersToRefund)
            {
                try
                {
                    var totalReturn = user.Tickets * lottery.EntryFee;
                    var kudosLog = new AddKudosLogDTO
                    {
                        ReceivingUserIds = new List<string> { user.UserId },
                        PointsTypeId = 3,
                        MultiplyBy = totalReturn,
                        Comment = FormatComment(lottery, totalReturn),
                        UserId = user.UserId,
                        OrganizationId = 2
                    };

                    _kudosService.RefundLotteryTicket(kudosLog, userOrg);
                    _participantService.SetTicketsAsRefunded(lottery.Id, user.UserId);
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            }
        }

        private string FormatComment(Lottery lottery, int total)
        {
            return $"Refund for lottery {lottery.Title}. Returned {total} kudos.";
        }
    }
}