using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.Steam;

namespace RimWorld;

public class Alert_ColonistNeedsRescuing : Alert_Critical
{
	private List<Pawn> colonistsNeedingRescueResult = new List<Pawn>();

	private List<Pawn> ColonistsNeedingRescue
	{
		get
		{
			colonistsNeedingRescueResult.Clear();
			foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				if (NeedsRescue(item))
				{
					colonistsNeedingRescueResult.Add(item);
				}
			}
			foreach (Pawn item2 in PawnsFinder.AllMaps_ColonySubhumansSpawnedPlayerControlled)
			{
				if (NeedsRescue(item2))
				{
					colonistsNeedingRescueResult.Add(item2);
				}
			}
			return colonistsNeedingRescueResult;
		}
	}

	public static bool NeedsRescue(Pawn p)
	{
		if (p.Downed && HealthAIUtility.WantsToBeRescued(p) && !p.InBed() && !(p.ParentHolder is Pawn_CarryTracker))
		{
			if (p.jobs?.jobQueue != null && p.jobs.jobQueue.Count > 0 && p.jobs.jobQueue.Peek().job.CanBeginNow(p))
			{
				return false;
			}
			if (ChildcareUtility.BabyBeingPlayedWith(p))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override string GetLabel()
	{
		if (colonistsNeedingRescueResult.Count == 1)
		{
			return "ColonistNeedsRescue".Translate();
		}
		return "ColonistsNeedRescue".Translate();
	}

	public override TaggedString GetExplanation()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Pawn item in colonistsNeedingRescueResult)
		{
			stringBuilder.AppendLine("  - " + item.NameShortColored.Resolve());
		}
		if (SteamDeck.IsSteamDeckInNonKeyboardMode)
		{
			return "ColonistsNeedRescueDescController".Translate(stringBuilder.ToString());
		}
		return "ColonistsNeedRescueDesc".Translate(stringBuilder.ToString());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(ColonistsNeedingRescue);
	}
}
