using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class ShipJob_Unload : ShipJob
{
	public TransportShipDropMode dropMode = TransportShipDropMode.All;

	private List<Thing> droppedThings = new List<Thing>();

	public bool unforbidAll = true;

	private const int DropInterval = 60;

	protected override bool ShouldEnd => dropMode == TransportShipDropMode.None;

	public override bool ShowGizmos => false;

	private Thing ShipThing => transportShip.shipThing;

	public override bool TryStart()
	{
		if (!transportShip.ShipExistsAndIsSpawned)
		{
			return false;
		}
		return base.TryStart();
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (transportShip.ShipExistsAndIsSpawned && ShipThing.IsHashIntervalTick(60, delta))
		{
			Drop();
		}
	}

	private void Drop()
	{
		Thing thing = null;
		float num = 0f;
		Map map = ShipThing.Map;
		for (int i = 0; i < transportShip.TransporterComp.innerContainer.Count; i++)
		{
			Thing thing2 = transportShip.TransporterComp.innerContainer[i];
			float dropPriority = GetDropPriority(thing2);
			if (dropPriority > num)
			{
				thing = thing2;
				num = dropPriority;
			}
		}
		if (thing != null && (thing is Pawn || dropMode != TransportShipDropMode.PawnsOnly))
		{
			UnloadThingFromShuttle(transportShip, thing, droppedThings, unforbidAll);
			return;
		}
		transportShip.TransporterComp.TryRemoveLord(map);
		End();
	}

	public static void UnloadThingFromShuttle(TransportShip ship, Thing thingToDrop, List<Thing> droppedThings = null, bool unforbidAll = false)
	{
		Thing shipThing = ship.shipThing;
		if (shipThing == null || !shipThing.Spawned)
		{
			Log.Error($"Tried to unload {thingToDrop} from unspawned shuttle");
			return;
		}
		Map map = ship.shipThing.Map;
		IntVec3 interactionCell = ship.shipThing.InteractionCell;
		if (!ship.TransporterComp.innerContainer.TryDrop(thingToDrop, interactionCell, map, ThingPlaceMode.Near, out var _, null, delegate(IntVec3 c)
		{
			if (c.Fogged(map))
			{
				return false;
			}
			return (!(thingToDrop is Pawn { Downed: not false }) || c.GetFirstPawn(map) == null) ? true : false;
		}, !(thingToDrop is Pawn)))
		{
			return;
		}
		ship.TransporterComp.Notify_ThingRemoved(thingToDrop);
		droppedThings?.Add(thingToDrop);
		if (unforbidAll)
		{
			thingToDrop.SetForbidden(value: false, warnOnFail: false);
		}
		if (thingToDrop is Pawn pawn)
		{
			if (pawn.IsColonist && pawn.Spawned && !map.IsPlayerHome)
			{
				pawn.drafter.Drafted = true;
			}
			if (pawn.guest != null && pawn.guest.IsPrisoner)
			{
				pawn.guest.WaitInsteadOfEscapingForDefaultTicks();
			}
			if (pawn.IsColonist && map.IsPlayerHome)
			{
				pawn.inventory.UnloadEverything = true;
			}
		}
	}

	private float GetDropPriority(Thing t)
	{
		if (t is Pawn p)
		{
			if (droppedThings.Contains(t))
			{
				return 0f;
			}
			if (dropMode == TransportShipDropMode.NonRequired && transportShip.ShuttleComp.IsRequired(t))
			{
				return 0f;
			}
			Lord lord = p.GetLord();
			if (lord?.CurLordToil != null && lord.CurLordToil is LordToil_EnterShuttleOrLeave lordToil_EnterShuttleOrLeave && lordToil_EnterShuttleOrLeave.shuttle == ShipThing)
			{
				return 0f;
			}
			if (!p.AnimalOrWildMan())
			{
				return 1f;
			}
			return 0.5f;
		}
		return 0.25f;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref droppedThings, "droppedThings", LookMode.Reference);
		Scribe_Values.Look(ref dropMode, "dropMode", TransportShipDropMode.None);
		Scribe_Values.Look(ref unforbidAll, "unforbidAll", defaultValue: true);
	}
}
