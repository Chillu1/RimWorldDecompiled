using Verse;

namespace RimWorld;

public class RoomPart_GestationTanks : RoomPartWorker
{
	private static readonly FloatRange CountPer100Range = new FloatRange(3f, 7f);

	public new RoomPart_GestationTankDef def => (RoomPart_GestationTankDef)base.def;

	public RoomPart_GestationTanks(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		RoomGenUtility.FillWithPadding(ThingDefOf.AncientMechGestatorTank, CountPer100Range, room, map, Rot4.South, Validator, null, 1, null, alignWithRect: false, snapToGrid: true, SpawnTank);
		bool Validator(IntVec3 cell, Rot4 rot, CellRect rect)
		{
			return GenSpawn.CanSpawnAt(ThingDefOf.AncientMechGestatorTank, cell, map, rot, canWipeEdifices: false);
		}
	}

	private Thing SpawnTank(IntVec3 cell, Rot4 rot, Map map)
	{
		RoomPart_GestationTankDef.State state = def.options.RandomElementByWeight((RoomPart_GestationTankDef.TankOption value) => value.weight).state;
		ThingWithComps obj = (ThingWithComps)GenSpawn.Spawn(ThingDefOf.AncientMechGestatorTank, cell, map, rot);
		obj.GetComp<CompMechGestatorTank>().State = state switch
		{
			RoomPart_GestationTankDef.State.Proximity => CompMechGestatorTank.TankState.Proximity, 
			RoomPart_GestationTankDef.State.Dormant => CompMechGestatorTank.TankState.Dormant, 
			_ => CompMechGestatorTank.TankState.Empty, 
		};
		return obj;
	}
}
