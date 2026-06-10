
namespace Phoebe.CLI;

public class DungeonFlowNameFilter(string FlowName) : IDungeonFilter {
	public bool Evaluate(PhoebeDungeonInfo dungeon) {
		return dungeon.FlowName == FlowName;
	}
}