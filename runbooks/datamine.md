Use AssetRipper to export the project.

## Scenes

Scenes are located in `ExportedProject/Assets/Scenes`

To get a list of all unity scenes:
```bash
find . -type f -name "*.unity" -printf '%f\n'
```

## Collectibles

ScriptedAssets are located in `ExportedProject/Assets/MonoBehaviour/`

To get a list of assets which are Collectible instances, we need to filter for something that only a Collectible has. For example, we can filter by the value of `Object.m_Script` based on a known Collectible's value:

```bash
cd ExportedProject/Assets/MonoBehaviour/
grep -l "m_Script: {fileID: 11500000, guid: 78be0469487174fd55003b01a7ef778b, type: 3}" *
```

That gets us:
```
./RoomKey101.asset
./Skill_Dash.asset
./SeedA.asset
./Carotte.asset
./Dumbell_big.asset
./MG_GemRed.asset
./CoffeeCupSpecial.asset
... etc
```

We can use [yq](https://github.com/kislyuk/yq) to query the asset files for the names of the items:

```bash
grep -rl "m_Script: {fileID: 11500000, guid: 78be0469487174fd55003b01a7ef778b, type: 3}" . | while read line
do
    name=$(yq .MonoBehaviour.m_Name "$line")
    echo "${line}: ${name}"
done
```

That gets us an output like:
```yaml
./AnimeFigureA.asset: "AnimeFigureA"
./AnimeFigureB.asset: "AnimeFigureB"
./AnimeFigureC.asset: "AnimeFigureC"
./BagOfWires.asset: "BagOfWires"
./BeerCap.asset: "BeerCap"
./BeerMug.asset: "BeerMug"
./Bobette_A.asset: "Bobette_A"
... etc
```

So, each asset's name always matches the filename. We can simplify our original collectibles query to just:

```bash
grep -l "m_Script: {fileID: 11500000, guid: 78be0469487174fd55003b01a7ef778b, type: 3}" * | while read line; do name=${line%.asset}; echo $name; done
```

which gives us the file stem of every matching asset file:

```
AnimeFigureA
AnimeFigureB
AnimeFigureC
BagOfWires
BeerCap
BeerMug
Bobette_A
Bolt
Botch
BrontoMeat
BrontoPosterPiece
BrothersCloth
BubbleGum
BubbleWrap
Bug
Cadre_A
Cadre_B
Cadre_C
CameraPhoto
CarKey
CarnivalTicket
CarnivalTicket_super
Carotte
CatFur
CD_rom
CeraLetter
Chandail_A
Chandail_B
Chandail_C
Chandail_D
chaudronCover
Cheese
CheeseCrust
Cheetos
Chicken
CigYoshi
ClawPatrolPlush
CleanTowel
Clou
Coconut
Cocotte
CoffeeCupSpecial
CoffeeGear
CoffeeMug
Coin
ComputerPart
Concombre
CrushedSoda
Danger
DesertMineral
Dirt
DirtyMoney
DirtyTowel
Documentaire
Donut
DSRT_scentE
Dumbell_big
Dumbell_small
FakeCheese
Flashlight
FluffPile
Folliage
FoodWaste
ForestCreepA
FridgeKey_0
Garbage
Gem_blue
Gem_gold
Gem_green
Gem_red
GhostCheese
Glass
Glue
GoldenBall
Honey
hotelPan_cover
Jam
Junk
Key_Survivaliste
kitchenPermit
Laitue
LichenA
LifeFruit
LobbyCandy
LongueVue
louche
Loupe
MagicMushroom
Mammoth
Meat
MeatBag
Metal
MG_GemBlue
MG_GemGreen
MG_GemRed
Mold
MoleRatKid
MoneyBill
MoonFlower
MushroomB
MushroomCapA
MysteryScent
NenuphareFlower
NewsPaper
Nigesaurus
NutsNBolt
Ocarinut_pieces
Oil
PaperBall
ParaCrumbs
Parfum
Pebble
PickleChip
PickleChip_Bag
Pizza
Plunger
PoultryColis
Rat
RedPotion
RoomKey100
RoomKey101
RoomKey103
RoomKey104
RoomKey105
RoomKey107
RoomKey200
RoomKey201
RoomKey202
RoomKey203
RoomKey204
RoomKey205
RoomKey206
RoomKey207
RoomKey208
RoomKey209
RoomKey_basement
RoomKey_Janitor
RootA
RubberChicken
RuchePiece
SecretDetergent
SeedA
Shit
ShitGold
Skill_Dash
Skill_Nest
Skill_RedSpeed
SmallStep
Sock_A
Soda
SpecialCoin
SpiderWeb
SteveAutograph
SteveLetter
Stone
Straw
SunFlower
Tomate
TreeFruit
Water
WifiAdapter
XP50
Yum
```

Most of these assets map pretty obviously to their actual in-game display name. I haven't figured out a reliable way to get the actual name for each asset yet.
