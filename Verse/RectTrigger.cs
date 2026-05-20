using System.Collections.Generic;

namespace Verse;

public class RectTrigger : PawnTrigger
{
	private CellRect rect;

	public bool destroyIfUnfogged;

	public bool activateOnExplosion;

	public CellRect Rect
	{
		get
		{
			return rect;
		}
		set
		{
			rect = value;
			if (base.Spawned)
			{
				rect.ClipInsideMap(base.Map);
			}
		}
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		rect.ClipInsideMap(base.Map);
	}

	protected override void Tick()
	{
		if (destroyIfUnfogged && !rect.CenterCell.Fogged(base.Map))
		{
			Destroy();
		}
		else
		{
			if (!this.IsHashIntervalTick(60) || base.Destroyed)
			{
				return;
			}
			Map map = base.Map;
			for (int i = rect.minZ; i <= rect.maxZ; i++)
			{
				for (int j = rect.minX; j <= rect.maxX; j++)
				{
					List<Thing> thingList = new IntVec3(j, 0, i).GetThingList(map);
					for (int k = 0; k < thingList.Count; k++)
					{
						if (TriggeredBy(thingList[k]))
						{
							ActivatedBy((Pawn)thingList[k]);
							return;
						}
					}
				}
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref rect, "rect");
		Scribe_Values.Look(ref destroyIfUnfogged, "destroyIfUnfogged", defaultValue: false);
		Scribe_Values.Look(ref activateOnExplosion, "activateOnExplosion", defaultValue: false);
	}
}
