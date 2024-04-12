using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace Balatro_APK_Maker
{
    internal class Program
    {

        const string javaCommand = "java -version";
        const string jre8installerLink = "https://javadl.oracle.com/webapps/download/AutoDL?BundleId=249553_4d245f941845490c91360409ecffb3b4";
        const string javaDownloadCommand = "explorer https://www.java.com/download/";
        const string sevenzipLink = "https://github.com/blake502/balatro-apk-maker/releases/download/Additional-Tools-1.0/7za.exe";
        const string apktoolLink = "https://bitbucket.org/iBotPeaches/apktool/downloads/apktool_2.9.3.jar";
        const string uberapktoolLink = "https://github.com/patrickfav/uber-apk-signer/releases/download/v1.3.0/uber-apk-signer-1.3.0.jar";
        const string balatroApkPatchLink = "https://github.com/blake502/balatro-apk-maker/releases/download/Additional-Tools-1.0/Balatro-APK-Patch.zip";
        const string love2dApkLink = "https://github.com/love2d/love-android/releases/download/11.5a/love-11.5-android-embed.apk";
        const string platformTools = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";

        static bool verboseMode = false;

        //This applies a "patch" given a file path, a string from a line to be replaced, and the text with which to replace it.
        static bool applyPatch(string file, string lineContains, string replaceWith)
        {
            //Read the file
            log("Loading " + file + " file...");
            string[] loadedFile = File.ReadAllLines("Balatro\\" + file);

            //Search for the line to replace
            bool found = false;
            for (int i = 0; i < loadedFile.Length; i++)
                if (loadedFile[i].IndexOf(lineContains) != -1)
                {
                    //Replace the line
                    loadedFile[i] = replaceWith;
                    found = true;
                    break;
                }

            if (found)
            {
                //If it is found, write the file.
                log("Successfully applied patch...");
                File.WriteAllLines("Balatro\\" + file, loadedFile);
            }
            else
                log("Unable to find patch location...");

            return found;
        }

        //Prompts the user to select which patches they want, then applies them.
        //This is hideous. But it works.
        static void applyPatches()
        {
            log("Applying Android compatibilty patch...");
            //Android platform support
            applyPatch("globals.lua", "loadstring", @"    -- Removed 'loadstring' line which generated lua code that exited upon starting on mobile
    if love.system.getOS() == 'Android' then
        self.F_DISCORD = true
        self.F_NO_ACHIEVEMENTS = true
        self.F_SOUND_THREAD = true
        self.F_VIDEO_SETTINGS = false
        self.F_ENGLISH_ONLY = false
        self.F_QUIT_BUTTON = false
    end");
            //On-screen keyboard
            applyPatch("functions/button_callbacks.lua", "G.CONTROLLER.text_input_hook == e and G.CONTROLLER.HID.controller", "  if G.CONTROLLER.text_input_hook == e and (G.CONTROLLER.HID.controller or G.CONTROLLER.HID.touch) then");

            //Ask whether they want the FPS cap patch
            if (askQuestion("Would you like to apply the FPS cap patch?"))
            {
                int fps = -1;
                do {
                    log("Please enter your desired FPS cap (or leave blank to set to device refresh rate):");
                    string input = Console.ReadLine().ToLower();

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
                        continue;
                    }
                } while (fps <= 0 || fps > 999);

                if (fps > 0)
                {
                    //Apply the patch using the given FPS
                    applyPatch("main.lua", "G.FPS_CAP = G.FPS_CAP or", "        G.FPS_CAP = " + fps.ToString());
                }
                else
                {
                    //Apply the patch using the display refresh rate
                    applyPatch("main.lua", "G.FPS_CAP = G.FPS_CAP or", @"        p_ww, p_hh, p_wflags = love.window.getMode()
        G.FPS_CAP = p_wflags['refreshrate']");
                }
            }

            //Extra patches

            if (askQuestion("Would you like to apply the landscape orientation patch?"))
                applyPatch("main.lua", "local os = love.system.getOS()", "    local os = love.system.getOS()\n    love.window.setMode(2, 1)");

            if (askQuestion("Would you like to apply the CRT shader disable patch? (Required for Pixel and some other devices!)"))
            {
                applyPatch("globals.lua", "crt = ", "            crt = 0,");
                applyPatch("game.lua", "G.SHADERS['CRT'])", "");
            }

            //Disabled, since this seems to actually be the HARDER way of doing this
            //I'll leave it in the code base for now though...
            if (false && askQuestion("Would you like to apply the accessible saves patch?")) 
                applyPatch("conf.lua", "t.window.width = 0", "    t.window.width = 0\n    t.externalstorage = true");
        }

        //Attempts to download a file if it does not exist.
        //Take a friendly name, a URL, and a file name.
        static void tryDownloadFile(string name, string link, string fileName)
        {
            //If the file does not already exist
            if (!File.Exists(fileName))
            {
                log("Downloading " + name + "...");
                using (var client = new WebClient())
                {
                    client.DownloadFile(link, fileName);
                }

                //Make sure it exists
                if (File.Exists(fileName))
                    log(name + " downloaded successfully.");
                else
                {
                    //If it does not, that's a critical error
                    log("Failed to download " + name + "!");
                    exit();
                }
            }
            else
            {
                //File already exists
                log(fileName + " already exists.");
            }
        }

        //This saves me from writing Console.WriteLine a million times
        //There's probably a better way to make an alias in C#. Oh well
        static void log(string text)
        {
            Console.WriteLine(text);
        }

        //Exits the application after the user presses any key
        static void exit()
        {
            log("Press any key to exit...");
            Console.ReadKey();
            System.Environment.Exit(1);
        }

        //Prompts the user for a 'Y' or 'N' (not case-sensitive)
        static bool askQuestion(string question)
        {
            string input = null;
            do {
                if (input != null)
                    log("Enter either 'Y' or 'N'!");
                log(question + " (y/n):");
                input = Console.ReadLine().ToLower();
            } while (input != "y" && input != "n");

            return input == "y";
        }

        //Prepare platform-tools to be used, and prompt the user to enable USB Debugging
        static void prepareAndroidPlatformTools()
        {
            //Check whether they already exist
            if (!Directory.Exists("platform-tools"))
            {
                log("Platform tools not found...");

                if (!File.Exists("platform-tools.zip"))
                    tryDownloadFile("platform-tools", platformTools, "platform-tools.zip");

                if (!File.Exists("7za.exe"))
                    tryDownloadFile("7-Zip", sevenzipLink, "7za.exe");

                log("Extracting platform-tools...");
                commandLine("7za x platform-tools.zip -oplatform-tools");
            }

            //Prompt user
            while (!askQuestion("Is your Android device connected to the host with USB Debugging enabled?"))
                log("Please enable USB Debugging on your Android device, and connect it to the host.");
        }

        static void Main(string[] args)
        {
            bool exeProvided = File.Exists("Balatro.exe");

            log("====Balatro APK Maker====\n7-Zip is licensed under the GNU LGPL license. Please visit: www.7-zip.org\n\n");

            //Initial prompts
            bool cleanup = askQuestion("Would you like to automatically clean up once complete?");
            verboseMode = askQuestion("Would you like to enable extra logging information?");

            //If balatro.apk already exists, ask before beginning build process again
            if (!File.Exists("balatro.apk") || askQuestion("A previous build of balatro.apk was found... Would you like to build again?"))
            {
                #region Download tools
                #region Java
                //Check for Java
                log("Checking for Java...");
                if (commandLine(javaCommand).ExitCode == 0)
                    log("Java found.");
                else
                {
                    log("Java not found, please install Java!");

                    //Prompt user to automatically download install Java
                    if (askQuestion("Would you like to automatically download and install Java?"))
                    {
                        //Download
                        tryDownloadFile("Java", jre8installerLink, "java-installer.exe");
                        //Install
                        log("Installing Java...");
                        commandLine("java-installer.exe /s");

                        //Check again for Java
                        if (commandLine(javaCommand).ExitCode != 0)
                        {
                            //Critical error
                            log("Java still not detected!");
                            exit();
                        }
                    }
                    else
                    {
                        //User does not wish to automatically download and install Java
                        //Take them to the download link instead. Halt program
                        commandLine(javaDownloadCommand);
                        exit();
                    }
                }
                #endregion

                #region All other tools
                //Downloading tools. Handled in threads to allow simultaneous downloads
                Thread[] downloadThreads = {
                    new Thread(() => { tryDownloadFile("7-Zip", sevenzipLink, "7za.exe"); }),
                    new Thread(() => { tryDownloadFile("APKTool", apktoolLink, "apktool.jar"); }),
                    new Thread(() => { tryDownloadFile("uber-apk-signer", uberapktoolLink, "uber-apk-signer.jar"); }),
                    new Thread(() => { tryDownloadFile("Balatro-APK-Patch", balatroApkPatchLink, "Balatro-APK-Patch.zip"); }),
                    new Thread(() => { tryDownloadFile("Love2D APK", love2dApkLink, "love-11.5-android-embed.apk"); })
                };

                //Start all the downloads
                for (int i = 0; i < downloadThreads.Length; i++)
                    downloadThreads[i].Start();

                //Wait for all the downloads to complete
                for (int i = 0; i < downloadThreads.Length; i++)
                    downloadThreads[i].Join();
                #endregion
                #endregion

                #region Prepare workspace
                #region Find and extract Balatro.exe
                if (!exeProvided)
                {
                    //Attempt to copy Balatro.exe from Steam directory
                    log("Copying Balatro.exe from Steam directory...");
                    commandLine("xcopy \"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Balatro\\Balatro.exe\" \"Balatro.exe\" /E /H /Y /V /-I");

                    if (!File.Exists("Balatro.exe"))
                    {
                        //Balatro.exe still not found. Critical error.
                        log("Could not find Balatro.exe! Please place it in this folder, then try again!");
                        exit();
                    }
                }
                else
                    log("Balatro.exe already exists.");

                log("Extracting Balatro.exe...");
                if (Directory.Exists("Balatro"))
                {
                    //Delete the Balatro folder if it already exists
                    log("Balatro directory already exists! Deleting Balatro directory...");
                    commandLine("rmdir Balatro\\ /S /Q");
                }

                //Extract Balatro.exe with 7-Zip
                commandLine("7za x Balatro.exe -oBalatro");

                //Check for failure
                if (!Directory.Exists("Balatro"))
                {
                    log("Failed to extract Balatro.exe!");
                    exit();
                }
                #endregion

                #region Extract APK
                log("Unpacking Love2D APK with APK Tool...");
                if (Directory.Exists("balatro-apk"))
                {
                    //Delete the balatro-apk folder if it already exists
                    log("balatro-apk directory already exists! Deleting balatro-apk directory...");
                    commandLine("rmdir balatro-apk\\ /S /Q");
                }

                //Unpack Love2D APK
                commandLine("java.exe -jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" d -s -o balatro-apk love-11.5-android-embed.apk");

                //Check for failure
                if (!Directory.Exists("balatro-apk"))
                {
                    log("Failed to unpack Love2D APK with APK Tool!");
                    exit();
                }
                #endregion

                #region Extract APK patch
                log("Extracting patch zip...");
                if (Directory.Exists("Balatro-APK-Patch"))
                {
                    log("Balatro-APK-Patch directory already exists! Deleting Balatro-APK-Patch directory...");
                    commandLine("rmdir Balatro-APK-Patch\\ /S /Q");
                }

                //Extract Balatro-APK-Patch
                commandLine("7za.exe  x Balatro-APK-Patch.zip -oBalatro-APK-Patch");

                if (!Directory.Exists("Balatro-APK-Patch"))
                {
                    log("Failed to extract Balatro-APK-Patch");
                    exit();
                }
                #endregion
                #endregion

                #region Patches
                //Base APK patch
                log("Patching APK folder...");
                commandLine("xcopy \"Balatro-APK-Patch\\\" \"balatro-apk\\\" /E /H /Y /V");

                //Balatro code patches
                log("Patching...");
                applyPatches();
                #endregion

                #region Building

                #region Balatro.exe -> game.love
                log("Packing Balatro folder...");
                commandLine("\"cd Balatro && ..\\7za.exe a balatro.zip && cd ..\"");

                if (!File.Exists("Balatro\\balatro.zip"))
                {
                    log("Failed to pack Balatro folder!");
                    exit();
                }

                log("Moving archive...");
                commandLine("move Balatro\\balatro.zip balatro-apk\\assets\\game.love");
                #endregion

                #region APK
                #region Packing
                log("Repacking APK...");
                commandLine("java.exe -jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" b -o balatro.apk balatro-apk");

                if (!File.Exists("balatro.apk"))
                {
                    log("Failed to pack Balatro apk!");
                    exit();
                }
                #endregion

                #region Signing
                log("Signing APK...");
                commandLine("java -jar uber-apk-signer.jar -a balatro.apk");

                if (!File.Exists("balatro-aligned-debugSigned.apk"))
                {
                    log("Failed to sign APK!");
                    exit();
                }

                log("Renaming unsigned apk...");
                commandLine("move balatro.apk balatro-unsigned.apk");

                log("Renaming signed apk...");
                commandLine("move balatro-aligned-debugSigned.apk balatro.apk");
                #endregion

                log("Build successful!");
                #endregion
                #endregion
            }

            #region Android options
            #region Auto-install
            if (askQuestion("Would you like to automaticaly install balatro.apk on your Android device?"))
            {
                prepareAndroidPlatformTools();

                log("Attempting to install. If prompted, please allow the USB Debugging connection on your Android device.");
                commandLine("cd platform-tools && cd platform-tools && adb install ..\\..\\balatro.apk && adb kill-server");
            }
            #endregion

            #region Save transfer
            if (Directory.Exists(Environment.GetEnvironmentVariable("AppData") + "\\Balatro") && askQuestion("Would you like to transfer saves from your Steam copy of Balatro to your Anroid device?"))
            {
                log("Thanks to TheCatRiX for figuring out save transfers!");

                prepareAndroidPlatformTools();

                log("Attempting to transfer saves. If prompted, please allow the USB Debugging connection on your Android device.");
                commandLine("cd platform-tools && cd platform-tools && adb push %AppData%/Balatro/. /data/local/tmp/balatro/files/save/game && adb shell am force-stop com.unofficial.balatro && adb shell run-as com.unofficial.balatro cp -r /data/local/tmp/balatro/files . && adb shell rm -r /data/local/tmp/balatro && adb kill-server");

            }
            else
            {
                if (askQuestion("Would you like to pull saves from your Android device?"))
                {
                    log("Warning! If Steam Cloud is enabled, it will overwrite the save you transfer!");
                    while (!askQuestion("Have you backed up your saves?"))
                        log("Please back up your saves! I am not responsible if your saves get deleted!");

                    prepareAndroidPlatformTools();

                    log("Backing up your files...");
                    commandLine("xcopy \"%appdata%\\Balatro\\\" \"%appdata%\\BalatroBACKUP\\\" /E /H /Y /V");
                    commandLine("rmdir %appdata%\\Balatro\\ /S /Q");
                    commandLine("mkdir %appdata%\\Balatro\\");

                    //This sure isn't pretty, but it should work!
                    commandLine("cd platform-tools && cd platform-tools && adb shell rm -r /data/local/tmp/balatro");
                    commandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/");
                    commandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/");
                    commandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/1/");
                    commandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/2/");
                    commandLine("cd platform-tools && cd platform-tools && adb shell mkdir /data/local/tmp/balatro/files/3/");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/settings.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/settings.jkr > /data/local/tmp/balatro/files/settings.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/1/profile.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/1/profile.jkr > /data/local/tmp/balatro/files/1/profile.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/1/meta.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/1/meta.jkr > /data/local/tmp/balatro/files/1/meta.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/1/save.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/1/save.jkr > /data/local/tmp/balatro/files/1/save.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/2/profile.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/2/profile.jkr > /data/local/tmp/balatro/files/2/profile.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/2/meta.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/2/meta.jkr > /data/local/tmp/balatro/files/2/meta.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/2/save.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/2/save.jkr > /data/local/tmp/balatro/files/2/save.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/3/profile.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/3/profile.jkr > /data/local/tmp/balatro/files/3/profile.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/3/meta.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/3/meta.jkr > /data/local/tmp/balatro/files/3/meta.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb shell touch /data/local/tmp/balatro/files/3/save.jkr");
                    commandLine("cd platform-tools && cd platform-tools && adb shell \"run-as com.unofficial.balatro cat files/save/game/3/save.jkr > /data/local/tmp/balatro/files/3/save.jkr\"");
                    commandLine("cd platform-tools && cd platform-tools && adb pull /data/local/tmp/balatro/files/. %AppData%/Balatro/");

                    log("Attempting to pull save files from Android device.");
                }
            }
            #endregion
            #endregion

            #region Cleanup
            if (cleanup)
            {
                log("Deleting temporary files...");

                commandLine("del java-installer.exe");
                commandLine("del love-11.5-android-embed.apk");
                commandLine("del Balatro-APK-Patch.zip");
                commandLine("del apktool.jar");
                commandLine("del uber-apk-signer.jar");
                commandLine("del 7za.exe");
                commandLine("del balatro-aligned-debugSigned.apk.idsig");
                commandLine("del balatro-unsigned.apk");
                commandLine("del platform-tools.zip");
                commandLine("rmdir platform-tools\\ /S /Q");
                commandLine("rmdir Balatro-APK-Patch\\ /S /Q");
                commandLine("rmdir Balatro\\ /S /Q");
                commandLine("rmdir balatro-apk\\ /S /Q");
                if (!exeProvided)
                    commandLine("del Balatro.exe");
            }
            #endregion

            log("Finished!");
            exit();
        }

        //Starts Command Prompt process with a given command
        static Process commandLine(string args)
        {
            //Create a new cmd process
            Process commandLineProccess = new Process();
            commandLineProccess.StartInfo.FileName = "cmd.exe";
            commandLineProccess.StartInfo.CreateNoWindow = true;
            commandLineProccess.StartInfo.UseShellExecute = false;

            //Output and error handling
            commandLineProccess.StartInfo.RedirectStandardOutput = true;
            commandLineProccess.StartInfo.RedirectStandardError = true;
            commandLineProccess.OutputDataReceived += processOutputHandler;
            commandLineProccess.ErrorDataReceived += processOutputHandler;

            //Apply args
            commandLineProccess.StartInfo.Arguments = "/c " + args;

            //Start the process
            commandLineProccess.Start();
            commandLineProccess.BeginOutputReadLine();

            //This could be changed to allow for multi-threading, but that's handled with System.Threading anyway
            commandLineProccess.WaitForExit();

            //On exit
            commandLineProccess.Exited += (object sender, EventArgs e) =>
                {
                    //Check for errors
                    if (commandLineProccess.ExitCode != 0)
                    {
                        //Error occurred
                        log("An unexpected error occurred!");
                        if (!verboseMode)
                            log("Try running in verbose mode to determine the cause of the error.");
                    }
                    else
                        log("\n");
                };

            //Return the process
            return commandLineProccess;
        }

        //Prints the output (or errors) if verbose mode is enabled
        private static void processOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (verboseMode && e.Data != null)
                log("    " + e.Data.ToString());
        }
    }
}
