using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class GatheringWorker_Concert : GatheringWorker
{
	private static List<Building_MusicalInstrument> tmpInstruments = new List<Building_MusicalInstrument>();

	protected override LordJob CreateLordJob(IntVec3 spot, Pawn organizer)
	{
		return new LordJob_Joinable_Concert(spot, organizer, def);
	}

	protected override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
	{
		bool enjoyableOutside = JoyUtility.EnjoyableOutsideNow(organizer);
		_ = organizer.Map;
		IEnumerable<Building_MusicalInstrument> enumerable = organizer.Map.listerBuildings.AllBuildingsColonistOfClass<Building_MusicalInstrument>();
		try
		{
			int num = -1;
			foreach (Building_MusicalInstrument item in enumerable)
			{
				if (GatheringsUtility.ValidateGatheringSpot(item.InteractionCell, def, organizer, enjoyableOutside, ignoreRequiredColonistCount: false) && InstrumentAccessible(item, organizer))
				{
					float instrumentRange = item.def.building.instrumentRange;
					if ((float)num < instrumentRange)
					{
						tmpInstruments.Clear();
					}
					else if ((float)num > instrumentRange)
					{
						continue;
					}
					tmpInstruments.Add(item);
				}
			}
			if (!tmpInstruments.TryRandomElement(out var result))
			{
				spot = IntVec3.Invalid;
				return false;
			}
			spot = result.InteractionCell;
			return true;
		}
		finally
		{
			tmpInstruments.Clear();
		}
	}

	public static bool InstrumentAccessible(Building_MusicalInstrument i, Pawn p)
	{
		if (!i.IsBeingPlayed && p.CanReach(i.InteractionCell, PathEndMode.OnCell, p.NormalMaxDanger()))
		{
			return p.CanReserveSittableOrSpot(i.InteractionCell);
		}
		return false;
	}
}
