﻿using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioX.Configuration;
using StudioX.Dependency;

namespace StudioX.Web.Settings
{
    /// <summary>
    /// This class is used to build setting script.
    /// </summary>
    public class SettingScriptManager : ISettingScriptManager, ISingletonDependency
    {
        private readonly ISettingDefinitionManager settingDefinitionManager;
        private readonly ISettingManager settingManager;

        public SettingScriptManager(ISettingDefinitionManager settingDefinitionManager, ISettingManager settingManager)
        {
            this.settingDefinitionManager = settingDefinitionManager;
            this.settingManager = settingManager;
        }

        public async Task<string> GetScriptAsync()
        {
            var script = new StringBuilder();

            script.AppendLine("(function(){");
            script.AppendLine("    studiox.setting = studiox.setting || {};");
            script.AppendLine("    studiox.setting.values = {");

            var settingDefinitions = settingDefinitionManager
                .GetAllSettingDefinitions()
                .Where(sd => sd.IsVisibleToClients);

            var added = 0;
            foreach (var settingDefinition in settingDefinitions)
            {
                if (added > 0)
                {
                    script.AppendLine(",");
                }
                else
                {
                    script.AppendLine();
                }

                var settingValue = await settingManager.GetSettingValueAsync(settingDefinition.Name);

                script.Append("        '" +
                              settingDefinition.Name .Replace("'", @"\'") + "': " +
                              (settingValue == null ? "null" : "'" + settingValue.Replace(@"\", @"\\").Replace("'", @"\'") + "'"));

                ++added;
            }

            script.AppendLine();
            script.AppendLine("    };");

            script.AppendLine();
            script.Append("})();");

            return script.ToString();
        }
    }
}