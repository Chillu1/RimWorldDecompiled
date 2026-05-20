using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CreepJoinerWorker_PsychicAgony : BaseCreepJoinerWorker
{
	private const float Range = 14.9f;

	private static readonly IntRange DisappearSeconds = new IntRange(2000, 4000);

	public override void DoResponse(List<TargetInfo> looktargets, List<NamedArgument> namedArgs)
	{
		ApplyOrRefreshHediff(base.Pawn);
		foreach (Pawn item in base.Pawn.MapHeld.mapPawns.AllPawnsSpawned)
		{
			if (item.Position.InHorDistOf(base.Pawn.Position, 14.9f))
			{
				ApplyOrRefreshHediff(item);
			}
		}
		SoundDefOf.PsychicBanshee.PlayOneShot(base.Pawn);
		MoteMaker.MakeAttachedOverlay(base.Pawn, ThingDefOf.Mote_PsychicBanshee, Vector3.zero);
	}

	private void ApplyOrRefreshHediff(Pawn pawn)
	{
		if (pawn.health.hediffSet.TryGetHediff(HediffDefOf.AgonyPulse, out var hediff))
		{
			hediff.Severity = 0f;
		}
		else
		{
			hediff = pawn.health.AddHediff(HediffDefOf.AgonyPulse);
		}
		HediffComp_Disappears hediffComp_Disappears = (hediff as HediffWithComps)?.GetComp<HediffComp_Disappears>();
		if (hediffComp_Disappears != null)
		{
			hediffComp_Disappears.ticksToDisappear = DisappearSeconds.RandomInRange * 60;
		}
	}
}
