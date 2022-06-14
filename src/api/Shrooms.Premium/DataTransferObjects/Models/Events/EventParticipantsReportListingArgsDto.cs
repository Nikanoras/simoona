﻿using Shrooms.Contracts.DataTransferObjects.Paging;
using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventParticipantsReportListingArgsDto : BasePagingDto
    {
        public Guid EventId { get; set; }

        public IEnumerable<int> KudosTypeIds { get; set; }

        public IEnumerable<int> EventTypeIds { get; set; }
    }
}
