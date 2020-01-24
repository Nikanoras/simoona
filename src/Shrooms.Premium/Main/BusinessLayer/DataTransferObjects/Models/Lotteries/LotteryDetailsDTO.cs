﻿using System;
using Shrooms.EntityModels.Models.Lottery;

namespace Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Lotteries
{
    public class LotteryDetailsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public int EntryFee { get; set; }
        public ImagesCollection Images { get; set; }
        public int Participants { get; set; }
        public bool RefundFailed { get; set; }
    }
}
