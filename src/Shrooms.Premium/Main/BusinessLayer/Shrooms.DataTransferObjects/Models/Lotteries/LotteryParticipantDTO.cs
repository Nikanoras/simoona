﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Lotteries
{
    public class LotteryParticipantDTO
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public int Tickets { get; set; }
    }
}
