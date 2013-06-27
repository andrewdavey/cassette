using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Utilities;

namespace Cassette.MSBuild
{
    [Serializable]
    public class CreateBundlesCommand : MarshalByRefObject
    {
        public CreateBundlesCommand(string source, string bin, string output, string appVirtualPath, bool includeRawFiles, TaskLoggingHelper taskLoggingHelper)
        {
            this.source = source;
            this.bin = bin;
            this.output = output;
            this.appVirtualPath = appVirtualPath;
            this.includeRawFiles = includeRawFiles;
            this.taskLoggingHelper = taskLoggingHelper;
        }

        readonly string source;
        readonly string bin;
        readonly string output;
        readonly string appVirtualPath;
        readonly bool includeRawFiles;
        readonly TaskLoggingHelper taskLoggingHelper;

        public void Execute()
        {
            using (var host = new MSBuildHost(source, bin, output, appVirtualPath, includeRawFiles, taskLoggingHelper))
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

        static Assembly OnCurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                Assembly assembly = Assembly.Load(args.Name);
                if (assembly != null)
                    return assembly;
            }
            catch
            {
                // ignore load error 
            }

            // *** Try to load by filename - split out the filename of the full assembly name
            // *** and append the base path of the original assembly (ie. look in the same dir)
            // *** NOTE: this doesn't account for special search paths but then that never
            //           worked before either.
            string[] Parts = args.Name.Split(',');
            string File = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + Parts[0].Trim() + ".dll";
            return System.Reflection.Assembly.LoadFrom(File);
        }

        static CreateBundlesCommand CreateCommandInAppDomain(CreateBundlesCommand command, AppDomain appDomain)
        {
            var assemblyLocation = typeof(CreateBundlesCommand).Assembly.Location;
            var typeString = typeof(CreateBundlesCommand).FullName;

            // This is like calling
            //   new CreateBundlesCommand(command.source, command.bin, command.output, command.logInformation, command.logError);
            // but the object lives in the other AppDomain.

            var constructorArguments = new object[] { command.source, command.bin, command.output, command.appVirtualPath, command.includeRawFiles, command.taskLoggingHelper };
            
#if NET35
            var objectHandle = Activator.CreateInstanceFrom(
                appDomain,
                assemblyLocation,
                typeString,
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
                assemblyLocation,
                typeString,
                false,
                0,
                null,
                constructorArguments,
                null,
                null
            );
#endif
            var unwrappedObject = objectHandle.Unwrap();

            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainAssemblyResolve;
            var typedObject = (CreateBundlesCommand)unwrappedObject;
            AppDomain.CurrentDomain.AssemblyResolve -= OnCurrentDomainAssemblyResolve;

            return typedObject;
        }


    }

}