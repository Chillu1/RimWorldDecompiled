using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class ShipJob_Arrive : ShipJob
	{
		public IntVec3 cell = IntVec3.Invalid;

		public Pawn mapOfPawn;

		public MapParent mapParent;

		public Faction factionForArrival;

		protected override bool ShouldEnd
		{
			get
			{
				if (transportShip?.shipThing != null)
				{
					return transportShip.shipThing.Spawned;
				}
				return false;
			}
		}

		public override bool Interruptible => false;

		public override bool TryStart()
		{
			if (!base.TryStart())
			{
				Log.Error("Failed to start arrive ship job");
				return false;
			}
			MapParent mapParent = mapOfPawn?.MapHeld?.Parent ?? this.mapParent;
			if (mapParent is PocketMapParent pocketMapParent && pocketMapParent?.sourceMap?.Parent != null)
			{
				mapParent = pocketMapParent.sourceMap.Parent;
			}
			if (mapParent == null || !mapParent.HasMap)
			{
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				if (anyPlayerHomeMap == null)
				{
					Log.Error("Trying to start ShipJob_Arrive with a null map.");
					return false;
				}
				mapParent = anyPlayerHomeMap.Parent;
			}
			if (!cell.IsValid)
			{
				cell = DropCellFinder.GetBestShuttleLandingSpot(mapParent.Map, factionForArrival ?? Faction.OfPlayer);
			}
			ThingOwner innerContainer = transportShip.TransporterComp.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (innerContainer[i] is Pawn p && p.IsWorldPawn())
				{
					Find.WorldPawns.RemovePawn(p);
				}
			}
			GenSpawn.Spawn(SkyfallerMaker.MakeSkyfaller(transportShip.def.arrivingSkyfaller, transportShip.shipThing), cell, mapParent.Map);
			QuestUtility.SendQuestTargetSignals(transportShip.questTags, "Arrived", transportShip.Named("SUBJECT"));
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref cell, "cell");
			Scribe_References.Look(ref mapParent, "mapParent");
			Scribe_References.Look(ref mapOfPawn, "mapOfPawn");
			Scribe_References.Look(ref factionForArrival, "factionForArrival");
		}
	}
}
