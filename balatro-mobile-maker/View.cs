using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using static balatro_mobile_maker.Constants;
using static balatro_mobile_maker.Tools;
using static balatro_mobile_maker.Program;

namespace balatro_mobile_maker;

/// <summary>
/// Command line UI for Balatro APK Maker.
/// </summary>
// NOTE: Much should be refactored out of UI logic land, and, into a Controller which we can query state from.
internal class View
{

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
        if (!(fileExists("balatro.apk") || fileExists("balatro.ipa")) || AskQuestion("A previous build was found... Would you like to build again?"))
        {
            _androidBuild = AskQuestion("Would you like to build for Android?");
            _iosBuild = AskQuestion("Would you like to build for iOS (experimental)?");


            if (_androidBuild || _iosBuild)
            {
                #region Download tools
                if (_androidBuild)
                {
                    #region Android tools
                    //Downloading tools. Handled in threads to allow simultaneous downloads
                    Thread[] downloadThreads =
                    [
                        new Thread(() => { TryDownloadFile("OpenJDK", Platform.getOpenJDKDownloadLink(), "openjdk"); }),
                        new Thread(() => { Platform.download7Zip(); }),

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
                        new Thread(() => { Platform.download7Zip(); }),
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
                if (directoryExists("Balatro"))
                {
                    //Delete the Balatro folder if it already exists
                    Log("Balatro directory already exists! Deleting Balatro directory...");
                    tryDelete("Balatro");
                }

                //Extract Balatro.exe with 7-Zip
                useTool(ProcessTools.SevenZip, "x Balatro.exe -oBalatro");

                //Check for failure
                if (!directoryExists("Balatro"))
                {
                    Log("Failed to extract Balatro.exe!");
                    Exit();
                }
                #endregion

                if (_androidBuild)
                {
                    #region Extract APK
                    Log("Unpacking Love2D APK with APK Tool...");
                    if (directoryExists("balatro-apk"))
                    {
                        //Delete the balatro-apk folder if it already exists
                        Log("balatro-apk directory already exists! Deleting balatro-apk directory...");
                        tryDelete("balatro-apk");
                    }

                    //Unpack Love2D APK
                    useTool(ProcessTools.Java, "-jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" d -s -o balatro-apk love-11.5-android-embed.apk");

                    //Check for failure
                    if (!directoryExists("balatro-apk"))
                    {
                        Log("Failed to unpack Love2D APK with APK Tool!");
                        Exit();
                    }
                    #endregion

                    #region APK patch
                    Log("Extracting patch zip...");
                    if (directoryExists("Balatro-APK-Patch"))
                    {
                        Log("Balatro-APK-Patch directory already exists! Deleting Balatro-APK-Patch directory...");
                        tryDelete("Balatro-APK-Patch");
                    }

                    //Extract Balatro-APK-Patch
                    useTool(ProcessTools.SevenZip, "x Balatro-APK-Patch.zip -oBalatro-APK-Patch");

                    if (!directoryExists("Balatro-APK-Patch"))
                    {
                        Log("Failed to extract Balatro-APK-Patch");
                        Exit();
                    }

                    //Base APK patch
                    Log("Patching APK folder...");
                    //This isn't pretty, but I'm planning to change how icons are done at some point. So this is fine for now.
                    fileCopy("Balatro-APK-Patch/AndroidManifest.xml", "balatro-apk/AndroidManifest.xml");
                    fileCopy("Balatro-APK-Patch/res/drawable-hdpi/love.png", "balatro-apk/res/drawable-hdpi/love.png");
                    fileCopy("Balatro-APK-Patch/res/drawable-mdpi/love.png", "balatro-apk/res/drawable-mdpi/love.png");
                    fileCopy("Balatro-APK-Patch/res/drawable-xhdpi/love.png", "balatro-apk/res/drawable-xhdpi/love.png");
                    fileCopy("Balatro-APK-Patch/res/drawable-xxhdpi/love.png", "balatro-apk/res/drawable-xxhdpi/love.png");
                    fileCopy("Balatro-APK-Patch/res/drawable-xxxhdpi/love.png", "balatro-apk/res/drawable-xxxhdpi/love.png");
                    #endregion
                }

                if (_iosBuild)
                {
                    #region Prepare IPA
                    Log("Preparing iOS Base...");
                    fileMove("balatro-base.ipa", "balatro-base.zip");
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
                useTool(ProcessTools.SevenZip, "a balatro.zip Balatro/.");

                if (!fileExists("balatro.zip"))
                {
                    Log("Failed to pack Balatro folder!");
                    Exit();
                }

                Log("Moving archive...");
                if (_androidBuild)
                    fileCopy("balatro.zip", "balatro-apk/assets/game.love");

                if (_iosBuild)
                    fileCopy("balatro.zip", "game.love");
                #endregion

                if (_androidBuild)
                {
                    #region Packing APK
                    Log("Repacking APK...");
                    useTool(ProcessTools.Java, "-jar -Xmx1G -Duser.language=en -Dfile.encoding=UTF8 -Djdk.util.zip.disableZip64ExtraFieldValidation=true -Djdk.nio.zipfs.allowDotZipEntry=true \"apktool.jar\" b -o balatro.apk balatro-apk");

                    if (!fileExists("balatro.apk"))
                    {
                        Log("Failed to pack Balatro apk!");
                        Exit();
                    }
                    #endregion

                    #region Signing APK
                    Log("Signing APK...");
                    useTool(ProcessTools.Java, "-jar uber-apk-signer.jar -a balatro.apk");

                    if (!fileExists("balatro-aligned-debugSigned.apk"))
                    {
                        Log("Failed to sign APK!");
                        Exit();
                    }

                    Log("Renaming unsigned apk...");
                    fileMove("balatro.apk", "balatro-unsigned.apk");

                    Log("Renaming signed apk...");
                    fileMove("balatro-aligned-debugSigned.apk", "balatro.apk");
                    #endregion
                }

                if (_iosBuild)
                {
                    #region Packing IPA
                   
                    Log("Repacking iOS app...");
                    ModifyZip();

                    fileMove("balatro-base.zip", "balatro.ipa");
                    #endregion
                }
                Log("Build successful!");
                #endregion
            }
        }

        //TODO: Implement for OSX and Linux!!!
        if ((!_iosBuild || _androidBuild) && Platform.isWindows)
        {
            #region Android options
            #region Auto-install
            if (fileExists("balatro.apk") && AskQuestion("Would you like to automaticaly install balatro.apk on your Android device?"))
            {
                PrepareAndroidPlatformTools();

                Log("Attempting to install. If prompted, please allow the USB Debugging connection on your Android device.");

                useTool(ProcessTools.ADB, "install balatro.apk");
                useTool(ProcessTools.ADB, "kill-server");
            }
            #endregion

            #region Save transfer

            if (directoryExists(Environment.GetEnvironmentVariable("AppData") + "\\Balatro") && AskQuestion("Would you like to transfer saves from your Steam copy of Balatro to your Android device?"))
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
                if (AskQuestion("Would you like to pull saves from your Android device?"))
                {
                    Log("Warning! If Steam Cloud is enabled, it will overwrite the save you transfer!");
                    while (!AskQuestion("Have you backed up your saves?"))
                        Log("Please back up your saves! I am not responsible if your saves get deleted!");

                    PrepareAndroidPlatformTools();

                    Log("Backing up your files...");
                    if (!directoryExists(Platform.getGameSaveLocation() + "BACKUP"))
                        System.IO.Directory.CreateDirectory(Platform.getGameSaveLocation() + "BACKUP/");
                    //TODO: No xcopy
                    RunCommand("xcopy", "\"" + Platform.getGameSaveLocation() + "\" \"" + Platform.getGameSaveLocation() + "BACKUP\\\" /E /H /Y /V");
                    tryDelete(Platform.getGameSaveLocation());
                    System.IO.Directory.CreateDirectory(Platform.getGameSaveLocation());

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
                    useTool(ProcessTools.ADB, "shell find /data/local/tmp/balatro/files/ -maxdepth 2 -size 0c -exec rm '{}' \\;");
                    useTool(ProcessTools.ADB, "pull /data/local/tmp/balatro/files/. \"" + Platform.getGameSaveLocation() + "\"");

                    useTool(ProcessTools.ADB, "kill-server");
                }
            }
            #endregion
            #endregion
        }

        Log("Finished!");
        Exit();
    }

    public static void Cleanup()
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
            tryDelete("openjdk.tar.gz");
            tryDelete("openjdk");
            tryDelete("balatro-aligned-debugSigned.apk.idsig");
            tryDelete("balatro-unsigned.apk");
            tryDelete("platform-tools.zip");
            tryDelete("ios.py");
            tryDelete("balatro.zip");
            tryDelete("game.love");

            //extras for Linux
            //TODO: Fix 7-Zip extraction on Linux
            tryDelete("License.txt");
            tryDelete("readme.txt");
            tryDelete("History.txt");
            tryDelete("7zzs");
            tryDelete("7zz");
            tryDelete("7zz.tar.xz");
            tryDelete("MANUAL");

            tryDelete("platform-tools");
            tryDelete("jdk-21.0.3+9");
            tryDelete("Balatro-APK-Patch");//TODO: remove when Android build changes
            //tryDelete("icons");//TODO: enable when Android build changes
            tryDelete("Balatro");
            tryDelete("balatro-apk");
            if (!gameProvided)
                tryDelete("Balatro.exe");
        }
    }

    /// <summary>
    /// Prepare Android platform-tools, and prompt user to enable USB debugging
    /// </summary>
    void PrepareAndroidPlatformTools()
    {
        //Check whether they already exist
        if (!directoryExists("platform-tools"))
        {
            Log("Platform tools not found...");

            if (!fileExists("platform-tools.zip"))
                TryDownloadFile("platform-tools", PlatformToolsLink, "platform-tools.zip");

            Platform.download7Zip();

            Log("Extracting platform-tools...");
            useTool(ProcessTools.SevenZip, "x platform-tools.zip -oplatform-tools");
        }

        //Prompt user
        while (!AskQuestion("Is your Android device connected to the host with USB Debugging enabled?"))
            Log("Please enable USB Debugging on your Android device, and connect it to the host.");
    }
}
