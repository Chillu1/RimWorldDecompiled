using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public class Tale_SinglePawn : Tale
{
	public TaleData_Pawn pawnData;

	public override Pawn DominantPawn => pawnData.pawn;

	public override string ShortSummary => string.Concat(def.LabelCap + ": ", pawnData.name?.ToString());

	public Tale_SinglePawn()
	{
	}

	public Tale_SinglePawn(Pawn pawn)
	{
		pawnData = TaleData_Pawn.GenerateFrom(pawn);
		if (pawn.SpawnedOrAnyParentSpawned)
		{
			surroundings = TaleData_Surroundings.GenerateFrom(pawn.PositionHeld, pawn.MapHeld);
		}
	}

	public override bool Concerns(Thing th)
	{
		if (!base.Concerns(th))
		{
			return pawnData.pawn == th;
		}
		return true;
	}

	public override void Notify_FactionRemoved(Faction faction)
	{
		if (pawnData.faction == faction)
		{
			pawnData.faction = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref pawnData, "pawnData");
	}

	protected override IEnumerable<Rule> SpecialTextGenerationRules(Dictionary<string, string> outConstants)
	{
		foreach (Rule rule in pawnData.GetRules("ANYPAWN", outConstants))
		{
			yield return rule;
		}
		foreach (Rule rule2 in pawnData.GetRules("PAWN", outConstants))
		{
			yield return rule2;
		}
	}

	public override void GenerateTestData()
	{
		base.GenerateTestData();
		pawnData = TaleData_Pawn.GenerateRandom();
	}
}
