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
}