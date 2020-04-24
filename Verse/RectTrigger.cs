using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class RectTrigger : Thing
	{
		private CellRect rect;

		public bool destroyIfUnfogged;

		public bool activateOnExplosion;

		public string signalTag;

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

		public override void Tick()
		{
			if (destroyIfUnfogged && !rect.CenterCell.Fogged(base.Map))
			{
				Destroy();
			}
			else
			{
				if (!this.IsHashIntervalTick(60))
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
							if (thingList[k].def.category == ThingCategory.Pawn && thingList[k].def.race.intelligence == Intelligence.Humanlike && thingList[k].Faction == Faction.OfPlayer)
							{
								ActivatedBy((Pawn)thingList[k]);
								return;
							}
						}
					}
				}
			}
		}

		public void ActivatedBy(Pawn p)
		{
			Find.SignalManager.SendSignal(new Signal(signalTag, p.Named("SUBJECT")));
			if (!base.Destroyed)
			{
				Destroy();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref rect, "rect");
			Scribe_Values.Look(ref destroyIfUnfogged, "destroyIfUnfogged", defaultValue: false);
			Scribe_Values.Look(ref activateOnExplosion, "activateOnExplosion", defaultValue: false);
			Scribe_Values.Look(ref signalTag, "signalTag");
		}
	}
}
