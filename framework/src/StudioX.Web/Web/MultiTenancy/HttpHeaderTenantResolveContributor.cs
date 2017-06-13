﻿using System.Web;
using StudioX.Dependency;
using StudioX.Extensions;
using StudioX.MultiTenancy;
using Castle.Core.Logging;

namespace StudioX.Web.MultiTenancy
{
    public class HttpHeaderTenantResolveContributor : ITenantResolveContributor, ITransientDependency
    {
        public ILogger Logger { get; set; }

        public HttpHeaderTenantResolveContributor()
        {
            Logger = NullLogger.Instance;
        }

        public int? ResolveTenantId()
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                return null;
            }

            var tenantIdHeader = httpContext.Request.Headers[MultiTenancyConsts.TenantIdResolveKey];
            if (tenantIdHeader.IsNullOrEmpty())
            {
                return null;
            }

            int tenantId;
            return !int.TryParse(tenantIdHeader, out tenantId) ? (int?) null : tenantId;
        }
    }
}
