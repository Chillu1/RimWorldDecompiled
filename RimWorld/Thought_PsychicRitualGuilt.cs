using Verse;

namespace RimWorld;

public class Thought_PsychicRitualGuilt : Thought_Memory
{
	public PsychicRitualDef ritualDef;

	public override string LabelCap => base.CurStage.label.Formatted(ritualDef.label.Named("RITUAL")).CapitalizeFirst();

	public override int CurStageIndex
	{
		get
		{
			if (!ModsConfig.IdeologyActive)
			{
				return 0;
			}
			if (pawn.Ideo.HasPrecept(PreceptDefOf.PsychicRituals_Disapproved))
			{
				return 1;
			}
			if (pawn.Ideo.HasPrecept(PreceptDefOf.PsychicRituals_Abhorrent))
			{
				return 2;
			}
			return 0;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref ritualDef, "ritualDef");
	}

	public override void CopyFrom(Thought_Memory m)
	{
		base.CopyFrom(m);
		if (m is Thought_PsychicRitualGuilt thought_PsychicRitualGuilt)
		{
			ritualDef = thought_PsychicRitualGuilt.ritualDef;
		}
	}

	public override bool GroupsWith(Thought other)
	{
		if (base.GroupsWith(other) && other is Thought_PsychicRitualGuilt thought_PsychicRitualGuilt)
		{
			return ritualDef == thought_PsychicRitualGuilt.ritualDef;
		}
		return false;
	}
}
