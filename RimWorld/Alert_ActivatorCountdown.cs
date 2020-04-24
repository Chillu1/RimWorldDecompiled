using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Alert_ActivatorCountdown : Alert
	{
		private List<Thing> activatorCountdownsResult = new List<Thing>();

		private List<Thing> ActivatorCountdowns
		{
			get
			{
				activatorCountdownsResult.Clear();
				foreach (Map map in Find.Maps)
				{
					if (map.mapPawns.AnyColonistSpawned)
					{
						foreach (Thing item in map.listerThings.ThingsMatching(ThingRequest.ForDef(ThingDefOf.ActivatorCountdown)))
						{
							CompSendSignalOnCountdown compSendSignalOnCountdown = item.TryGetComp<CompSendSignalOnCountdown>();
							if (compSendSignalOnCountdown != null && compSendSignalOnCountdown.ticksLeft > 0)
							{
								activatorCountdownsResult.Add(item);
							}
						}
					}
				}
				return activatorCountdownsResult;
			}
		}

		public Alert_ActivatorCountdown()
		{
			defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			if (!ModsConfig.RoyaltyActive)
			{
				return false;
			}
			return AlertReport.CulpritsAre(ActivatorCountdowns);
		}

		public override string GetLabel()
		{
			int count = ActivatorCountdowns.Count;
			if (count > 1)
			{
				return "ActivatorCountdownMultiple".Translate(count);
			}
			if (count == 0)
			{
				return "";
			}
			CompSendSignalOnCountdown compSendSignalOnCountdown = ActivatorCountdowns[0].TryGetComp<CompSendSignalOnCountdown>();
			return "ActivatorCountdown".Translate(compSendSignalOnCountdown.ticksLeft.ToStringTicksToPeriod());
		}

		public override TaggedString GetExplanation()
		{
			int num = ActivatorCountdowns.Count();
			if (num > 1)
			{
				return "ActivatorCountdownDescMultiple".Translate(num);
			}
			if (num == 0)
			{
				return "";
			}
			return "ActivatorCountdownDesc".Translate();
		}
	}
}
