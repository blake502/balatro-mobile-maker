# Balatro APK Maker

## Goal
This goal of this script is to allow *Balatro* fans to play *Balatro* on their mobile devices before the official release. This script provides a **NON-PIRACY** avenue for players to do so, by converting their *Steam* version of *Balatro* to an APK format.

## Quick Start Guide
 - Download [**balatro-apk-maker.bat**](https://smudge.codes/files/balatro-apk-maker.bat) (Right-click, "Save Page As...")
 - Run **balatro-apk-maker.bat** as admin.
 - When *Notepad* appears, modify the file according to the **Code Changes** section below.
 - Save the file, then close *Notepad*.
 - Copy the resulting **balatro.apk** to your Android device.

## Code Changes
When *Notepad* appears during this script, you must add two minus symbols to the line that begins with `loadstring("` ([approximately line 43](https://youtu.be/CfUHN2HJbj8?si=ASQsLVJb1fxNeFP0&t=234)) such that it begins with `--loadstring("` instead.

Under that line, directly above the line that begins with `if love.system.getOS() == 'Windows' then`, you must paste in this code:
```

if love.system.getOS() == 'Android' then
    self.F_DISCORD = true
    self.F_SOUND_THREAD = true
    self.F_VIDEO_SETTINGS = false
    self.F_NO_ACHIEVEMENTS = true
end

```
Save the file with *Notepad*.

## Notes
 - This script assumes that **Balatro.exe** is located in the default *Steam* directory (`C:\Program Files (x86)\Steam\steamapps\common\Balatro\Balatro.exe`). If it is not, simply copy your **Balatro.exe** to the same folder as **balatro-apk-maker.bat**
 - This script will automatically download and install [7-Zip](https://www.7-zip.org/)
 - This script will automatically download and install [Java](https://www.java.com/en/download/)
 - This script will automatically download [APK Tool](https://apktool.org/)
 - This script will automatically download [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/)
 - This script will automatically download [love-11.5-android-embed.apk](https://github.com/love2d/love-android/)
 - This script will automatically download [Balatro-APK-Patch](http://smudge.codes/files/Balatro-APK-Patch.zip)
