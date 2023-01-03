/* MIT License

Copyright (c) 2022 - 2023 Alexander Pluzhnikov

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
using System.Reflection;
using System.Text;
using UnityVersionChangeset.Models;

namespace UnityVersionChangeset.App
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                new Startup(args);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex);
#else
                Console.WriteLine(ex.Message);
#endif
            }
        }
    }
    
    internal class Startup
    {
        internal Startup(IReadOnlyCollection<string> args)
        {
            if (args.Count == 0 || args.Contains("-h") || args.Contains("--help"))
            {
                PrintHelp();
                return;
            }
            
            if (args.Contains("-v") || args.Contains("--version"))
            {
                Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
                return;
            }

            var arg = args.FirstOrDefault(x => x.Contains("--show-versions"));
            if (!string.IsNullOrEmpty(arg))
            {
                PrintVersions(arg);
                return;
            }
            
            arg = args.FirstOrDefault(x => x.Contains("--show-version"));
            if (!string.IsNullOrEmpty(arg))
            {
                PrintOneVersion(arg);
                return;
            }
            
            arg = args.FirstOrDefault(x => x.Contains("--changeset"));
            if (!string.IsNullOrEmpty(arg))
            {
                PrintChangeSet(arg);
            }
            
            arg = args.FirstOrDefault(x => x.Contains("--modules"));
            if (!string.IsNullOrEmpty(arg))
            {
                var platformArg = args.FirstOrDefault(x => x.Contains("--platform"));
                PrintModules(arg, platformArg);
            }
        }

        private static void PrintVersions(string arg)
        {
            var argParts = arg.Split('=');

            if (argParts.Length < 2)
            {
                Console.WriteLine("Can't determine list type");
                PrintHelp();
                return;
            }
            
            Console.WriteLine("Loading data ...");
            var data = UnityVersionManager.GetAllVersions();

            if (data.Status != ResultStatus.Ok)
            {
                PrintError(data.Status);
                return;
            }
            
            switch (argParts[1])
            {
                case "all":
                    Print(data.Result.ToList());
                    break;
                
                case "release":
                    Print(data.Result.Where(x => x.Version.Type == UnityVersion.VersionType.Release).ToList());
                    break;
                
                case "alpha":
                    Print(data.Result.Where(x => x.Version.Type == UnityVersion.VersionType.Alpha).ToList());
                    break;
                
                case "beta":
                    Print(data.Result.Where(x => x.Version.Type == UnityVersion.VersionType.Beta).ToList());
                    break;
            }

            void Print(IList<BuildData> buildData)
            {
                var table = new TableView(new List<string>
                {
                    "VERSION",
                    "DATE"
                },
                buildData.Select(x => new List<string>
                {
                    x.Version.ToString(),
                    x.ReleaseDate.ToString("dd/MM/yyyy")
                }).ToList());
                table.Print();
            }
        }
        
        private static void PrintOneVersion(string arg)
        {
            var argParts = arg.Split('=');

            if (argParts.Length < 2)
            {
                Console.WriteLine("Can't determine version string");
                PrintHelp();
                return;
            }

            Console.WriteLine("Loading data ...");
            var data = UnityVersionManager.GetVersion(argParts[1]);
            
            if (data.Status != ResultStatus.Ok)
            {
                PrintError(data.Status);
                return;
            }
            
            var table = new TableView(new List<string>
            {
                "VERSION",
                "DATE",
                "CHANGESET"
            }, new List<List<string>>
            {
                new()
                {
                    data.Result.Version.ToString(),
                    data.Result.ReleaseDate.ToString("dd/MM/yyyy"),
                    data.Result.ChangeSet
                }
            });
            table.Print();
        }
        
        private static void PrintChangeSet(string arg)
        {
            var argParts = arg.Split('=');

            if (argParts.Length < 2)
            {
                Console.WriteLine("Can't determine version string");
                PrintHelp();
                return;
            }

            Console.WriteLine("Loading data ...");
            var data = UnityVersionManager.GetChangeSet(argParts[1]);
            
            if (data.Status != ResultStatus.Ok)
            {
                PrintError(data.Status);
                return;
            }
            
            Console.WriteLine($"Changeset: {data.Result}");
        }

        private static void PrintModules(string versionArg, string platformArg)
        {
            var versionArgParts = versionArg?.Split('=');
            var platformArgParts = platformArg?.Split('=');

            if (versionArgParts == null || versionArgParts.Length < 2)
            {
                Console.WriteLine("Can't determine version string");
                PrintHelp();
                return;
            }
            
            if (platformArgParts == null || platformArgParts.Length < 2)
            {
                Console.WriteLine("Can't determine platform string");
                PrintHelp();
                return;
            }
            
            Console.WriteLine("Loading data ...");
            var version = new UnityVersion(versionArgParts[1]);
            var platform = EditorPlatformUtil.PlatformFromString(platformArgParts[1]);
            var data = UnityVersionManager.GetModules(version, platform);
            
            if (data.Status != ResultStatus.Ok)
            {
                PrintError(data.Status);
                return;
            }

            var table = new TableView(new List<string>
            {
                "VERSION",
                "PLATFORM",
                "MODULES"
            }, new List<List<string>>
            {
                new()
                {
                    version.ToString(),
                    platform.ToString(),
                    string.Join(", ", data.Result.Select(x => x.Id))
                }
            });
            table.Print();
        }

        private static void PrintHelp()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"UnityVersionChangeset console example app, ver. {Assembly.GetExecutingAssembly().GetName().Version}");
            builder.AppendLine("Parameters:");
            
            builder.Append("\t-h, --help\t\t");
            builder.AppendLine("Show this help");
            
            builder.Append("\t-v, --version\t\t");
            builder.AppendLine("Show app version");
            
            builder.Append("\t--show-versions=type\t");
            builder.AppendLine("List all available versions of engine. \"Type\" argument can take following values:");
            builder.AppendLine("\t\tall\t\tAll versions: released, alpha and beta. Be accurate: the final list can be too large.");
            builder.AppendLine("\t\trelease\t\tReleased versions");
            builder.AppendLine("\t\talpha\t\tAlpha versions");
            builder.AppendLine("\t\tbeta\t\tBeta versions");
            
            builder.Append("\t--show-version=version\t");
            builder.AppendLine("Show the info about required version of engine");
            
            builder.Append("\t--changeset=version\t");
            builder.AppendLine("Get changeset sum of required version of engine");
            
            builder.Append("\t--modules=version\t\t\t");
            builder.AppendLine("List all available installable modules");
            builder.AppendLine("\t\t--platform=[win, linux, osx]\tThe platform where Unity editor is running");
            
            Console.WriteLine(builder);
        }

        private static void PrintError(ResultStatus status)
        {
            Console.WriteLine($"Error: {status}");
        }
    }
}