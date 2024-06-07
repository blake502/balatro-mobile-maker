# Standalone shell script for OSX
# Pulls saves from android device to local disk
rm -r ./BalatroTMP
mkdir ./BalatroTMP/
mkdir ./BalatroTMP/files/
touch ./BalatroTMP/files/settings.jkr
adb shell run-as com.unofficial.balatro cat files/save/game/settings.jkr > ./BalatroTMP/files/settings.jkr

for i in {1..3}; do
  mkdir ./BalatroTMP/files/$i/
  # touch ./BalatroTMP/files/$i/profile.jkr
  adb shell run-as com.unofficial.balatro cat files/save/game/$i/profile.jkr > ./BalatroTMP/files/$i/profile.jkr
  # touch ./BalatroTMP/files/$i/meta.jkr
  adb shell run-as com.unofficial.balatro cat files/save/game/$i/meta.jkr > ./BalatroTMP/files/$i/meta.jkr
  # touch ./BalatroTMP/files/$i/save.jkr
  adb shell run-as com.unofficial.balatro cat files/save/game/$i/save.jkr > ./BalatroTMP/files/$i/save.jkr
done

find ./BalatroTMP/files/ -maxdepth 2 -size 0c | xargs rm
find ./BalatroTMP/files/ -type d -empty -delete
cp -r ./BalatroTMP/files/. ~/Library/Application\ Support/Balatro
rm -r ./BalatroTMP