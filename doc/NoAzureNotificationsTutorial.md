# Tutorial using mobile app without Azure notifications
Tutorial is assumes that you have a Raspberry Pi, run all needed services on that and use Android mobile app. You have two RuuviTag sensors, which data you want to get, and you already know their MAC addresses. 

### Create Google Firebase realtime database
Read [Create Firebase database](CreateFirebaseDatabase.md) to get information on how to create Firebase database, where data is stored.

### Set up Raspberry Pi
Read [Setting up Raspberry Pi](SettingUpRaspberryPi.md) to get detailed information about needed Raspberry Pi setup.

### Install the Android application APK
Download and install the latest APK from [Releases](https://github.com/ilpork/vahti/releases/latest/). Alternatively you can install the development environment (like described in [Full tutorial](FullTutorial.md)) and compile it yourself.

In application go to Options page and update the database URL (and database secret if applicable).
