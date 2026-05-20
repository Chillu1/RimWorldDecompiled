using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_NeedDoctor : Alert
{
	private List<Pawn> patientsResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> Patients
	{
		get
		{
			patientsResult.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (!maps[i].IsPlayerHome)
				{
					continue;
				}
				bool flag = false;
				foreach (Pawn item in maps[i].mapPawns.FreeColonistsSpawned)
				{
					if ((item.Spawned || item.BrieflyDespawned()) && !item.Downed && item.workSettings != null && item.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
				foreach (Pawn item2 in maps[i].mapPawns.FreeColonistsSpawned)
				{
					if ((item2.Spawned || item2.BrieflyDespawned()) && ((item2.Downed && !LifeStageUtility.AlwaysDowned(item2) && item2.needs?.food != null && (int)item2.needs.food.CurCategory > 0 && item2.InBed()) || HealthAIUtility.ShouldBeTendedNowByPlayer(item2)))
					{
						patientsResult.Add(item2);
					}
				}
			}
			return patientsResult;
		}
	}

	public Alert_NeedDoctor()
	{
		defaultLabel = "NeedDoctor".Translate();
		defaultPriority = AlertPriority.High;
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (Pawn item in patientsResult)
		{
			sb.AppendLine("  - " + item.NameShortColored.Resolve());
		}
		return "NeedDoctorDesc".Translate(sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		if (Find.AnyPlayerHomeMap == null)
		{
			return false;
		}
		return AlertReport.CulpritsAre(Patients);
	}
}
