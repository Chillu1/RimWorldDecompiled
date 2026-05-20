using System.Collections.Generic;

namespace Verse;

public class RadialTrigger : PawnTrigger
{
	public int maxRadius;

	public bool lineOfSight;

	protected override void Tick()
	{
		if (!this.IsHashIntervalTick(60))
		{
			return;
		}
		Map map = base.Map;
		int num = GenRadial.NumCellsInRadius(maxRadius);
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = base.Position + GenRadial.RadialPattern[i];
			if (!c.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (TriggeredBy(thingList[j]) && (!lineOfSight || GenSight.LineOfSightToThing(base.Position, thingList[j], map)))
				{
					ActivatedBy((Pawn)thingList[j]);
					return;
				}
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref maxRadius, "maxRadius", 0);
		Scribe_Values.Look(ref lineOfSight, "lineOfSight", defaultValue: false);
	}
}
