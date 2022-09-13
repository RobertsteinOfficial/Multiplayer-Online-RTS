# Multiplayer-Online-RTS
Prototype of a multiplayer online RTS made as exercise

CONTROLS
To spawn buildings simply drag them from the UI into the level. They must stay close to another allied building to be placed succesfully.
To spawn units from the buildings, click on the building of choice.


Steamworks releases: https://github.com/rlabrecque/Steamworks.NET/releases
FizzySteamworks releases:  https://github.com/Chykary/FizzySteamworks/releases

To Make the build work via Steam:
1. check "Use Steam" on the "Main Menu" script in the MainMenuDisplay gameObject in the editor
2. disable the "Kcp Transport" script on the NetworkManager in the editor
3. enable "Steam Manager" and "Fizzy Steamworks" scripts on the NetworkManager in the editor
4. copy the the file  “steam_appid.txt” into the build folder
5. open Steam, go to Games->Add a Non-Steam Game to My Library and select the .exe file from the build folder
