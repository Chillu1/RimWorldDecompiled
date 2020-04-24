using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_StarvationAnimals : Alert
	{
		private List<Pawn> starvingAnimalsResult = new List<Pawn>();

		private List<Pawn> StarvingAnimals
		{
			get
			{
				starvingAnimalsResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep)
				{
					if (item.HostFaction == null && !item.RaceProps.Humanlike && item.needs.food != null && (item.needs.food.TicksStarving > 30000 || (item.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true) && item.needs.food.TicksStarving > 5000)))
					{
						starvingAnimalsResult.Add(item);
					}
				}
				return starvingAnimalsResult;
			}
		}

		public Alert_StarvationAnimals()
		{
			defaultLabel = "StarvationAnimals".Translate();
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn item in StarvingAnimals.OrderBy((Pawn a) => a.def.label))
			{
				stringBuilder.Append("    " + item.LabelShortCap);
				if (item.Name.IsValid && !item.Name.Numerical)
				{
					stringBuilder.Append(" (" + item.def.label + ")");
				}
				stringBuilder.AppendLine();
			}
			return "StarvationAnimalsDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(StarvingAnimals);
		}
	}
}
