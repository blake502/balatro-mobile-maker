# Balatro Mobile Maker

The goal of this project is to allow *Balatro* fans to play *Balatro* on their mobile devices. This project provides a **NON-PIRACY** avenue for players to do so, by converting their *Steam* copy of *Balatro* to a mobile app. Balatro Mobile Maker also supports automatically transferring your saves back and forth between your *Steam* copy of the game and your mobile device (save transfer features only officially available on Windows and Android, for now).

Mods are not _officially_ supported, [but they will probably work if you transfer your saves from a modded system](https://github.com/blake502/balatro-mobile-maker/issues/11).

Keep in mind that Balatro Mobile Maker is still in beta! Please report any bugs you encouter in the [issues section](https://github.com/blake502/balatro-mobile-maker/issues). If you encounter bugs with the latest release, try the previous release.

## Quick Start Guide
Please review the **Notes** section before you begin.
 - Download or compile [**balatro-mobile-maker**](https://github.com/blake502/balatro-mobile-maker/releases).
 - Run **balatro-mobile-maker**.
 - Follow the prompts to apply optional patches. If you're unsure, always select "Y".
 ### For Android:
 - Copy the resulting **balatro.apk** to your Android device, or allow the program to automatically install using [USB Debugging](https://developer.android.com/studio/debug/dev-options).
 - Optionally, allow the program to automatically transfer your saves from your *Steam* copy of *Balatro* using [USB Debugging](https://developer.android.com/studio/debug/dev-options).
 ### For iOS:
 - Sideload **balatro.ipa** using [AltStore](https://altstore.io/)
 - Optionally, [copy your saves to your iOS device](https://github.com/blake502/balatro-mobile-maker/issues/64#issuecomment-2094660508).

 ## Optional Patches
- **FPS Cap** — Caps FPS to a desired number (Or to the device's native refresh rate-- recommended for battery performance)
- **Landscape Orientation** — Locks the game to landscape orientation (Recommended, since portrait orientation does not behave very well)
- **High DPI** — Enables [High DPI graphics mode in Love](https://love2d.org/wiki/love.window.setMode) (Recommended for iOS)
- **CRT Shader Disable** — Disables the CRT Shader (Recommended for Pixel and some other devices)

## Notes
 - This script assumes that **Balatro.exe** or **Game.love** is located in the default *Steam* directory. If it is not, simply copy your **Balatro.exe** or **Game.love** to the same folder as **balatro-mobile-maker**
 - This script will automatically download [7-Zip](https://www.7-zip.org/)
 ### For Android:
 - This script will automatically download [OpenJDK](https://www.microsoft.com/openjdk)
 - This script will automatically download [APK Tool](https://apktool.org/)
 - This script will automatically download [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/)
 - This script will automatically download [love-11.5-android-embed.apk](https://github.com/love2d/love-android/)
 - This script will automatically download [Balatro-APK-Patch](https://github.com/blake502/balatro-mobile-maker/releases/tag/Additional-Tools-1.0)
 - This script can automatically download [Android Developer Bridge](https://developer.android.com/tools/adb) (optional)
 ### For iOS:
 - This script will automatically download [Balatro-IPA-Base](https://github.com/blake502/balatro-mobile-maker/releases/tag/Additional-Tools-1.0)

 ## Recogition (in no particular order)
 - [Every contributor](https://github.com/blake502/balatro-mobile-maker/graphs/contributors)
 - Developers of [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer)
 - Developers of [LÖVE](https://love2d.org/)
 - Developers of [7-Zip](https://www.7-zip.org/)
 - Developers of [APKTool](https://apktool.org/)
 - Delevopers of [Balatro](https://www.playbalatro.com/)

 ## License
 - [7-Zip](https://github.com/ip7z/7zip/blob/main/DOC/License.txt) is licensed under the GNU LGPL license.
 - This project uses [APKTool](https://github.com/iBotPeaches/Apktool/blob/master/LICENSE.md)
 - This project uses [uber-apk-signer](https://github.com/patrickfav/uber-apk-signer/blob/main/LICENSE)
 - This project uses [LÖVE](https://github.com/love2d/love/blob/main/license.txt)
 - This project uses [OpenJDK](https://www.microsoft.com/openjdk)
