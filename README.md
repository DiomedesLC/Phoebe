# Phoebe
Phoebe is mainly a standalone C# library that simulates Lethal Compay seeds.

`Phoebe.Core` is the core library that contains the simulation code and some utilties for seedfinding.

`Phoebe.CLI` is a command line tool to find seeds

`Phoebe.Standardization` fixes issues with randomness in the vanilla game, alongside making it more consistent to simulate
 - Fixes [Random numbers get re-initalized](https://canary.discord.com/channels/750645598293590077/1505728846018642081)
 - Fixes [Problematic seeds with incorrect item spawns](https://canary.discord.com/channels/750645598293590077/1498990527507464294)
 - Uncouples dungeon generation from scrap value

`Phoebe.ContentExporter` is a debug tool used for development

## Using the CLI
Windows builds are available from the [latest release](https://github.com/DiomedesLC/Phoebe/releases/latest). The CLI requires a `.toml` config file ([Examples](https://github.com/DiomedesLC/Phoebe/Phoebe.CLI/examples)).

Important to note is when you start the CLI it will log one of the following:
```
Needed Step: Interior (1)
Needed Step: ScrapSetup (2)
Needed Step: ScrapCalc (3)
```
In general, the higher the number the more complicated the filters are and will need more time. The tool will filter out seeds as soon as it possibly can (even on higher steps) to improve performance.

### Full list of current options
```toml
moons = ["220 Assurance"] # Required, case sensitive and (currently) needs the planet number
start_seed = 1
end_seed = 100_000_000
phoebe_fix = false

# Below are "filters" for a seed to be considered valid they must pass all these filters
# To exclude a filter, do not include the value
[scrap]
items = ["Zed Dog"] # Names are case sensitive.
single_item_day = true
min_scrap = 11
max_scrap = 13
# the min/max total value will give warnings without phoebe fix
min_total_value = 400
max_total_value = 200

[interior]
flows = ["Level3Flow"]
```