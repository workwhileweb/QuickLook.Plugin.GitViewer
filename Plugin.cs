// Copyright © 2017 Paddy Xu
// 
// This file is part of QuickLook program.
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using QuickLook.Common.Plugin;
using QuickLook.Plugin.HtmlViewer;

// ReSharper disable UnusedMember.Global

namespace QuickLook.Plugin.GitViewer
{
    /// <summary>
    /// C:\Users\TuanAdmin\AppData\Local\Programs\QuickLook\QuickLook.Plugin
    /// </summary>
    public class Plugin : IViewer
    {
        private WebpagePanel _panel;
        private string _file;
        public int Priority => 0;

        public void Init() { }

        public bool CanHandle(string path)
        {
            return Directory.Exists(path) && Directory.Exists(Path.Combine(path,"./.git"));
        }

        public void Prepare(string path, ContextObject context)
        {
            context.PreferredSize = new Size {Width = 800, Height = 600};
        }

        public void View(string path, ContextObject context)
        {
            _panel = new WebpagePanel();
            context.ViewerContent = _panel;
            context.Title = Path.GetFileName(path);
            _file = GenerateHtml(path);
            _panel.NavigateToFile(_file);

            /*_panel.Navigating += (sender, e) =>
            {
                if (e.Uri == null || !e.Uri.IsAbsoluteUri) return;
                e.Cancel = true;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
            };*/
           
            _panel.Dispatcher.Invoke(() => { context.IsBusy = false; }, DispatcherPriority.Loaded);
        }

        public void Cleanup()
        {
            GC.SuppressFinalize(this);

            _panel?.Dispose();
            _panel = null;

            if (_file!=null && Directory.Exists(_file)) Directory.Delete(_file, true);
        }


        private static (string output, string error) Exec2(string exe, string args, string dir)
        {
            try
            {
                return (Exec(exe, args, dir), null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        private static string Exec(string exe, string args, string dir)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = dir,
                }
            };

            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0) throw new Exception(error);
            return result;
        }

        private static string RunGit2(string path)
        {
            var args = new[]{
                "remote -v",
                "status",
                "remote show origin",
                "--no-pager diff --shortstat",
            };
            var result = new StringBuilder();

            foreach (var arg in args)
            {
                var r = Exec2("git", arg, path);
                result.Append(r.output??r.error);
            }
            return result.ToString();
        }

        private static string ConvertToHtmlWithHighlighting(string content)
        {
            // Define CSS styles for syntax highlighting
            const string cssStyles = """
                                     
                                                 <style>
                                                     body { font-family: monospace; background-color: #f4f4f4; padding: 20px; }
                                                     .git-command { color: #005cc5; font-weight: bold; font-size: 1.2em; }
                                                     .file-change { color: #d73a49; font-size: 1.5em; }
                                                     .branch-info { color: #6f42c1; font-size: 1.2em; }
                                                     .status-info { color: #22863a; font-size: 1.2em; }
                                                     a.url { color: #032b6b; text-decoration: underline; font-size: 1.3em; }
                                                     .copy-btn {
                                                         margin-left: 5px;
                                                         cursor: pointer;
                                                         color: #005cc5;
                                                         font-size: 0.9em;
                                                         text-decoration: none;
                                                     }
                                                     .copy-btn:hover {
                                                         text-decoration: underline;
                                                     }
                                                 </style>
                                     """;
            // JavaScript for copying to clipboard
            const string jsScript = """
                                    
                                                <script>
                                                    function copyToClipboard(text) {
                                                        navigator.clipboard.writeText(text).then(() => {
                                                            alert('Copied to clipboard: ' + text);
                                                        }).catch(err => {
                                                            console.error('Failed to copy text: ', err);
                                                        });
                                                    }
                                                </script>
                                    """;

            // Wrap the content in a <pre> tag for preserving whitespace
            var htmlContent = $"<!DOCTYPE html>\n<html>\n<head>\n{cssStyles}{jsScript}\n</head>\n<body>\n<pre>";

            // Split the content into lines
            var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var highlightedLine = line;

                // Highlight Git commands
                if (line.Contains("git"))
                {
                    highlightedLine = $"<span class='git-command'>{line}</span>";
                }

                // Highlight file changes
                if (line.Contains("modified") || line.Contains("insertions") || line.Contains("deletion"))
                {
                    highlightedLine = $"<span class='file-change'>{line}</span>";
                }

                // Highlight branch information
                if (line.Contains("branch"))
                {
                    highlightedLine = $"<span class='branch-info'>{line}</span>";
                }

                // Highlight status information
                if (line.Contains("Your branch") || line.Contains("can be fast-forwarded"))
                {
                    highlightedLine = $"<span class='status-info'>{line}</span>";
                }

                // Highlight URLs
                if (line.Contains("https://"))
                {
                    // Use a regular expression to find URLs in the line
                    highlightedLine = Regex.Replace(line, @"(https?://[^\s]+)", match =>
                    {
                        var url = match.Value;
                        return $"<a class='url' href='{url}'>{url}</a> <span class='copy-btn' onclick=\"copyToClipboard('{url}')\">📋</span>";
                    });
                }

                // Add the processed line to the HTML content
                htmlContent += $"{highlightedLine}\n";
            }

            // Close the HTML tags
            htmlContent += "</pre>\n</body>\n</html>";

            return htmlContent;
        }

        private static string GenerateHtml(string path)
        {
            var file = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.html");
            var content = RunGit2(path);
            File.WriteAllText(file, ConvertToHtmlWithHighlighting(content));
            return file;
        }
    }
}