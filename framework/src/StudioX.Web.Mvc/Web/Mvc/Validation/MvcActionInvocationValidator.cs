﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Web.Mvc;
using StudioX.Collections.Extensions;
using StudioX.Configuration.Startup;
using StudioX.Dependency;
using StudioX.Extensions;
using StudioX.Runtime.Validation.Interception;

namespace StudioX.Web.Mvc.Validation
{
    public class MvcActionInvocationValidator : MethodInvocationValidator
    {
        protected ActionExecutingContext ActionContext { get; private set; }

        private bool isValidatedBefore;

        public MvcActionInvocationValidator(IValidationConfiguration configuration, IIocResolver iocResolver) 
            : base(configuration, iocResolver)
        {

        }

        public void Initialize(ActionExecutingContext actionContext, MethodInfo methodInfo)
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
            if (isValidatedBefore)
            {
                return;
            }

            isValidatedBefore = true;

            var modelState = ActionContext.Controller.As<Controller>().ModelState;
            if (modelState.IsValid)
            {
                return;
            }

            foreach (var state in modelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    ValidationErrors.Add(new ValidationResult(error.ErrorMessage, new[] {state.Key}));
                }
            }
        }

        protected virtual object[] GetParameterValues(ActionExecutingContext actionContext, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var parameterValues = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                parameterValues[i] = actionContext.ActionParameters.GetOrDefault(parameters[i].Name);
            }

            return parameterValues;
        }
    }
}