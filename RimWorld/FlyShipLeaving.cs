using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class FlyShipLeaving : Skyfaller, IActiveTransporter, IThingHolder
{
	public int groupID = -1;

	public PlanetTile destinationTile = PlanetTile.Invalid;

	public TransportersArrivalAction arrivalAction;

	public bool createWorldObject = true;

	public WorldObjectDef worldObjectDef;

	private bool alreadyLeft;

	private static List<Thing> tmpActiveTransporters = new List<Thing>();

	public ActiveTransporterInfo Contents
	{
		get
		{
			return ((ActiveTransporter)innerContainer[0]).Contents;
		}
		set
		{
			((ActiveTransporter)innerContainer[0]).Contents = value;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref groupID, "groupID", 0);
		Scribe_Values.Look(ref destinationTile, "destinationTile");
		Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
		Scribe_Values.Look(ref alreadyLeft, "alreadyLeft", defaultValue: false);
		Scribe_Values.Look(ref createWorldObject, "createWorldObject", defaultValue: true);
		Scribe_Defs.Look(ref worldObjectDef, "worldObjectDef");
	}

	protected override void LeaveMap()
	{
		if (alreadyLeft || !createWorldObject)
		{
			if (Contents != null)
			{
				foreach (Thing item in (IEnumerable<Thing>)Contents.innerContainer)
				{
					if (item is Pawn pawn)
					{
						pawn.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
					}
				}
				Contents.innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.QuestLogic);
			}
			base.LeaveMap();
			return;
		}
		if (groupID < 0)
		{
			Log.Error($"Drop pod left the map, but its group ID is {groupID}");
			Destroy();
			return;
		}
		if (!destinationTile.Valid)
		{
			Log.Error($"Drop pod left the map, but its destination tile is {destinationTile}");
			Destroy();
			return;
		}
		Lord lord = TransporterUtility.FindLord(groupID, base.Map);
		if (lord != null)
		{
			base.Map.lordManager.RemoveLord(lord);
		}
		TravellingTransporters travellingTransporters = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(worldObjectDef ?? WorldObjectDefOf.TravellingTransporters);
		travellingTransporters.SetFaction(Faction.OfPlayer);
		travellingTransporters.destinationTile = destinationTile;
		travellingTransporters.arrivalAction = arrivalAction;
		PlanetTile planetTile = base.Map.Tile;
		if (planetTile.Layer != destinationTile.Layer)
		{
			planetTile = destinationTile.Layer.GetClosestTile_NewTemp(planetTile);
		}
		travellingTransporters.Tile = planetTile;
		Find.WorldObjects.Add(travellingTransporters);
		tmpActiveTransporters.Clear();
		tmpActiveTransporters.AddRange(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveTransporter));
		for (int i = 0; i < tmpActiveTransporters.Count; i++)
		{
			if (tmpActiveTransporters[i] is FlyShipLeaving flyShipLeaving && flyShipLeaving.groupID == groupID)
			{
				flyShipLeaving.alreadyLeft = true;
				travellingTransporters.AddTransporter(flyShipLeaving.Contents, justLeftTheMap: true);
				flyShipLeaving.Contents = null;
				flyShipLeaving.Destroy();
			}
		}
	}
}
