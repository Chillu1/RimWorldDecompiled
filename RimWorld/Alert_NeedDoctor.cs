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
					foreach (Pawn item in maps[i].mapPawns.FreeColonistsSpawned)
					{
						if (!item.Downed && item.workSettings != null && item.workSettings.WorkIsActive(WorkTypeDefOf.Doctor))
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
						if ((item2.Downed && (int)item2.needs.food.CurCategory < 0 && item2.InBed()) || HealthAIUtility.ShouldBeTendedNowByPlayer(item2))
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
