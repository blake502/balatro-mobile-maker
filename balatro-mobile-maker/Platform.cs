using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
//using System.IO;
using static balatro_mobile_maker.View;
using static balatro_mobile_maker.Tools;

namespace balatro_mobile_maker;

internal class Platform
{
    //I'm not sure which way will be easier to work with, so I'm doing both.
    public static bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private static bool isOSX = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private static bool isLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    private static bool isX64 = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.X64;
    private static bool isX86 = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.X86;
    private static bool isArm64 = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.Arm64;
    private static bool isArm = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.Arm;

    //Uses ADB with args
    public static void useADB(string args)
    {
        if (isWindows)
            RunCommand("platform-tools\\platform-tools\\adb.exe", args);


        //TODO: Implement ADB for OSX and Linux
        if (isOSX) { /*...*/ }

        if (isLinux) { /*...*/ }
    }

    //Uses Java with args
    public static void useOpenJDK(string args)
    {
        if (isWindows)
        {
            if (!fileExists("jdk-21.0.3+9\\bin\\java.exe"))
            {
                Log("Preparing OpenJDK...");
                fileMove("openjdk", "openjdk.zip");
                tryDelete("jdk-21.0.3+9");
                extractZip("openjdk.zip", ".");
            }

            RunCommand("jdk-21.0.3+9\\bin\\java.exe", args);
        }

        //TODO: OSX and Linux implementation is purely speculative! Untested!!!
        if (isOSX)
        {
            if (!fileExists("jdk-21.0.3+9/Contents/Home/bin/java"))
            {
                Log("Preparing OpenJDK...");
                fileMove("openjdk", "openjdk.tar.gz");
                tryDelete("jdk-21.0.3+9");
                RunCommand("tar", "-xf openjdk.tar.gz");
                RunCommand("chmod", "-R +x jdk-21.0.3+9");
            }

            RunCommand("./jdk-21.0.3+9/Contents/Home/bin/java", args);
        }

        if (isLinux)
        {
            if (!fileExists("jdk-21.0.3+9/bin/java"))
            {
                Log("Preparing OpenJDK...");
                fileMove("openjdk", "openjdk.tar.gz");
                tryDelete("jdk-21.0.3+9");
                RunCommand("tar", "-xf openjdk.tar.gz");
                RunCommand("chmod", "-R +x jdk-21.0.3+9");
            }

            RunCommand("./jdk-21.0.3+9/bin/java", args);
        }
    }

    public static string getOpenJDKDownloadLink()
    {
        if (isWindows)
        {
            if (isX64)
                return Constants.OpenJDKWinX64Link;
            //TODO: uhh something maybe
            //if (isX86)
            //    return Constants.OpenJDKWinX86Link; 
            if (isArm64)
                return Constants.OpenJDKWinArm64Link;
        }

        if (isOSX)
        {
            if (isX64)
                return Constants.OpenJDKOSXX64Link;
            if (isArm64)
                return Constants.OpenJDKOSXArm64Link;
        }

        if (isLinux)
        {
            if (isX64)
                return Constants.OpenJDKLinuxX64Link;
            if (isArm64)
                return Constants.OpenJDKLinuxArm64Link;
        }


        return "";
    }


    public static string getGameSaveLocation()
    {
        if (isWindows)
            return Environment.GetEnvironmentVariable("AppData") + "\\Balatro";

        //TODO: Test Linux location
        if (isLinux)
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.local/share/Steam/steamapps/compatdata/2379780/pfx/drive_c/users/steamuser/AppData/Roaming/Balatro";

        //TODO: Implement
        if (isOSX)
           return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Library/Application Support/Balatro";

        return ".";
    }

    //Checks whether the game already exists in the directory
    //If it does not exist, it attempts to grab it from the default location
    //If it does already exist, this returns true
    public static bool gameExists()
    {
        if(isWindows)
            return fileExists("Balatro.exe");


        if (isOSX)
        {
            //TODO: This isn't great, but it should work
            if (fileExists("Balatro.love"))
                fileCopy("Balatro.love", "Balatro.exe");

            if (fileExists("Balatro.exe"))
                return true;
        }

        if (isLinux)
            return fileExists("Balatro.exe");

        return false;
    }

    //Attempts to copy game from default directory if it does not already exist
    //Returns false if the game is not located
    public static bool tryLocateGame()
    {
        //If game exists, it's successfully located
        if (gameExists())
            return true;

        string location = "";

        if (isWindows)
            location = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Balatro\\Balatro.exe";

        //TODO: Test OSX and Linux locations!!!
        if (isOSX)
            location = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Library/Application Support/Steam/steamapps/common/Balatro/Balatro.app/Contents/Resources/Balatro.love";

        if (isLinux)
            location = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.local/share/Steam/steamapps/common/Balatro/Balatro.exe";


        //Attempt to copy Balatro from Steam directory
        if (fileExists(location))
        {
            Log("Copying Balatro from Steam directory...");
            fileCopy(location, "Balatro.exe");
        }

        //Return whether the game exists now after attempting to copy
        return gameExists();
    }
}
