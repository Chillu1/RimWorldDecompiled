using System.Text;
using Verse;

namespace RimWorld;

public static class ReliquaryUtility
{
	private static readonly SimpleCurve ConvertPowerFactorFromInstalledRelicsCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(1f, 1.1f),
		new CurvePoint(3f, 1.2f),
		new CurvePoint(5f, 1.25f),
		new CurvePoint(10f, 1.3f)
	};

	public static float GetRelicConvertPowerFactorForPawn(Pawn pawn, StringBuilder sb = null)
	{
		if (!ModsConfig.IdeologyActive || !pawn.Spawned || pawn.Faction == null || pawn.Ideo == null)
		{
			return 1f;
		}
		int num = 0;
		foreach (Thing item in pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Reliquary))
		{
			if (item.Faction != pawn.Faction)
			{
				continue;
			}
			CompRelicContainer compRelicContainer = item.TryGetComp<CompRelicContainer>();
			if (compRelicContainer != null && !compRelicContainer.Empty)
			{
				Precept_ThingStyle precept_ThingStyle = compRelicContainer.ContainedThing.TryGetComp<CompStyleable>()?.SourcePrecept;
				if (precept_ThingStyle != null && precept_ThingStyle.ideo == pawn.Ideo)
				{
					num++;
				}
			}
		}
		float num2 = ConvertPowerFactorFromInstalledRelicsCurve.Evaluate(num);
		if (sb != null && num2 != 1f)
		{
			sb.AppendInNewLine(" -  " + "AbilityIdeoConvertBreakdownRelic".Translate(num.Named("RELICCOUNT")).CapitalizeFirst() + ": " + num2.ToStringPercent());
		}
		return num2;
	}

	public static bool IsRelic(this Thing thing)
	{
		if (!ModsConfig.IdeologyActive)
		{
			return false;
		}
		if (!(thing is ThingWithComps thingWithComps))
		{
			return false;
		}
		Precept_ThingStyle precept_ThingStyle = thingWithComps.compStyleable?.SourcePrecept;
		if (precept_ThingStyle == null)
		{
			return false;
		}
		return typeof(Precept_Relic).IsAssignableFrom(precept_ThingStyle.GetType());
	}
}
