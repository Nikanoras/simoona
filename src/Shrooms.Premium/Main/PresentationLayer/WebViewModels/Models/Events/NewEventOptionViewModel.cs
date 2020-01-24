﻿using Shrooms.Constants.BusinessLayer.Events;

namespace Shrooms.Premium.Main.PresentationLayer.WebViewModels.Models.Events
{
    public class NewEventOptionViewModel
    {
        public string Option { get; set; }

        public OptionRules Rule { get; set; }
    }
}
