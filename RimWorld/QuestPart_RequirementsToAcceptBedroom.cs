using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_RequirementsToAcceptBedroom : QuestPart_RequirementsToAccept
{
	public List<Pawn> targetPawns = new List<Pawn>();

	public MapParent mapParent;

	private List<Thing> tmpOccupiedBeds = new List<Thing>();

	private List<Pawn> culpritsResult = new List<Pawn>();

	public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks => CulpritsAre().Select(delegate(Pawn p)
	{
		RoyalTitle royalTitle = p.royalty.HighestTitleWithBedroomRequirements();
		return new Dialog_InfoCard.Hyperlink(royalTitle.def, royalTitle.faction);
	});

	public override AcceptanceReport CanAccept()
	{
		int num = CulpritsAre().Count();
		if (num > 0)
		{
			return ((num > 1) ? "QuestBedroomRequirementsUnsatisfied" : "QuestBedroomRequirementsUnsatisfiedSingle").Translate() + " " + (from p in CulpritsAre()
				select p.royalty.MainTitle().GetLabelFor(p).CapitalizeFirst() + " " + p.LabelShort).ToCommaList(useAnd: true);
		}
		return true;
	}

	private List<Pawn> CulpritsAre()
	{
		culpritsResult.Clear();
		if (targetPawns.Any())
		{
			foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
			{
				if (allMapsCaravansAndTravellingTransporters_Alive_Colonist.royalty?.HighestTitleWithBedroomRequirements() != null && !allMapsCaravansAndTravellingTransporters_Alive_Colonist.Suspended && (!allMapsCaravansAndTravellingTransporters_Alive_Colonist.royalty.HasPersonalBedroom() || allMapsCaravansAndTravellingTransporters_Alive_Colonist.royalty.AnyUnmetBedroomRequirements()))
				{
					culpritsResult.Add(allMapsCaravansAndTravellingTransporters_Alive_Colonist);
				}
			}
		}
		if (mapParent == null || !mapParent.HasMap)
		{
			return culpritsResult;
		}
		tmpOccupiedBeds.Clear();
		List<Thing> list = mapParent.Map.listerThings.ThingsInGroup(ThingRequestGroup.Bed);
		foreach (Pawn targetPawn in targetPawns)
		{
			RoyalTitle royalTitle = targetPawn.royalty.HighestTitleWithBedroomRequirements();
			if (royalTitle == null)
			{
				continue;
			}
			Thing thing = null;
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing2 = list[i];
				if (thing2.Faction == Faction.OfPlayer && thing2.GetRoom() != null && !tmpOccupiedBeds.Contains(thing2))
				{
					CompAssignableToPawn compAssignableToPawn = thing2.TryGetComp<CompAssignableToPawn>();
					if (compAssignableToPawn != null && compAssignableToPawn.AssignedPawnsForReading.Count <= 0 && RoyalTitleUtility.BedroomSatisfiesRequirements(thing2.GetRoom(), royalTitle))
					{
						thing = thing2;
						break;
					}
				}
			}
			if (thing != null)
			{
				tmpOccupiedBeds.Add(thing);
			}
			else
			{
				culpritsResult.Add(targetPawn);
			}
		}
		tmpOccupiedBeds.Clear();
		return culpritsResult;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Collections.Look(ref targetPawns, "targetPawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			targetPawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		targetPawns.Replace(replace, with);
	}
}
