using System;
using System.Linq;
using System.Text;
using RimWorld;

namespace Verse;

public class HediffComp_GrowthMode : HediffComp_SeverityModifierBase
{
	private const int CheckGrowthModeChangeInterval = 5000;

	private const float GrowthModeChangeMtbDays = 100f;

	public HediffGrowthMode growthMode;

	private float severityPerDayGrowingRandomFactor = 1f;

	private float severityPerDayRemissionRandomFactor = 1f;

	public HediffCompProperties_GrowthMode Props => (HediffCompProperties_GrowthMode)props;

	public override string CompLabelInBracketsExtra => growthMode.GetLabel();

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref growthMode, "growthMode", HediffGrowthMode.Growing);
		Scribe_Values.Look(ref severityPerDayGrowingRandomFactor, "severityPerDayGrowingRandomFactor", 1f);
		Scribe_Values.Look(ref severityPerDayRemissionRandomFactor, "severityPerDayRemissionRandomFactor", 1f);
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		base.CompPostPostAdd(dinfo);
		growthMode = ((HediffGrowthMode[])Enum.GetValues(typeof(HediffGrowthMode))).RandomElement();
		severityPerDayGrowingRandomFactor = Props.severityPerDayGrowingRandomFactor.RandomInRange;
		severityPerDayRemissionRandomFactor = Props.severityPerDayRemissionRandomFactor.RandomInRange;
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		base.CompPostTickInterval(ref severityAdjustment, delta);
		if (base.Pawn.IsHashIntervalTick(5000, delta) && Rand.MTBEventOccurs(100f, 60000f, 5000f))
		{
			ChangeGrowthMode();
		}
	}

	public override float SeverityChangePerDay()
	{
		switch (growthMode)
		{
		case HediffGrowthMode.Growing:
		{
			float num = Props.severityPerDayGrowing * severityPerDayGrowingRandomFactor;
			if (ModsConfig.BiotechActive && parent.def == HediffDefOf.Carcinoma)
			{
				num *= base.Pawn.GetStatValue(StatDefOf.CancerRate);
			}
			return num;
		}
		case HediffGrowthMode.Stable:
			return 0f;
		case HediffGrowthMode.Remission:
			return Props.severityPerDayRemission * severityPerDayRemissionRandomFactor;
		default:
			throw new NotImplementedException("GrowthMode");
		}
	}

	private void ChangeGrowthMode()
	{
		growthMode = ((HediffGrowthMode[])Enum.GetValues(typeof(HediffGrowthMode))).Where((HediffGrowthMode x) => x != growthMode).RandomElement();
		if (PawnUtility.ShouldSendNotificationAbout(base.Pawn))
		{
			switch (growthMode)
			{
			case HediffGrowthMode.Growing:
				Messages.Message("DiseaseGrowthModeChanged_Growing".Translate(base.Pawn.LabelShort, base.Def.label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.NegativeHealthEvent);
				break;
			case HediffGrowthMode.Stable:
				Messages.Message("DiseaseGrowthModeChanged_Stable".Translate(base.Pawn.LabelShort, base.Def.label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.NeutralEvent);
				break;
			case HediffGrowthMode.Remission:
				Messages.Message("DiseaseGrowthModeChanged_Remission".Translate(base.Pawn.LabelShort, base.Def.label, base.Pawn.Named("PAWN")), base.Pawn, MessageTypeDefOf.PositiveEvent);
				break;
			}
		}
	}

	public override string CompDebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.CompDebugString());
		stringBuilder.AppendLine("severity: " + parent.Severity.ToString("F3") + ((parent.Severity >= base.Def.maxSeverity) ? " (reached max)" : ""));
		stringBuilder.AppendLine("severityPerDayGrowingRandomFactor: " + severityPerDayGrowingRandomFactor.ToString("0.##"));
		stringBuilder.AppendLine("severityPerDayRemissionRandomFactor: " + severityPerDayRemissionRandomFactor.ToString("0.##"));
		return stringBuilder.ToString();
	}
}
