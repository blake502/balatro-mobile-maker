namespace Balatro_APK_Maker;

/// <summary>
/// Constant values.
/// Please keep constants in here, as it allows for conditional compilation to control per-platform behaviour.
/// </summary>
internal static class Constants
{
    public const string JavaCommand = "java -version";
    public const string PythonCommand = "python --version 3>NUL";
    public const string Jre8InstallerLink = "https://javadl.oracle.com/webapps/download/AutoDL?BundleId=249553_4d245f941845490c91360409ecffb3b4";
    public const string JavaDownloadCommand = "explorer https://www.java.com/download/";
    public const string SevenzipLink = "https://github.com/blake502/balatro-apk-maker/releases/download/Additional-Tools-1.0/7za.exe";
    public const string ApktoolLink = "https://bitbucket.org/iBotPeaches/apktool/downloads/apktool_2.9.3.jar";
    public const string UberapktoolLink = "https://github.com/patrickfav/uber-apk-signer/releases/download/v1.3.0/uber-apk-signer-1.3.0.jar";
    public const string BalatroApkPatchLink = "https://github.com/blake502/balatro-apk-maker/releases/download/Additional-Tools-1.0/Balatro-APK-Patch.zip";
    public const string Love2dApkLink = "https://github.com/love2d/love-android/releases/download/11.5a/love-11.5-android-embed.apk";
    public const string PlatformToolsLink = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip";
    public const string IosBaseLink = "https://github.com/blake502/balatro-apk-maker/releases/download/Additional-Tools-1.0/balatro-base.ipa";
    public const string PythonLink = "https://www.python.org/ftp/python/3.12.3/python-3.12.3-amd64.exe";
}