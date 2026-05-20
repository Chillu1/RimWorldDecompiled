using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class ShipJob_FlyAway : ShipJob
{
	public PlanetTile destinationTile = PlanetTile.Invalid;

	public TransportersArrivalAction arrivalAction;

	public TransportShipDropMode dropMode = TransportShipDropMode.All;

	private bool initialized;

	protected override bool ShouldEnd => initialized;

	public override bool Interruptible => false;

	public override bool HasDestination => destinationTile.Valid;

	public override bool TryStart()
	{
		if (!base.TryStart())
		{
			return false;
		}
		if (!transportShip.ShipExistsAndIsSpawned)
		{
			return false;
		}
		if (!transportShip.ShuttleComp.AllRequiredThingsLoaded && dropMode != TransportShipDropMode.None && transportShip.TransporterComp.innerContainer.Any)
		{
			ShipJob_Unload shipJob_Unload = (ShipJob_Unload)ShipJobMaker.MakeShipJob(ShipJobDefOf.Unload);
			shipJob_Unload.dropMode = dropMode;
			transportShip.SetNextJob(shipJob_Unload);
			return false;
		}
		IntVec3 position = transportShip.shipThing.Position;
		Map map = transportShip.shipThing.Map;
		if (!transportShip.TransporterComp.LoadingInProgressOrReadyToLaunch)
		{
			TransporterUtility.InitiateLoading(Gen.YieldSingle(transportShip.TransporterComp));
		}
		transportShip.TransporterComp.TryRemoveLord(map);
		transportShip.ShuttleComp.SendLaunchedSignals();
		QuestUtility.SendQuestTargetSignals(transportShip.questTags, "FlewAway", transportShip.Named("SUBJECT"));
		ActiveTransporter activeTransporter = (ActiveTransporter)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
		activeTransporter.Contents = new ActiveTransporterInfo();
		activeTransporter.Contents.innerContainer.TryAddRangeOrTransfer(transportShip.TransporterComp.GetDirectlyHeldThings(), canMergeWithExistingStacks: true, destroyLeftover: true);
		activeTransporter.Rotation = transportShip.shipThing.Rotation;
		int groupID = transportShip.TransporterComp.groupID;
		if (!transportShip.shipThing.Destroyed)
		{
			transportShip.shipThing.DeSpawn();
		}
		FlyShipLeaving flyShipLeaving = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(transportShip.def.leavingSkyfaller, activeTransporter);
		flyShipLeaving.groupID = groupID;
		if (!destinationTile.Valid)
		{
			flyShipLeaving.createWorldObject = false;
			if (!transportShip.shipThing.Destroyed && !Find.QuestManager.IsReservedByAnyQuest(transportShip))
			{
				transportShip.shipThing.Destroy(DestroyMode.QuestLogic);
			}
		}
		else
		{
			flyShipLeaving.createWorldObject = true;
			flyShipLeaving.worldObjectDef = transportShip.def.worldObject;
			flyShipLeaving.destinationTile = destinationTile;
			flyShipLeaving.arrivalAction = arrivalAction;
		}
		if (transportShip.shipThing.HasComp<CompShuttle>())
		{
			activeTransporter.Contents.SetShuttle(transportShip.shipThing);
		}
		GenSpawn.Spawn(flyShipLeaving, position, map);
		initialized = true;
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref destinationTile, "destinationTile");
		Scribe_Values.Look(ref initialized, "initialized", defaultValue: false);
		Scribe_Values.Look(ref dropMode, "dropMode", TransportShipDropMode.None);
		Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
	}
}
