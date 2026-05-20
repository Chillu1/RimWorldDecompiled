using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public class CompAbilityEffect_MoteOnTarget : CompAbilityEffect
{
	public new CompProperties_AbilityMoteOnTarget Props => (CompProperties_AbilityMoteOnTarget)props;

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
		if (!Props.moteDefs.NullOrEmpty())
		{
			for (int i = 0; i < Props.moteDefs.Count; i++)
			{
				SpawnMote(target, Props.moteDefs[i]);
			}
		}
		else
		{
			SpawnMote(target, Props.moteDef);
		}
	}

	private void SpawnMote(LocalTargetInfo target, ThingDef def)
	{
		if (target.HasThing)
		{
			MoteMaker.MakeAttachedOverlay(target.Thing, def, Vector3.zero, Props.scale);
		}
		else
		{
			MoteMaker.MakeStaticMote(target.Cell, parent.pawn.Map, def, Props.scale);
		}
	}
}
