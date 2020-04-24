using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
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
				return colonistsNeedingRescueResult;
			}
		}

		public static bool NeedsRescue(Pawn p)
		{
			if (p.Downed && !p.InBed() && !(p.ParentHolder is Pawn_CarryTracker))
			{
				if (p.jobs.jobQueue != null && p.jobs.jobQueue.Count > 0 && p.jobs.jobQueue.Peek().job.CanBeginNow(p))
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public override string GetLabel()
		{
			if (ColonistsNeedingRescue.Count == 1)
			{
				return "ColonistNeedsRescue".Translate();
			}
			return "ColonistsNeedRescue".Translate();
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn item in ColonistsNeedingRescue)
			{
				stringBuilder.AppendLine("  - " + item.NameShortColored.Resolve());
			}
			return "ColonistsNeedRescueDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(ColonistsNeedingRescue);
		}
	}
}
