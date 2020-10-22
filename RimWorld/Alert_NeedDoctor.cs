using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_NeedDoctor : Alert
	{
		private List<Pawn> patientsResult = new List<Pawn>();

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
					foreach (Pawn freeColonist in maps[i].mapPawns.FreeColonists)
					{
						if ((freeColonist.Spawned || freeColonist.BrieflyDespawned()) && !freeColonist.Downed && freeColonist.workSettings != null && freeColonist.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
					foreach (Pawn freeColonist2 in maps[i].mapPawns.FreeColonists)
					{
						if ((freeColonist2.Spawned || freeColonist2.BrieflyDespawned()) && ((freeColonist2.Downed && freeColonist2.needs != null && (int)freeColonist2.needs.food.CurCategory < 0 && freeColonist2.InBed()) || HealthAIUtility.ShouldBeTendedNowByPlayer(freeColonist2)))
						{
							patientsResult.Add(freeColonist2);
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
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn patient in Patients)
			{
				stringBuilder.AppendLine("  - " + patient.NameShortColored.Resolve());
			}
			return "NeedDoctorDesc".Translate(stringBuilder.ToString());
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
}
