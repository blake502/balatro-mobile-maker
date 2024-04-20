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
            CommandLine("platform-tools\\platform-tools\\adb.exe " + args);

        if (isOSX) { /*...*/ }

        if (isLinux) { /*...*/ }
    }

    //Uses Java with args
    public static void useJava(string args)
    {
        if (isWindows)
            CommandLine("jdk-21.0.3+9\\bin\\java.exe " + args);

        if (isOSX) { /*...*/ }

        if (isLinux) { /*...*/ }
    }

    //Uses Python with args
    public static void usePython(string args)
    {
        if (isWindows)
            CommandLine("python\\python.exe " + args);

        if (isOSX) { /*...*/ }

        if (isLinux) { /*...*/ }
    }

    public static string get7ZipDownloadLink()
    {
        if (isWindows)
        {
            if (isX64)
                return Constants.SevenzipWinX64Link;
            if (isX86)
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
            if (isX86)
                return Constants.SevenzipLinuxX86Link;
            if (isArm64)
                return Constants.SevenzipLinuxArm64Link;
            if (isArm)
                return Constants.SevenzipLinuxArmLink;
        }


        return "";
    }

    public static string getGameSaveLocation()
    {
        if (isWindows)
            return "%AppData%/Balatro/";

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
            CommandLine("xcopy \"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Balatro\\Balatro.exe\" \"Balatro.exe\" /E /H /Y /V /-I");
        }

        if (isOSX)
        {
            //Attempt to copy Balatro game from default location
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
