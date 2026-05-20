using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_Hyperlinks : QuestPart
{
	public List<ThingDef> thingDefs = new List<ThingDef>();

	public List<Pawn> pawns = new List<Pawn>();

	public List<Faction> factions = new List<Faction>();

	public List<ResearchProjectDef> researchProjects = new List<ResearchProjectDef>();

	private IEnumerable<Dialog_InfoCard.Hyperlink> cachedHyperlinks;

	public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
	{
		get
		{
			if (cachedHyperlinks == null)
			{
				cachedHyperlinks = GetHyperlinks();
			}
			return cachedHyperlinks;
		}
	}

	private IEnumerable<Dialog_InfoCard.Hyperlink> GetHyperlinks()
	{
		if (thingDefs != null)
		{
			for (int i = 0; i < thingDefs.Count; i++)
			{
				yield return new Dialog_InfoCard.Hyperlink(thingDefs[i]);
			}
		}
		if (pawns != null)
		{
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].royalty != null && pawns[i].royalty.AllTitlesForReading.Any())
				{
					RoyalTitle mostSeniorTitle = pawns[i].royalty.MostSeniorTitle;
					if (mostSeniorTitle != null)
					{
						yield return new Dialog_InfoCard.Hyperlink(mostSeniorTitle.def, mostSeniorTitle.faction);
					}
				}
			}
		}
		if (factions != null)
		{
			for (int i = 0; i < factions.Count; i++)
			{
				yield return new Dialog_InfoCard.Hyperlink(factions[i]);
			}
		}
		if (researchProjects != null)
		{
			for (int i = 0; i < researchProjects.Count; i++)
			{
				yield return new Dialog_InfoCard.Hyperlink(researchProjects[i]);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref thingDefs, "thingDefs", LookMode.Undefined);
		Scribe_Collections.Look(ref pawns, "pawns", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (thingDefs == null)
			{
				thingDefs = new List<ThingDef>();
			}
			thingDefs.RemoveAll((ThingDef x) => x == null);
			if (pawns == null)
			{
				pawns = new List<Pawn>();
			}
			pawns.RemoveAll((Pawn x) => x == null);
		}
	}

	public override void ReplacePawnReferences(Pawn replace, Pawn with)
	{
		pawns.Replace(replace, with);
	}
}
