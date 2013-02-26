using System;
using System.IO;

namespace Cassette.MSBuild
{
    [Serializable]
    public class CreateBundlesCommand : MarshalByRefObject
    {        
        public CreateBundlesCommand(string source, string bin, string output, bool includeRawFiles, Action<string> logInformation, Action<string> logError)
        {
            this.source = source;
            this.bin = bin;
            this.output = output;
            this.includeRawFiles = includeRawFiles;
            this.logInformation = logInformation;
            this.logError = logError;
        }

        readonly string source;
        readonly string bin;
        readonly string output;
        readonly bool includeRawFiles;
        readonly Action<string> logInformation;
        readonly Action<string> logError;

        public void Execute()
        {
            using (var host = new MSBuildHost(source, bin, output, includeRawFiles, logInformation, logError))
            {
                host.Initialize();
                host.Execute();
            }
        }

        public static void ExecuteInSeparateAppDomain(CreateBundlesCommand command)
        {
            var setup = CreateAppDomainSetup(command);
            var appDomain = AppDomain.CreateDomain("Cassette-MSBuild-AppDomain", null, setup);
            try
            {
                var commandInAppDomain = CreateCommandInAppDomain(command, appDomain);
                commandInAppDomain.Execute();
            }
            finally
            {
                AppDomain.Unload(appDomain);
            }
        }

        static AppDomainSetup CreateAppDomainSetup(CreateBundlesCommand command)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = command.bin,
                ShadowCopyFiles = "true"
            };
            AssignConfigurationFile(command, setup);
            return setup;
        }

        static void AssignConfigurationFile(CreateBundlesCommand command, AppDomainSetup setup)
        {
            var configFilename = Path.Combine(command.source, "web.config");
            if (File.Exists(configFilename))
            {
                setup.ConfigurationFile = configFilename;
            }
        }

        static CreateBundlesCommand CreateCommandInAppDomain(CreateBundlesCommand command, AppDomain appDomain)
        {
            // This is like calling
            //   new CreateBundlesCommand(command.source, command.bin, command.output);
            // but the object lives in the other AppDomain.

            var constructorArguments = new object[] { command.source, command.bin, command.output, command.includeRawFiles };
            
#if NET35
            var objectHandle = Activator.CreateInstanceFrom(
                appDomain,
                typeof(CreateBundlesCommand).Assembly.Location,
                typeof(CreateBundlesCommand).FullName,
                false,
                0,
                null,
                constructorArguments,
                null,
                null,
                null
            );
#else
            var objectHandle = Activator.CreateInstanceFrom(
                appDomain,
                typeof(CreateBundlesCommand).Assembly.Location,
                typeof(CreateBundlesCommand).FullName,
                false,
                0,
                null,
                constructorArguments,
                null,
                null
            );
#endif
            return (CreateBundlesCommand)objectHandle.Unwrap();
        }
    }
}