using UnityEngine;
using Verse;

namespace RimWorld;

public class CompNoctolEyes : ThingComp
{
	private Mote mote;

	private Hediff hediff;

	private Pawn parentPawn;

	public CompProperties_NoctolEyes Props => (CompProperties_NoctolEyes)props;

	public Pawn Parent
	{
		get
		{
			if (parentPawn == null)
			{
				parentPawn = parent as Pawn;
			}
			return parentPawn;
		}
	}

	public float EyeBrightness
	{
		get
		{
			if (!Parent.Downed)
			{
				return 1f - hediff.Severity;
			}
			return 0f;
		}
	}

	public override void CompTick()
	{
		base.CompTick();
		if (parent.Spawned)
		{
			if (mote == null || mote.Destroyed)
			{
				mote = MoteMaker.MakeAttachedOverlay(parent, Props.moteDef, Vector3.zero);
				mote.link1.rotateWithTarget = true;
			}
			if (hediff == null)
			{
				Parent.health.hediffSet.TryGetHediff(HediffDefOf.LightExposure, out hediff);
			}
			mote.instanceColor = new Color(1f, 1f, 1f, EyeBrightness);
			mote.Maintain();
		}
	}
}
