﻿using AutoMapper;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.ViewModels.Notifications;
using Shrooms.Premium.DataTransferObjects.Models.Events;
using Shrooms.Premium.Domain.Services.Notifications;
using Shrooms.Presentation.Api.Hubs;

namespace Shrooms.Premium.Presentation.Api.BackgroundWorkers
{
    public class NewEventNotifier : IBackgroundWorker
    {
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public NewEventNotifier(INotificationService notificationService, IMapper mapper)
        {
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public void Notify(CreateEventDto eventDto, UserAndOrganizationHubDto userAndOrganizationHubDto)
        {
            var notification = _notificationService.CreateForEvent(userAndOrganizationHubDto, eventDto).GetAwaiter().GetResult();

            NotificationHub.SendNotificationToAllUsers(_mapper.Map<NotificationViewModel>(notification), userAndOrganizationHubDto);
        }
    }
}