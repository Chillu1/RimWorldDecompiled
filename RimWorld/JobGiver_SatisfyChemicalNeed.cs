using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_SatisfyChemicalNeed : ThinkNode_JobGiver
{
	private static List<Need_Chemical> tmpChemicalNeeds = new List<Need_Chemical>();

	public override float GetPriority(Pawn pawn)
	{
		if (pawn.needs.AllNeeds.Any((Need x) => ShouldSatisfy(x)))
		{
			return 9.25f;
		}
		return 0f;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		tmpChemicalNeeds.Clear();
		List<Need> allNeeds = pawn.needs.AllNeeds;
		for (int i = 0; i < allNeeds.Count; i++)
		{
			if (ShouldSatisfy(allNeeds[i]))
			{
				tmpChemicalNeeds.Add((Need_Chemical)allNeeds[i]);
			}
		}
		if (!tmpChemicalNeeds.Any())
		{
			return null;
		}
		tmpChemicalNeeds.SortBy((Need_Chemical x) => x.CurLevel);
		for (int num = 0; num < tmpChemicalNeeds.Count; num++)
		{
			Thing thing = FindDrugFor(pawn, tmpChemicalNeeds[num]);
			if (thing != null)
			{
				tmpChemicalNeeds.Clear();
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
		tmpChemicalNeeds.Clear();
		return null;
	}

	private bool ShouldSatisfy(Need need)
	{
		if (need is Need_Chemical { CurCategory: <=DrugDesireCategory.Desire })
		{
			return true;
		}
		return false;
	}

	private Thing FindDrugFor(Pawn pawn, Need_Chemical need)
	{
		Hediff_Addiction addictionHediff = need.AddictionHediff;
		if (addictionHediff == null)
		{
			return null;
		}
		ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
		for (int i = 0; i < innerContainer.Count; i++)
		{
			if (DrugValidator(pawn, addictionHediff, innerContainer[i]))
			{
				return innerContainer[i];
			}
		}
		Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => DrugValidator(pawn, addictionHediff, x));
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
					if (DrugValidator(pawn, addictionHediff, item) && !spawnedColonyAnimal.IsForbidden(pawn) && pawn.CanReach(spawnedColonyAnimal, PathEndMode.OnCell, Danger.Some))
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	private static bool DrugValidator(Pawn pawn, Hediff_Addiction addiction, Thing drug)
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
		if (compDrug?.Props.chemical == null)
		{
			return false;
		}
		if (compDrug.Props.chemical.addictionHediff != addiction.def)
		{
			return false;
		}
		DrugPolicy drugPolicy = pawn.drugs?.CurrentPolicy;
		if (drugPolicy != null && !drugPolicy[drug.def].allowedForAddiction && pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) <= 0 && (!pawn.InMentalState || !pawn.MentalStateDef.ignoreDrugPolicy))
		{
			return false;
		}
		return true;
	}
}
