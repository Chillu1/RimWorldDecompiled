using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Alert_BestowerWaiting : Alert
{
	private List<Pawn> bestowersWaitingResult = new List<Pawn>();

	private List<Pawn> BestowersWaiting
	{
		get
		{
			bestowersWaitingResult.Clear();
			foreach (Map map in Find.Maps)
			{
				foreach (Lord lord in map.lordManager.lords)
				{
					if (lord.CurLordToil is LordToil_BestowingCeremony_Wait)
					{
						bestowersWaitingResult.Add(lord.ownedPawns[0]);
					}
				}
			}
			return bestowersWaitingResult;
		}
	}

	public Alert_BestowerWaiting()
	{
		defaultPriority = AlertPriority.High;
		defaultLabel = "BestowerWaitingAlert".Translate();
		defaultExplanation = "BestowerWaitingAlertDesc".Translate();
		requireRoyalty = true;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(BestowersWaiting);
	}
}
