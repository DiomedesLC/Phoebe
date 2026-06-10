namespace Phoebe.CLI;

public interface IDungeonFilter {
    bool Evaluate(PhoebeDungeonInfo dungeon);
}