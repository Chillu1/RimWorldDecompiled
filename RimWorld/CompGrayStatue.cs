using Verse;

namespace RimWorld;

public abstract class CompGrayStatue : ThingComp
{
	private float triggerRadius;

	private bool triggered;

	private CompProperties_GrayStatue Props => (CompProperties_GrayStatue)props;

	public override void PostPostMake()
	{
		triggerRadius = Props.triggerRadiusRange.RandomInRange;
	}

	public override string CompInspectStringExtra()
	{
		return (triggered ? "Activated" : "ActivatedByProximity").Translate().CapitalizeFirst();
	}

	public override void CompTick()
	{
		if (!triggered && parent.IsHashIntervalTick(250))
		{
			CheckTrigger();
		}
	}

	private void CheckTrigger()
	{
		if (!parent.Spawned)
		{
			return;
		}
		foreach (IntVec3 item in GenRadial.RadialCellsAround(parent.Position, triggerRadius, useCenter: false))
		{
			if (!item.InBounds(parent.Map) || !GenSight.LineOfSight(parent.Position, item, parent.Map))
			{
				continue;
			}
			foreach (Thing thing in item.GetThingList(parent.Map))
			{
				if (thing is Pawn { IsColonist: not false } pawn)
				{
					triggered = true;
					Trigger(pawn);
					TaggedString label = Props.letterLabel.Formatted(pawn.Named("PAWN"));
					TaggedString text = Props.letterText.Formatted(pawn.Named("PAWN"));
					Find.LetterStack.ReceiveLetter(label, text, Props.letterDef ?? LetterDefOf.ThreatBig, pawn);
					return;
				}
			}
		}
	}

	protected abstract void Trigger(Pawn target);

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref triggerRadius, "triggerRadius", 0f);
		Scribe_Values.Look(ref triggered, "triggered", defaultValue: false);
	}
}
