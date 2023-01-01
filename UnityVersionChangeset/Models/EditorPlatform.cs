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

namespace UnityVersionChangeset.Models
{
    /// <summary>
    /// An OS code where Unity editor is running
    /// </summary>
    public enum Platform
    {
        Windows,
        Linux,
        OSX
    }
    
    internal static class EditorPlatformUtil
    {
        internal static Platform PlatformFromString(string platform)
        {
            return platform switch
            {
                "win" => Platform.Windows,
                "linux" => Platform.Linux,
                "osx" => Platform.OSX,
                _ => throw new ArgumentOutOfRangeException(nameof(platform), CheckPossibleValues(platform))
            };
        }
        
        internal static string PlatformToString(Platform platform)
        {
            return platform switch
            {
                Platform.Windows => "win",
                Platform.Linux => "linux",
                Platform.OSX => "osx",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static string CheckPossibleValues(string platform)
        {
            var value = "";
            var possiblePlatformMessage = "";
            
            switch (platform)
            {
                case "windows":
                case "win32":
                    value = "win";
                    break;
                    
                case "mac":
                case "macos":
                    value = "osx";
                    break;
            }

            if (!string.IsNullOrEmpty(value))
                possiblePlatformMessage = $"Maybe you meant \"{value}\"?";

            return $"There is no platform with key \"{platform}\". {possiblePlatformMessage}";
        }
    }
}