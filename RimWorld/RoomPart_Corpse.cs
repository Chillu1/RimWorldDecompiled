using Verse;

namespace RimWorld;

public class RoomPart_Corpse : RoomPartWorker
{
	private static readonly IntRange Count = IntRange.One;

	public RoomPart_Corpse(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		PawnKindDef villager = PawnKindDefOf.Villager;
		faction?.RandomPawnKind();
		RoomGenUtility.SpawnCorpses(room, map, Count, villager, DamageDefOf.ExecutionCut, null, ThingDefOf.MeleeWeapon_Knife.tools[1]);
	}
}
