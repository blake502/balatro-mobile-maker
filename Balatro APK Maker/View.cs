using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace Balatro_APK_Maker;

/// <summary>
/// Command line UI for Balatro APK Maker.
/// </summary>
// NOTE: Much should be refactored out of UI logic land, and, into a Controller which we can query state from.
internal class View
{
    private bool _verboseMode;

    private bool _androidBuild;
    private bool _iosBuild;
    
    /// <summary>
    /// Start CLI operation.
    /// </summary>
    public void Begin() {
        bool exeProvided = File.Exists("Balatro.exe");

        Log("====Balatro APK Maker====\n7-Zip is licensed under the GNU LGPL license. Please visit: www.7-zip.org\n\n");

        //Initial prompts
        bool cleanup = AskQuestion("Would you like to automatically clean up once complete?");
        _verboseMode = AskQuestion("Would you like to enable extra logging information?");

        //If balatro.apk already exists, ask before beginning build process again
        if ((!File.Exists("balatro.apk") && !File.Exists("balatro.ipa")) || AskQuestion("A previous build was found... Would you like to build again?"))
        {
            _androidBuild = AskQuestion("Would you like to build for Android?");
            _iosBuild = !_androidBuild && AskQuestion("Would you like to build for iOS (experimental)?");

            #region Download tools
            if (_androidBuild)
            {
                #region Java
                //Check for Java
                Log("Checking for Java...");
                if (CommandLine(Constants.JavaCommand).ExitCode == 0)
                    Log("Java found.");
                else
                {
                    Log("Java not found, please install Java!");

                    //Prompt user to automatically download install Java
                    if (AskQuestion("Would you like to automatically download and install Java?"))
                    {
                        //Download
                        TryDownloadFile("Java", Constants.Jre8InstallerLink, "java-installer.exe");
                        //Install
                        Log("Installing Java...");
                        CommandLine("java-installer.exe /s");

                        //Check again for Java
                        if (CommandLine(Constants.JavaCommand).ExitCode != 0)
                        {
                            //Critical error
                            Log("Java still not detected! Try to re-launch.");
                            Exit();
                        }
                    }
                    else
                    {
                        //User does not wish to automatically download and install Java
                        //Take them to the download link instead. Halt program
                        CommandLine(Constants.JavaDownloadCommand);
                        Exit();
                    }
                }
                #endregion

                #region Android tools
                //Downloading tools. Handled in threads to allow simultaneous downloads
                Thread[] downloadThreads =
                [
                    new Thread(() => { TryDownloadFile("7-Zip", Constants.SevenzipLink, "7za.exe"); }),
                    new Thread(() => { TryDownloadFile("APKTool", Constants.ApktoolLink, "apktool.jar"); }),
                    new Thread(() => { TryDownloadFile("uber-apk-signer", Constants.UberapktoolLink, "uber-apk-signer.jar"); }),
                    new Thread(() => { TryDownloadFile("Balatro-APK-Patch", Constants.BalatroApkPatchLink, "Balatro-APK-Patch.zip"); }),
                    new Thread(() => { TryDownloadFile("Love2D APK", Constants.Love2dApkLink, "love-11.5-android-embed.apk"); })
                ];

                //Start all the downloads
                foreach (var t in downloadThreads) t.Start();

                //Wait for all the downloads to complete
                foreach (var t in downloadThreads) t.Join();

                #endregion
            }

            if (_iosBuild)
            {
                #region Python
                //Check for Python
                Log("Checking for Python...");
                if (CommandLine(Constants.PythonCommand).ExitCode == 0)
                    Log("Python found.");
                else
                {
                    Log("Python not found, please install Python!");

                    //Prompt user to automatically download install Python
                    if (AskQuestion("Would you like to automatically download and install Python?"))
                    {
                        //Download
                        TryDownloadFile("Python", Constants.PythonLink, "python-installer.exe");
                        //Install
                        Log("Installing Python...");
                        CommandLine("python-installer.exe /quiet");

                        //Check again for Python
                        if (CommandLine(Constants.PythonCommand).ExitCode != 0)
                        {
                            //Critical error
                            Log("Python still not detected! Try to re-launch, or install Python manually from the Microsoft Store.");
                            CommandLine("python");
                            Exit();
                        }
                    }
                    else
                    {
                        //User does not wish to automatically download and install Python
                        //Take them to the download link instead. Halt program
                        CommandLine("python");
                        Exit();
                    }
                }
                #endregion

                #region iOS Tools
                //Downloading tools. Handled in threads to allow simultaneous downloads
                Thread[] downloadThreads =
                [
                    new Thread(() => { TryDownloadFile("7-Zip", Constants.SevenzipLink, "7za.exe"); }),
                    new Thread(() => { TryDownloadFile("iOS Base", Constants.IosBaseLink, "balatro-base.ipa"); })
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
            if (!exeProvided)
            {
                //Attempt to copy Balatro.exe from Steam directory
                Log("Copying Balatro.exe from Steam directory...");
                CommandLine("xcopy \"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Balatro\\Balatro.exe\" \"Balatro.exe\" /E /H /Y /V /-I");

                if (!File.Exists("Balatro.exe"))
                {
                    //Balatro.exe still not found. Critical error.
                    Log("Could not find Balatro.exe! Please place it in this folder, then try again!");
                    Exit();
                }
            }
            else
                Log("Balatro.exe already exists.");

            Log("Extracting Balatro.exe...");
            if (Directory.Exists("Balatro"))
            {
                //Delete the Balatro folder if it already exists
                Log("Balatro directory already exists! Deleting Balatro directory...");
                CommandLine("rmdir Balatro\\ /S /Q");
            }

            //Extract Balatro.exe with 7-Zip
            CommandLine("7za x Balatro.exe -oBalatro");

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
                    CommandLine("rmdir balatro-apk\\ /S /Q");
                }

                //Unpack Love2D APK
                CommandLine("java.exe -jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" d -s -o balatro-apk love-11.5-android-embed.apk");

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
                    CommandLine("rmdir Balatro-APK-Patch\\ /S /Q");
                }

                //Extract Balatro-APK-Patch
                CommandLine("7za.exe  x Balatro-APK-Patch.zip -oBalatro-APK-Patch");

                if (!Directory.Exists("Balatro-APK-Patch"))
                {
                    Log("Failed to extract Balatro-APK-Patch");
                    Exit();
                }

                //Base APK patch
                Log("Patching APK folder...");
                CommandLine("xcopy \"Balatro-APK-Patch\\\" \"balatro-apk\\\" /E /H /Y /V");
                #endregion
            }

            if (_iosBuild)
            {
                #region Prepare IPA
                Log("Preparing iOS Base...");
                CommandLine("move balatro-base.ipa balatro-base.zip");
                #endregion
            }

            #endregion

            #region Patch
            Log("Patching...");
            ApplyPatches();
            #endregion

            #region Building

            #region Balatro.exe -> game.love
            Log("Packing Balatro folder...");
            CommandLine("\"cd Balatro && ..\\7za.exe a balatro.zip && cd ..\"");

            if (!File.Exists("Balatro\\balatro.zip"))
            {
                Log("Failed to pack Balatro folder!");
                Exit();
            }

            Log("Moving archive...");
            if (_androidBuild)
                CommandLine("move Balatro\\balatro.zip balatro-apk\\assets\\game.love");

            if (_iosBuild)
                CommandLine("move Balatro\\balatro.zip game.love");
            #endregion

            if (_androidBuild)
            {
                #region Packing APK
                Log("Repacking APK...");
                CommandLine("java.exe -jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" b -o balatro.apk balatro-apk");

                if (!File.Exists("balatro.apk"))
                {
                    Log("Failed to pack Balatro apk!");
                    Exit();
                }
                #endregion

                #region Signing APK
                Log("Signing APK...");
                CommandLine("java -jar uber-apk-signer.jar -a balatro.apk");

                if (!File.Exists("balatro-aligned-debugSigned.apk"))
                {
                    Log("Failed to sign APK!");
                    Exit();
                }

                Log("Renaming unsigned apk...");
                CommandLine("move balatro.apk balatro-unsigned.apk");

                Log("Renaming signed apk...");
                CommandLine("move balatro-aligned-debugSigned.apk balatro.apk");
                #endregion
            }

            if (_iosBuild)
            {
                #region Packing IPA
                Log("Repacking iOS app...");
                File.WriteAllText("ios.py", @"import zipfile
existing_zip = zipfile.ZipFile('balatro-base.zip', 'a')
new_file_path = 'game.love'
existing_zip.write(new_file_path, arcname='Payload/Balatro.app/game.love')
existing_zip.close()");
                CommandLine("python ios.py");
                CommandLine("move balatro-base.zip balatro.ipa");
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
            CommandLine("cd platform-tools && cd platform-tools && adb install ..\\..\\balatro.apk && adb kill-server");
        }
        #endregion

        #region Save transfer
        if (!_iosBuild && File.Exists("balatro.apk") && Directory.Exists(Environment.GetEnvironmentVariable("AppData") + "\\Balatro") && AskQuestion("Would you like to transfer saves from your Steam copy of Balatro to your Android device?"))
        {
            Log("Thanks to TheCatRiX for figuring out save transfers!");

            PrepareAndroidPlatformTools();

            Log("Attempting to transfer saves. If prompted, please allow the USB Debugging connection on your Android device.");
            CommandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro");
            CommandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files");
            CommandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/save");
            CommandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/save/game");
            CommandLine("cd platform-tools && cd platform-tools && adb push \"%AppData%/Balatro/.\" /data/local/tmp/balatro/files/save/game && adb shell am force-stop com.unofficial.balatro && adb shell run-as com.unofficial.balatro cp -r /data/local/tmp/balatro/files . && adb shell rm -r /data/local/tmp/balatro && adb kill-server");

        }
        else
        {
            if (AskQuestion("Would you like to pull saves from your Android device?"))
            {
                Log("Warning! If Steam Cloud is enabled, it will overwrite the save you transfer!");
                while (!AskQuestion("Have you backed up your saves?"))
                    Log("Please back up your saves! I am not responsible if your saves get deleted!");

                PrepareAndroidPlatformTools();

                Log("Backing up your files...");
                CommandLine("xcopy \"%appdata%\\Balatro\\\" \"%appdata%\\BalatroBACKUP\\\" /E /H /Y /V");
                CommandLine("rmdir \"%appdata%\\Balatro\\\" /S /Q");
                CommandLine("mkdir \"%appdata%\\Balatro\\\"");

                //This sure isn't pretty, but it should work!
                CommandLine("cd platform-tools && cd platform-tools && adb shell rm -r /data/local/tmp/balatro");
                CommandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/");
                CommandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/");
                CommandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/1/");
                CommandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/2/");
                CommandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/3/");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/settings.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/settings.jkr > /data/local/tmp/balatro/files/settings.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/1/profile.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/1/profile.jkr > /data/local/tmp/balatro/files/1/profile.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/1/meta.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/1/meta.jkr > /data/local/tmp/balatro/files/1/meta.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/1/save.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/1/save.jkr > /data/local/tmp/balatro/files/1/save.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/2/profile.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/2/profile.jkr > /data/local/tmp/balatro/files/2/profile.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/2/meta.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/2/meta.jkr > /data/local/tmp/balatro/files/2/meta.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/2/save.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/2/save.jkr > /data/local/tmp/balatro/files/2/save.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/3/profile.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/3/profile.jkr > /data/local/tmp/balatro/files/3/profile.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/3/meta.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/3/meta.jkr > /data/local/tmp/balatro/files/3/meta.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/3/save.jkr");
                CommandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/3/save.jkr > /data/local/tmp/balatro/files/3/save.jkr\"");
                CommandLine("cd platform-tools && cd platform-tools && adb pull /data/local/tmp/balatro/files/. %AppData%/Balatro/");

                Log("Attempting to pull save files from Android device.");
            }
        }
        #endregion
        #endregion

        #region Cleanup
        if (cleanup)
        {
            Log("Deleting temporary files...");

            CommandLine("del java-installer.exe");
            CommandLine("del love-11.5-android-embed.apk");
            CommandLine("del Balatro-APK-Patch.zip");
            CommandLine("del apktool.jar");
            CommandLine("del uber-apk-signer.jar");
            CommandLine("del 7za.exe");
            CommandLine("del balatro-aligned-debugSigned.apk.idsig");
            CommandLine("del balatro-unsigned.apk");
            CommandLine("del platform-tools.zip");
            CommandLine("del python-installer.exe");
            CommandLine("del ios.py");
            CommandLine("del game.love");
            CommandLine("rmdir platform-tools\\ /S /Q");
            CommandLine("rmdir Balatro-APK-Patch\\ /S /Q");
            CommandLine("rmdir Balatro\\ /S /Q");
            CommandLine("rmdir balatro-apk\\ /S /Q");
            if (!exeProvided)
                CommandLine("del Balatro.exe");
        }
        #endregion

        Log("Finished!");
        Exit();
    }

    /// <summary>
    /// Apply a "patch" given a file path, a string from a line to be replaced, and the text with which to replace it.
    /// </summary>
    /// <param name="file">File to patch</param>
    /// <param name="lineContains">Line to be replaced</param>
    /// <param name="replaceWith">New contents for the line</param>
    /// <returns></returns>
    // We wish to keep the return type, incase we want to make use of this later.
    // ReSharper disable once UnusedMethodReturnValue.Local
    bool ApplyPatch(string file, string lineContains, string replaceWith)
    {
        //Read the file
        Log("Loading " + file + " file...");
        string[] loadedFile = File.ReadAllLines("Balatro\\" + file);

        //Search for the line to replace
        bool found = false;
        for (int i = 0; i < loadedFile.Length; i++)
            // This has to be made culture-invariant, or, in some regions this could result in unexpected behaviour
            // Consider also using .Contains here - is there a reason we make use of IndexOf?
            if (loadedFile[i].IndexOf(lineContains, StringComparison.Ordinal) != -1)
            {
                //Replace the line
                loadedFile[i] = replaceWith;
                found = true;
                break;
            }

        if (found)
        {
            //If it is found, write the file.
            Log("Successfully applied patch...");
            File.WriteAllLines("Balatro\\" + file, loadedFile);
        }
        else
            Log("Unable to find patch location...");

        return found;
    }

    /// <summary>
    /// Prompts the user to select which patches they want, then applies them.
    /// </summary>
    // This is hideous, but it works.
     void ApplyPatches()
    {
        Log("Applying mobile compatibilty patch...");
        //Android platform support
        ApplyPatch("globals.lua", "loadstring", @"    -- Removed 'loadstring' line which generated lua code that exited upon starting on mobile
    if love.system.getOS() == 'Android' or love.system.getOS() == 'iOS' then
        self.F_DISCORD = true
        self.F_NO_ACHIEVEMENTS = true
        self.F_SOUND_THREAD = true
        self.F_VIDEO_SETTINGS = false
        self.F_ENGLISH_ONLY = false
        self.F_QUIT_BUTTON = false
    end");
        //On-screen keyboard
        ApplyPatch("functions/button_callbacks.lua", "G.CONTROLLER.text_input_hook == e and G.CONTROLLER.HID.controller", "  if G.CONTROLLER.text_input_hook == e and (G.CONTROLLER.HID.controller or G.CONTROLLER.HID.touch) then");

        //Ask whether they want the FPS cap patch
        if (AskQuestion("Would you like to apply the FPS cap patch?"))
        {
            int fps = -1;
            do
            {
                Log("Please enter your desired FPS cap (or leave blank to set to device refresh rate):");
                // Conditional access as ReadLine is nullable
                string input = Console.ReadLine()?.ToLower();

                if (String.IsNullOrWhiteSpace(input))
                {
                    //Set to refresh rate if blank
                    fps = -2;
                    break;
                }

                try
                {
                    //Set to specific value
                    fps = Convert.ToInt32(input);
                }
                catch
                {
                    // ignored
                }
            } while (fps <= 0 || fps > 999);

            if (fps > 0)
            {
                //Apply the patch using the given FPS
                ApplyPatch("main.lua", "G.FPS_CAP = G.FPS_CAP or", "        G.FPS_CAP = " + fps.ToString());
            }
            else
            {
                //Apply the patch using the display refresh rate
                ApplyPatch("main.lua", "G.FPS_CAP = G.FPS_CAP or", @"        p_ww, p_hh, p_wflags = love.window.getMode()
        G.FPS_CAP = p_wflags['refreshrate']");
            }
        }

        //Extra patches

        if (AskQuestion("Would you like to apply the landscape orientation patch (required for high DPI)?"))
        {
            // Asking ReSharper to disable naming here, as, DPI (all-caps) is correct, not Dpi
            // ReSharper disable once InconsistentNaming
            var highDPI = AskQuestion("Would you like to apply the high DPI patch (recommended for devices with high resolution)?");
            ApplyPatch("main.lua", "local os = love.system.getOS()", "    local os = love.system.getOS()\n    love.window.setMode(2, 1" + (highDPI ? ", {highdpi = true}" : "") + ")");
        }

        if (AskQuestion("Would you like to apply the CRT shader disable patch? (Required for Pixel and some other devices!)"))
        {
            ApplyPatch("globals.lua", "crt = ", "            crt = 0,");
            ApplyPatch("game.lua", "G.SHADERS['CRT'])", "");
        }

        //Disabled, since this seems to actually be the HARDER way of doing this
        //I'll leave it in the code base for now though...
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable CS0162 // Unreachable code detected
        if (false && AskQuestion("Would you like to apply the accessible saves patch?"))
            // ReSharper disable once HeuristicUnreachableCode
            ApplyPatch("conf.lua", "t.window.width = 0", "    t.window.width = 0\n    t.externalstorage = true");
#pragma warning restore CS0162 // Unreachable code detected
    }
        
    /// <summary>
    /// Attempts to download a file if it does not exist
    /// </summary>
    /// <param name="name">Friendly name for file (for logging)</param>
    /// <param name="link">Download URL</param>
    /// <param name="fileName">File path to save to</param>
    private  void TryDownloadFile(string name, string link, string fileName)
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
     void Log(string text)
    {
        Console.WriteLine(text);
    }

    /// <summary>
    /// Exits the application after the user presses any key
    /// </summary>
     void Exit()
    {
        Log("Press any key to exit...");
        Console.ReadKey();
        Environment.Exit(1);
    }

    /// <summary>
    /// Prompt user for a 'Y' or a 'N' (not case-sensitive)
    /// </summary>
    /// <param name="question">Prompt for the user</param>
    /// <returns>Status of prompt - true for 'Y', false for 'N'</returns>
     bool AskQuestion(string question)
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
                TryDownloadFile("platform-tools", Constants.PlatformToolsLink, "platform-tools.zip");

            if (!File.Exists("7za.exe"))
                TryDownloadFile("7-Zip", Constants.SevenzipLink, "7za.exe");

            Log("Extracting platform-tools...");
            CommandLine("7za x platform-tools.zip -oplatform-tools");
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
    private  void ProcessOutputHandler(object sender, DataReceivedEventArgs e)
    {
        if (_verboseMode && e.Data != null)
            Log("    " + e.Data);
    }
    
    
    /// <summary>
    /// Starts process using the platform's shell
    /// Currently this is restricted to Windows.
    /// </summary>
    /// <param name="args">Command to pass to the shell</param>
    /// <returns>Process, post finishing.</returns>
     Process CommandLine(string args)
    {
        //Create a new cmd process
        Process commandLineProcess = new Process();
        commandLineProcess.StartInfo.FileName = "cmd.exe";
        commandLineProcess.StartInfo.CreateNoWindow = true;
        commandLineProcess.StartInfo.UseShellExecute = false;

        //Output and error handling
        commandLineProcess.StartInfo.RedirectStandardOutput = true;
        commandLineProcess.StartInfo.RedirectStandardError = true;
        commandLineProcess.OutputDataReceived += ProcessOutputHandler;
        commandLineProcess.ErrorDataReceived += ProcessOutputHandler;

        //Apply args
        commandLineProcess.StartInfo.Arguments = "/c " + args;

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