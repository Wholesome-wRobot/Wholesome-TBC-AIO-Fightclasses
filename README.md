## Project Wholesome - Burning Crusade AIO-FC – 9 FightClasses in one file (1-70)

### Overview

Hi guys. After weeks of development and testing, I’m proud to present to you my All-In-One TBC FightClasses. 9 classes are included in one single file: 

-	Z.E. Beast Master Hunter
-	Z.E. Retribution Paladin
-	Z.E. Frost Mage 
-	Z.E. Enhancement Shaman
-	Z.E. Fury Warrior
-	Z.E. Feral Druid
-	Z.E. Affliction Warlock
-	Z.E. Shadow Priest
-	Z.E. Combat Rogue

This file only works with the English client of the game. Those fight classes are meant for leveling/grinding purpose (1-70). I do not recommend you use them for dungeons/raids or PvP. They are designed around smooth grinding, prioritizing combat uptime over high DPS. Please read their description carefully before using them.
This AIO is free to use and share, although I have set up a Paypal donation link in case you are happy with my work and want to encourage me via a donation.

[Paypal link](https://www.paypal.com/paypalme2/zerowrobot?locale.x=fr_FR)
 

### Project Wholesome – A quick word
 
The goal of Project Wholesome is to develop, test, and freely share wRobot products. All the code created under the project is transparent, open-source, and then released in the store for free. Nothing released under Project Wholesome is sold for profit, although individual donations are welcome. We are always looking for more testers and developers. If you want to participate, please join our Discord channel.
Project Wholesome Discord: https://discord.gg/NEq4VA6

### How to install

Copy the .dll file into your FightClass folder, and then select it in the General Settings tab of wRobot. The AIO will automatically detect your class and launch the appropriate Fight Class.

### Automatic Talent Assignation

This AIO FightClass can automatically assign talents.
The option is deactivated by default. It must be manually activated in the settings.
Each class comes with a default recommended talent build. If you use the recommended build,
Make sure to reset your talents in the game beforehand. Talent checks will be done at the start of
the Fight Class, and then every 5 minutes.

You also have the option to input your own talent build. In order to do so, please
use http://calculators.iradei.eu/talents/ and then copy the code that comes after the URL once
your build is done. 
Talents will be learned in order from left to 
right. Therefore, several talent codes can be set in the settings for sequential learning.

--------------------

### Z.E. Beast Master Hunter

#### Features:
-	Keeps your pet fed and happy
-	Will fight in melee when out of arrows
-	Uses a mana saving rotation for maximum combat uptime
-	Backs up from melee if the pet takes back aggro
-	Will switch between different aspects depending on the situation
-	Calls/Revives/Mends pet
-	Feigns death in the most dire situation
-	Uses freezing trap on multi aggro (WIP)

#### Options:
-	Set weapon speed for Steady Shot use (WIP)
-	Choose whether to backup from melee or not
-	Choose to use freezing trap on multi aggro
-	Choose whether you want Z.E.BMHunter to manage your pet feeding, in case you prefer using a third party plugin
-	Choose to either use high CD spells on multi aggro or as soon as available

#### Recommendations:
The hunter is a weak class before level 10. Once you get your pet, it becomes unstoppable. Complete your pet quest as soon as you hit level 10. Get yourself a large Quiver/Ammo pouch. Matenia’s HumanMasterPlugin can help you automate buying ammunitions. Make sure you always have food for your pet. Be aware that the pet will not attack if it is in the process of eating.

#### Recommended settings:
Tab: Food / Drink

- Food: 30% to 99%
- Drink: 15% to 99%
- Use drink: ON

Tab: Vendor (Selling or Buying)
- Food Amount : 40
- Drink Amount : 40

General Settings:
- Attack before being attacked : ON (will help restarting fight against trap-frozen enemy)

--------------------

### Z.E. Retribution Paladin

#### Features:
-	Uses a different rotation depending on your mana situation
-	Can keep you healed up between fights to lower food consumption
-	Purifies/Cleanses poisons, diseases and magic debuffs
-	Uses Lay on Hands in the most dire situation
-	Switches between appropriate seals
-	Uses Rank 1 Seal of Command on low mana

#### Options:
-	You can set under which mana percentage threshold the class enters a mana saving rotation (50 recommended)
-	Choose whether to use Hammer of Wrath
-	Choose whether to use Exorcism against Undead and Demon
-	Choose whether to heal up between fights. This is designed to save on food consumption
-	Choose to use Blessing of Wisdom over Blessing of Might
-	Choose to use Seal of Command over Seal of Righteousness

#### Recommendations:
The Retribution paladin is a safe class for leveling, but it’s also not the fastest. He is largely gear dependant. Getting yourself a good 2H weapon should be your number one priority.

#### Recommended settings:
Tab: Food / Drink
- Food: 5% to 50% (if Flash Heal between fights is ON)
- Food: 45% to 99% (if Flash Heal between fights is OFF)
- Drink: 35% to 99%
- Use drink : ON

Tab: Vendor (Selling or Buying)
- Food Amount : 20
- Drink Amount : 60

--------------------

### Z.E. Frost Mage

#### Features:
-	Backs up from melee when the target is frozen
-	Creates best available food and drinks (will always make sure you have at least 10 of each)
-	Uses wand under a set enemy HP threshold
-	Uses mana stones
-	Uses evocation
-	Removes curses
-	Uses a set of high CD spells on multi aggro
-	Uses Ice Lance once available
-   Blinks when backing up from target

#### Options:
-	You can set the HP percentage threshold under which the class will use a wand
-	Choose whether to use Cone of Cold in your rotation
-	Choose whether to use Icy Veins on multi aggro
-   Choose whether to use BLink when backing up

#### Recommendations:
Get yourself a wand as soon as possible for best performances. The Frost Mage is amongst the slowest classes for leveling. Not because it lacks DPS, but because it will be drinking every 2-3 fights. On the bright side, Food and Drinks are free. Your gear will strongly affect your performance. A larger mana pool will greatly increase its sustenance when grinding. Make sure you automate selling your items so you don’t end up lacking room to conjure your food.

#### Recommended settings:
Tab: Food / Drink
- Food: 65% to 99%
- Drink: 50% to 99%
- Use drink: ON
- Search in the bag best food/drink item available: ON

Tab: Vendor (Selling or Buying)
- Food Amount : 0
- Drink Amount : 0

--------------------

### Z.E. Enhancement Shaman

#### Features:
-	Smart totems management and recall
-	Uses Ghost Wolf
-	Smart pulls
-	Out of combat healing
-	Interrupts casters using Earth Shock
-	Cures poisons and diseases
-	Keeps your weapons enchants up

#### Options:
-	Choose whether to use Ghost Wolf
-	Choose whether to use Totemic Call
-	Choose whether Magma Totem should be used on multi aggro
-	Choose whether to use Flame Shock in your rotation
-	Choose to use Stoneskin Totem instead of Strength of Earth Totem 
-	Choose whether to use Lightning shield
-	Choose to prioritize Water Shield over Lightning Shield
-	Option to pull using rank 1 Lightning Bolt in order to save mana
-	Option to interrupt using rank 1 Earth Shock in order to save mana
-	Choose to only use Shamanistic Rage on multi aggro
-	Activate/Deactivate totems by element

#### Recommendations:
The Enhancement Shaman is a very strong leveling class, especially once you get the WindFury enchant for your weapons. Do not hesitate to make use of all the totems, they greatly increase your performance and are smartly managed by the Fight Class. It is recommended to use 2x 1 hand weapons. Take some time to carefully craft your rotation using the settings. For example, using Lightning Shield will greatly increase your DPS, but will also largely increase your mana consumption.

#### Recommended settings:
Tab: Food / Drink
- Food: 35% to 99%
- Drink: 35% to 99%
- Use drink: ON
- Search in the bag best food/drink item available: ON

Tab: Vendor (Selling or Buying)
- Food Amount : 60
- Drink Amount : 60

--------------------

### Z.E. Fury Warrior

#### Features:
-	Smart pulls. Will only charge in when no enemies are around the target. Otherwise, uses a range weapon to pull the enemy in
-	Keeps shouts up
-	Uses the appropriate stance depending on the situation
-	Interrupts casters using Pummel (Prioritizes Berserker stance against casters)
-	Makes use of all available skills in the right situation
-	Smart use of AoE skills. For example, will not use Demoralizing Shout if a neutral enemy is too close, in order to avoid aggroing it

#### Options:
-	Choose to prioritize Berserker stance over Battle Stance
-	Choose to always range pull
-	Choose to use Hamstring against Humanoids in order to keep them from fleeing
-	Choose whether to use Bloodrage
-	Choose whether to use Demoralizing Shout
-	Choose to use Commanding Shout instead of Battle Shout
-	Choose whether to use Rend
-	Choose whether to use Cleave on multi aggro

#### Recommendations:
The Fury Warrior can be a strong leveling class with the appropriate gear. Good weapons, Stamina and Strength should be your priority. 2x 1Hand are recommended. Get yourself a ranged weapon as soon as possible. A Thrown weapon is recommended over a bow or a gun, so you don’t need to buy ammunition.

#### Recommended settings:
Tab: Food / Drink
- Food: 45% to 99%
- Use drink: OFF

Tab: Vendor (Selling or Buying)
- Food Amount : 60

--------------------

### Z.E. Feral Druid

#### Features:
-	Moves behind the target when prowling or pouncing
-	Out of combat healing
-	Removes curses and poisons
-	Keeps buffs up
-	Uses all appropriate feral forms depending on the situation (Bear/Cat/Travel)
-	Smart pulls. Will only approach when no enemies are around the target. Otherwise, uses a range spell to pull the enemy in
-	Uses a different rotation depending on the feral form used
-	Calculates which healing spell rotation to use during combat in order to make sure you have enough mana to get back to your animal form afterwards

#### Options:
-	Choose to always range pull
-	Choose whether to use Travel Form. Might not be recommended. Can cause unwanted shapeshifts, therefore wasting mana
-	Choose whether to use Innervate
-	Choose whether to use Barkskin
-	Choose whether to use Enrage (Bear)
-	Choose whether to use Swipe on multi aggro (Bear)
-	Choose whether to use Tiger’s Fury in the rotation (Cat)
-	Choose whether to use Prowl to sneak up behind the target (Cat) – Can be buggy

#### Recommendations:
The Druid is a difficult class up until level 10. Once you get your Bear form, things go much smoother and quicker. You are not strongly gear dependant. Stamina and Agility should be your priority. Make sure you improve your stealth and your cat speed in your talent tree. Your healing spells will keep you healed up between fights, so you will mostly need drinks.

#### Recommended settings:
Tab: Food / Drink
- Food: 25% to 99%
- Drink: 35% to 99%
- Use drink: ON

Tab: Vendor (Selling or Buying)
- Food Amount : 20
- Drink Amount : 60

--------------------

### Z.E. Affliction Warlock

#### Features:
-	Takes smart control of your pet
-	Make sure you always have your pet by your sides. Will force regen if you can’t summon due to lack of mana
-	Uses a combination of Life Tap, Health Funnel, Dark Pact and Drain Life to keep you grinding as long as possible
-	Keeps your buffs up
-	Uses Health Stone
-	Uses Soul Stone (not reviving yet, WIP)
-	Uses wand
-	Manages multiple enemies and fear on multi aggro
-	Makes sure you always have at least a set amount of Soul Shards in your bags at all time
-	Reacts to Shadow Trance

#### Options:
-	Choose whether to use Life Tap (Highly recommended)
-	Choose whether to use Soul Shatter on multi aggro
-	Choose whether to use Incinerate (High mana consumption)
-	Choose whether to fear additional enemies
-	Choose whether to prioritize using a wand over Shadow bolt (Highly recommended in order to save mana)
-	Choose whether to let Z.E Affliction Warlock control the Torment spell of your Voidwalker or leave it on autocast
-	Choose whether to keep using Immolate once Unstable Affliction is learnt
-	Choose whether to use Siphon Life (only recommended after decent green TBC gear is acquired)
-	Choose to put the pet on passive when out of combat (can be useful if you want to ignore fights when traveling)
-	Set the number of soul shards you want in your bags at all times (default: 4)
-	Choose whether to use Unending Breath
-	Choose whether to use Dark Pact
-	Choose whether to use Fel Armor over Demon Armor
-	Choose whether to use the Soul Stone

#### Recommendations:
The Warlock is an excellent grinder with formidable survivability. Stamina and Intellect are both paramount stats. Getting yourself a wand as soon as possible should also be your priority. How you set your Food/Drink threshold defines how risky the class will play. If you find the bot putting itself in dangerous situations too often, raise the Food/Drink threshold. Make sure to take some time to craft your rotation using the options depending on your level, gear and spot difficulty.

#### Recommended settings:
Tab: Food / Drink
- Food: 35% to 99%
- Drink: 35% to 99%
- Use drink: ON

Tab: Vendor (Selling or Buying)
- Food Amount : 60
- Drink Amount : 60

It is highly recommended to adjust those values to your specific needs.

--------------------

### Z.E. Shadow Priest

#### Features:
-	Uses wand
-	Cures diseases, dispels magic
-	Out of combat healing
-	Keeps your buffs up
-	Silences casters

#### Options:
-	Set the wand threshold (Enemy HP percentage under which the wand should be used)
-	Choose whether to use Inner Fire
-	Choose to use Power Word: Shield on pull (useful since Mind Flay will only be used if your shield is up)
-	Choose whether to use Shadowguard
-	Choose whether to use Shadow Word: Death
-	Choose whether to use  Shadow Protection 

#### Recommendations:
The Shadow Priest is a powerful leveler and grinder with very few downsides. He is not too gear dependant, although getting a wand as soon as possible should be a priority. It is recommended to take some time to adjust your rotation using the settings in order to improve its performance.

#### Recommended settings:
Tab: Food / Drink
- Food: 35% to 99%
- Drink: 35% to 99%
- Use drink: ON

Tab: Vendor (Selling or Buying)
- Food Amount : 20
- Drink Amount : 60

--------------------

### Z.E. Combat Rogue

#### Features:
-	Uses Stealth
-   Uses appropriate opening
- 	Smart pulls. Will only approach when no enemies are around the target. Otherwise, uses a range weapon to pull the enemy in 
-   Interrupts caster
-   Uses and refreshes weapon poisons
-   Uses High CDs on multi aggro
-   Ripostes against humanoids
-   Will detect the type of weapon you have in your main hand and choose skills accordingly
-   The default talent build for this class specializes in Swords

#### Options:
-	Option to always range pull
-   Choose whether to approach in Stealth
-   Choose whether to use Garrote as an opener
-   Choose to still enter Stealth even if poisoned
-   Option to use Sprint as soon as available
-   Option to use Blind + Bandages during combat (Garrote should be off, and weapons poisons should all be instant)

#### Recommendations:
The combat Rogue, despite is high DPS, is amongst the hardest classes to level. He is very gear dependant. You should always try to get him good weapons and equipment. Using bandages and poisons appropriately will help you in the long run. The weapon you'll specialize in is up to your personal preference.

#### Recommended settings:
Tab: Food / Drink
- Food: 50% to 99%

Tab: Vendor (Selling or Buying)
- Food Amount : 60

--------------------

### Special thanks
Special thanks to the entire Wholesome team for their help and feedback, in particular Mars, Bigmac, Talamin, Kamogli and g2bazman. 

### Feedback
Feedback and bug reports are very much appreciated. Please feel free to join our discord channel: https://discord.gg/NEq4VA6

Enjoy!


