using RimWorld;

namespace Verse;

public class HediffComp_ChangeImplantLevel : HediffComp
{
	public int lastChangeLevelTick = -1;

	public HediffCompProperties_ChangeImplantLevel Props => (HediffCompProperties_ChangeImplantLevel)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		float mtbDays = Props.probabilityPerStage[parent.CurStageIndex].mtbDays;
		if (!(mtbDays <= 0f) && base.Pawn.IsHashIntervalTick(60, delta))
		{
			ChangeImplantLevel_Probability changeImplantLevel_Probability = Props.probabilityPerStage[parent.CurStageIndex];
			if ((lastChangeLevelTick < 0 || (float)(Find.TickManager.TicksGame - lastChangeLevelTick) >= changeImplantLevel_Probability.minIntervalDays * 60000f) && Rand.MTBEventOccurs(mtbDays, 60000f, 60f) && parent.pawn.health.hediffSet.GetFirstHediffOfDef(Props.implant) is Hediff_Level hediff_Level)
			{
				hediff_Level.ChangeLevel(Props.levelOffset);
				lastChangeLevelTick = Find.TickManager.TicksGame;
				Messages.Message("MessageLostImplantLevelFromHediff".Translate(parent.pawn.Named("PAWN"), hediff_Level.LabelBase, parent.Label), parent.pawn, MessageTypeDefOf.NegativeEvent);
			}
		}
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref lastChangeLevelTick, "lastChangeLevelTick", 0);
	}
}
