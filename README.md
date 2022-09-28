![logo](resource/logo3.png)

This is the project for **Your very own ringtone!**, available at [VS Marketplace](https://marketplace.visualstudio.com/items?itemName=isainstars.yourveryownringtone). This enables you to add sounds to all sorts of Visual Studio events.

### How do I build it?
Open `YourVeryOwnRingtone.sln` in Visual Studio and click "build". That's it! You can try it by clicking "F5", which will spawn a new Visual Studio instance with the extension installed.

### How do I install it?
I recommend installing it through the [VS Marketplace](https://marketplace.visualstudio.com/items?itemName=isainstars.yourveryownringtone) page.

### How do I use it?
1. Create a json configuration file.
Check [settings.json](src/YourVeryOwnRingtone/themes/lofi/settings.json) for an example. 
2. Open Visual Studio and go to _Tools > Options > Your very own ringtone!_.
3. Add the file with its complete path to "Configuration file".

✨ **Features**
- Play different sounds for the same command \
These will play randomly once the command is emitted.
- .wav and .mp3 formats \
Check [NAudio](https://github.com/naudio/NAudio) for a complete list of supported formats.
- Full path or relative path \
We support either full path or a relative path from the configuration file directory.

✨ **Supported commands**
>- **apply** code changes 🔥
>- **build.start** build started 
>- **build.onsuccess** build finished successfully ✅ 
>- **build.onfail** build finished with errors 💀 
>- **breakpoint** is hit 🔴 
>- **continue** (while debugging) 
>- **exception** is hit 
>- **find** ctrl+F 🔎 
>- **restart** your application 
>- **save** file 💾 
>- **start** debugging 
>- **step** (any step) 
>- **stepover** step over 
>- **stepinto** step into 
>- **stepout** step out 🏃‍♀️ 
>- **stop** debugging 
>- **undo**

![cat](resource/cat.png)
