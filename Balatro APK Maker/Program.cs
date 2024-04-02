using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Balatro_APK_Maker
{
    internal class Program
    {

        const string javaCommand = "java -version";
        const string javaDownloadCommand = "explorer https://www.java.com/download/";
        const string sevenzipLink = "http://smudge.codes/files/7za.exe";
        const string apktoolLink = "https://bitbucket.org/iBotPeaches/apktool/downloads/apktool_2.9.3.jar";
        const string uberapktoolLink = "https://github.com/patrickfav/uber-apk-signer/releases/download/v1.3.0/uber-apk-signer-1.3.0.jar";
        const string balatroApkPatchLink = "http://smudge.codes/files/Balatro-APK-Patch.zip";
        const string love2dApkLink = "https://github.com/love2d/love-android/releases/download/11.5a/love-11.5-android-embed.apk";

        static bool verboseMode = false;

        //This applies a "patch" given a file path, a string from a line to be replaced, and the text with which to replace it.
        static void applyPatch(string file, string lineContains, string replaceWith)
        {
            //Read the file
            log("Loading " + file + " file...");
            List<string> loadedFile = File.ReadAllLines("Balatro\\" + file).ToList();

            //Search for the line to replace
            bool success = false;
            for (int i = 0; i < loadedFile.Count; i++)
            {
                //If it's found
                if (loadedFile[i].IndexOf(lineContains) != -1)
                {
                    //Replace the line
                    loadedFile[i] = replaceWith;
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                //If it's not found
                log("Unable to find patch location...");
                //exit();
            }
            else
            {
                //If it is found, write the file.
                log("Successfully applied patch...");
                File.WriteAllLines("Balatro\\" + file, loadedFile);
            }
        }

        //Prompts the user to select which patches they want, then applies them.
        //This is hideous. But it works.
        static void applyPatches()
        {
            log("Applying Android compatibilty patch...");
            applyPatch("globals.lua", "loadstring(\"", @"    --loadstring(""\105\102\32\108\111\118\101\46\115\121\115\116\101\109\46\103\101\116\79\83\40\41\32\61\61\32\39\105\79\83\39\32\111\114\32\108\111\118\101\46\115\121\115\116\101\109\46\103\101\116\79\83\40\41\32\61\61\32\39\65\110\100\114\111\105\100\39\32\116\104\101\110\10\32\32\108\111\118\101\46\101\118\101\110\116\46\113\117\105\116\40\41\10\101\110\100\10"")()
    if love.system.getOS() == 'Android' then
        self.F_DISCORD = true
        self.F_SOUND_THREAD = true
        self.F_VIDEO_SETTINGS = false
        self.F_NO_ACHIEVEMENTS = true
    end
");

            //Ask whether they want the FPS cap patch
            if (askQuestion("Would you like to apply the FPS cap patch?"))
            {
                //Have the user select a number between 15 and 120
                string input = null;
                int fps = -1;
                do
                {
                    if (input != null)
                        log("Enter a number between 15 and 120!");
                    log("Please enter your desired FPS cap (15 to 120):");
                    input = Console.ReadLine().ToLower();
                    try
                    {
                        fps = Convert.ToInt32(input);
                    }
                    catch
                    {
                        continue;
                    }

                } while (fps < 15 || fps > 120);
                //Apply the patch using the given FPS
                applyPatch("main.lua", "G.FPS_CAP = G.FPS_CAP or", "        G.FPS_CAP = " + fps.ToString());
            }

            //Extra patches

            if (askQuestion("Would you like to apply the landscape orientation patch?"))
                applyPatch("main.lua", "local os = love.system.getOS()", "    local os = love.system.getOS()\n    love.window.setMode(2, 1)");

            if (askQuestion("Would you like to apply the CRT shader disable patch?"))
            {
                applyPatch("globals.lua", "crt = ", "            crt = 0,");
                applyPatch("game.lua", "G.SHADERS['CRT'])", "");
            }

            if (askQuestion("Would you like to apply the accessible saves patch?"))
                applyPatch("conf.lua", "t.window.width = 0", "    t.window.width = 0\n    t.externalstorage = true");
        }

        //Attempts to download a file if it does not exist.
        //Take a friendly name, a URL, and a file name.
        static void tryDownloadFile(string name, string link, string fileName)
        {
            //If the file does not already exist
            if (!File.Exists(fileName))
            {
                //Call powershell to download it
                //Yes, yes, I know there are better, faster ways to do this. But this is easy.
                log("Downloading " + name + "...");
                commandLine("powershell -Command \"Invoke-WebRequest " + link + " -OutFile " + fileName + "\"");

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
            do
            {
                if (input != null)
                    log("Enter either 'Y' or 'N'!");
                log(question + " (y/n):");
                input = Console.ReadLine().ToLower();

            } while (input != "y" && input != "n");
            return input == "y";
        }

        static void Main(string[] args)
        {
            log("====Balatro APK Maker====\n7-Zip is licensed under the GNU LGPL license. Please visit: www.7-zip.org\n\n");

            //Initial prompts
            bool cleanUpTools = askQuestion("Would you like to clean up temporary tools?");
            bool cleanUpFiles = askQuestion("Would you like to clean up temporary folders and files?");
            verboseMode = askQuestion("Would you like to run in verbose mode?");
            
            //Check for Java
            log("Checking for Java...");
            if (commandLine(javaCommand).ExitCode == 0) //Nothing to do if it's found
                log("Java found.");
            else
            {
                log("Java not found, please install Java!");
                //Prompt user to automatically download install Java
                if (askQuestion("Would you like to automatically download and install Java?"))
                {
                    //Download
                    tryDownloadFile("Java", "https://javadl.oracle.com/webapps/download/AutoDL?BundleId=249553_4d245f941845490c91360409ecffb3b4", "java-installer.exe");
                    //Install
                    log("Installing Java...");
                    commandLine("java-installer.exe /s");

                    //Delete the installer if user elects to clean up tools
                    if (cleanUpTools)
                    {
                        log("Deleting Java installer...");
                        commandLine("del java-installer.exe");
                    }

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


            bool exeProvided = false;
            if (!File.Exists("Balatro.exe"))
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
            {
                //User provided Balatro.exe
                exeProvided = true;
                log("Balatro.exe already exists.");
            }

            log("Extracting Balatro.exe");
            if(Directory.Exists("Balatro"))
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

            //Delete the temporary Balatro.exe if user elected to clean up files, unless the user provided it.
            if (!exeProvided && cleanUpFiles)
            {
                log("Deleting Balatro.exe");
                commandLine("del Balatro.exe");
            }

            log("Unpacking Love2D APK with APK Tool...");
            if (Directory.Exists("balatro-apk"))
            {
                //Delete the balatro-apk folder if it already exists
                log("balatro-apk directory already exists! Deleting balatro-apk directory...");
                commandLine("rmdir balatro-apk\\ /S /Q");
            }

            //Unpack Love2D APK
            commandLine("java.exe -jar -Xmx1024M -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" d -s -o balatro-apk love-11.5-android-embed.apk");

            //Check for failure
            if(!Directory.Exists("balatro-apk"))
            {
                log("Failed to unpack Love2D APK with APK Tool!");
                exit();
            }

            //Delete temporary Love2D APK.
            if (cleanUpTools)
            {
                log("Deleting Love2D APK...");
                commandLine("del love-11.5-android-embed.apk");
            }

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
            
            if (cleanUpTools)
            {
                log("Deleting patch zip...");
                commandLine("del Balatro-APK-Patch.zip");
            }

            log("Patching APK folder...");
            commandLine("xcopy \"Balatro-APK-Patch\\\" \"balatro-apk\\\" /E /H /Y /V");

            if (cleanUpFiles)
            {
                log("Deleting patch folder...");
                commandLine("rmdir Balatro-APK-Patch\\ /S /Q");
            }

            log("Patching...");
            applyPatches();

            log("Packing Balatro folder...");
            commandLine("\"cd Balatro && ..\\7za.exe a balatro.zip && cd ..\"");

            if(!File.Exists("Balatro\\balatro.zip"))
            {
                log("Failed to pack Balatro folder!");
                exit();
            }

            if (cleanUpTools)
            {
                log("Deleting 7-Zip...");
                commandLine("del 7za.exe");
            }

            log("Moving archive...");
            commandLine("move Balatro\\balatro.zip balatro-apk\\assets\\game.love");

            if (cleanUpFiles)
            {
                log("Deleting Balatro folder...");
                commandLine("rmdir Balatro\\ /S /Q");
            }

            log("Repacking APK...");
            commandLine("java.exe -jar -Xmx1024M -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" b -o balatro.apk balatro-apk");

            if (!File.Exists("balatro.apk"))
            {
                log("Failed to pack Balatro apk!");
                exit();
            }

            if (cleanUpTools)
            {
                log("Deleting APK Tool...");
                commandLine("del apktool.jar");
            }

            if (cleanUpFiles)
            {
                log("Deleting balatro-apk folder...");
                commandLine("rmdir balatro-apk\\ /S /Q");
            }

            log("Signing APK...");
            commandLine("java -jar uber-apk-signer.jar -a balatro.apk");

            if (!File.Exists("balatro-aligned-debugSigned.apk"))
            {
                log("Failed to sign APK!");
                exit();
            }

            if (cleanUpTools)
            {
                log("Deleting uber-apk-signer...");
                commandLine("del uber-apk-signer.jar");
            }

            if (cleanUpFiles)
            {
                log("Deleting unsigned balatro.apk...");
                commandLine("del balatro.apk");
            }
            else
            {
                log("Renaming unsigned apk...");
                commandLine("move balatro.apk balatro-unsigned.apk");
            }

            if (cleanUpFiles)
            {
                log("Deleting idsig file...");
                commandLine("del balatro-aligned-debugSigned.apk.idsig");
            }

            log("Renaming signed apk...");
            commandLine("move balatro-aligned-debugSigned.apk balatro.apk");

            log("Success!");

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
                };
            
            //Return the process
            return commandLineProccess;
        }

        //Prints the output (or errors) if verbose mode is enabled
        private static void processOutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && verboseMode)
            {
                log(e.Data.ToString());
            }
        }
    }
}
