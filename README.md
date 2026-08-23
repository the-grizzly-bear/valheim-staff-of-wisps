# Staff of Wisps

A Valheim mod (BepInEx/Jotunn) that adds the Staff of Wisps: a Mistlands-tier
staff that throws a bound wisp instead of a fireball. Deals spirit damage
instead of fire, arcs like a thrown projectile, clears mist as it flies, and
on impact spawns a lingering glowing wisp that keeps clearing mist around
itself for a while - so you can lob it ahead to light up the path through
the Mistlands fog.

## Requirements

- [BepInEx](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) for Valheim
- [Jotunn](https://github.com/Valheim-Modding/Jotunn) v2.29.2 or later, installed as its own BepInEx plugin

## Install

1. Install BepInEx and Jotunn as normal Valheim plugins.
2. Drop `StaffOfWisps.dll` into `BepInEx/plugins/StaffOfWisps/`.
3. Needs to be installed on both client and server (or all players in a
   non-dedicated game). This registers brand-new prefabs (item, projectile,
   and the landed-wisp piece it spawns on impact) that don't exist in
   vanilla, so a client without the mod has no way to resolve their hash
   when a ZDO references them - different from mods that just reroll/modify
   existing spawns, where ZDO ownership means a single unmodded client can
   degrade more gracefully. Whichever player's client throws the staff is
   also the one whose version determines that particular throw's behavior,
   so keep both client and server updated together, not just one side.

## Recipe

Crafted at a Galdr table: Yggdrasil wood x20, Wisp x4, Refined eitr x16.
Upgradeable to quality 4 like other staves; higher quality makes the landed
wisp linger longer (25s / 35s / 45s / 60s for quality 1-4).

## Build from source

Requires the .NET 8 SDK.

1. Download `Jotunn.dll` from the [Jotunn releases page](https://github.com/Valheim-Modding/Jotunn/releases)
   into `libs/Jotunn.dll`.
2. If Valheim isn't installed at the default Steam location, pass
   `-p:ValheimPath=/path/to/Valheim` to the build.
3. `dotnet build -c Release`

The build copies the compiled DLL straight into your local
`BepInEx/plugins/StaffOfWisps/` folder.
