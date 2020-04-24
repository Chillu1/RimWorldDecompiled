using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
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
			for (int j = 0; j < tmpChemicalNeeds.Count; j++)
			{
				Thing thing = FindDrugFor(pawn, tmpChemicalNeeds[j]);
				if (thing != null)
				{
					tmpChemicalNeeds.Clear();
					return DrugAIUtility.IngestAndTakeToInventoryJob(thing, pawn, 1);
				}
			}
			tmpChemicalNeeds.Clear();
			return null;
		}

		private bool ShouldSatisfy(Need need)
		{
			Need_Chemical need_Chemical = need as Need_Chemical;
			if (need_Chemical != null && (int)need_Chemical.CurCategory <= 1)
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
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Drug), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => DrugValidator(pawn, addictionHediff, x));
		}

		private bool DrugValidator(Pawn pawn, Hediff_Addiction addiction, Thing drug)
		{
			if (!drug.def.IsDrug)
			{
				return false;
			}
			if (drug.Spawned)
			{
				if (drug.IsForbidden(pawn))
				{
					return false;
				}
				if (!pawn.CanReserve(drug))
				{
					return false;
				}
				if (!drug.IsSociallyProper(pawn))
				{
					return false;
				}
			}
			CompDrug compDrug = drug.TryGetComp<CompDrug>();
			if (compDrug == null || compDrug.Props.chemical == null)
			{
				return false;
			}
			if (compDrug.Props.chemical.addictionHediff != addiction.def)
			{
				return false;
			}
			if (pawn.drugs != null && !pawn.drugs.CurrentPolicy[drug.def].allowedForAddiction && pawn.story != null && pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) <= 0 && (!pawn.InMentalState || !pawn.MentalStateDef.ignoreDrugPolicy))
			{
				return false;
			}
			return true;
		}
	}
}
