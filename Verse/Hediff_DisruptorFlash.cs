using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class Hediff_DisruptorFlash : HediffWithComps
{
	private HediffStage stage;

	public override HediffStage CurStage => stage;

	public override void PostAdd(DamageInfo? dinfo)
	{
		base.PostAdd(dinfo);
		SetupStage();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			SetupStage();
		}
	}

	public void SetupStage()
	{
		stage = new HediffStage
		{
			capMods = new List<PawnCapacityModifier>
			{
				new PawnCapacityModifier
				{
					capacity = PawnCapacityDefOf.Consciousness,
					postFactor = 1f - pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 0.1f
				},
				new PawnCapacityModifier
				{
					capacity = PawnCapacityDefOf.Moving,
					postFactor = 1f - pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 0.2f
				}
			}
		};
	}
}
