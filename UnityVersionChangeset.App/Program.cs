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
using System.Reflection;
using System.Text;

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
                    CreateTableView(data.Result);
                    break;
                
                case "release":
                    CreateTableView(data.Result.Where(x => x.Version.Type == UnityVersion.VersionType.Release));
                    break;
                
                case "alpha":
                    CreateTableView(data.Result.Where(x => x.Version.Type == UnityVersion.VersionType.Alpha));
                    break;
                
                case "beta":
                    CreateTableView(data.Result.Where(x => x.Version.Type == UnityVersion.VersionType.Beta));
                    break;
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
            
            CreateTableView(new []{ data.Result });
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

        private static void CreateTableView(IEnumerable<BuildData> data)
        {
            var row = new StringBuilder();
            const int padding = 5;
            const string versionLabel = "VERSION";
            const string dateLabel = "DATE";
            const string changesetLabel = "CHANGESET";
            var versionColumnWidth = versionLabel.Length + padding * 2;
            var dateColumnWidth = dateLabel.Length + padding * 2;
            var changesetColumnWidth = changesetLabel.Length + padding * 2;

            row.Append('-', versionColumnWidth + dateColumnWidth + changesetColumnWidth + 2);
            var rowStr = row.ToString();
            
            row.Clear();
            row.Append(' ', padding);
            row.Append(versionLabel);
            row.Append(' ', padding);
            row.Append('|');
            row.Append(' ', padding);
            row.Append(dateLabel);
            row.Append(' ', padding);
            row.Append('|');
            row.Append(' ', padding);
            row.Append(changesetLabel);
            row.Append(' ', padding);
            var titleStr = row.ToString();
            
            Console.WriteLine(rowStr);
            
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine(titleStr);
            Console.ResetColor();
            
            Console.WriteLine(rowStr);

            foreach (var item in data)
            {
                var version = item.Version.ToString();
                var date = item.ReleaseDate.ToString("dd/MM/yyyy");
                var changeset = item.ChangeSet;

                row.Clear();
                row.Append(' ', 2);
                row.Append(version);
                row.Append(' ', versionColumnWidth - version.Length);
                row.Append(date);
                row.Append(' ', dateColumnWidth - version.Length);
                row.Append(changeset);
                
                Console.WriteLine(row);
            }
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
            
            builder.Append("\t--show-version=ver\t");
            builder.AppendLine("Show the info about required version of engine");
            
            builder.Append("\t--changeset=ver\t\t");
            builder.AppendLine("Get changeset sum of required version of engine");
            
            Console.WriteLine(builder);
        }

        private static void PrintError(ResultStatus status)
        {
            Console.WriteLine($"Error: {status}");
        }
    }
}