using RimWorld;

namespace Verse;

public class Projectile_SpawnsPawnZeroAge : Projectile
{
	protected override void Impact(Thing hitThing, bool blockedByShield = false)
	{
		Map map = base.Map;
		base.Impact(hitThing, blockedByShield);
		IntVec3 loc = base.Position;
		if (def.projectile.tryAdjacentFreeSpaces && base.Position.GetFirstBuilding(map) != null)
		{
			foreach (IntVec3 item in GenAdjFast.AdjacentCells8Way(base.Position))
			{
				if (item.GetFirstBuilding(map) == null && item.Standable(map))
				{
					loc = item;
					break;
				}
			}
		}
		PawnKindDef spawnsPawnKind = def.projectile.spawnsPawnKind;
		Faction faction = base.Launcher.Faction;
		float? fixedChronologicalAge = 0f;
		float? fixedBiologicalAge = 0f;
		GenSpawn.Spawn(PawnGenerator.GeneratePawn(new PawnGenerationRequest(spawnsPawnKind, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge, fixedChronologicalAge)), loc, map);
	}
}
