﻿using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Email;
using Shrooms.Premium.Constants;
using Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels;
using Shrooms.Premium.DataTransferObjects.Models.ServiceRequest;
using ServiceRequestModel = Shrooms.DataLayer.EntityModels.Models.ServiceRequest;

namespace Shrooms.Premium.Domain.Services.Email.ServiceRequest
{
    public class ServiceRequestNotificationService : NotificationServiceBase, IServiceRequestNotificationService
    {
        private readonly IDbSet<Organization> _organizationsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<ApplicationRole> _rolesDbSet;
        private readonly IDbSet<ServiceRequestStatus> _serviceRequestStatusDbSet;
        private readonly IDbSet<ServiceRequestModel> _serviceRequestDbSet;

        private readonly IApplicationSettings _appSettings;

        public ServiceRequestNotificationService(IUnitOfWork2 uow,
            IMailingService mailingService,
            IMailTemplate mailTemplate,
            IApplicationSettings appSettings)
            :
            base(appSettings, mailTemplate, mailingService)
        {
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _rolesDbSet = uow.GetDbSet<ApplicationRole>();
            _serviceRequestStatusDbSet = uow.GetDbSet<ServiceRequestStatus>();
            _serviceRequestDbSet = uow.GetDbSet<ServiceRequestModel>();
            _appSettings = appSettings;
        }

        public async Task NotifyAboutNewServiceRequestAsync(CreatedServiceRequestDto createdServiceRequest)
        {
            var newServiceRequest = await _serviceRequestDbSet.SingleAsync(s => s.Id == createdServiceRequest.ServiceRequestId);
            var organizationName = await GetOrganizationNameAsync(newServiceRequest.OrganizationId);
            var emails = await _usersDbSet
                .Where(x => x.ServiceRequestCategoriesAssigned.Any(y => y.Name == newServiceRequest.CategoryName))
                .Where(x => x.Id != newServiceRequest.EmployeeId)
                .Select(x => x.Email)
                .ToListAsync();

            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organizationName);
            var serviceRequestUrl = _appSettings.ServiceRequestUrl(organizationName, newServiceRequest.Id);
            var emailTemplateViewModel = new ServiceRequestEmailTemplateViewModel(userNotificationSettingsUrl,
                newServiceRequest.Title,
                await GetUserFullNameAsync(newServiceRequest.EmployeeId),
                serviceRequestUrl);

            await SendMultipleEmailsAsync(
                emails,
                Resources.Models.ServiceRequest.ServiceRequest.EmailMessageSubject,
                emailTemplateViewModel,
                EmailPremiumTemplateCacheKeys.ServiceRequest);
        }

        public async Task NotifyAboutNewCommentAsync(ServiceRequestCreatedCommentDto createdComment)
        {
            var serviceRequest = await _serviceRequestDbSet.SingleAsync(s => s.Id == createdComment.ServiceRequestId);
            var organizationName = await GetOrganizationNameAsync(serviceRequest.OrganizationId);
            var serviceRequestNotificationRoleId = await GetUserNotificationRoleIdAsync();
            var emails = await _usersDbSet
                .Where(x => x.Roles.Any(y => y.RoleId == serviceRequestNotificationRoleId) ||
                            x.Id == serviceRequest.EmployeeId)
                .Where(x => x.Id != createdComment.CommentedEmployeeId)
                .Select(x => x.Email)
                .ToListAsync();

            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organizationName);
            var serviceRequestUrl = _appSettings.ServiceRequestUrl(organizationName, serviceRequest.Id);
            var emailTemplateViewModel = new ServiceRequestCommentEmailTemplateViewModel(
                userNotificationSettingsUrl,
                serviceRequest.Title,
                await GetUserFullNameAsync(createdComment.CommentedEmployeeId),
                createdComment.CommentContent,
                serviceRequestUrl);

            await SendMultipleEmailsAsync(
                emails,
                Resources.Common.ServiceRequestAdminCommentedSubject,
                emailTemplateViewModel,
                EmailPremiumTemplateCacheKeys.ServiceRequestComment);
        }

        public async Task NotifyAboutServiceRequestStatusUpdateAsync(UpdatedServiceRequestDto updatedRequest, UserAndOrganizationDto userAndOrganizationDto)
        {
            var serviceRequest = _serviceRequestDbSet.Single(s => s.Id == updatedRequest.ServiceRequestId);
            var newStatusName = _serviceRequestStatusDbSet.Where(x => x.Id == serviceRequest.StatusId).Select(x => x.Title).First();
            var organizationName = await GetOrganizationNameAsync(serviceRequest.OrganizationId);
            var email = await _usersDbSet
                .Where(x => x.Id == serviceRequest.EmployeeId)
                .Where(x => x.Id != userAndOrganizationDto.UserId)
                .Select(x => x.Email)
                .FirstOrDefaultAsync();
            if (email == null)
            {
                return;
            }

            var userNotificationSettingsUrl = GetNotificationSettingsUrl(organizationName);
            var serviceRequestUrl = _appSettings.ServiceRequestUrl(organizationName, serviceRequest.Id);

            var emailTemplateViewModel = new ServiceRequestUpdateEmailTemplateViewModel(userNotificationSettingsUrl,
                serviceRequest.Title,
                await GetUserFullNameAsync(userAndOrganizationDto.UserId),
                newStatusName,
                serviceRequestUrl);

            await SendSingleEmailAsync(
                email,
                Resources.Common.ServiceRequestAdminChangedStatusSubject,
                emailTemplateViewModel,
                EmailPremiumTemplateCacheKeys.ServiceRequestUpdate);
        }

        private async Task<string> GetUserFullNameAsync(string userId)
        {
            return await _usersDbSet
                .Where(u => u.Id == userId)
                .Select(u => u.FirstName + " " + u.LastName)
                .FirstAsync();
        }

        private async Task<string> GetOrganizationNameAsync(int? organizationId)
        {
            return await _organizationsDbSet
                .Where(organization => organization.Id == organizationId)
                .Select(organization => organization.ShortName)
                .FirstOrDefaultAsync();
        }

        private async Task<string> GetUserNotificationRoleIdAsync()
        {
            return await _rolesDbSet
                .Where(x => x.Name == Roles.ServiceRequestNotification)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }
    }
}
