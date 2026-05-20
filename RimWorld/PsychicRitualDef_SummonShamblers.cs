using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_SummonShamblers : PsychicRitualDef_InvocationCircle
{
	private FloatRange combatPointsFromQualityRange;

	private bool shamblersAllowed;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
		list.Add(new PsychicRitualToil_SummonShamblers(InvokerRole, combatPointsFromQualityRange));
		return list;
	}

	public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
	{
		foreach (string item in base.BlockingIssues(assignments, map))
		{
			yield return item;
		}
		if (!shamblersAllowed)
		{
			yield return "NoNearbyShamblers".Translate();
		}
	}

	public override void InitializeCast(Map map)
	{
		base.InitializeCast(map);
		int num = 0;
		bool flag = false;
		while (!flag && num < 5)
		{
			RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Hostile);
			flag = RCellFinder.TryFindTravelDestFrom(result, map, out var _);
			num++;
		}
		shamblersAllowed = map.Biome != BiomeDefOf.Undercave && flag;
	}
}
