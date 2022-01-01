# ACT Auto End Combat

This is a Plugin for Dalamud (XivLauncher), which echos "end" whenever you leave combat.

ACT can not exactly detect when you leave combat in the game. To get around this, ACT ends the current combat when no combat actions were taken for a certain (configurable) amount of time.
This has two major drawback: 
First, if there are fights with cutscenes (or transitions) it will end the combat during that cutscene, unless you set the ACT reset timer to a value longer than the duration of the cutscene. 
Second, if you wipe and pull again before the ACT reset timer is up (very likely if you set it to a high value) it won't split the fight correctly.

These two issues will cause inaccurate encounter data inside of ACT and overlays. (Note that some further tools (mainly fflogs) will split the encounter correctly regardless of how ACT split the encounters)

There is a feature where you can manually end the ACT encounter from the game by echoing "end" in the chat ("/e end"), and normally you would have to do this manually every time.
This plugin automates that functionality whenever you leave combat. 

You can customize it to be only active in certain zones or zone types.

## Commands

```
/actautoend             Toggles the configuration window
/actautoend help        Shows a list of valid subcommands
/actautoend check       Check whether the plugin is active in your current zone
/actautoend activate    Activates the plugin for you current zone. Note that this reverts to your automatic settings once you change zones
/actautoend deactivate  Deactivates the plugin for you current zone. Note that this reverts to your automatic settings once you change zones
```

## Acknowledgements

* goat and the [goat place discord](https://discord.gg/3NMcUV5) (For making XivLauncher/Dalamud and providing support)
* [MidoriKami](https://github.com/MidoriKami/NoTankYou) (author of NoTankYou from which I stole the code structure)

