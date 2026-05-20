using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AddShipJob_Arrive : QuestNode_AddShipJob
{
	public SlateRef<Map> map;

	public SlateRef<IntVec3?> landingCell;

	public SlateRef<List<Pawn>> forPawns;

	protected override ShipJobDef DefaultShipJobDef => ShipJobDefOf.Arrive;

	protected override void AddJobVars(ShipJob shipJob, Slate slate)
	{
		if (shipJob is ShipJob_Arrive shipJob_Arrive)
		{
			if (forPawns.TryGetValue(slate, out var value) && !value.NullOrEmpty())
			{
				shipJob_Arrive.mapOfPawn = value.First();
			}
			else
			{
				Map map = this.map.GetValue(slate) ?? slate.Get<Map>("map");
				shipJob_Arrive.mapParent = map.Parent;
			}
			IntVec3? intVec = landingCell.GetValue(slate) ?? slate.Get<IntVec3?>("landingCell");
			if (intVec.HasValue)
			{
				shipJob_Arrive.cell = intVec.Value;
			}
		}
	}
}
