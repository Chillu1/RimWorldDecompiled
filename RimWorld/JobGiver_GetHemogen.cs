using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_GetHemogen : ThinkNode_JobGiver
{
	private static float? cachedHemogenPackHemogenGain;

	public static float HemogenPackHemogenGain
	{
		get
		{
			if (!cachedHemogenPackHemogenGain.HasValue)
			{
				if (!ModsConfig.BiotechActive)
				{
					cachedHemogenPackHemogenGain = 0f;
				}
				else if (!(ThingDefOf.HemogenPack.ingestible?.outcomeDoers?.FirstOrDefault((IngestionOutcomeDoer x) => x is IngestionOutcomeDoer_OffsetHemogen) is IngestionOutcomeDoer_OffsetHemogen ingestionOutcomeDoer_OffsetHemogen))
				{
					cachedHemogenPackHemogenGain = 0f;
				}
				else
				{
					cachedHemogenPackHemogenGain = ingestionOutcomeDoer_OffsetHemogen.offset;
				}
			}
			return cachedHemogenPackHemogenGain.Value;
		}
	}

	public static void ResetStaticData()
	{
		cachedHemogenPackHemogenGain = null;
	}

	public override float GetPriority(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return 0f;
		}
		if (pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>() == null)
		{
			return 0f;
		}
		return 9.1f;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return null;
		}
		Gene_Hemogen gene_Hemogen = pawn.genes?.GetFirstGeneOfType<Gene_Hemogen>();
		if (gene_Hemogen == null)
		{
			return null;
		}
		if (!gene_Hemogen.ShouldConsumeHemogenNow())
		{
			return null;
		}
		if (pawn.IsBloodfeeder())
		{
			Pawn prisoner = GetPrisoner(pawn);
			if (prisoner != null)
			{
				return JobMaker.MakeJob(JobDefOf.PrisonerBloodfeed, prisoner);
			}
		}
		if (gene_Hemogen.hemogenPacksAllowed)
		{
			int num = Mathf.FloorToInt((gene_Hemogen.Max - gene_Hemogen.Value) / HemogenPackHemogenGain);
			if (num > 0)
			{
				Thing hemogenPack = GetHemogenPack(pawn);
				if (hemogenPack != null)
				{
					Job job = JobMaker.MakeJob(JobDefOf.Ingest, hemogenPack);
					job.count = Mathf.Min(hemogenPack.stackCount, num);
					job.ingestTotalCount = true;
					return job;
				}
			}
		}
		return null;
	}

	private Thing GetHemogenPack(Pawn pawn)
	{
		Thing carriedThing = pawn.carryTracker.CarriedThing;
		if (carriedThing != null && carriedThing.def == ThingDefOf.HemogenPack)
		{
			return carriedThing;
		}
		for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
		{
			if (pawn.inventory.innerContainer[i].def == ThingDefOf.HemogenPack)
			{
				return pawn.inventory.innerContainer[i];
			}
		}
		return GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.listerThings.ThingsOfDef(ThingDefOf.HemogenPack), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => pawn.CanReserve(t) && !t.IsForbidden(pawn));
	}

	public static AcceptanceReport CanFeedOnPrisoner(Pawn bloodfeeder, Pawn prisoner)
	{
		if (prisoner.WouldDieFromAdditionalBloodLoss(0.4499f))
		{
			return "CannotFeedOnWouldKill".Translate(prisoner.Named("PAWN"));
		}
		if (!prisoner.IsPrisonerOfColony || !prisoner.guest.PrisonerIsSecure || prisoner.guest.IsInteractionDisabled(PrisonerInteractionModeDefOf.Bloodfeed) || prisoner.IsForbidden(bloodfeeder) || !bloodfeeder.CanReserveAndReach(prisoner, PathEndMode.OnCell, bloodfeeder.NormalMaxDanger()) || prisoner.InAggroMentalState)
		{
			return false;
		}
		return true;
	}

	private Pawn GetPrisoner(Pawn pawn)
	{
		return (Pawn)GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, pawn.Map.mapPawns.PrisonersOfColonySpawned, PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing t) => t is Pawn prisoner && CanFeedOnPrisoner(pawn, prisoner).Accepted);
	}
}
