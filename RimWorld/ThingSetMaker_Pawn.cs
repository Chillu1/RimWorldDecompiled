using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ThingSetMaker_Pawn : ThingSetMaker
{
	public PawnKindDef pawnKind;

	public bool? alive;

	public FloatRange? corpseAgeRangeDays;

	protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
	{
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind));
		outThings.Add(pawn);
		if (!alive.HasValue || alive.Value)
		{
			return;
		}
		pawn.Kill(null, null);
		if (corpseAgeRangeDays.HasValue)
		{
			int num = Mathf.RoundToInt(corpseAgeRangeDays.Value.RandomInRange * 60000f);
			pawn.Corpse.timeOfDeath = Find.TickManager.TicksGame - num;
			CompRottable compRottable = pawn.Corpse.TryGetComp<CompRottable>();
			if (compRottable != null)
			{
				compRottable.RotProgress += num;
			}
		}
	}

	protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
	{
		yield return pawnKind.race;
	}

	public override IEnumerable<string> ConfigErrors()
	{
		if (pawnKind == null)
		{
			yield return "pawnKind is null.";
		}
	}
}
