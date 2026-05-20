using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JoyGiver_TakeDrug : JoyGiver_Ingest
{
	private static readonly List<ThingDef> takeableDrugs = new List<ThingDef>();

	protected override Thing BestIngestItem(Pawn pawn, Predicate<Thing> extraValidator)
	{
		if (pawn.drugs == null)
		{
			return null;
		}
		ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
		for (int i = 0; i < innerContainer.Count; i++)
		{
			if (Validator(innerContainer[i]))
			{
				return innerContainer[i];
			}
		}
		bool flag = pawn.story != null && (pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) > 0 || pawn.InMentalState);
		takeableDrugs.Clear();
		DrugPolicy currentPolicy = pawn.drugs.CurrentPolicy;
		for (int j = 0; j < currentPolicy.Count; j++)
		{
			if (flag || currentPolicy[j].allowedForJoy)
			{
				takeableDrugs.Add(currentPolicy[j].drug);
			}
		}
		takeableDrugs.Shuffle();
		for (int k = 0; k < takeableDrugs.Count; k++)
		{
			List<Thing> list = pawn.Map.listerThings.ThingsOfDef(takeableDrugs[k]);
			if (list.Count > 0)
			{
				Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, list, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, Validator);
				if (thing != null)
				{
					return thing;
				}
			}
		}
		return null;
		bool Validator(Thing t)
		{
			if (!CanIngestForJoy(pawn, t))
			{
				return false;
			}
			if (extraValidator != null && !extraValidator(t))
			{
				return false;
			}
			if (t.def.ingestible == null || t.def.ingestible.drugCategory == DrugCategory.None)
			{
				return false;
			}
			return true;
		}
	}

	public override float GetChance(Pawn pawn)
	{
		int num = 0;
		if (pawn.story != null)
		{
			num = pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire);
		}
		if (num < 0)
		{
			return 0f;
		}
		float num2 = def.baseChance;
		if (num == 1)
		{
			num2 *= 2f;
		}
		if (num == 2)
		{
			num2 *= 5f;
		}
		return num2;
	}

	protected override Job CreateIngestJob(Thing ingestible, Pawn pawn)
	{
		return DrugAIUtility.IngestAndTakeToInventoryJob(ingestible, pawn);
	}
}
