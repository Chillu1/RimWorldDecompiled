using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_CaravanIdle : Alert
	{
		private List<Caravan> idleCaravansResult = new List<Caravan>();

		private List<Caravan> IdleCaravans
		{
			get
			{
				idleCaravansResult.Clear();
				foreach (Caravan caravan in Find.WorldObjects.Caravans)
				{
					if (caravan.Spawned && caravan.IsPlayerControlled && !caravan.pather.MovingNow && !caravan.CantMove)
					{
						idleCaravansResult.Add(caravan);
					}
				}
				return idleCaravansResult;
			}
		}

		public override string GetLabel()
		{
			return "CaravanIdle".Translate();
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Caravan idleCaravan in IdleCaravans)
			{
				stringBuilder.AppendLine("  - " + idleCaravan.Label);
			}
			return "CaravanIdleDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(IdleCaravans);
		}
	}
}
