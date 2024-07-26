using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Jumoo.uSync.BackOffice;
using Jumoo.uSync.Core.Serializers;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.IO;
using Newtonsoft.Json;
using uSync.Migration.Pack.Seven.Serializers;

namespace uSync.Migration.Pack.Seven.Services
{
    internal class MigrationPackService
    {
        private const string siteFolder = "_site";
        private const string uSyncFolder = "data";

        private string _root;

        public MigrationPackService()
        {
            _root = IOHelper.MapPath("~/uSync/MigrationPacks");
        }

        public string PackExport()
        {
            var id = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            var folder = GetExportFolder(id);

            // grab the things we want to include in the pack 

            // a full uSync export
            CreateExport(Path.Combine(folder, uSyncFolder));
            
            // Add an export of users and user groups
            ExportUsers(folder);
            ExportUserGroups(folder);
            
            // Add an export of members and member groups
            ExportMemberTypes(folder);
            ExportMembers(folder);
            ExportMemberGroups(folder);

            // the grid.config for the site
            GetGridConfig(folder);

            // views
            CopyViews(folder);

            // css and scripts
            CopyCss(folder);
            CopyScripts(folder);

            // make the stream
            var filePath = ZipFolder(folder);

            // clean the folder we used
            CleanFolder(folder);

            return filePath;
        }

        /// <summary>
        /// Get uSync to do a full export. 
        /// </summary>
        private void CreateExport(string folder)
        {
            _ = uSyncBackOfficeContext.Instance.ExportAll(folder);
        }
        
        private void ExportUserGroups(string folder)
        {
            var userService = ApplicationContext.Current.Services.UserService;
            var serializer = new UserSerializer(userService);
            
            // Set the uSync Data directory
            folder = Path.Combine(folder, uSyncFolder);
            
            // Add the UserGroups directory to the folder path
            folder = Path.Combine(folder, "UserGroups");
            
            serializer.SerializeUserGroupsToFile(folder);
        }

        private void ExportUsers(string folder)
        {
            var userService = ApplicationContext.Current.Services.UserService;
            var serializer = new UserSerializer(userService);

            // Set the uSync Data directory
            folder = Path.Combine(folder, uSyncFolder);
            
            // Add the UserGroups directory to the folder path
            folder = Path.Combine(folder, "Users");

            serializer.SerializeUsersToFile(folder);
        }

        private void ExportMemberTypes(string folder)
        {
            var serializer = new MemberSerializer();

            // Set the uSync Data directory
            folder = Path.Combine(folder, uSyncFolder);
            
            // Add the UserGroups directory to the folder path
            folder = Path.Combine(folder, "MemberTypes");

            serializer.SerializeMemberTypes(folder);
        }
        
        private void ExportMembers(string folder)
        {
            var serializer = new MemberSerializer();

            // Set the uSync Data directory
            folder = Path.Combine(folder, uSyncFolder);
            
            // Add the UserGroups directory to the folder path
            folder = Path.Combine(folder, "Members");

            serializer.SerializeMembersToFile(folder);
        }
        
        private void ExportMemberGroups(string folder)
        {
            var serializer = new MemberSerializer();
            
            // Set the uSync Data directory
            folder = Path.Combine(folder, uSyncFolder);
            
            // Add the UserGroups directory to the folder path
            folder = Path.Combine(folder, "MemberGroups");
            
            serializer.SerializeMemberGroupsToFile(folder);
        }

        private void GetGridConfig(string folder)
        {
            var appPlugins = "..\\App_Plugins";
            var configFolder = "..\\Config";
            var debugging = false;

            var gridConfig = UmbracoConfig.For.GridConfig(
                ApplicationContext.Current.ProfilingLogger.Logger,
                ApplicationContext.Current.ApplicationCache.RuntimeCache,
                new DirectoryInfo(appPlugins),
                new DirectoryInfo(configFolder),
                debugging);


            var configJson = JsonConvert.SerializeObject(gridConfig.EditorsConfig.Editors, Formatting.Indented);

            var configFile = Path.Combine(folder, siteFolder, "config", "grid.editors.config.js");

            Directory.CreateDirectory(Path.GetDirectoryName(configFile));   
              
            File.WriteAllText(configFile, configJson);
        }

        private void CopyViews(string folder)
        {
            var viewsRoot = IOHelper.MapPath("~/views");
            var viewsTarget = Path.Combine(folder, siteFolder, "views");

            CopyFolder(viewsRoot, viewsTarget);
        }

        private void CopyCss(string folder) {
            CopyFolder(IOHelper.MapPath("~/css"), Path.Combine(folder, siteFolder, "css"));
        }

        private void CopyScripts(string folder)
        {
            CopyFolder(IOHelper.MapPath("~/scripts"), Path.Combine(folder, siteFolder, "scripts"));
        }

        private void CopyFolder(string sourceFolder, string targetFolder)
        {
            if (!Directory.Exists(sourceFolder)) return;

            var files = Directory.GetFiles(sourceFolder, "*.*");
            Directory.CreateDirectory(targetFolder);
            foreach(var file in files)
            {
                var target = Path.Combine(targetFolder, Path.GetFileName(file));  
                File.Copy(file, target);
            }

            foreach(var folder in Directory.GetDirectories(sourceFolder))
            {
                CopyFolder(folder, Path.Combine(targetFolder, Path.GetFileName(folder)));
            }
        }


        /// <summary>
        ///  Zip the folder up and return a filepath to the saved file 
        /// </summary>
        private string ZipFolder(string folder)
        {
            var filename = $"migration_data_{DateTime.Now.ToString("yyyy_MM_dd_HHmmss")}.zip";
            var folderInfo = new DirectoryInfo(folder);
            var files = folderInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList();

            // Create a new MemoryStream and add the exported files
            var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            { 
                foreach(var file in files)
                {
                    var relativePath = file.FullName.Substring(folder.Length + 1);
                    archive.CreateEntryFromFile(file.FullName, relativePath);
                }
            }

            stream.Seek(0, SeekOrigin.Begin);

            // Create the complete filepath
            var filePath = Path.Combine(_root, filename);
            
            // Write the zip file contents to a new filestream
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                stream.WriteTo(fs);
            }

            // Return the path to the zip file we created
            return filePath;
        }

        private void CleanFolder(string folder)
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
                // it can be locked, and this will throw. 
            }
        }

        /// <summary>
        /// Get the folder path for the export
        /// </summary>
        /// <param name="id">The unique id for the folder</param>
        /// <returns>The full path to the folder to export to</returns>
        private string GetExportFolder(string id) => Path.Combine(_root, id);
    }
}
