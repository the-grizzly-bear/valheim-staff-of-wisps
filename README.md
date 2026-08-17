# Staff of Wisps

A Valheim mod (BepInEx/Jotunn) that adds the Staff of Wisps: a Mistlands-tier
staff that throws a bound wisp instead of a fireball. Deals spirit damage
instead of fire, arcs like a thrown projectile, and the wisp lingers and
glows where it lands - clears mist around it as it flies and after landing,
so you can lob it ahead to light up the path through the Mistlands fog.

## Requirements

- [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) for Valheim
- [Jotunn](https://github.com/Valheim-Modding/Jotunn) v2.29.2 or later, installed as its own BepInEx plugin

## Install

1. Install BepInEx and Jotunn as normal Valheim plugins.
2. Drop `StaffOfWisps.dll` into `BepInEx/plugins/StaffOfWisps/`.
3. Needs to be installed on both client and server (or all players in a
   non-dedicated game) since it registers a new item/prefab.

## Recipe

Crafted at a Black Forge: Yggdrasil wood x20, Wisp x4, Refined eitr x16.

## Build from source

Requires the .NET 8 SDK.

1. Download `Jotunn.dll` from the [Jotunn releases page](https://github.com/Valheim-Modding/Jotunn/releases)
   into `libs/Jotunn.dll`.
2. If Valheim isn't installed at the default Steam location, pass
   `-p:ValheimPath=/path/to/Valheim` to the build.
3. `dotnet build -c Release`

The build copies the compiled DLL straight into your local
`BepInEx/plugins/StaffOfWisps/` folder.
