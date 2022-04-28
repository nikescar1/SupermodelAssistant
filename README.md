# Supermodel's Assistant
A Frontend for the Supermodel Sega Model 3 Emulator (Made in Unity)

# Unity Project
Current Unity version of this project is 2021.3.1f1

The project is currently in a rough state and hasn't been touched in two years. It should work but your mileage may vary.

# Release Installation
1. Unzip
2. Place Model3 roms in "SupermodelsAssistant\Supermodel\Roms" folder.

# Release Usage
Run "Supermodel's Assistant.exe".

Settings can be accessed in the upper left corner from the game selection screen.

Control mapping configuration is saved in the "SupermodelsAssistant\Supermodel\Config\Supermodel.ini" file.

Game confguration batch files are saved in the "SupermodelsAssistant\GameConfigurationsBatch" folder.

These can be used with other frontends. They must be placed in the same folder as the "Supermodel.exe".

If you want to install a different version of Supermodel to use wit this frontend, delete the contents of "SupermodelsAssistant\Supermodel" folder and extract the contents of the Supermodel zip to "SupermodelsAssistant\Supermodel" folder. Don't forget to backup the "SupermodelsAssistant\Supermodel\Config\Supermodel.ini" file if you want to save your input configurations.

Debug error messages can be accessed via the Debug Console activated by pressing the tilde/backquote key on the keyboard.

The current version number is displayed in the upper right corner of the options menu.

Switch to CoverFlow mode in the options menu. This is a controller only mode. The thinking here is you setup all your games in the standard mode and then switch to CoverFlow for a more arcadey experience.

Auto-patching roms will look for missing files from the rom zips in other roms. If it finds those files it will move them over and try to relaunch the game.

# Attribution
Game images sourced from:
- https://gamesdb.launchbox-app.com/
- https://flyers.arcade-museum.com/

Third Party Packages:
- Archiver https://github.com/LightBuzz/Archiver-Unity
- UnityCoverFlow https://github.com/IainS1986/UnityCoverFlow
- In-game Debug Console https://github.com/yasirkula/UnityIngameDebugConsole
- JsonDoNet https://github.com/JamesNK/Newtonsoft.Json
- NaughtyAttributes https://github.com/dbrizov/NaughtyAttributes
- Uween https://github.com/beinteractive/Uween
- SharpDX https://github.com/sharpdx/SharpDX
