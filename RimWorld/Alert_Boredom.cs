using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_Boredom : Alert
{
	private const float JoyNeedThreshold = 0.24000001f;

	private List<Pawn> boredPawnsResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> BoredPawns
	{
		get
		{
			boredPawnsResult.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (!maps[i].IsPlayerHome)
				{
					continue;
				}
				List<Pawn> freeColonistsSpawned = maps[i].mapPawns.FreeColonistsSpawned;
				if (!freeColonistsSpawned.Any())
				{
					continue;
				}
				List<JoyKindDef> list = JoyUtility.JoyKindsOnMapTempList(maps[i]);
				try
				{
					foreach (Pawn item in freeColonistsSpawned)
					{
						if (item.needs.joy != null && (item.needs.joy.CurLevelPercentage < 0.24000001f || item.GetTimeAssignment() == TimeAssignmentDefOf.Joy) && item.needs.joy.tolerances.BoredOfKinds(list))
						{
							boredPawnsResult.Add(item);
						}
					}
				}
				finally
				{
					list.Clear();
				}
			}
			return boredPawnsResult;
		}
	}

	public Alert_Boredom()
	{
		defaultLabel = "Boredom".Translate();
		defaultPriority = AlertPriority.Medium;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(BoredPawns);
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		Pawn pawn = null;
		foreach (Pawn item in boredPawnsResult)
		{
			sb.AppendLine("  - " + item.NameShortColored.Resolve());
			if (pawn == null)
			{
				pawn = item;
			}
		}
		string text = ((pawn != null) ? JoyUtility.JoyKindsOnMapString(pawn.Map) : string.Empty);
		return "BoredomDesc".Translate(sb.ToString().TrimEndNewlines(), pawn.LabelShort, text, pawn.Named("PAWN"));
	}
}
