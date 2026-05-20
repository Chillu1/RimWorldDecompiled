using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public sealed class OutfitDatabase : IExposable
{
	private List<ApparelPolicy> outfits = new List<ApparelPolicy>();

	public List<ApparelPolicy> AllOutfits => outfits;

	public OutfitDatabase()
	{
		GenerateStartingOutfits();
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref outfits, "outfits", LookMode.Deep);
	}

	public ApparelPolicy DefaultOutfit()
	{
		if (outfits.Count == 0)
		{
			MakeNewOutfit();
		}
		return outfits[0];
	}

	public void SetDefault(ApparelPolicy policy)
	{
		int index = outfits.IndexOf(policy);
		ApparelPolicy value = outfits[0];
		outfits[0] = policy;
		outfits[index] = value;
	}

	public AcceptanceReport TryDelete(ApparelPolicy apparelPolicy)
	{
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
		{
			if (item.outfits != null && item.outfits.CurrentApparelPolicy == apparelPolicy)
			{
				return new AcceptanceReport("OutfitInUse".Translate(item));
			}
		}
		foreach (Pawn item2 in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
		{
			if (item2.outfits != null && item2.outfits.CurrentApparelPolicy == apparelPolicy)
			{
				item2.outfits.CurrentApparelPolicy = null;
			}
		}
		outfits.Remove(apparelPolicy);
		return AcceptanceReport.WasAccepted;
	}

	public ApparelPolicy MakeNewOutfit()
	{
		int id = ((!outfits.Any()) ? 1 : (outfits.Max((ApparelPolicy o) => o.id) + 1));
		ApparelPolicy apparelPolicy = new ApparelPolicy(id, "ApparelPolicy".Translate() + " " + id.ToString());
		apparelPolicy.filter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
		outfits.Add(apparelPolicy);
		return apparelPolicy;
	}

	private void GenerateStartingOutfits()
	{
		MakeNewOutfit().label = "OutfitAnything".Translate();
		ApparelPolicy apparelPolicy = MakeNewOutfit();
		apparelPolicy.label = "OutfitWorker".Translate();
		apparelPolicy.filter.SetDisallowAll();
		apparelPolicy.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, allow: false);
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef.apparel != null && ((allDef.apparel.defaultOutfitTags != null && allDef.apparel.defaultOutfitTags.Contains("Worker")) || allDef.thingCategories.NotNullAndContains(ThingCategoryDefOf.ApparelUtility)))
			{
				apparelPolicy.filter.SetAllow(allDef, allow: true);
			}
		}
		ApparelPolicy apparelPolicy2 = MakeNewOutfit();
		apparelPolicy2.label = "OutfitSoldier".Translate();
		apparelPolicy2.filter.SetDisallowAll();
		apparelPolicy2.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, allow: false);
		foreach (ThingDef allDef2 in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef2.apparel != null && ((allDef2.apparel.defaultOutfitTags != null && allDef2.apparel.defaultOutfitTags.Contains("Soldier")) || allDef2.thingCategories.NotNullAndContains(ThingCategoryDefOf.ApparelUtility)))
			{
				apparelPolicy2.filter.SetAllow(allDef2, allow: true);
			}
		}
		ApparelPolicy apparelPolicy3 = MakeNewOutfit();
		apparelPolicy3.label = "OutfitNudist".Translate();
		apparelPolicy3.filter.SetDisallowAll();
		apparelPolicy3.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, allow: false);
		foreach (ThingDef allDef3 in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef3.apparel != null && (allDef3.apparel.defaultOutfitTags.NotNullAndContains("Nudist") || (!allDef3.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs) && !allDef3.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))))
			{
				apparelPolicy3.filter.SetAllow(allDef3, allow: true);
			}
		}
		if (ModsConfig.IdeologyActive)
		{
			ApparelPolicy apparelPolicy4 = MakeNewOutfit();
			apparelPolicy4.label = "OutfitSlave".Translate();
			apparelPolicy4.filter.SetDisallowAll();
			apparelPolicy4.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, allow: false);
			foreach (ThingDef allDef4 in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef4.apparel != null && allDef4.apparel.defaultOutfitTags != null && allDef4.apparel.defaultOutfitTags.Contains("Slave"))
				{
					apparelPolicy4.filter.SetAllow(allDef4, allow: true);
				}
			}
		}
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		ApparelPolicy apparelPolicy5 = MakeNewOutfit();
		apparelPolicy5.label = "OutfitSpacefarer".Translate();
		apparelPolicy5.filter.SetDisallowAll();
		apparelPolicy5.filter.SetAllow(SpecialThingFilterDefOf.AllowDeadmansApparel, allow: false);
		foreach (ThingDef allDef5 in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef5.apparel != null && ((allDef5.apparel.defaultOutfitTags != null && allDef5.apparel.defaultOutfitTags.Contains("Spacefarer")) || allDef5.thingCategories.NotNullAndContains(ThingCategoryDefOf.ApparelUtility)))
			{
				apparelPolicy5.filter.SetAllow(allDef5, allow: true);
			}
		}
	}
}
