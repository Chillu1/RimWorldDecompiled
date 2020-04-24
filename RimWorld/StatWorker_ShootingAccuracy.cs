using System.Text;
using Verse;

namespace RimWorld
{
	public class StatWorker_ShootingAccuracy : StatWorker
	{
		public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 5; i <= 45; i += 5)
			{
				float f = ShotReport.HitFactorFromShooter(finalVal, i);
				stringBuilder.AppendLine("distance".Translate().CapitalizeFirst() + " " + i.ToString() + ": " + f.ToStringPercent("F1"));
			}
			stringBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
			return stringBuilder.ToString();
		}
	}
}
