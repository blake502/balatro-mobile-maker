using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static balatro_mobile_maker.View;

namespace balatro_mobile_maker;

internal class Platform
{
    //I'm not sure which way will be easier to work with, so I'm doing both.
    private static bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    private static bool isOSX = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    private static bool isLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    private static bool isX64 = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.X64;
    private static bool isX86 = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.X86;
    private static bool isArm64 = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.Arm64;
    private static bool isArm = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == Architecture.Arm;

    private static Architecture architecture = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;

    //Uses ADB with args
    public static void useADB(string args)
    {
        if (isWindows)
            CommandLine("platform-tools\\platform-tools\\adb.exe", args);

        if (isOSX) { /*...*/ }

        if (isLinux) { /*...*/ }
    }

    //Uses 7Zip with args
    public static void useSevenZip(string args)
    {
        if (isWindows)
            CommandLine("7za.exe", args);

        if (isOSX) { /*...*/ }

        if (isLinux) {
            if (!File.Exists("7zzs"))
            {
                //tar -xf filename.tar.gz
            }
            else
            {
                CommandLine("7zzs", args);
            }
        }
    }

    //Uses Java with args
    public static void useJava(string args)
    {
        if (isWindows)
            CommandLine("jdk-21.0.3+9\\bin\\java.exe", args);

        if (isOSX) { /*...*/ }

        if (isLinux) { /*...*/ }
    }

    //Uses Python with args
    public static void usePython(string args)
    {
        if (isWindows)
            CommandLine("python\\python.exe", args);

        if (isOSX) { /*...*/ }

        if (isLinux) { /*...*/ }
    }
    public static string getOpenJDKDownloadLink()
    {
        if (isWindows)
        {
            if (isX64)
                return Constants.OpenJDKWinX64Link;
            //TODO: uhh something maybe
            //if (isX86)
            //return Constants.OpenJDKWinX86Link; 
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

    //There's probably a better way to do this, but oh well.
    public static string getOpenJDKDownloadExtension()
    {
        if (isWindows)
            return ".zip";

        if (isOSX)
            return ".tar.gz";

        if (isLinux)
            return ".tar.gz";

        return "";
    }

    public static string get7ZipDownloadLink()
    {
        if (isWindows)
        {
            if (isX64)
                return Constants.SevenzipWinX64Link;
            if (isX86) //May not be supported, but included for now
                return Constants.SevenzipWinX86Link;
            if (isArm64)
                return Constants.SevenzipWinArm64Link;
        }

        if (isOSX)
            return Constants.SevenzipOSXLink;

        if (isLinux)
        {
            if (isX64)
                return Constants.SevenzipLinuxX64Link;
            if (isX86) //May not be supported, but included for now
                return Constants.SevenzipLinuxX86Link;
            if (isArm64)
                return Constants.SevenzipLinuxArm64Link;
            if (isArm) //May not be supported, but included for now
                return Constants.SevenzipLinuxArmLink;
        }


        return "";
    }
    public static string getPythonDownloadLink()
    {
        if (isWindows)
        {
            if (isX64)
                return Constants.PythonWinX64Link;
            if (isX86) //May not be supported, but included for now
                return Constants.PythonWinX86Link;
            if (isArm64)
                return Constants.PythonWinArm64Link;
        }

        if (isOSX) { /*...*/ }

        if (isLinux) { /*...*/ }

        return "";
    }

    public static string getGameSaveLocation()
    {
        if (isWindows)
            return Environment.GetEnvironmentVariable("AppData") + "\\Balatro";

        return ".";
    }

    //Checks whether the game already exists in the directory
    //If it does not exist, it attempts to grab it from the default location
    //If it does already exist, this returns true
    public static bool gameExists()
    {
        if(isWindows)
            return File.Exists("Balatro.exe");

        if (isOSX) { /*...*/ }

        if (isLinux) { /*...*/ }

        return false;
    }

    //Attempts to copy game from default directory if it does not already exist
    //Returns false if the game is not located
    public static bool tryLocateGame()
    {
        //If game exists, it's successfully located
        if (gameExists())
            return true;

        if (isWindows)
        {
            //Attempt to copy Balatro.exe from Steam directory
            Log("Copying Balatro.exe from Steam directory...");
            File.Copy("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Balatro\\Balatro.exe", "Balatro.exe");
            //CommandLine("xcopy", "\"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Balatro\\Balatro.exe\" \"Balatro.exe\" /E /H /Y /V /-I");
        }

        if (isOSX)
        {
            //Attempt to copy Balatro game from default location

            //Here-ish maybe?  ~/Library/Application Support/Steam ??? \steamapps\common\Balatro\Balatro.app\Contents\Resources\Balatro.love

            //...
        }

        if (isLinux)
        {
            //Attempt to copy Balatro game from default location
            //...
        }

        //Return whether the game exists now after attempting to copy
        return gameExists();
    }
}
