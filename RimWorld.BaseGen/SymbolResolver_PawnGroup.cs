using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_PawnGroup : SymbolResolver
	{
		private const float DefaultPoints = 250f;

		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			if (!rp.rect.Cells.Where((IntVec3 x) => x.Standable(BaseGen.globalSettings.map)).Any())
			{
				return false;
			}
			return true;
		}

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			PawnGroupMakerParms pawnGroupMakerParms = rp.pawnGroupMakerParams;
			if (pawnGroupMakerParms == null)
			{
				pawnGroupMakerParms = new PawnGroupMakerParms();
				pawnGroupMakerParms.tile = map.Tile;
				pawnGroupMakerParms.faction = Find.FactionManager.RandomEnemyFaction();
				pawnGroupMakerParms.points = 250f;
			}
			pawnGroupMakerParms.groupKind = rp.pawnGroupKindDef ?? PawnGroupKindDefOf.Combat;
			List<PawnKindDef> list = new List<PawnKindDef>();
			foreach (Pawn item in PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms))
			{
				list.Add(item.kindDef);
				ResolveParams resolveParams = rp;
				resolveParams.singlePawnToSpawn = item;
				BaseGen.symbolStack.Push("pawn", resolveParams);
			}
		}
	}
}
