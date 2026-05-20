using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompSpawnPawnsOnDamaged : ThingComp
{
	private static readonly IntRange BloodFilthCountRange = new IntRange(1, 2);

	private const int InitialJumpDistance = 5;

	private int lastSpawned = -99999;

	private static List<IntVec3> tmpTakenCells = new List<IntVec3>();

	public CompProperties_SpawnPawnsOnDamaged Props => (CompProperties_SpawnPawnsOnDamaged)props;

	public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
		if (ModLister.CheckAnomaly("Pawn spawning pawns on hit") && Rand.Chance(Props.chance) && Find.TickManager.TicksGame >= lastSpawned + Props.cooldownTicks)
		{
			int randomInRange = Props.spawnPawnCountRange.RandomInRange;
			for (int i = 0; i < randomInRange; i++)
			{
				PawnKindDef kind = Props.spawnPawnKindOptions.RandomElement();
				Faction faction = parent.Faction;
				float? fixedBiologicalAge = 0f;
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge));
				SpawnPawn(pawn, parent.PositionHeld, parent.MapHeld);
			}
			tmpTakenCells.Clear();
			FleshbeastUtility.MeatSplatter(BloodFilthCountRange.RandomInRange, parent.PositionHeld, parent.MapHeld);
			FilthMaker.TryMakeFilth(parent.PositionHeld, parent.MapHeld, ThingDefOf.Filth_TwistedFlesh);
			lastSpawned = Find.TickManager.TicksGame;
		}
	}

	private void SpawnPawn(Pawn pawn, IntVec3 position, Map map)
	{
		GenSpawn.Spawn(pawn, position, map, WipeMode.VanishOrMoveAside);
		if (CellFinder.TryFindRandomSpawnCellForPawnNear(position, map, out var result, 5, (IntVec3 c) => !tmpTakenCells.Contains(c) && GenSight.LineOfSight(position, c, map, skipFirstCell: true)))
		{
			pawn.rotationTracker.FaceCell(result);
			tmpTakenCells.Add(result);
			PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer_Stun, pawn, result, null, null);
			if (pawnFlyer != null)
			{
				GenSpawn.Spawn(pawnFlyer, result, map);
			}
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref lastSpawned, "lastSpawned", -99999);
	}
}
