using Verse.AI.Group;

namespace RimWorld
{
	public class LordJob_ManTurrets : LordJob
	{
		public override StateGraph CreateGraph()
		{
			return new StateGraph
			{
				StartingToil = new LordToil_ManClosestTurrets()
			};
		}
	}
}
