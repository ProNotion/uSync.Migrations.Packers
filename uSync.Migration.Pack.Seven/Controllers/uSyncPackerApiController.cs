using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using uSync.Migration.Pack.Seven.Services;

namespace uSync.Migration.Pack.Seven.Controllers
{
    [PluginController("uSync")]
    public class uSyncPackerApiController : UmbracoAuthorizedApiController
    {
        /// <summary>
        ///  Finder method (so we can programatically find the route)
        /// </summary>
        public bool GetApi() => true;

        /// <summary>
        /// Packs the file exports into a zip file and returns it as a HttpResponseMessage.
        /// </summary>
        /// <returns>The HttpResponseMessage containing the zip file.</returns>
        [HttpPost]
        public HttpResponseMessage MakePack()
        {
            var migrationPackService = new MigrationPackService();
            var zipFilePath = migrationPackService.PackExport();

            byte[] fileData = File.ReadAllBytes(zipFilePath);
            var filename = Path.GetFileName(zipFilePath);

            // Create the result
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);

            // Set the content to the byte array containing the file contents
            result.Content = new ByteArrayContent(fileData);

            // Set the content headers
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = filename
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-zip-compressed");
            result.Content.Headers.Add("x-filename", filename);

            return result;
        }
    }
}