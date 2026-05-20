using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class RaidStrategyWorker_WithRequiredPawnKinds : RaidStrategyWorker
	{
		protected abstract bool MatchesRequiredPawnKind(PawnKindDef kind);

		protected abstract int MinRequiredPawnsForPoints(float pointsTotal, Faction faction = null);

		public override bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (!PawnGenOptionsWithRequiredPawns(parms.faction, groupKind).Any())
			{
				return false;
			}
			if (!base.CanUseWith(parms, groupKind))
			{
				return false;
			}
			return true;
		}

		public bool CanUseWithGroupMaker(PawnGroupMaker groupMaker)
		{
			if (groupMaker.options != null)
			{
				return groupMaker.options.Any((PawnGenOption o) => MatchesRequiredPawnKind(o.kind));
			}
			return false;
		}

		public override float MinimumPoints(Faction faction, PawnGroupKindDef groupKind)
		{
			return Mathf.Max(base.MinimumPoints(faction, groupKind), CheapestRequiredPawnCost(faction, groupKind));
		}

		public override float MinMaxAllowedPawnGenOptionCost(Faction faction, PawnGroupKindDef groupKind)
		{
			return CheapestRequiredPawnCost(faction, groupKind);
		}

		private float CheapestRequiredPawnCost(Faction faction, PawnGroupKindDef groupKind)
		{
			IEnumerable<PawnGroupMaker> enumerable = PawnGenOptionsWithRequiredPawns(faction, groupKind);
			if (!enumerable.Any())
			{
				Log.Error("Tried to get MinimumPoints for " + GetType().ToString() + " for faction " + faction.ToString() + " but the faction has no groups with the required pawn kind. groupKind=" + groupKind);
				return 99999f;
			}
			float num = 9999999f;
			foreach (PawnGroupMaker item in enumerable)
			{
				foreach (PawnGenOption item2 in item.options.Where((PawnGenOption op) => MatchesRequiredPawnKind(op.kind)))
				{
					if (item2.Cost < num)
					{
						num = item2.Cost;
					}
				}
			}
			return num;
		}

		public override bool CanUsePawnGenOption(float pointsTotal, PawnGenOption opt, List<PawnGenOptionWithXenotype> chosenOpts, Faction faction = null)
		{
			if (chosenOpts != null && chosenOpts.Count < MinRequiredPawnsForPoints(pointsTotal, faction) && !MatchesRequiredPawnKind(opt.kind))
			{
				return false;
			}
			return base.CanUsePawnGenOption(pointsTotal, opt, chosenOpts, faction);
		}

		private IEnumerable<PawnGroupMaker> PawnGenOptionsWithRequiredPawns(Faction faction, PawnGroupKindDef groupKind)
		{
			if (faction.def.pawnGroupMakers == null)
			{
				return Enumerable.Empty<PawnGroupMaker>();
			}
			return faction.def.pawnGroupMakers.Where((PawnGroupMaker gm) => gm.kindDef == groupKind && gm.options != null && gm.options.Any((PawnGenOption op) => MatchesRequiredPawnKind(op.kind)));
		}
	}
}
