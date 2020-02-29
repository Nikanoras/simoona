﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.Domain.ServiceExceptions;
using Shrooms.Premium.DataTransferObjects.Models.Committees;
using Shrooms.Premium.Domain.Services.Committees;
using Shrooms.Premium.Presentation.WebViewModels.Committees;
using Shrooms.Presentation.Api.Controllers;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Premium.Presentation.Api.Controllers
{
    [Authorize]
    public class CommitteesController : AbstractWebApiController<Committee, CommitteeViewModel, CommitteePostViewModel>
    {
        private readonly ICommitteesService _committeesService;

        public CommitteesController(IMapper mapper, IUnitOfWork unitOfWork, ICommitteesService committeesService)
            : base(mapper, unitOfWork, "Created")
        {
            _committeesService = committeesService;
        }

        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public override IEnumerable<CommitteeViewModel> GetAll(int maxResults = 0, string orderBy = null, string includeProperties = null)
        {
            return base.GetAll(maxResults, orderBy, includeProperties);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public override HttpResponseMessage Delete(int id)
        {
            return base.Delete(id);
        }

        [HttpPut]
        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public override HttpResponseMessage Put(CommitteePostViewModel postViewModel)
        {
            var modelDTO = _mapper.Map<CommitteePostViewModel, CommitteePostDTO>(postViewModel);
            try
            {
                if (modelDTO.Name != null && modelDTO.Description != null)
                {
                    _committeesService.PutCommittee(modelDTO);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.NameDescriptionError });
                }
            }
            catch (ServiceException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public HttpResponseMessage KudosCommittee()
        {
            var kudosCommittee = _committeesService.GetKudosCommittee();

            return Request.CreateResponse(HttpStatusCode.OK, kudosCommittee);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public HttpResponseMessage KudosCommitteeId()
        {
            var id = _committeesService.GetKudosCommitteeId();

            return Request.CreateResponse(HttpStatusCode.OK, new { id });
        }

        [HttpPost]
        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public override HttpResponseMessage Post(CommitteePostViewModel postViewModel)
        {
            var modelDTO = _mapper.Map<CommitteePostViewModel, CommitteePostDTO>(postViewModel);
            try
            {
                if (modelDTO.Name != null && modelDTO.Description != null)
                {
                    _committeesService.PostCommittee(modelDTO);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.NameDescriptionError });
                }
            }
            catch (ServiceException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPost]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public HttpResponseMessage PostSuggestion(CommitteeSuggestionPostViewModel postViewModel)
        {
            if (string.IsNullOrWhiteSpace(postViewModel.Title))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.SuggestionTitleError });
            }

            if (string.IsNullOrWhiteSpace(postViewModel.Description))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.SuggestionTitleError });
            }

            if (postViewModel.CommitteeId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.SuggestionCommiteNotFound });
            }

            var modelDTO = _mapper.Map<CommitteeSuggestionPostDTO>(postViewModel);

            try
            {
                _committeesService.PostSuggestion(modelDTO, GetUserAndOrganization().UserId);
            }
            catch (ServiceException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { ex.Message });
            }
            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [PermissionAuthorize(Permission = AdministrationPermissions.Committees)]
        public HttpResponseMessage DeleteSuggestion(int comitteeId, int suggestionId)
        {
            var userAndOrg = GetUserAndOrganization();
            try
            {
                _committeesService.DeleteComitteeSuggestion(comitteeId, suggestionId, userAndOrg);
            }
            catch (ServiceException ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new[] { ex.Message });
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [PermissionAuthorize(Permission = BasicPermissions.Committees)]
        public HttpResponseMessage GetSuggestions(int id)
        {
            if (id == 0)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new[] { Resources.Models.Committee.Committee.SuggestionCommiteNotFound });
            }
            var suggestions = _committeesService.GetCommitteeSuggestions(id);

            return Request.CreateResponse(HttpStatusCode.OK, _mapper.Map<IEnumerable<CommitteeSuggestionViewModel>>(suggestions));
        }
    }
}