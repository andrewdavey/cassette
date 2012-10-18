using System.Collections.Generic;

namespace Cassette.RequireJS
{
    /// <summary>
    /// Run by Cassette at application start-up.
    /// Initializes static helpers.
    /// </summary>
    public class StartUpTask : IStartUpTask
    {
        readonly ViewHelperImplementation viewHelper;
        readonly IEnumerable<IConfiguration<RequireJsSettings>> settingsConfigurations;
        readonly RequireJsSettings settings;

        public StartUpTask(ViewHelperImplementation viewHelper, IEnumerable<IConfiguration<RequireJsSettings>> settingsConfigurations, RequireJsSettings settings)
        {
            this.viewHelper = viewHelper;
            this.settingsConfigurations = settingsConfigurations;
            this.settings = settings;
        }

        public void Start()
        {
            settingsConfigurations.Configure(settings);
            ViewHelper.Instance = viewHelper;
        }
    }
}