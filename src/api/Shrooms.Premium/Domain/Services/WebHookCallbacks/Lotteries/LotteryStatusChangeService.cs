﻿using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.Lotteries
{
    public class LotteryStatusChangeService : ILotteryStatusChangeService
    {
        private readonly ISystemClock _systemClock;
        private readonly IUnitOfWork2 _uow;
        
        private readonly IDbSet<Lottery> _lotteriesDbSet;

        public LotteryStatusChangeService(ISystemClock systemClock, IUnitOfWork2 uow)
        {
            _systemClock = systemClock;
            _uow = uow;

            _lotteriesDbSet = uow.GetDbSet<Lottery>();
        }

        public async Task UpdateStartedLotteriesToEndedAsync()
        {
            var lotteriesToUpdate = await _lotteriesDbSet
                .Where(lottery => lottery.Status == (int)LotteryStatus.Started && lottery.EndDate < _systemClock.UtcNow)
                .ToListAsync();

            foreach (var lottery in lotteriesToUpdate)
            {
                lottery.Status = (int)LotteryStatus.Ended;
            }

            await _uow.SaveChangesAsync(false);
        }
    }
}
