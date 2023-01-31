﻿namespace Shrooms.Premium.Presentation.WebViewModels.Events
{
    public class EventTypeViewModel
    {
        public int Id { get; set; }

        public bool IsSingleJoin { get; set; }

        public bool SendWeeklyReminders { get; set; }

        public string Name { get; set; }

        public string SingleJoinGroupName { get; set; }

        public bool IsShownWithMainEvents { get; set; }

        public bool SendEmailToManager { get; set; }

        public bool IsShownInUpcomingEvents { get; set; }

        public bool HasActiveEvents { get; set; }
    }
}
