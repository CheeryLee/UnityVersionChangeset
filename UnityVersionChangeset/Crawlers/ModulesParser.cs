/* MIT License

Copyright (c) 2022 Alexander Pluzhnikov

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityVersionChangeset.Models;

namespace UnityVersionChangeset.Crawlers
{
    internal class ModulesParser
    {
        private readonly string[] _targetInstallerKeys = {
            "TargetSupportInstaller",
            "LinuxEditorTargetInstaller",
            "MacEditorTargetInstaller"
        };

        internal async Task<RequestResult<IEnumerable<ModuleData>>> GetModules([NotNull] string changeSet, Platform platform,
            CancellationToken cancellationToken)
        {
            var data = await GetModulesIni(changeSet, platform, cancellationToken);
            var status = data.Status;
            var versions = new List<ModuleData>();
            
            if (status != ResultStatus.Ok)
                return new RequestResult<IEnumerable<ModuleData>>
                {
                    Status = status,
                    Result = versions
                };

            var header = "";
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(data.Result));
            using var streamReader = new StreamReader(memoryStream);

            while (!streamReader.EndOfStream)
            {
                var line = await streamReader.ReadLineAsync();
                
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                    header = line;

                if (_targetInstallerKeys.Any(x => line.Contains(x)))
                {
                    header = header.Replace("[", "").Replace("]", "");
                    versions.Add(new ModuleData
                    {
                        Id = header.ToLower(),
                        Name = header.Replace('-', ' ')
                    });
                }
            }
            
            return new RequestResult<IEnumerable<ModuleData>>
            {
                Status = status,
                Result = versions
            };
        }

        private async Task<RequestResult<string>> GetModulesIni([NotNull] string changeSet, Platform platform, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(changeSet))
                throw new ArgumentException("Changeset can't be null or empty");
            
            var platformStr = EditorPlatformUtil.PlatformToString(platform);
            var url = $"https://download.unity3d.com/download_unity/{changeSet}/unity-{platformStr}.ini";
            var data = "";
            var status = ResultStatus.Ok;
            
            try
            {
                var response = await NetworkManager.HttpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.OK)
                    data = await response.Content.ReadAsStringAsync();
                else if (response.StatusCode == HttpStatusCode.Unauthorized ||
                         response.StatusCode == HttpStatusCode.NotFound)
                    status = ResultStatus.NotFound;
            }
            catch (HttpRequestException)
            {
                status = ResultStatus.HttpError;
            }
            catch
            {
                status = ResultStatus.UnknownError;
            }
            
            return new RequestResult<string>
            {
                Status = status,
                Result = data
            };
        }
    }
}