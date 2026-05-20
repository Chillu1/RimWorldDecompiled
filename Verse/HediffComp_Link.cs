using RimWorld;

namespace Verse;

public class HediffComp_Link : HediffComp
{
	public Thing other;

	private MoteDualAttached mote;

	public bool drawConnection;

	public HediffCompProperties_Link Props => (HediffCompProperties_Link)props;

	public Pawn OtherPawn => (Pawn)other;

	public override bool CompShouldRemove
	{
		get
		{
			if (base.CompShouldRemove)
			{
				return true;
			}
			if (other == null || !parent.pawn.SpawnedOrAnyParentSpawned || !other.SpawnedOrAnyParentSpawned)
			{
				return true;
			}
			if (Props.maxDistance > 0f && !parent.pawn.PositionHeld.InHorDistOf(other.PositionHeld, Props.maxDistance))
			{
				return true;
			}
			if (Props.requireLinkOnOtherPawn)
			{
				if (!(other is Pawn pawn))
				{
					Log.Error("HediffComp_Link requires link on other pawn, but other thing is not a pawn!");
				}
				else
				{
					foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
					{
						if (hediff is HediffWithComps hediffWithComps && hediffWithComps.comps.FirstOrDefault((HediffComp c) => c is HediffComp_Link hediffComp_Link && hediffComp_Link.other == parent.pawn && hediffComp_Link.parent.def == parent.def) != null)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}
	}

	public override string CompLabelInBracketsExtra
	{
		get
		{
			if (!Props.showName || other == null)
			{
				return null;
			}
			return other.LabelShort;
		}
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (drawConnection && other.MapHeld == parent.pawn.MapHeld)
		{
			ThingDef moteDef = Props.customMote ?? ThingDefOf.Mote_PsychicLinkLine;
			if (mote == null)
			{
				mote = MoteMaker.MakeInteractionOverlay(moteDef, parent.pawn, other);
			}
			mote.Maintain();
		}
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_References.Look(ref other, "other");
		Scribe_Values.Look(ref drawConnection, "drawConnection", defaultValue: false);
	}
}
