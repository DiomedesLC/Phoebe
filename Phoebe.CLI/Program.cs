using Phoebe.CLI;
using Spectre.Console.Cli;

CommandApp<SeedfindingCommand> app = new CommandApp<SeedfindingCommand>();
app.Configure(c => {
    c.PropagateExceptions();
});
app.Run(args);