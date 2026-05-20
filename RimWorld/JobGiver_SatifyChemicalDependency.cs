using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_SatifyChemicalDependency : ThinkNode_JobGiver
{
	private static readonly List<Hediff_ChemicalDependency> tmpChemicalDependencies = new List<Hediff_ChemicalDependency>();

	public override float GetPriority(Pawn pawn)
	{
		if (!ModsConfig.BiotechActive)
		{
			return 0f;
		}
		if (pawn.health.hediffSet.hediffs.Any(ShouldSatify))
		{
			return 9.25f;
		}
		return 0f;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		tmpChemicalDependencies.Clear();
		if (!ModsConfig.BiotechActive)
		{
			return null;
		}
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (ShouldSatify(hediffs[i]))
			{
				tmpChemicalDependencies.Add((Hediff_ChemicalDependency)hediffs[i]);
			}
		}
		if (!tmpChemicalDependencies.Any())
		{
			return null;
		}
		tmpChemicalDependencies.SortBy((Hediff_ChemicalDependency x) => 0f - x.Severity);
		for (int num = 0; num < tmpChemicalDependencies.Count; num++)
		{
			Thing thing = FindDrugFor(pawn, tmpChemicalDependencies[num]);
			if (thing != null)
			{
				tmpChemicalDependencies.Clear();
				Pawn pawn2 = (thing.ParentHolder as Pawn_InventoryTracker)?.pawn;
				if (pawn2 != null && pawn2 != pawn)
				{
					Job job = JobMaker.MakeJob(JobDefOf.TakeFromOtherInventory, thing, pawn2);
					job.count = 1;
					return job;
				}
				return DrugAIUtility.IngestAndTakeToInventoryJob(thing, pawn, 1);
			}
		}
		tmpChemicalDependencies.Clear();
		return null;
	}

	private bool ShouldSatify(Hediff hediff)
	{
		if (!(hediff is Hediff_ChemicalDependency hediff_ChemicalDependency))
		{
			return false;
		}
		return hediff_ChemicalDependency.ShouldSatify;
	}

	private Thing FindDrugFor(Pawn pawn, Hediff_ChemicalDependency dependency)
	{
		ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
		for (int i = 0; i < innerContainer.Count; i++)
		{
			if (DrugValidator(pawn, dependency, innerContainer[i]))
			{
				return innerContainer[i];
			}
		}
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => DrugValidator(pawn, dependency, x));
		if (thing != null)
		{
			return thing;
		}
		if (pawn.IsColonist && pawn.Map != null)
		{
			foreach (Pawn spawnedColonyAnimal in pawn.Map.mapPawns.SpawnedColonyAnimals)
			{
				foreach (Thing item in spawnedColonyAnimal.inventory.innerContainer)
				{
					if (DrugValidator(pawn, dependency, item) && !spawnedColonyAnimal.IsForbidden(pawn) && pawn.CanReach(spawnedColonyAnimal, PathEndMode.OnCell, Danger.Some))
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	private static bool DrugValidator(Pawn pawn, Hediff_ChemicalDependency dependency, Thing drug)
	{
		if (!drug.def.IsDrug)
		{
			return false;
		}
		if (drug.Spawned && (!pawn.CanReserve(drug) || drug.IsForbidden(pawn) || !drug.IsSociallyProper(pawn) || !drug.IngestibleNow))
		{
			return false;
		}
		CompDrug compDrug = drug.TryGetComp<CompDrug>();
		if (compDrug == null || compDrug.Props.chemical == null || compDrug.Props.chemical != dependency.chemical)
		{
			return false;
		}
		if (pawn.drugs != null && !pawn.drugs.CurrentPolicy[drug.def].allowedForAddiction && (!pawn.InMentalState || pawn.MentalStateDef.ignoreDrugPolicy))
		{
			return false;
		}
		return true;
	}
}
