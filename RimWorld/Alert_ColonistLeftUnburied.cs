using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_ColonistLeftUnburied : Alert
{
	private List<Thing> unburiedColonistCorpsesResult = new List<Thing>();

	private List<Thing> UnburiedColonistCorpses
	{
		get
		{
			unburiedColonistCorpsesResult.Clear();
			foreach (Map map in Find.Maps)
			{
				if (!map.mapPawns.AnyFreeColonistSpawned)
				{
					continue;
				}
				List<Thing> list = map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Corpse));
				for (int i = 0; i < list.Count; i++)
				{
					Corpse corpse = (Corpse)list[i];
					if (IsCorpseOfColonist(corpse))
					{
						unburiedColonistCorpsesResult.Add(corpse);
					}
				}
			}
			return unburiedColonistCorpsesResult;
		}
	}

	public static bool IsCorpseOfColonist(Corpse corpse)
	{
		if (corpse.InnerPawn.Faction == Faction.OfPlayer && corpse.InnerPawn.def.race.Humanlike && !corpse.InnerPawn.IsQuestLodger() && !corpse.InnerPawn.IsSlave && !corpse.InnerPawn.IsSubhuman && !corpse.InnerPawn.IsShambler)
		{
			return corpse.Spawned;
		}
		return false;
	}

	public Alert_ColonistLeftUnburied()
	{
		defaultLabel = "AlertColonistLeftUnburied".Translate();
		defaultExplanation = "AlertColonistLeftUnburiedDesc".Translate();
		defaultPriority = AlertPriority.High;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(UnburiedColonistCorpses);
	}
}
