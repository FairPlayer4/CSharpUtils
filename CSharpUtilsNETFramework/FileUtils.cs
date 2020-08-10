#region Imports

using System;
using System.Windows.Forms;
using CSharpUtilsNETStandard.Utils;
using CSharpUtilsNETStandard.Utils.Extensions.General;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework
{
    [PublicAPI]
    public static class FileUtils
    {
        [NotNull]
        public static string ShowSaveFileDialogAndReturnFileNameOrEmpty([NotNull]string defaultFilename, [NotNull]string defaultFileExtensionWithDotInFront, string fileExtensionFilterName, [NotNull]string dialogTitle, [CanBeNull]string initialDirectory = null)
        {
            defaultFilename = CSharpUtilsNETStandard.Utils.FileUtils.GetValidFilename(defaultFilename);
            string fileName;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = dialogTitle;
                saveFileDialog.DefaultExt = string.Format("*{0}", defaultFileExtensionWithDotInFront);
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.AddExtension = true;
                saveFileDialog.Filter = string.Format("{0} (*{1})|*{1}", fileExtensionFilterName, defaultFileExtensionWithDotInFront);

                saveFileDialog.FileName = defaultFilename;
                saveFileDialog.OverwritePrompt = true;
                saveFileDialog.CreatePrompt = false;
                saveFileDialog.ShowHelp = true;
                saveFileDialog.ValidateNames = true;

                if (initialDirectory != null) saveFileDialog.InitialDirectory = initialDirectory;
                saveFileDialog.FileOk += (sender, args) =>
                {
                    try
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        string acceptedFileName = saveFileDialog.FileName;
                        if (string.IsNullOrWhiteSpace(acceptedFileName) || acceptedFileName.EndsWithOrdinal("\\") || acceptedFileName.EndsWithOrdinal("\\" + defaultFileExtensionWithDotInFront))
                        {
                            string folderName = "";
                            string onlyFileName = "";
                            int lastIndex = acceptedFileName.LastIndexOf('\\');
                            if (lastIndex >= 0)
                            {
                                folderName = acceptedFileName.Substring(0, lastIndex + 1);
                                onlyFileName = acceptedFileName.Substring(lastIndex + 1);
                            }
                            MessageBox.Show($"Please provide a valid file name.\nThe file name \"{onlyFileName}\" cannot be used.\nThe selected folder was \"{folderName}\"", "Invalid File Name", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                            args.Cancel = true;
                        }
                    }
                    catch (ObjectDisposedException e)
                    {
                        Logger.PrintWarning("An error occurred while saving the file.\nIt should still be saved but it may not have a valid file name.", nameof(FileUtils), e);
                    }
                };
                if (saveFileDialog.ShowDialog() != DialogResult.OK) return string.Empty;
                fileName = saveFileDialog.FileName;
            }
            return fileName;
        }
    }

}
