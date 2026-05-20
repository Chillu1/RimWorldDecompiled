using RimWorld;

namespace Verse;

public class HediffComp_PregnantHuman : HediffComp
{
	private int lastGivenMorningSicknessTick = -1;

	private int lastGivenPregnancyMoodTick = -1;

	private PregnancyAttitude? pregnancyAttitude;

	private const int MorningSicknessMinIntervalTicks = 48000;

	private static readonly float[] MorningSicknessMTBDaysPerStage = new float[3] { 4f, 8f, 8f };

	private const int PregnancyMoodMinIntervalTicks = 48000;

	private static readonly float[] PregnancyMoodMTBDaysPerStage = new float[3] { 4f, 8f, 8f };

	public PregnancyAttitude? Attitude => pregnancyAttitude;

	public override string CompTipStringExtra
	{
		get
		{
			Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)parent;
			TaggedString taggedString = "\n" + "FatherTip".Translate() + ": " + ((hediff_Pregnant.Father != null) ? hediff_Pregnant.Father.LabelShort.Colorize(ColoredText.NameColor) : "Unknown".Translate().ToString()).CapitalizeFirst();
			if (hediff_Pregnant.Mother != null && hediff_Pregnant.Mother != parent.pawn)
			{
				taggedString += "\n" + "MotherTip".Translate() + ": " + hediff_Pregnant.Mother.LabelShort.CapitalizeFirst().Colorize(ColoredText.NameColor);
			}
			return taggedString.Resolve();
		}
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		base.CompPostPostAdd(dinfo);
		SetupPregnancyAttitude();
	}

	private void SetupPregnancyAttitude()
	{
		pregnancyAttitude = ((!(Rand.Value < 0.5f)) ? PregnancyAttitude.Negative : PregnancyAttitude.Positive);
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (ModLister.CheckBiotech("Human pregnancy") && base.Pawn.IsHashIntervalTick(60, delta))
		{
			if ((lastGivenMorningSicknessTick == -1 || GenTicks.TicksGame - lastGivenMorningSicknessTick >= 48000) && Rand.MTBEventOccurs(MorningSicknessMTBDaysPerStage[parent.CurStageIndex], 60000f, 60f))
			{
				base.Pawn.health.AddHediff(HediffDefOf.MorningSickness);
				lastGivenMorningSicknessTick = GenTicks.TicksGame;
			}
			if ((lastGivenPregnancyMoodTick == -1 || GenTicks.TicksGame - lastGivenPregnancyMoodTick >= 48000) && Rand.MTBEventOccurs(PregnancyMoodMTBDaysPerStage[parent.CurStageIndex], 60000f, 60f))
			{
				base.Pawn.health.AddHediff(HediffDefOf.PregnancyMood, base.Pawn.health.hediffSet.GetBrain());
				lastGivenPregnancyMoodTick = GenTicks.TicksGame;
			}
		}
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref lastGivenMorningSicknessTick, "lastGivenMorningSicknessTick", -1);
		Scribe_Values.Look(ref lastGivenPregnancyMoodTick, "lastGivenPregnancyMoodTick", -1);
		Scribe_Values.Look(ref pregnancyAttitude, "pregnancyAttitude");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && !pregnancyAttitude.HasValue)
		{
			SetupPregnancyAttitude();
		}
	}
}
