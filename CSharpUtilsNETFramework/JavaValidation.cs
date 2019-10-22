#region Imports

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpUtilsNETStandard.Utils;
using CSharpUtilsNETStandard.Utils.Extensions.General;
using JetBrains.Annotations;

#endregion

namespace CSharpUtilsNETFramework
{
    public static class JavaValidation
    {

        private const string LogCategory = nameof(JavaValidation);

        private const string JavaVersionCommandMessage = "The command " + "\"java -version\"";

        private static int MinimumRequiredJavaMajorVersion = 8;

        private static bool AlwaysCheckJavaVersion = false;

        private static int _javaMajorVersionNumber; // Initial value 0

        private static string _javaVersionString = ""; // also contains the Release Date of the Java Version

        private static readonly object Lock = new object();

        public static bool IsJavaValidElseShowMessage()
        {
            lock (Lock)
            {
                if (IsJavaValid()) return true;

                Logger.PrintWarning("A valid Java installation could not be found!", LogCategory);
                ShowMessageThatJavaIsNotValid();
                return false;
            }
        }

        [NotNull]
        public static string GetInstalledJavaVersion()
        {
            lock (Lock)
            {
                if (!AlwaysCheckJavaVersion && !string.IsNullOrWhiteSpace(_javaVersionString)) return _javaVersionString;
                string readVersion = null;
                string readVersionFromStandardOutput = null;
                try
                {
                    Process clientProcess = new Process
                    {
                        StartInfo =
                        {
                            FileName = "java",
                            Arguments = "-version",
                            RedirectStandardOutput = true, // Just for Robustness
                            RedirectStandardError = true, //StandardOutput normally does not return anything => Use StandardError
                            CreateNoWindow = true,
                            UseShellExecute = false
                        }
                    };
                    if (!clientProcess.Start())
                    {
                        Logger.PrintTrace(JavaVersionCommandMessage + " did not work!", LogCategory);
                    }
                    else
                    {
                        readVersion = clientProcess.StandardError.ReadLine();
                        readVersionFromStandardOutput = clientProcess.StandardOutput.ReadLine();
                    }
                }
                catch (Exception ex)
                {
                    Logger.PrintWarning(JavaVersionCommandMessage + " caused an exception!", LogCategory, ex);
                }
                if (string.IsNullOrWhiteSpace(readVersion))
                {
                    if (string.IsNullOrWhiteSpace(readVersionFromStandardOutput))
                    {
                        Logger.PrintTrace(JavaVersionCommandMessage + " worked but did not produce any output!", LogCategory);
                        return "";
                    }
                    Logger.PrintTrace(JavaVersionCommandMessage + " worked but produced output on the wrong output stream! Using the output anyway for robustness in case of changes in the way Java outputs its version.", LogCategory);
                    readVersion = readVersionFromStandardOutput;
                }
                Logger.PrintTrace(JavaVersionCommandMessage + " produced the following output:\n" + readVersion, LogCategory);
                return _javaVersionString = readVersion.FindInBetweenTwoStrings("\"", "\"").Trim();
            }
        }


        //https://www.codeproject.com/Tips/1062118/Find-Java-Version-using-Csharp
        //Useful if you want to check if Java is valid without informing the user.
        private static bool IsJavaValid()
        {
            lock (Lock)
            {
                if (!AlwaysCheckJavaVersion && _javaMajorVersionNumber >= MinimumRequiredJavaMajorVersion) return true; //Avoid rechecking
                string javaVersion = GetInstalledJavaVersion();
                if (string.IsNullOrWhiteSpace(javaVersion)) return false;
                string[] version = javaVersion.Split('.');
                if (!int.TryParse(version[0], out int firstNumber)) return false;
                // First number is the major version since Java 9 otherwise the second number must be checked
                int requiredVersion = MinimumRequiredJavaMajorVersion <= 8 ? 9 : MinimumRequiredJavaMajorVersion;
                if (firstNumber >= requiredVersion)
                {
                    _javaMajorVersionNumber = firstNumber;
                    return true;
                }

                if (version.Length <= 1) return false;
                bool success = int.TryParse(version[1], out int secondNumber);
                _javaMajorVersionNumber = secondNumber;
                //Just in case the version scheme is changed again to 1.MAJOR and for Java 8 which is 1.8
                return success && firstNumber == 1 && secondNumber >= MinimumRequiredJavaMajorVersion;
            }
        }

        private static void ShowMessageThatJavaIsNotValid()
        {
            // Async to avoid problems with tasks that wait for the result of the Java Validation
            Task.Factory.StartNew(() =>
            {
                string javaWebsite = CultureInfo.CurrentUICulture.ToString().StartsWith("de", StringComparison.Ordinal) ? "https://www.java.com/de/download/" : "https://www.java.com/en/download/";
                DialogResult userDecision = MessageBox.Show("A Java Runtime Environment (JRE or JDK) was not found or is too old!\n" +
                                                            "Please download and install latest Java Version to run these services and also to ensure that your system is not vulnerable.\n\n" +
                                                            "If you are getting this message despite using a Java Version " + MinimumRequiredJavaMajorVersion + " or higher then make sure that your environment variables are set correctly.\n" +
                                                            "You can also try to reinstall Java with the official Java Installer which typically solves these issues.\n\n" +
                                                            "Do you want to navigate to the official Java download website \"" + javaWebsite + "\"?",
                    "Java Runtime Validation",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1
                );
                if (userDecision == DialogResult.Yes)
                {
                    Process.Start(javaWebsite);
                }
            });
        }

    }
}
