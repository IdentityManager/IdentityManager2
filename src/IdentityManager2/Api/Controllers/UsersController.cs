using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityManager2.Api.Models;
using IdentityManager2.Core;
using IdentityManager2.Core.Metadata;
using IdentityManager2.Extensions;
using IdentityManager2.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using static System.String;

namespace IdentityManager2.Api.Controllers
{
    [Route(Constants.UserRoutePrefix)]
    [Authorize(Constants.IdMgrAuthPolicy)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UsersController : Controller
    {
        private readonly IIdentityManagerService service;
        private readonly IUrlHelperFactory urlHelperFactory;
        private readonly IActionContextAccessor actionContextAccessor;
        private IdentityManagerMetadata metadata;

        public UsersController(IIdentityManagerService service, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.urlHelperFactory = urlHelperFactory ?? throw new ArgumentNullException(nameof(urlHelperFactory));
            this.actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
        }
        
        public IActionResult MethodNotAllowed()
        {
            return StatusCode(405);
        }

        public async Task<IdentityManagerMetadata> GetMetadataAsync()
        {
            if (metadata == null)
            {
                metadata = await service.GetMetadataAsync();
                if (metadata == null) throw new InvalidOperationException("GetMetadataAsync returned null");
                metadata.Validate();
            }

            return metadata;
        }

        [HttpGet, Route("", Name = Constants.RouteNames.GetUsers)]
        public async Task<IActionResult> GetUsersAsync(string filter = null, int start = 0, int count = 100)
        {
            var result = await service.QueryUsersAsync(filter, start, count);
            if (result.IsSuccess)
            {
                var meta = await GetMetadataAsync();

                var resource = new UserQueryResultResource(result.Result,
                    urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext),
                    meta.UserMetadata);
                return Ok(resource);
            }

            return BadRequest(result.ToError());
        }

        [HttpPost("", Name = Constants.RouteNames.CreateUser)]
        public async Task<IActionResult> CreateUserAsync([FromBody] PropertyValue[] properties)
        {
            var meta = await GetMetadataAsync();
            if (!meta.UserMetadata.SupportsCreate)
            {
                return MethodNotAllowed();
            }

            var errors = ValidateCreateProperties(meta.UserMetadata, properties);

            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var result = await service.CreateUserAsync(properties);
                if (result.IsSuccess)
                {
                    var urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);

                    var url = urlHelper.Link(Constants.RouteNames.GetUser, new { subject = result.Result.Subject });
                    var resource = new
                    {
                        Data = new { subject = result.Result.Subject },
                        Links = new { detail = url }
                    };

                    return Created(url, resource);
                }

                ModelState.AddModelError("", result.ToString());
            }

            return BadRequest(400);
        }

        [HttpGet("{subject}", Name = Constants.RouteNames.GetUser)]
        public async Task<IActionResult> GetUserAsync(string subject)
        {
            if (IsNullOrWhiteSpace(subject))
            {
                ModelState["subject.String"].Errors.Clear();
                ModelState.AddModelError("", Messages.SubjectRequired);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await service.GetUserAsync(subject);
            if (result.IsSuccess)
            {
                if (result.Result == null)
                {
                    return NotFound();
                }

                var meta = await GetMetadataAsync();
                RoleSummary[] roles = null;
                if (!IsNullOrWhiteSpace(meta.RoleMetadata.RoleClaimType))
                {
                    var roleResult = await service.QueryRolesAsync(null, -1, -1);
                    if (!roleResult.IsSuccess)
                    {
                        return BadRequest(roleResult.Errors);
                    }

                    roles = roleResult.Result.Items.ToArray();
                }

                return Ok(new UserDetailResource(result.Result, Url, meta, roles));
            }

            return BadRequest(result.ToError());
        }

        [HttpDelete, Route("{subject}", Name = Constants.RouteNames.DeleteUser)]
        public async Task<IActionResult> DeleteUserAsync(string subject)
        {
            var meta = await GetMetadataAsync();
            if (!meta.UserMetadata.SupportsDelete)
            {
                return MethodNotAllowed();
            }

            if (IsNullOrWhiteSpace(subject))
            {
                ModelState["subject.String"].Errors.Clear();
                ModelState.AddModelError("", Messages.SubjectRequired);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ToError());
            }

            var result = await service.DeleteUserAsync(subject);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        [HttpPut, Route("{subject}/properties/{type}", Name = Constants.RouteNames.UpdateUserProperty)]
        public async Task<IActionResult> SetPropertyAsync(string subject, string type)
        {
            if (IsNullOrWhiteSpace(subject))
            {
                ModelState["subject.String"].Errors.Clear();
                ModelState.AddModelError("", Messages.SubjectRequired);
            }

            type = type.FromBase64UrlEncoded();

            var value = await Request.Body.ReadAsStringAsync();

            var meta = await GetMetadataAsync();
            ValidateUpdateProperty(meta.UserMetadata, type, value);

            if (ModelState.IsValid)
            {
                var result = await service.SetUserPropertyAsync(subject, type, value);
                if (result.IsSuccess)
                {
                    return NoContent();
                }

                ModelState.AddErrors(result);
            }

            return BadRequest(ModelState.ToError());
        }

        [HttpPost, Route("{subject}/claims", Name = Constants.RouteNames.AddClaim)]
        public async Task<IActionResult> AddClaimAsync(string subject, [FromBody] ClaimValue model)
        {
            var meta = await GetMetadataAsync();
            if (!meta.UserMetadata.SupportsClaims)
            {
                return MethodNotAllowed();
            }

            if (IsNullOrWhiteSpace(subject))
            {
                ModelState["subject.String"].Errors.Clear();
                ModelState.AddModelError("", Messages.SubjectRequired);
            }

            if (model == null)
            {
                ModelState.AddModelError("", Messages.ClaimDataRequired);
            }

            if (ModelState.IsValid)
            {
                // ReSharper disable once PossibleNullReferenceException
                var result = await service.AddUserClaimAsync(subject, model.Type, model.Value);
                if (result.IsSuccess)
                {
                    return NoContent();
                }

                ModelState.AddErrors(result);
            }
            
            return BadRequest(ModelState.ToError());
        }

        [HttpDelete, Route("{subject}/claims/{type}/{value}", Name = Constants.RouteNames.RemoveClaim)]
        public async Task<IActionResult> RemoveClaimAsync(string subject, string type, string value)
        {
            type = type.FromBase64UrlEncoded();
            value = value.FromBase64UrlEncoded();

            var meta = await GetMetadataAsync();
            if (!meta.UserMetadata.SupportsClaims)
            {
                return MethodNotAllowed();
            }

            if (IsNullOrWhiteSpace(subject) ||
                IsNullOrWhiteSpace(type) ||
                IsNullOrWhiteSpace(value))
            {
                return NotFound();
            }

            var result = await service.RemoveUserClaimAsync(subject, type, value);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        [HttpPost, Route("{subject}/roles/{role}", Name = Constants.RouteNames.AddRole)]
        public async Task<IActionResult> AddRoleAsync(string subject, string role)
        {
            var meta = await GetMetadataAsync();
            if (IsNullOrWhiteSpace(meta.RoleMetadata.RoleClaimType))
            {
                return MethodNotAllowed();
            }

            if (IsNullOrWhiteSpace(subject))
            {
                return NotFound();
            }

            role = role.FromBase64UrlEncoded();

            var result = await service.AddUserClaimAsync(subject, meta.RoleMetadata.RoleClaimType, role);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        [HttpDelete, Route("{subject}/roles/{role}", Name = Constants.RouteNames.RemoveRole)]
        public async Task<IActionResult> RemoveRoleAsync(string subject, string role)
        {
            var meta = await GetMetadataAsync();
            if (IsNullOrWhiteSpace(meta.RoleMetadata.RoleClaimType))
            {
                return MethodNotAllowed();
            }

            if (IsNullOrWhiteSpace(subject))
            {
                return NotFound();
            }

            role = role.FromBase64UrlEncoded();

            var result = await service.RemoveUserClaimAsync(subject, meta.RoleMetadata.RoleClaimType, role);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        private IEnumerable<string> ValidateCreateProperties(UserMetadata userMetadata, IEnumerable<PropertyValue> properties)
        {
            if (userMetadata == null) throw new ArgumentNullException(nameof(userMetadata));
            properties = properties ?? Enumerable.Empty<PropertyValue>();

            var meta = userMetadata.GetCreateProperties();
            return meta.Validate(properties);
        }

        private void ValidateUpdateProperty(UserMetadata userMetadata, string type, string value)
        {
            if (userMetadata == null) throw new ArgumentNullException(nameof(userMetadata));

            if (IsNullOrWhiteSpace(type))
            {
                ModelState.AddModelError("", Messages.PropertyTypeRequired);
                return;
            }

            var prop = userMetadata.UpdateProperties.SingleOrDefault(x => x.Type == type);
            if (prop == null)
            {
                ModelState.AddModelError("", Format(Messages.PropertyInvalid, type));
            }
            else
            {
                var error = prop.Validate(value);
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                }
            }
        }
    }
}
