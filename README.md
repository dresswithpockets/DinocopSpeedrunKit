# Dinocop Speedrun Kit

This kit includes an Accessibility plugin and an Autosplitter plugin for Dinocop.

## Install

In order to use the kit, you must have a Bleeding Edge version of BepInEx:

1. Download & install [BepInEx 6.x](https://docs.bepinex.dev/master/articles/user_guide/installation/index.html)
    - You'll want to download a "[Bleeding edge build](https://builds.bepinex.dev/projects/bepinex_be)" of BepInEx. This
      kit has been tested on build #753, so use that one if youre not sure.
1. Run the game once to ensure BepInEx generates configs. Close it immediately.
1. Download [the plugin](https://github.com/dresswithpockets/DinocopSpeedrunKit/releases) and place it in `path/to/game/BepInEx/plugins`
1. Run the game once more to ensure the kit's configs are generated. Close it immediately.
1. If you're using the autosplitter, [configure your splits](#configuring)

## DinocopSpeedrunKit.Accessibility

Adds some accessibility options to the main menu settings:

- Automatically skip the intro cutscene on a new save
- Automatically skip dialogue
  - does not skip choices
- Automatically pick up items by aiming at them
  - only picks up collectibles like mold, dirt, cans - only when the player would normally be able to pick them up. 
    
## DinocopSpeedrunKit.Autosplit

An autosplitter integration for LiveSplit.

### LiveSplit Setup

The autosplitter requires LiveSplit to start a server on startup.

1. With LiveSplit open, right click anywhere in the main window
1. Select "Settings"
1. At the bottom of the Settings window, find the "LiveSplit" Server section
1. Select "Start TCP Server" in the "Startup Behaviour" dropdown
   - You can change the Server Port if you know that you need to. If you do, you must change the autosplitters configuration to match the new port. 
1. Click "OK", then restart LiveSplit.

### Configuring

Open `path/to/game/BepInEx/config/DinocopSpeedrunKit.Autosplit.cfg` in Notepad or another text editor.

In this file you'll find a `[Splits]` section, underneath which you'll find some configurable fields:

```ini
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

#### `ResetOnScene`

The autosplitter will send a `reset` to LiveSplit if the current scene matches any of the scenes in the `ResetOnScene` 
list. By default, this value is `01_Title_level`, which is the internal name for the title screen's Unity scene.

For example, if you are in the middle of a run and want to reset, you can just exit to main menu. Once the game 
transitions to the main menu, the autosplitter will send a `reset` to LiveSplit.

#### `Splits`

`Splits` is configured as a space-separated list. Each Split in the list looks like `(Kind Value)` where:

- `Kind` is one of:
  - `Event` or `E`
  - `Scent` or `S`
  - `Collectible` or `C`
  - `Level` or `L`
  - `Dialogue` or `D`
- `Value` is a string that the autosplitter will match against based on the `Kind` chosen.

Heres an example configuration you might use for an any% route:

```ini
Splits = (Event Event_IntroCinematic) (Dialogue Rec_intro) (Collectible Plunger) (Collectible LongueVue) (Event resetGame)
```

Given that config, the autosplitter will perform the following splits *in order*<sup>1</sup>:
1. on any Event named `Event_IntroCinematic`
   - N.B. this is the game's internal name for the event that triggers after you start a new game.
1. on any Dialogue named `Rec_intro`
   - N.B. this is the game's internal name for the cutscene that plays when you first enter the hotel
1. on any Collectible named `Plunger`
1. on any Collectible named `LongueVue`
   - N.B. this is the game's internal name for the Binoculars
1. on any Event named `resetGame`
   - N.B. this is the game's internal name for the event that triggers after the end scene fades to black.
   
<sup>1. at the moment each split must be completed in the order that they are configured.</sup>

### Known Strings

These are strings that have been datamined from the game, and are valid as split config values.

#### Collectibles

<details>
<summary>Collectibles</summary>

| Internal Name          | Description |
|------------------------|--------------|
| AnimeFigureA           |              |
| AnimeFigureB           |              |
| AnimeFigureC           |              |
| BagOfWires             |              |
| BeerCap                |              |
| BeerMug                |              |
| Bobette_A              |              |
| Bolt                   |              |
| Botch                  |              |
| BrontoMeat             |              |
| BrontoPosterPiece      |              |
| BrothersCloth          |              |
| BubbleGum              |              |
| BubbleWrap             |              |
| Bug                    |              |
| Cadre_A                |              |
| Cadre_B                |              |
| Cadre_C                |              |
| CameraPhoto            |              |
| CarKey                 |              |
| CarnivalTicket         |              |
| CarnivalTicket_super   |              |
| Carotte                |              |
| CatFur                 |              |
| CD_rom                 |              |
| CeraLetter             |              |
| Chandail_A             |              |
| Chandail_B             |              |
| Chandail_C             |              |
| Chandail_D             |              |
| chaudronCover          |              |
| Cheese                 |              |
| CheeseCrust            |              |
| Cheetos                |              |
| Chicken                |              |
| CigYoshi               |              |
| ClawPatrolPlush        |              |
| CleanTowel             |              |
| Clou                   |              |
| Coconut                |              |
| Cocotte                |              |
| CoffeeCupSpecial       |              |
| CoffeeGear             |              |
| CoffeeMug              |              |
| Coin                   |              |
| ComputerPart           |              |
| Concombre              |              |
| CrushedSoda            |              |
| Danger                 |              |
| DesertMineral          |              |
| Dirt                   |              |
| DirtyMoney             |              |
| DirtyTowel             |              |
| Documentaire           |              |
| Donut                  |              |
| DSRT_scentE            |              |
| Dumbell_big            |              |
| Dumbell_small          |              |
| FakeCheese             |              |
| Flashlight             |              |
| FluffPile              |              |
| Folliage               |              |
| FoodWaste              |              |
| ForestCreepA           |              |
| FridgeKey_0            |              |
| Garbage                |              |
| Gem_blue               |              |
| Gem_gold               |              |
| Gem_green              |              |
| Gem_red                |              |
| GhostCheese            |              |
| Glass                  |              |
| Glue                   |              |
| GoldenBall             |              |
| Honey                  |              |
| hotelPan_cover         |              |
| Jam                    |              |
| Junk                   |              |
| Key_Survivaliste       |              |
| kitchenPermit          |              |
| Laitue                 |              |
| LichenA                |              |
| LifeFruit              |              |
| LobbyCandy             |              |
| LongueVue              |              |
| louche                 |              |
| Loupe                  |              |
| MagicMushroom          |              |
| Mammoth                |              |
| Meat                   |              |
| MeatBag                |              |
| Metal                  |              |
| MG_GemBlue             |              |
| MG_GemGreen            |              |
| MG_GemRed              |              |
| Mold                   |              |
| MoleRatKid             |              |
| MoneyBill              |              |
| MoonFlower             |              |
| MushroomB              |              |
| MushroomCapA           |              |
| MysteryScent           |              |
| NenuphareFlower        |              |
| NewsPaper              |              |
| Nigesaurus             |              |
| NutsNBolt              |              |
| Ocarinut_pieces        |              |
| Oil                    |              |
| PaperBall              |              |
| ParaCrumbs             |              |
| Parfum                 |              |
| Pebble                 |              |
| PickleChip             |              |
| PickleChip_Bag         |              |
| Pizza                  |              |
| Plunger                |              |
| PoultryColis           |              |
| Rat                    |              |
| RedPotion              |              |
| RoomKey100             |              |
| RoomKey101             |              |
| RoomKey103             |              |
| RoomKey104             |              |
| RoomKey105             |              |
| RoomKey107             |              |
| RoomKey200             |              |
| RoomKey201             |              |
| RoomKey202             |              |
| RoomKey203             |              |
| RoomKey204             |              |
| RoomKey205             |              |
| RoomKey206             |              |
| RoomKey207             |              |
| RoomKey208             |              |
| RoomKey209             |              |
| RoomKey_basement       |              |
| RoomKey_Janitor        |              |
| RootA                  |              |
| RubberChicken          |              |
| RuchePiece             |              |
| SecretDetergent        |              |
| SeedA                  |              |
| Shit                   |              |
| ShitGold               |              |
| Skill_Dash             |              |
| Skill_Nest             |              |
| Skill_RedSpeed         |              |
| SmallStep              |              |
| Sock_A                 |              |
| Soda                   |              |
| SpecialCoin            |              |
| SpiderWeb              |              |
| SteveAutograph         |              |
| SteveLetter            |              |
| Stone                  |              |
| Straw                  |              |
| SunFlower              |              |
| Tomate                 |              |
| TreeFruit              |              |
| Water                  |              |
| WifiAdapter            |              |
| XP50                   |              |
| Yum                    |              |

</details>

#### Events

| Internal Name        | Description                                                                     |
|----------------------|---------------------------------------------------------------------------------|
| Event_IntroCinematic | Triggers after the game fades from black, after you start a new save            | 
| resetGame            | Triggers when the ending scene fades to black, before the title scene is loaded |

#### Dialogue

| Internal Name | Description                                                                |
|---------------|----------------------------------------------------------------------------|
| Rec_intro     | The cutscene that plays when you first enter the hotel lobby on a new save |

#### Levels / Unity Scenes

| Internal Name               | Description                                                        |
|-----------------------------|--------------------------------------------------------------------|
| 00_Logos                    | Startup splash screens that display before you see the main menu.  |
| 01_Title_level              | The main menu                                                      |
| ALTA_level                  | Alternate ending A cutscene (you arrest the wrong person)          |
| ALTA_ro_01                  |                                                                    |
| ALTB_level                  | Alternate ending B cutscene (you let Mark go)                      |
| ALTB_ro_01                  |                                                                    |
| DiaTester_level             |                                                                    |
| DiaTester_ro_01             |                                                                    |
| END_level                   | The true ending cutscene                                           |
| END_ro_01                   |                                                                    |
| ESCAPE_level                | True ending level where you escape the hotel                       |
| ESCAPE_main                 |                                                                    |
| HOF_level                   |                                                                    |
| HOF_main                    |                                                                    |
| HOTEL_basementBathroom      |                                                                    |
| HOTEL_basement              |                                                                    |
| HOTEL_biblio                |                                                                    |
| HOTEL_convention            |                                                                    |
| HOTEL_coursInterieur        |                                                                    |
| HOTEL_DiningRoom            |                                                                    |
| HOTEL_floor2_L_transition_2 |                                                                    |
| HOTEL_floor2_L_transition   |                                                                    |
| HOTEL_floor2_L              |                                                                    |
| HOTEL_floor2_R_transition   |                                                                    |
| HOTEL_floor2_R              |                                                                    |
| HOTEL_fridge                |                                                                    |
| HOTEL_garden                |                                                                    |
| HOTEL_janitorRoom           |                                                                    |
| HOTEL_laundryRoom           |                                                                    |
| HOTEL_level                 |                                                                    |
| HOTEL_main                  |                                                                    |
| HOTEL_Reception_B           |                                                                    |
| HOTEL_Reception             |                                                                    |
| HOTEL_room101_weirdRoom     |                                                                    |
| HOTEL_room102_serialKiller  |                                                                    |
| HOTEL_room103               |                                                                    |
| HOTEL_room105_bert          |                                                                    |
| HOTEL_room107               |                                                                    |
| HOTEL_room200               |                                                                    |
| HOTEL_room201               |                                                                    |
| HOTEL_room202               |                                                                    |
| HOTEL_room204_paco          |                                                                    |
| HOTEL_room205_butcher       |                                                                    |
| HOTEL_room209_Jan           |                                                                    |
| HOTEL_room222               |                                                                    |
| HOTEL_roomAPIBLOP           | The APIBLOP room                                                   |
| HOTEL_roomPlayer            | The player's room. Probably always loaded once the hotel is loaded |
| HOTEL_Side                  |                                                                    |
| Intro                       |                                                                    |
| LAB_level                   | The secret lab                                                     |
| LAB_main                    |                                                                    |
| NodeLog_level               |                                                                    |
| NodeLog_ro_01               |                                                                    |
| OPTI_level                  |                                                                    |
| OPTI_ro_01                  |                                                                    |
| POSTE_level                 |                                                                    |
| POSTE_ro_01                 |                                                                    |
| POSTE_ro_02                 |                                                                    |
| SPEECH_level                | Jan's speech cutscene                                              |
| SPEECH_ro_01                |                                                                    |
| STREET_level                | New game cutscene                                                  |
| STREET_ro_01                |                                                                    |
| STUDIO_level                |                                                                    |
| STUDIO_main                 |                                                                    |

#### Scents

| Internal Name     | Notes                             |
|-------------------|-----------------------------------|
| Mammoth           |                                   |
| Mold              |                                   |
| CrushedSoda       |                                   |
| Dirt              |                                   |
| Nigesaurus        |                                   |
| LobbyCandy        |                                   |
| Donut             |                                   |
| BrontoPosterPiece | The torn pages in the gardens     |
| BrontoMeat        |                                   |
| Junk              |                                   |
| FoodWaste         |                                   |
| Pizza             |                                   |
| DirtyMoney        |                                   |
| Cheetos           | Chese puffs                       |
| Tomate            | Tomatos                           |
| Laitue            | Lettuce                           |
| Concombre         | Cucumber                          |
| Carotte           | Carrot                            |
| Chicken           |                                   |
| Parfum            | Cologne/perfume                   |
| MagicMushroom     | Mushrooms that make you act weird |
| SecretDetergent   | Cleaning spray                    |
| PickleChip        |                                   |
| Glue              |                                   |
| RuchePiece        | Honey                             |
| Danger            | Danger                            |

