# Valheim Speedchoice

This is a BepInEx mod for Valheim for that adds a number of new World Modifiers that speed up gameplay. Inspired by [Crystal Speedchoice](https://github.com/dabomstew/pokecrystal-speedchoice) and [TrophyHuntMod](https://github.com/smariotti/Valheim/tree/master/TrophyHuntMod).

Speedchoice modifiers are saved to a json file named worldName\_speedchoice.json. This file is human readable, and can be editted directly if the World Modifiers UI isn't available.

## Features

### Presets

- <b>`Speedchoice`</b> - Recommended Speedchoice settings. Faster gameplay, no grinding required, and Boss Reveals make the whole game completable in one sitting.
- <b>`Trailblazer`</b> - OatHorse's Trailblazer settings, without the trophy hunt aspect.
- <b>`Blazing`</b> - Accelerated version of a Reverse Boss playthrough. Defeat the bosses in order, and get rewarded in Skill Levels. However, the bosses have no drops. Thus no Forsaken Powers, or Moder's Tears for late game crafting.

### Sliders

- <b>`Boat Speed`</b> - Changes the speed at which ships sail.
- <b>`Time to Rest`</b> - Adjusts how long it takes to become rested.
- <b>`Trophy Odds`</b> - Adds an extra reroll for trophy drops to all enemies.

### Toggles

- <b>`Boss Reveals`</b> - Upon defeating a Forsaken, the next Forsaken's locations are revealed.
- <b>`Boss Skills`</b> - Upon defeating a Forsaken, all skills will be leveled up.
- <b>`Lootless Bosses`</b> - Forsaken do not drop items, including both their Trophies, and their "progressive" materials.
- <b>`Cheat Death`</b> - If one logs out shortly after dying, they'll log in where they died rather than their spawn point.
- <b>`Drop Materials`</b> - Materials, items used for crafting exclusively, are immediately removed from the inventory.
- <b>`Harmless Pieces`</b> - Player built structures, such as campfires or spikes, do no damage to creatures.
- <b>`Fast Crops`</b> - Crops grow quickly.
- <b>`Fast Fermenters`</b> - Fermenters brew quickly.
- <b>`Increased Exp`</b> - Increases Skill experience gains by 500%.
- <b>`Instant Upgrades`</b> - Instantly upgrades any upgradeable tool or armor.
- <b>`Structure Loot`</b> - Naturally occurring structures, such as houses, will drop into materials when deconstructed with a hammer in "No Build Cost".
- <b>`Where's my Portal?`</b> - Adds a portal pin to the map upon placing a portal.
- <b>`No Build Stations`</b> - Player built pieces can be constructed without the required nearby crafting stations. For instance a Portal can be built without a nearby Workbench.
- <b>`No Craft Cost`</b> - Items can be crafted without consuming or requiring the materials to do so.
- <b>`No Craft Levels`</b> - Items can be crafted without the prerequisite Crafting Station Level to do so.
- <b>`Show Deaths`</b> - Adds an UI element showing the number of Deaths.
- <b>`Show Logouts`</b> - Adds an UI element showing the number of Logouts.
- <b>`Show Timer`</b> - Adds an UI element showing how long there has been activity in the world. Starts on first input, and pauses when the game pauses.

### Secret Settings

These settings are not included in the UI. One can utilize them by changing the generated world\_speedchoice.json file. These are not recommended, but might be useful if looking for something specific like "Valheim without the ability to run or jump".

- <b>`alwaysRocky`</b> - Replaces every Stone with Stonerock, aka Rocky.
- <b>`runSpeed`</b> - Multiplies the player's run speed.
- <b>`jumpForce`</b> - Multiplies the player's jump force. Note due to how physics works, two times jump force is four times jump height. Specifically, Height = JumpForce^2 / 2g.
- <b>`sailForce`</b> - Multiplies the speed at which boats sail, stacks multiplicity with <b>`Boat Speed`</b>.
- <b>`restOverride`</b> - Overrides <b>`Time to Rest`</b>, causing the player to become rested after the given number of seconds.
- <b>`trophyOverride`</b> - Overrides <b>`Trophy Odds`</b>, adding an extra reroll for trophy drops equal to the given percent.
- <b>`unlockPieces`</b> - All build Pieces are immediately unlocked without acquiring the Materials.
- <b>`unlockRecipes`</b> - All crafting Recipes are immediately unlocked without acquiring the materials or Crafting Station levels.
