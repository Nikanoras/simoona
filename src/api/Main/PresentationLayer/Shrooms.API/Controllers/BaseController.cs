﻿using System.Web.Http;
using System.Web.Http.Results;
using Microsoft.AspNet.Identity;
using Shrooms.API.Helpers;
using Shrooms.DataTransferObjects.Models;
using Shrooms.Host.Contracts.Exceptions;

namespace Shrooms.API.Controllers
{
    public class BaseController : ApiController
    {
        public StatusCodeResult Forbidden()
        {
            return StatusCode(System.Net.HttpStatusCode.Forbidden);
        }

        public StatusCodeResult UnsupportedMediaType()
        {
            return StatusCode(System.Net.HttpStatusCode.UnsupportedMediaType);
        }

        public IHttpActionResult BadRequestWithError(ValidationException ex)
        {
            return Content(System.Net.HttpStatusCode.BadRequest, new { ErrorCode = ex.ErrorCode, ErrorMessage = ex.ErrorMessage });
        }

        public UserAndOrganizationDTO GetUserAndOrganization()
        {
            return User.Identity.GetUserAndOrganization();
        }

        public UserAndOrganizationHubDto GetUserAndOrganizationHub()
        {
            return new UserAndOrganizationHubDto
            {
                OrganizationId = User.Identity.GetOrganizationId(),
                UserId = User.Identity.GetUserId(),
                OrganizationName = User.Identity.GetOrganizationName()
            };
        }

        public int GetOrganizationId()
        {
            return User.Identity.GetOrganizationId();
        }

        public string GetOrganizationName()
        {
            return User.Identity.GetOrganizationName();
        }

        public void SetOrganizationAndUser(UserAndOrganizationDTO obj)
        {
            obj.OrganizationId = User.Identity.GetOrganizationId();
            obj.UserId = User.Identity.GetUserId();
        }
    }
}