using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_HaulResourcesToCarrier : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckBiotech("Haul resources to carrier"))
		{
			return false;
		}
		Pawn pawn2 = (Pawn)t;
		if (!pawn2.IsColonyMech || !pawn2.Spawned || pawn2.Downed)
		{
			return false;
		}
		CompMechCarrier comp = pawn2.GetComp<CompMechCarrier>();
		if (comp == null)
		{
			return false;
		}
		int amountToAutofill = comp.AmountToAutofill;
		if (amountToAutofill <= 0)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		return !HaulAIUtility.FindFixedIngredientCount(pawn, comp.Props.fixedIngredient, amountToAutofill).NullOrEmpty();
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		CompMechCarrier compMechCarrier = t.TryGetComp<CompMechCarrier>();
		if (compMechCarrier == null)
		{
			return null;
		}
		int amountToAutofill = compMechCarrier.AmountToAutofill;
		if (amountToAutofill <= 0)
		{
			return null;
		}
		List<Thing> list = HaulAIUtility.FindFixedIngredientCount(pawn, compMechCarrier.Props.fixedIngredient, amountToAutofill);
		if (!list.NullOrEmpty())
		{
			Job job = HaulAIUtility.HaulToContainerJob(pawn, list[0], t);
			job.count = Mathf.Min(job.count, amountToAutofill);
			job.targetQueueB = (from i in list.Skip(1)
				select new LocalTargetInfo(i)).ToList();
			return job;
		}
		return null;
	}
}
