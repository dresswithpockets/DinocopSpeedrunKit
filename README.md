# Dinocop Speedrun Kit

1. Download & install [BepInEx 6.x](https://docs.bepinex.dev/master/articles/user_guide/installation/index.html)
   1. You'll want to download a "Bleeding edge build" of BepInEx. This kit has been tested on build #753.
1. Run the game once to ensure BepInEx generates configs. Close it immediately.
1. Download [the plugin]() and place it in `path/to/game/BepInEx/plugins`
1. Run the game once more to ensure the kit's configs are generated. Close it immediately.
1. [Configure the plugin](#configuring)

## Configuring

Open `path/to/game/BepInEx/config/io.dresswithpockets.dinocop.autosplit.cfg` in Notepad or another text editor.

In this file you'll find a `[Splits]` section, underneath which you'll find some configurable fields:

```toml
[Splits]

## Semicolon-separated list of scene to reset the timer on. This will send a timer reset to LiveSplit whenever ANY of the listed scene are (re)loaded. Some scene are loaded simultaneously, and may remain loaded for the runtime of the game.
# Setting type: String
# Default value: 01_Title_level
ResetOnScene = 01_Title_level

## Configure which events will trigger LiveSplit splits
# Setting type: SplitConfig
# Default value: 
Splits = 
```

### `ResetOnScene`

The autosplitter will send a `reset` to LiveSplit if the current scene matches any of the scenes in the `ResetOnScene` 
list. By default, this value is `01_Title_level`, which is the internal name for the title screen's Unity scene.

For example, if you are in the middle of a run and want to reset, you can just exit to main menu. Once the game 
transitions to the main menu, the autosplitter will send a `reset` to LiveSplit.

### `Splits`

`Splits` is configured as a space-separated list. Each Split in the list looks like `(Kind Value)` where:

- `Kind` is one of:
  - `Event` or `E`
  - `Scent` or `S`
  - `Collectible` or `C`
  - `Level` or `L`
  - `Dialogue` or `D`
- `Value` is a string that the autosplitter will match against based on the `Kind` chosen.

For example, given the following configuration:

```toml
Splits = (Event Event_IntroCinematic) (Dialogue Rec_intro) (Collectible Plunger) (Collectible LongueVue)
```

The autosplitter will perform the following splits *in order*<sup>1</sup>:
1. on any Event named `Event_IntroCinematic`
1. on any Dialogue named `Rec_intro`
1. on any Collectible named `Plunger`
1. on any Collectible named `LongueVue`
   - N.B. this is the game's internal name for the Binoculars
   
<sup>1. at the moment each split must be completed in the order that they are configured.</sup>