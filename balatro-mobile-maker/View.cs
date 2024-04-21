using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using static balatro_mobile_maker.Constants;
using static balatro_mobile_maker.Tools;

namespace balatro_mobile_maker;

/// <summary>
/// Command line UI for Balatro APK Maker.
/// </summary>
// NOTE: Much should be refactored out of UI logic land, and, into a Controller which we can query state from.
internal class View
{
    private static bool _verboseMode;

    private bool _androidBuild;
    private bool _iosBuild;

    private static bool _cleaup;

    static bool gameProvided;

    /// <summary>
    /// Start CLI operation.
    /// </summary>
    public void Begin()
    {
        Log("====Balatro APK Maker====\n7-Zip is licensed under the GNU LGPL license. Please visit: www.7-zip.org\n\n");

        //Initial prompts
        _cleaup = AskQuestion("Would you like to automatically clean up once complete?");
        _verboseMode = AskQuestion("Would you like to enable extra logging information?");

        //If balatro.apk or balatro.ipa already exists, ask before beginning build process again
        if (!(File.Exists("balatro.apk") || File.Exists("balatro.ipa")) || AskQuestion("A previous build was found... Would you like to build again?"))
        {
            _androidBuild = AskQuestion("Would you like to build for Android?");
            _iosBuild = !_androidBuild && AskQuestion("Would you like to build for iOS (experimental)?");

            #region Download tools
            if (_androidBuild)
            {
                #region Android tools
                //Downloading tools. Handled in threads to allow simultaneous downloads
                Thread[] downloadThreads =
                [
                    //TODO: Platform specific file downloads for OpenJDK and 7-Zip
                    new Thread(() => { TryDownloadFile("OpenJDK", Platform.getOpenJDKDownloadLink(), "openjdk" + Platform.getOpenJDKDownloadExtension()); }),
                    new Thread(() => { TryDownloadFile("7-Zip", Platform.get7ZipDownloadLink(), "7za.exe"); }),

                    new Thread(() => { TryDownloadFile("APKTool", ApktoolLink, "apktool.jar"); }),
                    new Thread(() => { TryDownloadFile("uber-apk-signer", UberapktoolLink, "uber-apk-signer.jar"); }),
                    new Thread(() => { TryDownloadFile("Balatro-APK-Patch", BalatroApkPatchLink, "Balatro-APK-Patch.zip"); }),
                    new Thread(() => { TryDownloadFile("Love2D APK", Love2dApkLink, "love-11.5-android-embed.apk"); })
                ];

                //Start all the downloads
                foreach (var t in downloadThreads) t.Start();

                //Wait for all the downloads to complete
                foreach (var t in downloadThreads) t.Join();

                #endregion
            }

            if (_iosBuild)
            {
                #region iOS Tools
                //Downloading tools. Handled in threads to allow simultaneous downloads
                Thread[] downloadThreads =
                [
                    //TODO: Platform specific file downloads for Python and 7-Zip
                    new Thread(() => { TryDownloadFile("7-Zip", Platform.get7ZipDownloadLink(), "7za.exe"); }),
                    new Thread(() => { TryDownloadFile("Python", PythonWinX64Link, "python.zip"); }),

                    new Thread(() => { TryDownloadFile("iOS Base", IosBaseLink, "balatro-base.ipa"); })
                ];

                //Start all the downloads
                foreach (var t in downloadThreads) t.Start();

                //Wait for all the downloads to complete
                foreach (var t in downloadThreads) t.Join();
                #endregion
            }
            #endregion

            #region Prepare workspace
            #region Find and extract Balatro.exe

            gameProvided = Platform.gameExists();

            if (gameProvided)
                Log("Game found!");
            else
            {
                //Game not provided

                //Try to locate automatically
                if (Platform.tryLocateGame())
                    Log("Game copied!");
                else
                {
                    //Game not provided, and could not be located
                    Log("Could not find Balatro.exe! Please place it in this folder, then try again!");
                    Exit();
                }
            }

            Log("Extracting Balatro.exe...");
            if (Directory.Exists("Balatro"))
            {
                //Delete the Balatro folder if it already exists
                Log("Balatro directory already exists! Deleting Balatro directory...");
                tryDelete("Balatro");
            }

            //Extract Balatro.exe with 7-Zip
            useTool(ProcessTools.SevenZip, "x Balatro.exe -oBalatro");

            //Check for failure
            if (!Directory.Exists("Balatro"))
            {
                Log("Failed to extract Balatro.exe!");
                Exit();
            }
            #endregion

            if (_androidBuild)
            {
                #region Extract APK
                Log("Unpacking Love2D APK with APK Tool...");
                if (Directory.Exists("balatro-apk"))
                {
                    //Delete the balatro-apk folder if it already exists
                    Log("balatro-apk directory already exists! Deleting balatro-apk directory...");
                    tryDelete("balatro-apk");
                }

                //TODO: Prep OpenJDK better
                Log("Preparing OpenJDK...");
                tryDelete("jdk-21.0.3+9");
                useTool(ProcessTools.SevenZip, "x openjdk.zip");

                //Unpack Love2D APK
                useTool(ProcessTools.Java, "-jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" d -s -o balatro-apk love-11.5-android-embed.apk");

                //Check for failure
                if (!Directory.Exists("balatro-apk"))
                {
                    Log("Failed to unpack Love2D APK with APK Tool!");
                    Exit();
                }
                #endregion

                #region APK patch
                Log("Extracting patch zip...");
                if (Directory.Exists("Balatro-APK-Patch"))
                {
                    Log("Balatro-APK-Patch directory already exists! Deleting Balatro-APK-Patch directory...");
                    tryDelete("Balatro-APK-Patch");
                }

                //Extract Balatro-APK-Patch
                useTool(ProcessTools.SevenZip, "x Balatro-APK-Patch.zip -oBalatro-APK-Patch");

                if (!Directory.Exists("Balatro-APK-Patch"))
                {
                    Log("Failed to extract Balatro-APK-Patch");
                    Exit();
                }

                //Base APK patch
                Log("Patching APK folder...");
                CommandLine("xcopy",  "\"Balatro-APK-Patch\\\" \"balatro-apk\\\" /E /H /Y /V");
                #endregion
            }

            if (_iosBuild)
            {
                #region Prepare IPA
                Log("Preparing iOS Base...");
                File.Move("balatro-base.ipa", "balatro-base.zip");
                #endregion
            }

            #endregion

            #region Patch
            Log("Patching...");
            Patching.Begin();
            #endregion

            #region Building

            #region Balatro.exe -> game.love
            Log("Packing Balatro folder...");
            //TODO: Figure out how to NOT do this
            //I struggled to pack something other than the working directory
            //I'm probably dumb for this
            CommandLine("cmd", "/c \"cd Balatro && ..\\7za.exe a balatro.zip && cd ..\"");

            if (!File.Exists("Balatro\\balatro.zip"))
            {
                Log("Failed to pack Balatro folder!");
                Exit();
            }

            Log("Moving archive...");
            if (_androidBuild)
                File.Move("Balatro\\balatro.zip", "balatro-apk\\assets\\game.love");

            if (_iosBuild)
                File.Move("Balatro\\balatro.zip", "game.love");
            #endregion

            if (_androidBuild)
            {
                #region Packing APK
                Log("Repacking APK...");
                useTool(ProcessTools.Java, "-jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" b -o balatro.apk balatro-apk");

                if (!File.Exists("balatro.apk"))
                {
                    Log("Failed to pack Balatro apk!");
                    Exit();
                }
                #endregion

                #region Signing APK
                Log("Signing APK...");
                useTool(ProcessTools.Java, "-jar uber-apk-signer.jar -a balatro.apk");

                if (!File.Exists("balatro-aligned-debugSigned.apk"))
                {
                    Log("Failed to sign APK!");
                    Exit();
                }

                Log("Renaming unsigned apk...");
                File.Move("balatro.apk", "balatro-unsigned.apk");

                Log("Renaming signed apk...");
                File.Move("balatro-aligned-debugSigned.apk", "balatro.apk");
                #endregion
            }

            if (_iosBuild)
            {
                #region Packing IPA
                Log("Extracting Python");
                useTool(ProcessTools.SevenZip, "x python.zip -opython");

                Log("Repacking iOS app...");
                File.WriteAllText("ios.py", Constants.PythonScript);
                useTool(ProcessTools.Python, "ios.py");

                File.Move("balatro-base.zip", "balatro.ipa");
                #endregion
            }
            Log("Build successful!");
            #endregion
        }

        #region Android options
        #region Auto-install
        if (!_iosBuild && File.Exists("balatro.apk") && AskQuestion("Would you like to automaticaly install balatro.apk on your Android device?"))
        {
            PrepareAndroidPlatformTools();

            Log("Attempting to install. If prompted, please allow the USB Debugging connection on your Android device.");
            
            useTool(ProcessTools.ADB, "install balatro.apk");
            useTool(ProcessTools.ADB, "kill-server");
        }
        #endregion

        #region Save transfer
        if (!_iosBuild && File.Exists("balatro.apk") && Directory.Exists(Environment.GetEnvironmentVariable("AppData") + "\\Balatro") && AskQuestion("Would you like to transfer saves from your Steam copy of Balatro to your Android device?"))
        {
            Log("Thanks to TheCatRiX for figuring out save transfers!");

            PrepareAndroidPlatformTools();

            Log("Attempting to transfer saves. If prompted, please allow the USB Debugging connection on your Android device.");

            useTool(ProcessTools.ADB, "shell mkdir /data/local/tmp/balatro");
            useTool(ProcessTools.ADB, "shell mkdir /data/local/tmp/balatro/files");
            useTool(ProcessTools.ADB, "shell mkdir /data/local/tmp/balatro/files/save");
            useTool(ProcessTools.ADB, "shell mkdir /data/local/tmp/balatro/balatro/files/save/game");
            useTool(ProcessTools.ADB, "push \"" + Platform.getGameSaveLocation() + "\\.\" /data/local/tmp/balatro/files/save/game");
            useTool(ProcessTools.ADB, "shell am force-stop com.unofficial.balatro");
            useTool(ProcessTools.ADB, "shell run-as com.unofficial.balatro cp -r /data/local/tmp/balatro/files .");
            useTool(ProcessTools.ADB, "shell rm -r /data/local/tmp/balatro");
            useTool(ProcessTools.ADB, "kill-server");
        }
        else
        {
            if (!_iosBuild && AskQuestion("Would you like to pull saves from your Android device?"))
            {
                Log("Warning! If Steam Cloud is enabled, it will overwrite the save you transfer!");
                while (!AskQuestion("Have you backed up your saves?"))
                    Log("Please back up your saves! I am not responsible if your saves get deleted!");

                PrepareAndroidPlatformTools();

                //TODO: Platform
                Log("Backing up your files...");
                CommandLine("xcopy", "\"%appdata%\\Balatro\\\" \"%appdata%\\BalatroBACKUP\\\" /E /H /Y /V");
                //CommandLine("rmdir \"%appdata%\\Balatro\\\" /S /Q");
                tryDelete(Platform.getGameSaveLocation());
                //CommandLine("mkdir \"%appdata%\\Balatro\\\"");
                Directory.CreateDirectory(Platform.getGameSaveLocation());

                Log("Attempting to pull save files from Android device.");

                //This sure isn't pretty, but it should work!
                useTool(ProcessTools.ADB, "shell rm -r /data/local/tmp/balatro");
                useTool(ProcessTools.ADB, "shell mkdir /data/local/tmp/balatro/");
                useTool(ProcessTools.ADB, "shell mkdir /data/local/tmp/balatro/files/");
                useTool(ProcessTools.ADB, "shell mkdir /data/local/tmp/balatro/files/1/");
                useTool(ProcessTools.ADB, "shell mkdir /data/local/tmp/balatro/files/2/");
                useTool(ProcessTools.ADB, "shell mkdir /data/local/tmp/balatro/files/3/");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/settings.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/settings.jkr > /data/local/tmp/balatro/files/settings.jkr\"");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/1/profile.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/1/profile.jkr > /data/local/tmp/balatro/files/1/profile.jkr\"");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/1/meta.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/1/meta.jkr > /data/local/tmp/balatro/files/1/meta.jkr\"");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/1/save.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/1/save.jkr > /data/local/tmp/balatro/files/1/save.jkr\"");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/2/profile.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/2/profile.jkr > /data/local/tmp/balatro/files/2/profile.jkr\"");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/2/meta.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/2/meta.jkr > /data/local/tmp/balatro/files/2/meta.jkr\"");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/2/save.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/2/save.jkr > /data/local/tmp/balatro/files/2/save.jkr\"");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/3/profile.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/3/profile.jkr > /data/local/tmp/balatro/files/3/profile.jkr\"");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/3/meta.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/3/meta.jkr > /data/local/tmp/balatro/files/3/meta.jkr\"");
                useTool(ProcessTools.ADB, "shell touch /data/local/tmp/balatro/files/3/save.jkr");
                useTool(ProcessTools.ADB, "shell \"run-as com.unofficial.balatro cat files/save/game/3/save.jkr > /data/local/tmp/balatro/files/3/save.jkr\"");

                useTool(ProcessTools.ADB, "pull /data/local/tmp/balatro/files/. \"" + Platform.getGameSaveLocation() + "\"");

                useTool(ProcessTools.ADB, "kill-server");
            }
        }
        #endregion
        #endregion

        Log("Finished!");
        Exit();
    }

    static void tryDelete(string target)
    {
        if(Directory.Exists(target)) Directory.Delete(target, true);
        if(File.Exists(target)) File.Delete(target);
    }

    private static void Cleanup()
    {
        if (_cleaup)
        {
            Log("Deleting temporary files...");

            tryDelete("love-11.5-android-embed.apk");
            tryDelete("Balatro-APK-Patch.zip");//TODO: remove when Android build changes
            //tryDelete("AndroidManifest.xml");//TODO: enable when Android build changes
            tryDelete("apktool.jar");
            tryDelete("uber-apk-signer.jar");
            tryDelete("7za.exe");
            tryDelete("openjdk.zip");
            tryDelete("balatro-aligned-debugSigned.apk.idsig");
            tryDelete("balatro-unsigned.apk");
            tryDelete("platform-tools.zip");
            tryDelete("python.zip");
            tryDelete("ios.py");
            tryDelete("game.love");

            tryDelete("platform-tools");
            tryDelete("jdk-21.0.3+9");
            tryDelete("python");
            tryDelete("Balatro-APK-Patch");//TODO: remove when Android build changes
            //tryDelete("icons");//TODO: enable when Android build changes
            tryDelete("Balatro");
            tryDelete("balatro-apk");
            if (!gameProvided)
                tryDelete("Balatro.exe");
        }
    }

    /// <summary>
    /// Attempts to download a file if it does not exist
    /// </summary>
    /// <param name="name">Friendly name for file (for logging)</param>
    /// <param name="link">Download URL</param>
    /// <param name="fileName">File path to save to</param>
    private void TryDownloadFile(string name, string link, string fileName)
    {
        //If the file does not already exist
        if (!File.Exists(fileName))
        {
            Log("Downloading " + name + "...");
            // TODO: WebClient is Obsolete, and needs to be replaced.
            using (var client = new WebClient())
            {
                client.DownloadFile(link, fileName!);
            }

            //Make sure it exists
            if (File.Exists(fileName))
                Log(name + " downloaded successfully.");
            else
            {
                //If it does not, that's a critical error
                Log("Failed to download " + name + "!");
                Exit();
            }
        }
        else
        {
            //File already exists
            Log(fileName + " already exists.");
        }
    }

    /// <summary>
    /// Wrapper for logging to the console.
    /// </summary>
    /// <param name="text">Text to be logged.</param>
    // This saves me from writing Console.WriteLine a million times
    // ReSharper disable once GrammarMistakeInComment
    // There's probably a better way to make an alias in C#. Oh well
    public static void Log(string text)
    {
        Console.WriteLine(text);
    }

    /// <summary>
    /// Exits the application after the user presses any key
    /// </summary>
    public static void Exit()
    {
        Cleanup();
        Log("Press any key to exit...");
        Console.ReadKey();
        Environment.Exit(1);
    }

    /// <summary>
    /// Prompt user for a 'Y' or a 'N' (not case-sensitive)
    /// </summary>
    /// <param name="question">Prompt for the user</param>
    /// <returns>Status of prompt - true for 'Y', false for 'N'</returns>
    public static bool AskQuestion(string question)
    {
        string input = null;
        do
        {
            if (input != null)
                Log("Enter either 'Y' or 'N'!");
            Log(question + " (y/n):");
            input = Console.ReadLine()?.ToLower();
        } while (input != "y" && input != "n");

        return input == "y";
    }

    /// <summary>
    /// Prepare Android platform-tools, and prompt user to enable USB debugging
    /// </summary>
    void PrepareAndroidPlatformTools()
    {
        //Check whether they already exist
        if (!Directory.Exists("platform-tools"))
        {
            Log("Platform tools not found...");

            if (!File.Exists("platform-tools.zip"))
                TryDownloadFile("platform-tools", PlatformToolsLink, "platform-tools.zip");

            //TODO: Platform-specific 
            TryDownloadFile("7-Zip", Platform.get7ZipDownloadLink(), "7za.exe");

            Log("Extracting platform-tools...");
            useTool(ProcessTools.SevenZip, "x platform-tools.zip -oplatform-tools");
        }

        //Prompt user
        while (!AskQuestion("Is your Android device connected to the host with USB Debugging enabled?"))
            Log("Please enable USB Debugging on your Android device, and connect it to the host.");
    }

    /// <summary>
    /// Prints output (or errors) if verbose mode is enabled
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void ProcessOutputHandler(object sender, DataReceivedEventArgs e)
    {
        if (_verboseMode && e.Data != null && e.Data != "")
            Log("[" + ((System.Diagnostics.Process)sender).ProcessName + "]: " + e.Data);
        //I'd like to use another color for this text specifically, but I'm not sure if it's possible.
    }


    /// <summary>
    /// Starts process using the platform's shell
    /// Currently this is restricted to Windows.
    /// </summary>
    /// <param name="args">Command to pass to the shell</param>
    /// <returns>Process, post finishing.</returns>
    public static Process CommandLine(string command, string args)
    {
        //Create a new cmd process
        Process commandLineProcess = new Process();
        commandLineProcess.StartInfo.FileName = command;
        commandLineProcess.StartInfo.CreateNoWindow = true;
        commandLineProcess.StartInfo.UseShellExecute = false;

        //Output and error handling
        commandLineProcess.StartInfo.RedirectStandardOutput = true;
        commandLineProcess.StartInfo.RedirectStandardError = true;
        commandLineProcess.OutputDataReceived += ProcessOutputHandler;
        commandLineProcess.ErrorDataReceived += ProcessOutputHandler;

        //Apply args
        commandLineProcess.StartInfo.Arguments = args;

        //Start the process
        commandLineProcess.Start();
        commandLineProcess.BeginOutputReadLine();

        //This could be changed to allow for multi-threading, but that's handled with System.Threading anyway
        commandLineProcess.WaitForExit();

        //On exit
        commandLineProcess.Exited += (_, _) =>
        {
            //Check for errors
            if (commandLineProcess.ExitCode != 0)
            {
                //Error occurred
                Log("An unexpected error occurred!");
                if (!_verboseMode)
                    Log("Try running in verbose mode to determine the cause of the error.");
            }
            else
                Log("\n");
        };

        //Return the process
        return commandLineProcess;
    }
}
