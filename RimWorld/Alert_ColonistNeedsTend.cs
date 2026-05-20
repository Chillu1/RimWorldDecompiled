using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.Steam;

namespace RimWorld;

public class Alert_ColonistNeedsTend : Alert
{
	private List<Pawn> needingColonistsResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> NeedingColonists
	{
		get
		{
			needingColonistsResult.Clear();
			foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
			{
				if ((item.Spawned || item.BrieflyDespawned()) && item.health.HasHediffsNeedingTendByPlayer(forAlert: true))
				{
					Building_Bed building_Bed = item.CurrentBed();
					if ((building_Bed == null || !building_Bed.Medical) && !Alert_ColonistNeedsRescuing.NeedsRescue(item) && !ChildcareUtility.BabyBeingPlayedWith(item))
					{
						needingColonistsResult.Add(item);
					}
				}
			}
			return needingColonistsResult;
		}
	}

	public Alert_ColonistNeedsTend()
	{
		defaultLabel = "ColonistNeedsTreatment".Translate();
		defaultPriority = AlertPriority.High;
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (Pawn item in needingColonistsResult)
		{
			sb.AppendLine("  - " + item.NameShortColored.Resolve());
		}
		if (SteamDeck.IsSteamDeckInNonKeyboardMode)
		{
			return "ColonistNeedsTreatmentDescController".Translate(sb.ToString().TrimEndNewlines());
		}
		return "ColonistNeedsTreatmentDesc".Translate(sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(NeedingColonists);
	}
}
