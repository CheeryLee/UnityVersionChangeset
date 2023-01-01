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
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityVersionChangeset.Models;

namespace UnityVersionChangeset.Crawlers
{
    internal class UnityHtmlParser
    {
        private const string ReleaseArchiveUrl = "https://unity.com/releases/editor/archive";
        private const string BetaUrl = "https://unity.com/releases/editor/beta";
        private const string AlphaUrl = "https://unity.com/releases/editor/alpha";
        private const string ReleaseUrl = "https://unity.com/releases/editor/whats-new";
        
        private const string ReleaseTitleTagPattern = "<div class=\"release-title\">(.*?)<\\/div>";
        private const string ReleaseDateTagPattern = "<div class=\"release-date\">(.*?)<\\/div>";
        private const string AlphaBetaTitleTagPattern = "<td headers=\"view-title-table-column\".*?>(.*?)\\s*?<\\/td>";
        private const string AlphaBetaDateTagPattern = "<td headers=\"view-release-date-table-column\".*?><.*?>(.*?)<\\/time>\\s*?<\\/td>";
        private const string ChangeSetTagPattern = "<div class=\"changeset\">\\s+<div>.*?<\\/div>\\s+<div>(.*?)\\s?<\\/div>";

        internal async Task<RequestResult<string>> GetChangeSet([NotNull] UnityVersion version, CancellationToken cancellationToken)
        {
            if (cancellationToken == CancellationToken.None)
                cancellationToken = NetworkManager.CreateCancellationToken().Token;

            var url = version.Type switch
            {
                UnityVersion.VersionType.Release => ReleaseUrl,
                UnityVersion.VersionType.Alpha => AlphaUrl,
                UnityVersion.VersionType.Beta => BetaUrl,
                _ => throw new ArgumentOutOfRangeException()
            };
            var data = await GetPageContent($"{url}/{version}", cancellationToken);
            
            if (data.Status != ResultStatus.Ok)
                return new RequestResult<string>
                {
                    Status = data.Status,
                    Result = string.Empty
                };
            
            var match = Regex.Match(data.Result, ChangeSetTagPattern);

            if (match.Groups.Count < 2)
                return new RequestResult<string>
                {
                    Status = ResultStatus.RegexNoValue,
                    Result = string.Empty
                };
            
            return new RequestResult<string>
            {
                Status = ResultStatus.Ok,
                Result = match.Groups[1].Value
            };
        }
        
        internal async Task<RequestResult<IEnumerable<BuildData>>> GetReleaseVersions(CancellationToken cancellationToken)
        {
            if (cancellationToken == CancellationToken.None)
                cancellationToken = NetworkManager.CreateCancellationToken().Token;

            return await GetVersions(ReleaseArchiveUrl, ReleaseTitleTagPattern, ReleaseDateTagPattern,
                value =>
                {
                    var titleSplit = value.Split(' ');
                    return titleSplit[1];
                }, cancellationToken);
        }

        internal async Task<RequestResult<IEnumerable<BuildData>>> GetAlphaVersions(CancellationToken cancellationToken)
        {
            if (cancellationToken == CancellationToken.None)
                cancellationToken = NetworkManager.CreateCancellationToken().Token;

            return await GetVersions(AlphaUrl, AlphaBetaTitleTagPattern, AlphaBetaDateTagPattern,
                value => value, cancellationToken);
        }

        internal async Task<RequestResult<IEnumerable<BuildData>>> GetBetaVersions(CancellationToken cancellationToken)
        {
            if (cancellationToken == CancellationToken.None)
                cancellationToken = NetworkManager.CreateCancellationToken().Token;

            return await GetVersions(BetaUrl, AlphaBetaTitleTagPattern, AlphaBetaDateTagPattern,
                value => value, cancellationToken);
        }
        
        private async Task<RequestResult<IEnumerable<BuildData>>> GetVersions([NotNull] string url, [NotNull] string titlePattern,
            [NotNull] string datePattern, Func<string, string> titleFormatter, CancellationToken cancellationToken)
        {
            var data = await GetPageContent(url, cancellationToken);
            var status = data.Status;
            var versions = new List<BuildData>();

            if (status != ResultStatus.Ok)
                return new RequestResult<IEnumerable<BuildData>>
                {
                    Status = status,
                    Result = versions
                };
            
            var titleMatches = Regex.Matches(data.Result, titlePattern);
            var dateMatches = Regex.Matches(data.Result, datePattern);

            if (titleMatches.Count == 0 || dateMatches.Count == 0 || titleMatches.Count != dateMatches.Count)
                return new RequestResult<IEnumerable<BuildData>>
                {
                    Status = ResultStatus.RegexNoValue,
                    Result = versions
                };
            
            for (var i = 0; i < titleMatches.Count; i++)
            {
                var titleMatch = titleMatches[i];
                var dateMatch = dateMatches[i];
                
                if (titleMatch.Groups.Count < 2 || dateMatch.Groups.Count < 2)
                    return new RequestResult<IEnumerable<BuildData>>
                    {
                        Status = ResultStatus.RegexNoValue,
                        Result = versions
                    };
                
                var versionString = titleFormatter?.Invoke(titleMatches[i].Groups[1].Value) ?? string.Empty;
                var version = ParseVersion(versionString);

                if (!DateTime.TryParseExact(dateMatches[i].Groups[1].Value, "MMMM dd, yyyy", new CultureInfo("en-US"), 
                        DateTimeStyles.None, out var date))
                {
                    DateTime.TryParseExact(dateMatches[i].Groups[1].Value, "MMMM d, yyyy", new CultureInfo("en-US"),
                        DateTimeStyles.None, out date);
                }

                versions.Add(new BuildData
                {
                    Version = version,
                    ReleaseDate = date
                });
            }
            
            return new RequestResult<IEnumerable<BuildData>>
            {
                Status = status,
                Result = versions
            };
        }

        [NotNull]
        private UnityVersion ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException(nameof(version), "Version can't be null or empty");

            var versionSplit = version.Split('.');

            if (versionSplit.Length < 3)
                throw new Exception("Version must contain major, minor and patch parts");

            var major = versionSplit[0];
            var minor = versionSplit[1];
            var patch = versionSplit[2];
            var revision = 1;
            var versionType = UnityVersion.VersionType.Release;
            var patchSplit = versionSplit[2].Split(' ');

            if (patchSplit.Length > 1)
            {
                patch = patchSplit[0];
                revision = int.Parse(patchSplit[2]);
                versionType = patchSplit[1] switch
                {
                    "Alpha" => UnityVersion.VersionType.Alpha,
                    "Beta" => UnityVersion.VersionType.Beta,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            return new UnityVersion(int.Parse(major), int.Parse(minor), int.Parse(patch),
                revision, versionType);
        }
        
        private async Task<RequestResult<string>> GetPageContent([NotNull] string url, CancellationToken cancellationToken)
        {
            var data = "";
            var status = ResultStatus.Ok;
            
            try
            {
                var response = await NetworkManager.HttpClient.GetAsync(url, cancellationToken);

                if (response.StatusCode == HttpStatusCode.OK)
                    data = await response.Content.ReadAsStringAsync();
                else if (response.StatusCode == HttpStatusCode.NotFound)
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