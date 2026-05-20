using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class CompUseEffect : ThingComp
{
	private const float CameraShakeMag = 1f;

	private Effecter effecter;

	public virtual float OrderPriority => 0f;

	private CompProperties_UseEffect Props => (CompProperties_UseEffect)props;

	public virtual void DoEffect(Pawn usedBy)
	{
		if (usedBy.Map == Find.CurrentMap)
		{
			if (Props.doCameraShake && usedBy.Spawned)
			{
				Find.CameraDriver.shaker.DoShake(1f);
			}
			if (Props.moteOnUsed != null)
			{
				MoteMaker.MakeAttachedOverlay(usedBy, Props.moteOnUsed, Vector3.zero, Props.moteOnUsedScale);
			}
			if (Props.fleckOnUsed != null)
			{
				FleckMaker.AttachedOverlay(usedBy, Props.fleckOnUsed, Vector3.zero, Props.fleckOnUsedScale);
			}
			if (Props.effecterOnUsed != null)
			{
				Props.effecterOnUsed.SpawnMaintained(usedBy, new TargetInfo(parent.Position, parent.Map));
			}
			effecter?.Cleanup();
		}
	}

	public virtual void PrepareTick()
	{
		if (Props.warmupEffecter != null)
		{
			if (effecter == null)
			{
				effecter = Props.warmupEffecter.Spawn(parent, parent.Map);
			}
			effecter?.EffectTick(parent, parent);
		}
	}

	public virtual TaggedString ConfirmMessage(Pawn p)
	{
		return null;
	}

	public virtual bool SelectedUseOption(Pawn p)
	{
		return false;
	}

	public virtual AcceptanceReport CanBeUsedBy(Pawn p)
	{
		return true;
	}
}
