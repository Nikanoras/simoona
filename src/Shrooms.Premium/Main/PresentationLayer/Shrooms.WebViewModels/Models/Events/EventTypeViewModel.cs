﻿namespace Shrooms.WebViewModels.Models.Events
{
    public class EventTypeViewModel
    {
        public int Id { get; set; }

        public bool IsSingleJoin { get; set; }

        public string Name { get; set; }

        public bool HasActiveEvents { get; set; }
    }
}