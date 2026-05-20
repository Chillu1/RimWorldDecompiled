using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProximityLetter : ThingComp
{
	public bool letterSent;

	public CompProperties_ProximityLetter Props => (CompProperties_ProximityLetter)props;

	public void SendLetter(Pawn triggerer)
	{
		Find.LetterStack.ReceiveLetter(Props.letterLabel.Formatted(triggerer.Named("PAWN")), Props.letterText.Formatted(triggerer.Named("PAWN")), Props.letterDef, parent);
		letterSent = true;
	}

	public override void CompTick()
	{
		base.CompTick();
		if (letterSent || Find.Anomaly.Level > 0 || !parent.IsHashIntervalTick(60))
		{
			return;
		}
		Map map = parent.Map;
		int num = GenRadial.NumCellsInRadius(Props.radius);
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = parent.Position + GenRadial.RadialPattern[i];
			if (!c.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (thingList[j] is Pawn { IsColonistPlayerControlled: not false } pawn && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight) && pawn.Awake() && GenSight.LineOfSightToThing(pawn.Position, parent, parent.Map))
				{
					SendLetter(pawn);
					return;
				}
			}
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref letterSent, "letterSent", defaultValue: false);
	}
}
