namespace balatro_mobile_maker;

/// <summary>
/// Constant values.
/// Please keep constants in here, as it allows for conditional compilation to control per-platform behaviour.
/// </summary>
internal static class Constants
{
    //Extra tools download links
    public const string ApktoolLink = "https://github.com/iBotPeaches/Apktool/releases/download/v2.9.3/apktool_2.9.3.jar";
    public const string UberapktoolLink = "https://github.com/patrickfav/uber-apk-signer/releases/download/v1.3.0/uber-apk-signer-1.3.0.jar";
    public const string BalatroApkPatchLink = "https://github.com/blake502/balatro-apk-maker/releases/download/Additional-Tools-1.0/Balatro-APK-Patch.zip";
    public const string Love2dApkLink = "https://github.com/love2d/love-android/releases/download/11.5a/love-11.5-android-embed.apk";
    public const string IosBaseLink = "https://github.com/blake502/balatro-apk-maker/releases/download/Additional-Tools-1.0/balatro-base.ipa";

    //ADB
    public const string PlatformToolsLink = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";

    //OpenJDK Download Links
    //TODO: Find JDK links for all platforms
    //Win
    public const string OpenJDKWinX64Link = "https://aka.ms/download-jdk/microsoft-jdk-21.0.3-windows-x64.zip";
    //public const string OpenJDKWinX86Link = ""; //Uh oh: https://learn.microsoft.com/en-us/java/openjdk/download
    public const string OpenJDKWinArm64Link = "https://aka.ms/download-jdk/microsoft-jdk-21.0.3-windows-aarch64.zip";
    //Linux
    public const string OpenJDKLinuxX64Link = "https://aka.ms/download-jdk/microsoft-jdk-21.0.3-linux-x64.tar.gz";
    public const string OpenJDKLinuxArm64Link = "https://aka.ms/download-jdk/microsoft-jdk-21.0.3-linux-aarch64.tar.gz";
    //macOS
    public const string OpenJDKOSXX64Link = "https://aka.ms/download-jdk/microsoft-jdk-21.0.3-macos-x64.tar.gz";
    public const string OpenJDKOSXArm64Link = "https://aka.ms/download-jdk/microsoft-jdk-21.0.3-macos-aarch64.tar.gz";

    
    //7-Zip Download links
    //Win
    public const string SevenzipWinX64Link = "https://github.com/blake502/balatro-mobile-maker/releases/download/Additional-Tools-1.1/7za-x64.exe";
    public const string SevenzipWinX86Link = "https://github.com/blake502/balatro-mobile-maker/releases/download/Additional-Tools-1.1/7za-x86.exe";
    public const string SevenzipWinArm64Link = "https://github.com/blake502/balatro-mobile-maker/releases/download/Additional-Tools-1.1/7za-arm64.exe";
    //Linux
    public const string SevenzipLinuxX64Link = "https://www.7-zip.org/a/7z2403-linux-x64.tar.xz";
    public const string SevenzipLinuxX86Link = "https://www.7-zip.org/a/7z2403-linux-x86.tar.xz";
    public const string SevenzipLinuxArm64Link = "https://www.7-zip.org/a/7z2403-linux-arm64.tar.xz";
    public const string SevenzipLinuxArmLink = "https://www.7-zip.org/a/7z2403-linux-arm.tar.xz";
    //macOS
    public const string SevenzipOSXLink = "https://www.7-zip.org/a/7z2403-mac.tar.xz";


} 