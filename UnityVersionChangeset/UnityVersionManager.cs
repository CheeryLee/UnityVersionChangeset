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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityVersionChangeset.Crawlers;
using UnityVersionChangeset.Models;

namespace UnityVersionChangeset
{
    /// <summary>
    /// Static class that works with Unity archive registry
    /// </summary>
    public static class UnityVersionManager
    {
        private static readonly UnityHtmlParser _htmlParser = new();
        private static readonly ModulesParser _modulesParser = new();
        private static readonly Dictionary<UnityVersion, BuildData> _data = new();

        /// <summary>
        /// Get info about all available versions of engine with a cancellation token as an
        /// asynchronous operation. Be accurate: the final list can be too large.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation
        /// </param>
        /// <returns>A list with info</returns>
        [PublicAPI]
        public static async Task<RequestResult<IEnumerable<BuildData>>> GetAllVersionsAsync(CancellationToken cancellationToken = default)
        {
            var status = ResultStatus.Ok;
            
            if (_data.Count == 0)
            {
                var releaseVersions = await _htmlParser.GetReleaseVersions(cancellationToken);
                var alphaVersions = await _htmlParser.GetAlphaVersions(cancellationToken);
                var betaVersions = await _htmlParser.GetBetaVersions(cancellationToken);
                
                if (releaseVersions.Status != ResultStatus.Ok)
                {
                    status = releaseVersions.Status;
                }
                else if (alphaVersions.Status != ResultStatus.Ok)
                {
                    status = alphaVersions.Status;
                }
                else if (betaVersions.Status != ResultStatus.Ok)
                {
                    status = betaVersions.Status;
                }
                else
                {
                    AddToDictionary(releaseVersions.Result);
                    AddToDictionary(alphaVersions.Result);
                    AddToDictionary(betaVersions.Result);
                }
            }
            
            return new RequestResult<IEnumerable<BuildData>>
            {
                Status = status,
                Result = _data.Values
            };

            void AddToDictionary(IEnumerable<BuildData> enumerable)
            {
                foreach (var version in enumerable)
                {
                    if (!_data.ContainsKey(version.Version))
                        _data.Add(version.Version, version);
                }
            }
        }
        
        /// <summary>
        /// Get info about all available versions of engine. Be accurate: the final list can be too large.
        /// </summary>
        /// <returns>A list with info</returns>
        [PublicAPI]
        public static RequestResult<IEnumerable<BuildData>> GetAllVersions()
        {
            return GetAllVersionsAsync().ConfigureAwait(true).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get info about one required version of engine with a cancellation token as an
        /// asynchronous operation.
        /// </summary>
        /// <param name="version">String representation of the version of engine</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation
        /// </param>
        /// <returns>A build information</returns>
        [PublicAPI]
        [NotNull]
        public static async Task<RequestResult<BuildData>> GetVersionAsync([NotNull] string version,
            CancellationToken cancellationToken = default)
        {
            return await GetVersionAsync(new UnityVersion(version), cancellationToken);
        }

        /// <summary>
        /// Get info about one required version of engine with a cancellation token as an
        /// asynchronous operation.
        /// </summary>
        /// <param name="version">The version of engine</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation
        /// </param>
        /// <exception cref="ArgumentNullException">version is null</exception>
        /// <returns>A build information</returns>
        [PublicAPI]
        public static async Task<RequestResult<BuildData>> GetVersionAsync([NotNull] UnityVersion version,
            CancellationToken cancellationToken = default)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version), "Version can't be null");

            var versionsResponse = await GetAllVersionsAsync(cancellationToken);
            
            if (versionsResponse.Status != ResultStatus.Ok)
                return new RequestResult<BuildData>
                {
                    Status = versionsResponse.Status,
                    Result = null
                };
            
            var changeSetResponse = await GetChangeSetAsync(version, cancellationToken);
            
            if (changeSetResponse.Status != ResultStatus.Ok)
                return new RequestResult<BuildData>
                {
                    Status = changeSetResponse.Status,
                    Result = null
                };
            
            return new RequestResult<BuildData>
            {
                Status = _data.TryGetValue(version, out var buildData) ? ResultStatus.Ok : ResultStatus.NotFound,
                Result = buildData
            };
        }
        
        /// <summary>
        /// Get info about one required version of engine
        /// </summary>
        /// <param name="version">String representation of the version of engine</param>
        /// <returns>A build information</returns>
        [PublicAPI]
        public static RequestResult<BuildData> GetVersion([NotNull] string version)
        {
            return GetVersionAsync(version).ConfigureAwait(true).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Get info about one required version of engine
        /// </summary>
        /// <param name="version">The version of engine</param>
        /// <returns>A build information</returns>
        [PublicAPI]
        public static RequestResult<BuildData> GetVersion([NotNull] UnityVersion version)
        {
            return GetVersionAsync(version).ConfigureAwait(true).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Get unique changeset sum of the required version of engine with a cancellation token as an
        /// asynchronous operation.
        /// </summary>
        /// <param name="version">String representation of the version of engine</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation
        /// </param>
        /// <returns>
        /// A changeset sum, if version is valid and data has been gathered. Otherwise an empty string will be returned.
        /// </returns>
        [PublicAPI]
        public static async Task<RequestResult<string>> GetChangeSetAsync([NotNull] string version,
            CancellationToken cancellationToken = default)
        {
            return await GetChangeSetAsync(new UnityVersion(version), cancellationToken);
        }

        /// <summary>
        /// Get unique changeset sum of the required version of engine with a cancellation token as an
        /// asynchronous operation.
        /// </summary>
        /// <param name="version">The version of engine</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation
        /// </param>
        /// <exception cref="ArgumentNullException">version is null</exception>
        /// <returns>
        /// A changeset sum, if version is valid and data has been gathered. Otherwise an empty string will be returned.
        /// </returns>
        [PublicAPI]
        public static async Task<RequestResult<string>> GetChangeSetAsync([NotNull] UnityVersion version,
            CancellationToken cancellationToken = default)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version), "Version can't be null");
            
            var versionsResponse = await GetAllVersionsAsync(cancellationToken);
            
            if (versionsResponse.Status != ResultStatus.Ok)
                return new RequestResult<string>
                {
                    Status = versionsResponse.Status,
                    Result = string.Empty
                };

            if (!_data.TryGetValue(version, out var buildData))
                return new RequestResult<string>
                {
                    Status = ResultStatus.NotFound,
                    Result = string.Empty
                };

            if (!string.IsNullOrEmpty(buildData.ChangeSet))
                return new RequestResult<string>
                {
                    Status = ResultStatus.Ok,
                    Result = buildData.ChangeSet
                };
            
            var changeSetResponse = await _htmlParser.GetChangeSet(version, cancellationToken);
            buildData.ChangeSet = changeSetResponse.Result;

            return new RequestResult<string>
            {
                Status = changeSetResponse.Status,
                Result = changeSetResponse.Result
            };
        }
        
        /// <summary>
        /// Get unique changeset sum of the required version of engine
        /// </summary>
        /// <param name="version">String representation of the version of engine</param>
        /// <returns>
        /// A changeset sum, if version is valid and data has been gathered. Otherwise an empty string will be returned.
        /// </returns>
        [PublicAPI]
        public static RequestResult<string> GetChangeSet([NotNull] string version)
        {
            return GetChangeSetAsync(version).ConfigureAwait(true).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Get unique changeset sum of the required version of engine
        /// </summary>
        /// <param name="version">The version of engine</param>
        /// <returns>
        /// A changeset sum, if version is valid and data has been gathered. Otherwise an empty string will be returned.
        /// </returns>
        [PublicAPI]
        public static RequestResult<string> GetChangeSet([NotNull] UnityVersion version)
        {
            return GetChangeSetAsync(version).ConfigureAwait(true).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get info about all installable modules for the required engine version
        /// </summary>
        /// <param name="version">String representation of the version of engine</param>
        /// <param name="platform">The platform where Unity editor is running</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation
        /// </param>
        /// <returns>A list with modules</returns>
        [PublicAPI]
        public static async Task<RequestResult<IEnumerable<ModuleData>>> GetModulesAsync([NotNull] string version,
            Platform platform, CancellationToken cancellationToken = default)
        {
            return await GetModulesAsync(new UnityVersion(version), platform, cancellationToken);
        }
        
        /// <summary>
        /// Get info about all installable modules for the required engine version with a cancellation token as an
        /// asynchronous operation
        /// </summary>
        /// <param name="version">The version of engine</param>
        /// <param name="platform">The platform where Unity editor is running</param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation
        /// </param>
        /// <exception cref="ArgumentNullException">version is null</exception>
        /// <returns>A list with modules</returns>
        [PublicAPI]
        public static async Task<RequestResult<IEnumerable<ModuleData>>> GetModulesAsync([NotNull] UnityVersion version,
            Platform platform, CancellationToken cancellationToken = default)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version), "Version can't be null");
            
            var versionsResponse = await GetAllVersionsAsync(cancellationToken);

            if (versionsResponse.Status != ResultStatus.Ok)
                return CreateResult(versionsResponse.Status, Enumerable.Empty<ModuleData>());

            if (!_data.TryGetValue(version, out var buildData))
                return CreateResult(ResultStatus.NotFound, Enumerable.Empty<ModuleData>());

            if (buildData.Modules != null)
                return CreateResult(ResultStatus.Ok, buildData.Modules);
            
            var changeSetResponse = await GetChangeSetAsync(version, cancellationToken);
            
            if (changeSetResponse.Status != ResultStatus.Ok)
                return CreateResult(changeSetResponse.Status, Enumerable.Empty<ModuleData>());

            var modulesResponse = await _modulesParser.GetModules(buildData.ChangeSet, 
                platform, cancellationToken);

            if (modulesResponse.Status != ResultStatus.Ok)
                return CreateResult(modulesResponse.Status, Enumerable.Empty<ModuleData>());

            buildData.Modules = modulesResponse.Result.ToList();
            return CreateResult(modulesResponse.Status, modulesResponse.Result.ToList());

            RequestResult<IEnumerable<ModuleData>> CreateResult(ResultStatus status, IEnumerable<ModuleData> result)
            {
                return new RequestResult<IEnumerable<ModuleData>>
                {
                    Status = status,
                    Result = result
                };
            }
        }
        
        /// <summary>
        /// Get info about all installable modules for the required engine version
        /// </summary>
        /// <param name="version">String representation of the version of engine</param>
        /// <param name="platform">The platform where Unity editor is running</param>
        /// <returns>A list with modules</returns>
        [PublicAPI]
        public static RequestResult<IEnumerable<ModuleData>> GetModules([NotNull] string version, Platform platform)
        {
            return GetModulesAsync(new UnityVersion(version), platform).ConfigureAwait(true).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Get info about all installable modules for the required engine version
        /// </summary>
        /// <param name="version">The version of engine</param>
        /// <param name="platform">The platform where Unity editor is running</param>
        /// <returns>A list with modules</returns>
        [PublicAPI]
        public static RequestResult<IEnumerable<ModuleData>> GetModules([NotNull] UnityVersion version, Platform platform)
        {
            return GetModulesAsync(version, platform).ConfigureAwait(true).GetAwaiter().GetResult();
        }
            
        /// <summary>
        /// Clear all cached data from previous requests
        /// </summary>
        [PublicAPI]
        public static void Flush()
        {
            _data.Clear();
        }
    }
}