using System;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class CompSpawnPawnOnDestroyed : ThingComp
{
	private const int InitialJumpDistance = 5;

	protected virtual bool JoinLord => true;

	private CompProperties_SpawnPawnOnDestroyed Props => (CompProperties_SpawnPawnOnDestroyed)props;

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		if (mode != DestroyMode.KillFinalize)
		{
			return;
		}
		PawnKindDef pawnKind = Props.pawnKind;
		Faction faction = parent.Faction;
		float? fixedBiologicalAge = 0f;
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge));
		GenSpawn.Spawn(pawn, parent.Position, previousMap, WipeMode.VanishOrMoveAside);
		if (CellFinder.TryFindRandomSpawnCellForPawnNear(parent.Position, previousMap, out var result, 5, (IntVec3 c) => GenSight.LineOfSight(parent.Position, c, previousMap, skipFirstCell: true)))
		{
			pawn.rotationTracker.FaceCell(result);
			PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer_Stun, pawn, result, null, null);
			if (pawnFlyer != null)
			{
				GenSpawn.Spawn(pawnFlyer, result, previousMap);
			}
		}
		if (JoinLord && Props.lordJob != null)
		{
			Lord lord = CompSpawnerPawn.FindLordToJoin(parent, Props.lordJob, shouldTryJoinParentLord: false);
			if (lord == null)
			{
				lord = LordMaker.MakeNewLord(parent.Faction, Activator.CreateInstance(Props.lordJob) as LordJob, previousMap);
			}
			lord.AddPawn(pawn);
		}
		pawn.TryGetComp<CompCanBeDormant>()?.WakeUp();
	}
}
