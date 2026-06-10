using System.ComponentModel;
using Spectre.Console.Cli;

namespace Phoebe.CLI;

public class SeedfindingCommandSettings : CommandSettings {
    [CommandArgument(0, "<config>")]
    [Description("Path to .toml config file")]
    public required string ConfigPath { get; set; }

    [CommandOption("-o|--output")]
    [Description("Output .json file location")]
    [DefaultValue("result.json")]
    public string OutputPath { get; set; } = null!;

    [CommandOption("-v|--version")]
    [Description("Override the latest with a different game version, or provide a custom .json file")]
    [DefaultValue("v81")]
    public string GameVersion { get; set; } = null!;
}