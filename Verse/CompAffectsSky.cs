using UnityEngine;

namespace Verse;

public class CompAffectsSky : ThingComp
{
	private int autoAnimationStartTick;

	private int fadeInDuration;

	private int holdDuration;

	private int fadeOutDuration;

	private float autoAnimationTarget;

	public CompProperties_AffectsSky Props => (CompProperties_AffectsSky)props;

	public virtual float LerpFactor
	{
		get
		{
			if (HasAutoAnimation)
			{
				int ticksGame = Find.TickManager.TicksGame;
				float num = ((ticksGame < autoAnimationStartTick + fadeInDuration) ? ((float)(ticksGame - autoAnimationStartTick) / (float)fadeInDuration) : ((ticksGame >= autoAnimationStartTick + fadeInDuration + holdDuration) ? (1f - (float)(ticksGame - autoAnimationStartTick - fadeInDuration - holdDuration) / (float)fadeOutDuration) : 1f));
				return Mathf.Clamp01(num * autoAnimationTarget);
			}
			return 0f;
		}
	}

	public bool HasAutoAnimation => Find.TickManager.TicksGame < autoAnimationStartTick + fadeInDuration + holdDuration + fadeOutDuration;

	public virtual SkyTarget SkyTarget => new SkyTarget(Props.glow, Props.skyColors, Props.lightsourceShineSize, Props.lightsourceShineIntensity);

	public virtual Vector2? OverrideShadowVector => null;

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref autoAnimationStartTick, "autoAnimationStartTick", 0);
		Scribe_Values.Look(ref fadeInDuration, "fadeInDuration", 0);
		Scribe_Values.Look(ref holdDuration, "holdDuration", 0);
		Scribe_Values.Look(ref fadeOutDuration, "fadeOutDuration", 0);
		Scribe_Values.Look(ref autoAnimationTarget, "autoAnimationTarget", 0f);
	}

	public void StartFadeInHoldFadeOut(int fadeInDuration, int holdDuration, int fadeOutDuration, float target = 1f)
	{
		autoAnimationStartTick = Find.TickManager.TicksGame;
		this.fadeInDuration = fadeInDuration;
		this.holdDuration = holdDuration;
		this.fadeOutDuration = fadeOutDuration;
		autoAnimationTarget = target;
	}
}
