using System;
using System.Linq;
using IdentityManager2.Api.Models;
using IdentityManager2.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IdentityManager2.Extensions
{
    public static class ModelStateDictionaryExtensions
    {
        public static void AddErrors(this ModelStateDictionary modelState, IdentityManagerResult result)
        {
            if (modelState == null) throw new ArgumentNullException(nameof(modelState));
            if (result == null) throw new ArgumentNullException(nameof(result));

            foreach (var error in result.Errors)
            {
                modelState.AddModelError("", error);
            }
        }

        public static ErrorModel ToError(this ModelStateDictionary modelState)
        {
            if (modelState == null) throw new ArgumentNullException(nameof(modelState));

            return new ErrorModel
            {
                Errors = modelState.GetErrorMessages()
            };
        }

        public static string[] GetErrorMessages(this ModelStateDictionary modelState)
        {
            if (modelState == null) throw new ArgumentNullException(nameof(modelState));

            var errors =
                from error in modelState
                where error.Value.Errors.Any()
                from err in error.Value.Errors
                select String.IsNullOrWhiteSpace(err.ErrorMessage) ? err.Exception.Message : err.ErrorMessage;

            return errors.ToArray();
        }
    }

}
