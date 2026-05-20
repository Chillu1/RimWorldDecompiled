using RimWorld;

namespace Verse
{
	public abstract class AnimalPenEnclosureCalculator
	{
		private District rootDistrict;

		private bool isEnclosed;

		private readonly RegionProcessor regionProcessor;

		private readonly RegionEntryPredicate regionEntryPredicate;

		protected AnimalPenEnclosureCalculator()
		{
			regionProcessor = ProcessRegion;
			regionEntryPredicate = EnterRegion;
		}

		protected virtual void VisitDirectlyConnectedRegion(Region r)
		{
		}

		protected virtual void VisitIndirectlyDirectlyConnectedRegion(Region r)
		{
		}

		protected virtual void VisitPassableDoorway(Region r)
		{
		}

		protected virtual void VisitImpassableDoorway(Region r)
		{
		}

		protected bool VisitPen(IntVec3 position, Map map)
		{
			rootDistrict = position.GetDistrict(map);
			if (rootDistrict == null || rootDistrict.TouchesMapEdge)
			{
				return false;
			}
			isEnclosed = true;
			RegionTraverser.BreadthFirstTraverse(position, map, regionEntryPredicate, regionProcessor);
			return isEnclosed;
		}

		public static bool RoamerCanPass(Building_Door door)
		{
			if (!door.FreePassage)
			{
				return RoamerCanPass(door.def);
			}
			return true;
		}

		public static bool RoamerCanPass(ThingDef thingDef)
		{
			return thingDef.building.roamerCanOpen;
		}

		private bool EnterRegion(Region from, Region to)
		{
			if (from.IsDoorway && !RoamerCanPass(from.door))
			{
				return false;
			}
			if (to.type != RegionType.Normal)
			{
				return to.IsDoorway;
			}
			return true;
		}

		private bool ProcessRegion(Region reg)
		{
			if (reg.touchesMapEdge)
			{
				isEnclosed = false;
				return true;
			}
			if (reg.type == RegionType.Normal)
			{
				if (reg.District == rootDistrict)
				{
					VisitDirectlyConnectedRegion(reg);
				}
				else
				{
					VisitIndirectlyDirectlyConnectedRegion(reg);
				}
			}
			else if (reg.IsDoorway)
			{
				if (RoamerCanPass(reg.door))
				{
					VisitPassableDoorway(reg);
				}
				else
				{
					VisitImpassableDoorway(reg);
				}
			}
			return false;
		}
	}
}
