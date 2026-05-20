using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_RoyalTitleHyperlink : QuestPart
{
	public RoyalTitleDef titleDef;

	public FactionDef factionDef;

	public override IEnumerable<Dialog_InfoCard.Hyperlink> Hyperlinks
	{
		get
		{
			if (titleDef != null && factionDef != null)
			{
				Faction faction = Find.FactionManager.FirstFactionOfDef(factionDef);
				yield return new Dialog_InfoCard.Hyperlink(titleDef, faction);
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref titleDef, "titleDef");
		Scribe_Defs.Look(ref factionDef, "factionDef");
	}
}
