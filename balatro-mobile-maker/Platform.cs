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

    //Uses 7Zip with args
    public static void useSevenZip(string args)
    {
        args += " -y";//Always assume "Yes" is the answer to all queries

        if (isWindows)
            RunCommand("7za.exe", args);

        //TODO: OSX and Linux implementation is purely speculative! Untested!!!
        if (isOSX)
        {
            if (!fileExists("7zz"))
            {
                RunCommand("tar", "-xf 7zip.tar.xz");
                RunCommand("chmod", "+x 7zz");
            }

            RunCommand("./7zz", args);
        }

        if (isLinux)
        {
            if (!fileExists("7zzs"))
            {
                RunCommand("tar", "-xf 7zip.tar.xz");
                RunCommand("chmod", "+x 7zzs");
            }

            RunCommand("./7zzs", args);
        }
    }

    public static void download7Zip()
    {
        string link = "";
        if (isWindows)
        {
            if (isX64)
                link = Constants.SevenzipWinX64Link;
            if (isX86) //May not be supported, but included for now
                link = Constants.SevenzipWinX86Link;
            if (isArm64)
                link = Constants.SevenzipWinArm64Link;

            TryDownloadFile("7-Zip", link, "7za.exe");
        }

        //TODO: Test OSX and Linux implementation

        if (isOSX)
        {
            link = Constants.SevenzipOSXLink;
            TryDownloadFile("7-Zip", link, "7zip.tar.xz");
        }

        if (isLinux)
        {
            if (isX64)
                link = Constants.SevenzipLinuxX64Link;
            if (isX86) //May not be supported, but included for now
                link = Constants.SevenzipLinuxX86Link;
            if (isArm64)
                link = Constants.SevenzipLinuxArm64Link;
            if (isArm) //May not be supported, but included for now
                link = Constants.SevenzipLinuxArmLink;

            TryDownloadFile("7-Zip", link, "7zip.tar.xz");
        }
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
                useTool(ProcessTools.SevenZip, "x openjdk.zip");
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
                RunCommand("chmod", "+x jdk-21.0.3+9/Contents/Home/bin/java");
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
                RunCommand("chmod", "+x jdk-21.0.3+9/bin/java");
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
            fileCopy(location, "Balatro.exe"); //Note!!! On MacOS, this will rename game.love to balatro.exe! This may or may not work, depending on 7-Zip's willingness to play along.
        }

        //Return whether the game exists now after attempting to copy
        return gameExists();
    }
}
