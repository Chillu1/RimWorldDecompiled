using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompAbilityEffect_FleckOnTarget : CompAbilityEffect
{
	public new CompProperties_AbilityFleckOnTarget Props => (CompProperties_AbilityFleckOnTarget)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		if (Props.preCastTicks <= 0)
		{
			Props.sound?.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
			SpawnAll(target);
		}
	}

	public override IEnumerable<PreCastAction> GetPreCastActions()
	{
		if (Props.preCastTicks > 0)
		{
			yield return new PreCastAction
			{
				action = delegate(LocalTargetInfo t, LocalTargetInfo d)
				{
					SpawnAll(t);
					Props.sound?.PlayOneShot(new TargetInfo(t.Cell, parent.pawn.Map));
				},
				ticksAwayFromCast = Props.preCastTicks
			};
		}
	}

	private void SpawnAll(LocalTargetInfo target)
	{
		if (!Props.fleckDefs.NullOrEmpty())
		{
			for (int i = 0; i < Props.fleckDefs.Count; i++)
			{
				SpawnFleck(target, Props.fleckDefs[i]);
			}
		}
		else
		{
			SpawnFleck(target, Props.fleckDef);
		}
	}

	private void SpawnFleck(LocalTargetInfo target, FleckDef def)
	{
		if (target.HasThing && target.Thing.SpawnedOrAnyParentSpawned)
		{
			FleckMaker.AttachedOverlay(target.Thing, def, Vector3.zero, Props.scale);
		}
		else
		{
			FleckMaker.Static(target.Cell, parent.pawn.Map, def, Props.scale);
		}
	}
}
