using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_SummonPitGate : PsychicRitualDef_InvocationCircle
{
	private FloatRange combatPointMultiplierFromQualityRange;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
		list.Add(new PsychicRitualToil_SummonPitGate(InvokerRole, combatPointMultiplierFromQualityRange));
		return list;
	}

	public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
	{
		foreach (string item in base.BlockingIssues(assignments, map))
		{
			yield return item;
		}
		if (map.listerThings.ThingsOfDef(ThingDefOf.PitGate).Count > 0 || map.listerThings.ThingsOfDef(ThingDefOf.PitGateSpawner).Count > 0)
		{
			yield return "PitGateAlreadyExists".Translate();
		}
	}
}
