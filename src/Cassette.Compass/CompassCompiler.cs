using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Cassette.Compass
{
	public class CompassCompiler
	{
		const int CompassTimeoutInMs = 120000;

		readonly string compassPath;

		public CompassCompiler(string compassPath)
		{
			if (!File.Exists(compassPath)) throw new ArgumentException("Compass path was not valid!", "compassPath");

			this.compassPath = compassPath;
		}

		public void CompileCompass(string projectFilePath)
		{
			// since Cassette will be bundling/minning the output of Compass already, we do not need to have Compass support 'production mode' here
			if (!File.Exists(projectFilePath)) throw new ArgumentException("Compass project path was not valid!", "projectFilePath");

			// because we aren't using IronRuby to invoke Compass, we don't get the very nice PAL-based sass import detection that the normal Cassette SASS compiler gets.
			// there does not seem to be a good native way around this, so for the moment we'll use a hack: reference ALL sass files that are partials (start with _) that are siblings or children of the sass file.
			// (in CompileCompass.cs)

			var compassInfo = new ProcessStartInfo();
			compassInfo.FileName = compassPath;
			compassInfo.Arguments = " compile \"" + projectFilePath + "\"";
			compassInfo.UseShellExecute = false;
			compassInfo.RedirectStandardError = true;
			compassInfo.RedirectStandardOutput = true;

			using (var process = new Process())
			{
				process.StartInfo = compassInfo;
				process.Start();

				if (!process.WaitForExit(CompassTimeoutInMs))
					throw new CompassTimeoutException("Execution of compass at " + compassPath + " timed out!");

				if (process.ExitCode != 0)
				{
					var error = compassInfo.RedirectStandardError
									? process.StandardError.ReadToEnd()
									: string.Format("Process exited with code {0}.", process.ExitCode);
					throw new CompassException(string.Format("Compass compilation of '{0}' failed. Output: \r\n{1}", projectFilePath,
															error));
				}
			}
		}
	}
}
