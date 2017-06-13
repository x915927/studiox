﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web.Http.Controllers;
using StudioX.Collections.Extensions;
using StudioX.Configuration.Startup;
using StudioX.Dependency;
using StudioX.Runtime.Validation.Interception;

namespace StudioX.WebApi.Validation
{
    public class WebApiActionInvocationValidator : MethodInvocationValidator
    {
        protected HttpActionContext ActionContext { get; private set; }

        private bool isValidatedBefore;

        public WebApiActionInvocationValidator(IValidationConfiguration configuration, IIocResolver iocResolver)
            : base(configuration, iocResolver)
        {

        }

        public void Initialize(HttpActionContext actionContext, MethodInfo methodInfo)
        {
            ActionContext = actionContext;

            SetDataAnnotationAttributeErrors();

            base.Initialize(
                methodInfo,
                GetParameterValues(actionContext, methodInfo)
            );
        }

        protected override void SetDataAnnotationAttributeErrors(object validatingObject)
        {
            SetDataAnnotationAttributeErrors();
        }

        protected virtual void SetDataAnnotationAttributeErrors()
        {
            if (isValidatedBefore || ActionContext.ModelState.IsValid)
            {
                return;
            }

            foreach (var state in ActionContext.ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    ValidationErrors.Add(new ValidationResult(error.ErrorMessage, new[] {state.Key}));
                }
            }

            isValidatedBefore = true;
        }

        protected virtual object[] GetParameterValues(HttpActionContext actionContext, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var parameterValues = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                parameterValues[i] = actionContext.ActionArguments.GetOrDefault(parameters[i].Name);
            }

            return parameterValues;
        }
    }
}