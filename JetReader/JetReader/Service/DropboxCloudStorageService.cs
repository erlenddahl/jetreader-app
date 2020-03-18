using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Dropbox.Api;
using Dropbox.Api.Files;
using JetReader.DependencyService;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;

namespace JetReader.Service {
    public class DropboxCloudStorageService : ICloudStorageService {

        const string ProgressFilename = "progress.json";

        public bool IsConnected() {
            return !string.IsNullOrEmpty(UserSettings.Synchronization.Dropbox.AccessToken);
        }

        public async Task BackupFile(string sourcePath, string[] path)
        {
            try
            {
                var accessToken = UserSettings.Synchronization.Dropbox.AccessToken;
                var io = IocManager.Container.Resolve<FileService>();

                if (!string.IsNullOrEmpty(accessToken))
                {

                    using (var dbx = new DropboxClient(accessToken))
                    {
                        using (var mem = await io.LoadFileStreamAsync(sourcePath))
                        {
                            await dbx.Files.UploadAsync(
                                $"/{string.Join("/", path)}",
                                WriteMode.Overwrite.Instance,
                                body: mem,
                                mute: true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
            }
        }

        public async Task<List<string>> GetFileList(string path, Func<string, bool> filter = null)
        {
            try
            {
                var accessToken = UserSettings.Synchronization.Dropbox.AccessToken;
                var items = new List<string>();

                if (!string.IsNullOrEmpty(accessToken))
                {

                    using (var dbx = new DropboxClient(accessToken))
                    {
                        var list = await dbx.Files.ListFolderAsync("/backup/", recursive:true);
                        var more = true;
                        while (more)
                        {
                            foreach (var item in list.Entries.Where(i => i.IsFile))
                            {
                                if (filter != null && !filter(item.Name)) continue;
                                items.Add(item.PathDisplay);
                            }
                            more = list.HasMore;
                            if (more)
                            {
                                list = await dbx.Files.ListFolderContinueAsync(list.Cursor);
                            }
                        }
                    }
                }

                return items;
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                return null;
            }
        }

        public async Task<bool> RestoreFile(string toPath, string fromPath)
        {
            try
            {
                var accessToken = UserSettings.Synchronization.Dropbox.AccessToken;

                if (string.IsNullOrEmpty(accessToken)) throw new Exception("No Dropbox access token");

                using (var dbx = new DropboxClient(accessToken))
                using (var response = await dbx.Files.DownloadAsync(fromPath))
                using (var fileStream = File.Create(toPath))
                using (var responseStream = await response.GetContentAsStreamAsync())
                {
                    responseStream.CopyTo(fileStream);
                    return true;
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
                return false;
            }
        }

        public async Task<T> LoadJson<T>(string[] path) {

            try {
                var accessToken = UserSettings.Synchronization.Dropbox.AccessToken;

                if (!string.IsNullOrEmpty(accessToken)) {
                    using (var dbx = new DropboxClient(accessToken)) {
                        using (var response = await dbx.Files.DownloadAsync($"/{string.Join("/", path)}.json")) {
                            var json = await response.GetContentAsStringAsync();
                            return JsonConvert.DeserializeObject<T>(json);
                        }
                    }
                }
            } catch (DropboxException) { } catch (Exception e) {
                Crashes.TrackError(e);
            }


            return default(T);
        }

        public async void SaveJson<T>(T json, string[] path) {

            try {
                var accessToken = UserSettings.Synchronization.Dropbox.AccessToken;

                if (!string.IsNullOrEmpty(accessToken)) {

                    using (var dbx = new DropboxClient(accessToken)) {
                        using (var mem = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json)))) {
                            await dbx.Files.UploadAsync(
                                $"/{string.Join("/", path)}.json",
                                WriteMode.Overwrite.Instance,
                                body: mem,
                                mute: true);
                        }
                    }
                }
            } catch (Exception e) {
                Crashes.TrackError(e);
            }
        }

        public async void DeleteNode(string[] path) {

            try {
                var accessToken = UserSettings.Synchronization.Dropbox.AccessToken;

                if (!string.IsNullOrEmpty(accessToken)) {
                    using (var dbx = new DropboxClient(accessToken)) {
                        await dbx.Files.DeleteV2Async($"/{string.Join("/", path)}");
                    }
                }

            } catch (DropboxException) { } catch (Exception e) {
                Crashes.TrackError(e);
            }
        }

        public async Task<List<T>> LoadJsonList<T>(string[] path) {

            var result = new List<T>();

            try {
                var accessToken = UserSettings.Synchronization.Dropbox.AccessToken;

                if (!string.IsNullOrEmpty(accessToken)) {
                    using (var dbx = new DropboxClient(accessToken)) {

                        var files = await dbx.Files.ListFolderAsync($"/{string.Join("/", path)}");

                        foreach (var file in files.Entries) {

                            var filePath = file.PathLower.Replace(".json", "").Split('/').Where(o => !string.IsNullOrEmpty(o)).ToArray();

                            var bookmark = await LoadJson<T>(filePath);

                            result.Add(bookmark);
                        }
                    }
                }
            } catch (DropboxException) { } catch (Exception e) {
                Crashes.TrackError(e);
            }

            return result;
        }
    }
}