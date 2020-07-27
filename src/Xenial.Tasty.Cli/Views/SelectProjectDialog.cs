using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Terminal.Gui;

using static Xenial.Delicious.Utils.PromiseHelper;

namespace Xenial.Delicious.Cli.Views
{
    public class SelectProjectDialog : OpenDialog
    {
        public SelectProjectDialog(ColorScheme colorScheme)
        {
            AllowedFileTypes = new[]
            {
                "csproj",
                "proj",
                "vbproj",
                "fsproj",
                "exe",
                "dll"
            };
            CanChooseDirectories = false;
            AllowsMultipleSelection = false;
            ColorScheme = colorScheme;
        }

        public static Task<string?> ShowDialogAsync(ColorScheme colorScheme)
            => Promise<string?>((resolve) =>
            {
                var dialog = new SelectProjectDialog(colorScheme);
                Application.Run(dialog);
                if (!dialog.Canceled)
                {
                    var filePath = dialog.FilePath;
                    if (filePath != null)
                    {
                        var path = filePath.ToString();
                        if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                        {
                            resolve(path);
                            return;
                        }
                    }
                }
                resolve(null);
            });
    }
}
