﻿using System.Threading.Tasks;

namespace StudioX.Web.Settings
{
    /// <summary>
    /// Define interface to get setting scripts
    /// </summary>
    public interface ISettingScriptManager
    {
        /// <summary>
        /// Gets Javascript that contains setting values.
        /// </summary>
        Task<string> GetScriptAsync();
    }
}