using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class TransportersArrivalAction_LandInSpecificCell : TransportersArrivalAction
{
	private MapParent mapParent;

	private IntVec3 cell;

	private Rot4 rotation;

	private bool landInShuttle;

	public override bool GeneratesMap => false;

	public TransportersArrivalAction_LandInSpecificCell()
	{
	}

	public TransportersArrivalAction_LandInSpecificCell(MapParent mapParent, IntVec3 cell)
	{
		this.mapParent = mapParent;
		this.cell = cell;
	}

	public TransportersArrivalAction_LandInSpecificCell(MapParent mapParent, IntVec3 cell, Rot4 rotation, bool landInShuttle)
	{
		this.mapParent = mapParent;
		this.cell = cell;
		this.rotation = rotation;
		this.landInShuttle = landInShuttle;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref cell, "cell");
		Scribe_Values.Look(ref rotation, "rotation");
		Scribe_Values.Look(ref landInShuttle, "landInShuttle", defaultValue: false);
	}

	public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
		if (!floatMenuAcceptanceReport)
		{
			return floatMenuAcceptanceReport;
		}
		if (mapParent != null && mapParent.Tile != destinationTile)
		{
			return false;
		}
		return CanLandInSpecificCell(pods, mapParent);
	}

	public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
	{
		Thing lookTarget = TransportersArrivalActionUtility.GetLookTarget(transporters);
		if (landInShuttle)
		{
			if (transporters.Count > 1)
			{
				Log.Error("Shuttles can only have one transporter in group");
			}
			TransportersArrivalActionUtility.DropShuttle(transporters.FirstOrDefault(), mapParent.Map, cell, rotation);
			Messages.Message("MessageShuttleArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
		}
		else
		{
			TransportersArrivalActionUtility.DropTravellingDropPods(transporters, cell, mapParent.Map);
			Messages.Message("MessageTransportPodsArrived".Translate(), lookTarget, MessageTypeDefOf.TaskCompletion);
		}
	}

	public static bool CanLandInSpecificCell(IEnumerable<IThingHolder> pods, MapParent mapParent)
	{
		if (mapParent == null || !mapParent.Spawned || !mapParent.HasMap)
		{
			return false;
		}
		if (mapParent.EnterCooldownBlocksEntering())
		{
			return FloatMenuAcceptanceReport.WithFailMessage("MessageEnterCooldownBlocksEntering".Translate(mapParent.EnterCooldownTicksLeft().ToStringTicksToPeriod()));
		}
		return true;
	}
}
