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
using static System.String;

namespace IdentityManager2.Api.Controllers
{
    [Route(Constants.RoleRoutePrefix)]
    [Authorize(Constants.IdMgrAuthPolicy)]
    [ResponseCache(NoStore=true, Location=ResponseCacheLocation.None)]
    public class RolesController : Controller
    {
        private readonly IIdentityManagerService service;

        public RolesController(IIdentityManagerService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }
        
        public IActionResult MethodNotAllowed()
        {
            return StatusCode(405);
        }

        private IdentityManagerMetadata metadata;

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

        // GET api/roles
        [HttpGet, Route("", Name = Constants.RouteNames.GetRoles)]
        public async Task<IActionResult> GetRolesAsync(string filter = null, int start = 0, int count = 100)
        {
            var meta = await GetMetadataAsync();
            if (!meta.RoleMetadata.SupportsListing)
            {
                return MethodNotAllowed();
            }

            var result = await service.QueryRolesAsync(filter, start, count);
            if (result.IsSuccess)
            {
                try
                {
                    return Ok(new RoleQueryResultResource(result.Result, Url, meta.RoleMetadata));
                }
                catch (Exception exp)
                {
                    throw new ArgumentNullException(exp.ToString());
                }
            }

            return BadRequest(result.ToError());
        }

        // POST 
        [HttpPost, Route("", Name = Constants.RouteNames.CreateRole)]
        public async Task<IActionResult> CreateRoleAsync([FromBody]PropertyValue[] properties)
        {
            var meta = await GetMetadataAsync();
            if (!meta.RoleMetadata.SupportsCreate)
            {
                return MethodNotAllowed();
            }

            var errors = ValidateCreateProperties(meta.RoleMetadata, properties);

            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
            }

            if (ModelState.IsValid)
            {
                var result = await service.CreateRoleAsync(properties);
                if (result.IsSuccess)
                {
                    var url = Url.Link(Constants.RouteNames.GetRole, new { subject = result.Result.Subject });

                    var resource = new
                    {
                        Data = new { subject = result.Result.Subject },
                        Links = new { detail = url }
                    };
                    return Created(url, resource);
                }

                ModelState.AddModelError("", errors.ToString());
            }

            return BadRequest(ModelState.ToError());
        }

        [HttpGet("{subject}", Name = Constants.RouteNames.GetRole)]
        public async Task<IActionResult> GetRoleAsync(string subject)
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

            var meta = await GetMetadataAsync();
            if (!meta.RoleMetadata.SupportsListing)
            {
                return MethodNotAllowed();
            }

            var result = await service.GetRoleAsync(subject);

            if (result.IsSuccess)
            {
                if (result.Result == null)
                {
                    return NotFound();
                }

                var response = Ok(new RoleDetailResource(result.Result, Url, meta.RoleMetadata));


                return response;
            }
            return BadRequest(result.ToError());
        }

        [HttpDelete, Route("{subject}", Name = Constants.RouteNames.DeleteRole)]
        public async Task<IActionResult> DeleteRoleAsync(string subject)
        {
            var meta = await GetMetadataAsync();
            if (!meta.RoleMetadata.SupportsDelete)
            {
                return MethodNotAllowed();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ToError());
            }

            var result = await service.DeleteRoleAsync(subject);
            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.ToError());
        }

        [HttpPut, Route("{subject}/properties/{type}", Name = Constants.RouteNames.UpdateRoleProperty)]
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

            ValidateUpdateProperty(meta.RoleMetadata, type, value);

            if (ModelState.IsValid)
            {
                var result = await service.SetRolePropertyAsync(subject, type, value);

                if (result.IsSuccess)
                {
                    return NoContent();
                }

                ModelState.AddErrors(result);
            }

            return BadRequest(ModelState.ToError());
        }

        private IEnumerable<string> ValidateCreateProperties(RoleMetadata roleMetadata, IEnumerable<PropertyValue> properties)
        {
            if (roleMetadata == null) throw new ArgumentNullException(nameof(roleMetadata));
            properties = properties ?? Enumerable.Empty<PropertyValue>();

            var meta = roleMetadata.GetCreateProperties();
            return meta.Validate(properties);
        }

        private void ValidateUpdateProperty(RoleMetadata roleMetadata, string type, string value)
        {
            if (roleMetadata == null) throw new ArgumentNullException(nameof(roleMetadata));

            if (IsNullOrWhiteSpace(type))
            {
                ModelState.AddModelError("", Messages.PropertyTypeRequired);
                return;
            }

            var prop = roleMetadata.UpdateProperties.SingleOrDefault(x => x.Type == type);
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
