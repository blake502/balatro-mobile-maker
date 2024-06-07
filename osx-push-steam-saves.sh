# Standalone shell script for OSX
# Pushes steam saves to android device

adb shell rm -r /data/local/tmp/balatro
adb shell mkdir /data/local/tmp/balatro
adb shell mkdir /data/local/tmp/balatro/files
adb shell mkdir /data/local/tmp/balatro/files/save
adb shell mkdir /data/local/tmp/balatro/files/save/game

adb push ~/Library/Application\ Support/Balatro /data/local/tmp/balatro/files/save/game
adb shell am force-stop com.unofficial.balatro
adb shell run-as com.unofficial.balatro cp -r /data/local/tmp/balatro/files .
adb shell rm -r /data/local/tmp/balatro