using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompSmokeCloudMaker : ThingComp
{
	private Effecter effecter;

	private static List<IntVec3> tmpValidCells = new List<IntVec3>();

	private CompProperties_SmokeCloudMaker Props => (CompProperties_SmokeCloudMaker)props;

	public override void CompTick()
	{
		if (!ModLister.CheckIdeology("Smoke cloud maker") || !parent.Spawned)
		{
			return;
		}
		CompRefuelable compRefuelable = parent.TryGetComp<CompRefuelable>();
		CompPowerTrader compPowerTrader = parent.TryGetComp<CompPowerTrader>();
		if ((compRefuelable?.HasFuel ?? true) & (compPowerTrader?.PowerOn ?? true))
		{
			if (effecter == null)
			{
				effecter = Props.sourceStreamEffect.Spawn();
				effecter.Trigger(parent, parent);
			}
			effecter.EffectTick(parent, parent);
			if (!Rand.MTBEventOccurs(Props.fleckSpawnMTB, 1f, 1f))
			{
				return;
			}
			tmpValidCells.Clear();
			Room room = parent.GetRoom();
			CellRect cellRect = parent.OccupiedRect();
			foreach (IntVec3 item in cellRect.ExpandedBy(Mathf.FloorToInt(Props.cloudRadius + 1f)))
			{
				if (cellRect.ClosestCellTo(item).DistanceTo(item) <= Props.cloudRadius && item.GetRoom(parent.Map) == room)
				{
					tmpValidCells.Add(item);
				}
			}
			if (tmpValidCells.Count > 0)
			{
				Vector3 loc = tmpValidCells.RandomElement().ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead);
				parent.Map.flecks.CreateFleck(FleckMaker.GetDataStatic(loc, parent.Map, Props.cloudFleck, Props.fleckScale));
			}
		}
		else
		{
			effecter?.Cleanup();
			effecter = null;
		}
	}

	public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
	{
		effecter?.Cleanup();
		effecter = null;
	}
}
