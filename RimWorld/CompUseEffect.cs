using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class CompUseEffect : ThingComp
	{
		private const float CameraShakeMag = 1f;

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
			}
		}

		public virtual bool SelectedUseOption(Pawn p)
		{
			return false;
		}

		public virtual bool CanBeUsedBy(Pawn p, out string failReason)
		{
			failReason = null;
			return true;
		}
	}
}
