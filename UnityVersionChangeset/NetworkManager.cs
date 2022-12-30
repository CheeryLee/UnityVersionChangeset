using System;
using System.Net.Http;
using System.Threading;

namespace UnityVersionChangeset
{
    internal static class NetworkManager
    {
        internal static HttpClient HttpClient { get; } = new();
        
        private const int TimeoutSec = 10;
        
        internal static CancellationTokenSource CreateCancellationToken()
        {
            return new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSec));
        }
    }
}