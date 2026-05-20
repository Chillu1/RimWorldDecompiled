using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_SinglePawn : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (rp.singlePawnToSpawn != null && rp.singlePawnToSpawn.Spawned)
		{
			return true;
		}
		if (!TryFindSpawnCell(rp, out var _))
		{
			return false;
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		if (rp.singlePawnToSpawn != null && rp.singlePawnToSpawn.Spawned)
		{
			return;
		}
		Map map = BaseGen.globalSettings.map;
		if (!TryFindSpawnCell(rp, out var cell))
		{
			if (rp.singlePawnToSpawn != null)
			{
				Find.WorldPawns.PassToWorld(rp.singlePawnToSpawn);
			}
			return;
		}
		Pawn pawn;
		if (rp.singlePawnToSpawn == null)
		{
			PawnGenerationRequest request;
			if (rp.singlePawnGenerationRequest.HasValue)
			{
				request = rp.singlePawnGenerationRequest.Value;
			}
			else
			{
				PawnKindDef pawnKindDef = rp.singlePawnKindDef ?? DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef x) => x.defaultFactionDef == null || !x.defaultFactionDef.isPlayer).RandomElement();
				Faction result = rp.faction;
				if (result == null && pawnKindDef.RaceProps.Humanlike)
				{
					if (pawnKindDef.defaultFactionDef != null)
					{
						result = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionDef);
						if (result == null)
						{
							return;
						}
					}
					else if (!Find.FactionManager.AllFactions.Where((Faction x) => !x.IsPlayer).TryRandomElement(out result))
					{
						return;
					}
				}
				request = new PawnGenerationRequest(pawnKindDef, result, PawnGenerationContext.NonPlayer, map.Tile, forceGenerateNewPawn: false, allowDead: false, result.deactivated);
			}
			pawn = PawnGenerator.GeneratePawn(request);
			if (rp.postThingGenerate != null)
			{
				rp.postThingGenerate(pawn);
			}
		}
		else
		{
			pawn = rp.singlePawnToSpawn;
		}
		if (!pawn.Dead && rp.disableSinglePawn.HasValue && rp.disableSinglePawn.Value)
		{
			pawn.mindState.Active = false;
		}
		GenSpawn.Spawn(pawn, cell, map);
		if (rp.singlePawnLord != null)
		{
			rp.singlePawnLord.AddPawn(pawn);
		}
		if (rp.postThingSpawn != null)
		{
			rp.postThingSpawn(pawn);
		}
	}

	public static bool TryFindSpawnCell(ResolveParams rp, out IntVec3 cell)
	{
		Map map = BaseGen.globalSettings.map;
		return CellFinder.TryFindRandomCellInsideWith(rp.rect, (IntVec3 x) => x.Standable(map) && (rp.singlePawnSpawnCellExtraPredicate == null || rp.singlePawnSpawnCellExtraPredicate(x)), out cell);
	}
}
