using System.Text;
using Verse;

namespace RimWorld;

public class StatWorker_ShootingAccuracy : StatWorker
{
	public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("StatsReport_ShootingExampleDistances".Translate());
		float f = ShotReport.HitFactorFromShooter(req.Thing, 1f, finalVal);
		stringBuilder.AppendLine("distance".Translate().CapitalizeFirst() + " 1: " + f.ToStringPercent("F1"));
		for (int i = 5; i <= 55; i += 5)
		{
			f = ShotReport.HitFactorFromShooter(req.Thing, i, finalVal);
			stringBuilder.AppendLine("distance".Translate().CapitalizeFirst() + " " + i.ToString() + ": " + f.ToStringPercent("F1"));
		}
		return stringBuilder.ToString();
	}
}
