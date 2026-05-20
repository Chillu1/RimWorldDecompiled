using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_AbandonedBaby : Alert
{
	private static List<Pawn> tmpAllBabiesList = new List<Pawn>(32);

	private static List<Pawn> tmpAbandonedBabiesList = new List<Pawn>(32);

	public Alert_AbandonedBaby()
	{
		defaultLabel = "AlertAbandonedBaby".Translate();
		defaultExplanation = "AlertAbandonedBabyDesc".Translate("DesignatorAdopt".Translate());
		defaultPriority = AlertPriority.High;
		requireBiotech = true;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(AbandonedBabies());
	}

	private static List<Pawn> AbandonedBabies()
	{
		tmpAllBabiesList.Clear();
		tmpAbandonedBabiesList.Clear();
		ChildcareUtility.BreastfeedFailReason? reason;
		using (new ProfilerBlock("AllBabies"))
		{
			foreach (Map map in Find.Maps)
			{
				foreach (Pawn allPawn in map.mapPawns.AllPawns)
				{
					if (ChildcareUtility.CanSuckle(allPawn, out reason))
					{
						tmpAllBabiesList.Add(allPawn);
					}
				}
			}
		}
		using (new ProfilerBlock("AbandonedBabiesFilter"))
		{
			foreach (Pawn tmpAllBabies in tmpAllBabiesList)
			{
				if (tmpAllBabies.Faction == Faction.OfPlayer)
				{
					continue;
				}
				if (!tmpAllBabies.Spawned)
				{
					IThingHolder parentHolder = tmpAllBabies.ParentHolder;
					if (parentHolder != null && !(parentHolder is Pawn_CarryTracker))
					{
						continue;
					}
				}
				Map mapHeld = tmpAllBabies.MapHeld;
				bool flag = true;
				foreach (Pawn item in tmpAllBabies.mindState.Autofeeders())
				{
					if (item.MapHeld == mapHeld && ChildcareUtility.CanFeedBaby(item, tmpAllBabies, out reason))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				using (new ProfilerBlock("AdoptableBy"))
				{
					if (!tmpAllBabies.AdoptableBy(Faction.OfPlayer))
					{
						continue;
					}
				}
				tmpAbandonedBabiesList.Add(tmpAllBabies);
			}
		}
		return tmpAbandonedBabiesList;
	}
}
