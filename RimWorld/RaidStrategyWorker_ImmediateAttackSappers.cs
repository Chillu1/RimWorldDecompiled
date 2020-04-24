using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RaidStrategyWorker_ImmediateAttackSappers : RaidStrategyWorker
	{
		public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (!PawnGenOptionsWithSappers(parms.faction, groupKind).Any())
			{
				return false;
			}
			if (!base.CanUseWith(parms, groupKind))
			{
				return false;
			}
			return true;
		}

		public override float MinimumPoints(Faction faction, PawnGroupKindDef groupKind)
		{
			return Mathf.Max(base.MinimumPoints(faction, groupKind), CheapestSapperCost(faction, groupKind));
		}

		public override float MinMaxAllowedPawnGenOptionCost(Faction faction, PawnGroupKindDef groupKind)
		{
			return CheapestSapperCost(faction, groupKind);
		}

		private float CheapestSapperCost(Faction faction, PawnGroupKindDef groupKind)
		{
			IEnumerable<PawnGroupMaker> enumerable = PawnGenOptionsWithSappers(faction, groupKind);
			if (!enumerable.Any())
			{
				Log.Error("Tried to get MinimumPoints for " + GetType().ToString() + " for faction " + faction.ToString() + " but the faction has no groups with sappers. groupKind=" + groupKind);
				return 99999f;
			}
			float num = 9999999f;
			foreach (PawnGroupMaker item in enumerable)
			{
				foreach (PawnGenOption item2 in item.options.Where((PawnGenOption op) => op.kind.canBeSapper))
				{
					if (item2.Cost < num)
					{
						num = item2.Cost;
					}
				}
			}
			return num;
		}

		public override bool CanUsePawnGenOption(PawnGenOption opt, List<PawnGenOption> chosenOpts)
		{
			if (chosenOpts.Count == 0 && !opt.kind.canBeSapper)
			{
				return false;
			}
			return true;
		}

		public override bool CanUsePawn(Pawn p, List<Pawn> otherPawns)
		{
			if (otherPawns.Count == 0 && !SappersUtility.IsGoodSapper(p) && !SappersUtility.IsGoodBackupSapper(p))
			{
				return false;
			}
			if (p.kindDef.canBeSapper && SappersUtility.HasBuildingDestroyerWeapon(p) && !SappersUtility.IsGoodSapper(p))
			{
				return false;
			}
			return true;
		}

		private IEnumerable<PawnGroupMaker> PawnGenOptionsWithSappers(Faction faction, PawnGroupKindDef groupKind)
		{
			if (faction.def.pawnGroupMakers == null)
			{
				return Enumerable.Empty<PawnGroupMaker>();
			}
			return faction.def.pawnGroupMakers.Where((PawnGroupMaker gm) => gm.kindDef == groupKind && gm.options != null && gm.options.Any((PawnGenOption op) => op.kind.canBeSapper));
		}

		protected override LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed)
		{
			return new LordJob_AssaultColony(parms.faction, canKidnap: true, canTimeoutOrFlee: true, sappers: true, useAvoidGridSmart: true);
		}
	}
}
