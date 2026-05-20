using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class StatWorker_CaravanRidingSpeedFactor : StatWorker
	{
		public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (req.HasThing && req.Thing.def.race != null)
			{
				stringBuilder.Append("StatsReport_CaravanRideableAge".Translate()).Append(": ").Append(CaravanRideableUtility.RideableLifeStagesDesc(req.Thing.def.race))
					.AppendLine();
			}
			stringBuilder.Append(base.GetExplanationUnfinalized(req, numberSense));
			return stringBuilder.ToString();
		}
	}
}
