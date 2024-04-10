# Balatro APK Maker

This goal of this script is to allow *Balatro* fans to play *Balatro* on their mobile devices before the official release. This script provides a **NON-PIRACY** avenue for players to do so, by converting their *Steam* version of *Balatro* to an APK format.

## Quick Start Guide
Please review the **Notes** section before you begin.
 - Download or compile [**balatro-apk-maker.exe**](https://github.com/blake502/balatro-apk-maker/releases).
 - Run **balatro-apk-maker.exe**.
 - Follow the prompts to apply optional patches. If you're unsure, always select "Y".
 - Copy the resulting **balatro.apk** to your Android device.

 ## Optional Patches
- **FPS Cap**
Caps FPS to a desired number (Or to the device's native refresh rate-- recommended for battery performance)
- **Landscape Orientation**
Locks the game to landscape orientation (Recommended, since portrait orientation does not behave very well)
- **CRT Shader Disable**
Disables the CRT Shader (Recommended for Pixel and some other devices)
- **Accessible Saves**
Changes the save path from `data/data/com.unofficial.balatro/files` to `sdcard/Android/data/com.unofficial.balatro/files`. Accessing this location with the lastest versions of Android is still tricky, but it should make it easier for root users. (Recommended for most cases)

## Notes
 - This script assumes that **Balatro.exe** is located in the default *Steam* directory (`C:\Program Files (x86)\Steam\steamapps\common\Balatro\Balatro.exe`). If it is not, simply copy your **Balatro.exe** to the same folder as **balatro-apk-maker.exe**
 - This script can automatically download and install [Java](https://www.java.com/en/download/)
 - This script will automatically download [7-Zip](https://www.7-zip.org/)
 - This script will automatically download [APK Tool](https://apktool.org/)
 - This script will automatically download [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/)
 - This script will automatically download [love-11.5-android-embed.apk](https://github.com/love2d/love-android/)
 - This script will automatically download [Balatro-APK-Patch](http://smudge.codes/files/Balatro-APK-Patch.zip)

 ## License
 [7-Zip](https://github.com/ip7z/7zip/blob/main/DOC/License.txt) is licensed under the GNU LGPL license.
 
 This project uses [APKTool](https://github.com/iBotPeaches/Apktool/blob/master/LICENSE.md)
 
 This project uses [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/blob/main/LICENSE)
 
 This project uses [LOVE](https://github.com/love2d/love/blob/main/license.txt)
