using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class PsychicRitualDef_SummonAnimals : PsychicRitualDef_InvocationCircle
{
	public SimpleCurve manhunterSpawnChanceFromQualityCurve;

	private bool animalsAllowed;

	public override List<PsychicRitualToil> CreateToils(PsychicRitual psychicRitual, PsychicRitualGraph graph)
	{
		List<PsychicRitualToil> list = base.CreateToils(psychicRitual, graph);
		list.Add(new PsychicRitualToil_SummonAnimals(InvokerRole));
		return list;
	}

	public override TaggedString OutcomeDescription(FloatRange qualityRange, string qualityNumber, PsychicRitualRoleAssignments assignments)
	{
		return outcomeDescription.Formatted(manhunterSpawnChanceFromQualityCurve.Evaluate(qualityRange.min).ToStringPercent());
	}

	public override IEnumerable<string> BlockingIssues(PsychicRitualRoleAssignments assignments, Map map)
	{
		foreach (string item in base.BlockingIssues(assignments, map))
		{
			yield return item;
		}
		if (!animalsAllowed)
		{
			yield return "NotEnoughAnimals".Translate();
		}
	}

	public override void InitializeCast(Map map)
	{
		base.InitializeCast(map);
		animalsAllowed = map.Biome.AllWildAnimals.EnumerableCount() > 0 && map.Biome.wildAnimalsCanWanderInto && RCellFinder.TryFindRandomPawnEntryCell(out var _, map, CellFinder.EdgeRoadChance_Animal);
	}
}
