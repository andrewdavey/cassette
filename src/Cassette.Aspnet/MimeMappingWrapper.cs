using System.Reflection;
using System.Web;

namespace Cassette.Aspnet
{
	/// <summary>
	/// Wrapper around System.Web.MimeMapping which is an internal class.
	/// </summary>
	public class MimeMappingWrapper
	{
		readonly MethodInfo mimeMappingMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="MimeMappingWrapper"/> class.
		/// </summary>
		public MimeMappingWrapper()
		{
			// MimeMapping is internal to System.Web so we need to get it via reflection
			// http://stackoverflow.com/a/1302366/210370
			var assembly = Assembly.GetAssembly(typeof(HttpApplication));
			var type = assembly.GetType("System.Web.MimeMapping");
			mimeMappingMethod = type.GetMethod("GetMimeMapping", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		}

		/// <summary>
		/// Gets the MIME type of the specified file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <returns>MIME type of the file, or <c>application/octet-stream</c> if the extension is unrecognised</returns>
		public string GetMimeMapping(string fileName)
		{
			var extension = fileName.Substring(fileName.LastIndexOf('.'));
			switch (extension)
			{
				// PNG isn't in the MimeMapping class :(
				case ".png":
					return "image/png";
				default:
					return (string)mimeMappingMethod.Invoke(null, new[] { fileName });
			}
			
		}
	}
}
