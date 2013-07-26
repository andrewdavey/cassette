using System;
using System.Diagnostics;
using System.IO;

namespace Cassette.Compass
{
	public class CompassCompiler
	{
		const int CompassTimeoutInMs = 120000;

		readonly string rubyBinDirectory;

		public CompassCompiler(string rubyBinDirectory)
		{
			if (!File.Exists(Path.Combine(rubyBinDirectory, "ruby.exe"))) throw new ArgumentException("Ruby bin path was not valid!", "rubyBinDirectory");

			this.rubyBinDirectory = rubyBinDirectory;
		}

		public void CompileCompass(string projectDirectory)
		{
			// since Cassette will be bundling/minning the output of Compass already, we do not need to have Compass support 'production mode' here
			if (!File.Exists(Path.Combine(projectDirectory, "config.rb"))) throw new ArgumentException("Compass project path was not valid!", "projectFilePath");

			// because we aren't using IronRuby to invoke Compass, we don't get the very nice PAL-based sass import detection that the normal Cassette SASS compiler gets.
			// there does not seem to be a good native way around this, so for the moment we'll use a hack: reference ALL sass files that are partials (start with _) that are siblings or children of the sass file.
			// (in CompileCompass.cs)

			var compassInfo = new ProcessStartInfo();
			compassInfo.WorkingDirectory = rubyBinDirectory;
			compassInfo.FileName = Path.Combine(rubyBinDirectory, "ruby.exe");
			compassInfo.Arguments = " compass compile \"" + projectDirectory + "\"";
			compassInfo.UseShellExecute = false;
			compassInfo.RedirectStandardError = true;
			compassInfo.RedirectStandardOutput = true;

			using (var process = new Process())
			{
				process.StartInfo = compassInfo;
				process.Start();

				if (!process.WaitForExit(CompassTimeoutInMs))
					throw new CompassTimeoutException("Execution of compass at " + rubyBinDirectory + " timed out!");

				if (process.ExitCode != 0)
				{
					var error = compassInfo.RedirectStandardError
									? process.StandardError.ReadToEnd()
									: string.Format("Process exited with code {0}.", process.ExitCode);
					throw new CompassException(string.Format("Compass compilation of '{0}' failed. Output: \r\n{1}", projectDirectory,
															error));
				}
			}
		}
	}
}
