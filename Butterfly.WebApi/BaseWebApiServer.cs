﻿/*
 * Copyright 2017 Fireshark Studios, LLC
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using NLog;

namespace Butterfly.WebApi {

    /// <inheritdoc/>
    /// <summary>
    /// Base class implementing <see cref="IWebApiServer"/>. New implementations will normally extend this class.
    /// </summary>
    public abstract class BaseWebApiServer : IWebApiServer {
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected readonly List<WebHandler> webHandlers = new List<WebHandler>();

        public void OnGet(string path, Func<IHttpRequest, IHttpResponse, Task> listener) {
            logger.Debug($"OnGet():path={path}");
            webHandlers.Add(new WebHandler {
                method = HttpMethod.Get,
                path = path,
                listener = listener
            });
        }
        public void OnPost(string path, Func<IHttpRequest, IHttpResponse, Task> listener) {
            logger.Debug($"OnPost():path={path}");
            webHandlers.Add(new WebHandler {
                method = HttpMethod.Post,
                path = path,
                listener = listener
            });
        }

        public static async Task<string[]> FileUploadHandlerAsync(IHttpRequest req, IHttpResponse res, string tempPath, string finalPath, Func<string, string> getFileName, int chunkDelayInMillis = 0) {
            var fileStreamByName = new Dictionary<string, FileStream>();
            var uploadFileNameByName = new Dictionary<string, string>();

            // Parse stream
            try {
                req.ParseAsMultipartStream(
                    onData: (name, fileName, type, disposition, buffer, bytes) => {
                        if (!fileStreamByName.TryGetValue(name, out FileStream fileStream)) {
                            string uploadFileName = getFileName(fileName);
                            string uploadFile = Path.Combine(tempPath, uploadFileName);
                            fileStream = new FileStream(uploadFile, FileMode.CreateNew);
                            uploadFileNameByName[name] = uploadFile;
                            fileStreamByName[name] = fileStream;
                        }
                        fileStream.Write(buffer, 0, bytes);
                        if (chunkDelayInMillis > 0) {
                            Thread.Sleep(chunkDelayInMillis);
                        }
                    }
                );
            }
            catch (Exception e) {
                logger.Error(e);
            }

            // Move files from tempPath to finalPath
            List<string> mediaFileNames = new List<string>();
            foreach (var pair in fileStreamByName) {
                await pair.Value.FlushAsync();
                pair.Value.Close();
                var uploadFileName = uploadFileNameByName[pair.Key];
                var mediaFileName = Path.Combine(finalPath, Path.GetFileName(uploadFileName));
                mediaFileNames.Add(Path.GetFileName(mediaFileName));
                File.Move(uploadFileName, mediaFileName);
            }

            logger.Debug($"FileUploadHandler():Uploaded media files: {string.Join(", ", mediaFileNames)}");
            return mediaFileNames.ToArray();
        }

        public List<WebHandler> WebHandlers {
            get {
                return this.webHandlers;
            }
        }

        public abstract void Start();
        public abstract void Dispose();
    }
}
