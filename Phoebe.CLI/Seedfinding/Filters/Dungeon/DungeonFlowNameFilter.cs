
namespace Phoebe.CLI;

public class DungeonFlowNameFilter(HashSet<string> FlowNames) : IDungeonFilter {
	public bool Evaluate(PhoebeDungeonInfo dungeon) {
		return FlowNames.Contains(dungeon.FlowName);
	}
}