using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class RitualObligationTargetWorker_ChildBirth : RitualObligationTargetFilter
	{
		private static readonly List<Pawn> cachedFreeColonistsAndPrisoners = new List<Pawn>();

		private static int lastCacheTick = -1;

		public RitualObligationTargetWorker_ChildBirth()
		{
		}

		public RitualObligationTargetWorker_ChildBirth(RitualObligationTargetFilterDef def)
			: base(def)
		{
		}

		public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
		{
			foreach (Pawn item in ColonistsInLabor(map))
			{
				foreach (Building_Bed item2 in PregnancyUtility.BedsForBirth(item))
				{
					yield return item2;
				}
			}
		}

		protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
		{
			if (!target.HasThing)
			{
				return false;
			}
			if (!(target.Thing is Building_Bed building_Bed))
			{
				return false;
			}
			if (!building_Bed.def.building.bed_humanlike)
			{
				return false;
			}
			if (def.colonistThingsOnly && (target.Thing.Faction == null || !target.Thing.Faction.IsPlayer))
			{
				return false;
			}
			foreach (Pawn item in ColonistsInLabor(target.Map))
			{
				if (!(item.GetLord()?.LordJob is LordJob_Ritual_ChildBirth) && PregnancyUtility.IsUsableBedFor(item, item, building_Bed))
				{
					return true;
				}
			}
			return false;
		}

		protected IEnumerable<Pawn> ColonistsInLabor(Map map)
		{
			if (Find.TickManager.TicksGame != lastCacheTick)
			{
				lastCacheTick = Find.TickManager.TicksGame;
				cachedFreeColonistsAndPrisoners.Clear();
				cachedFreeColonistsAndPrisoners.AddRange(map.mapPawns.FreeColonistsAndPrisoners);
			}
			foreach (Pawn cachedFreeColonistsAndPrisoner in cachedFreeColonistsAndPrisoners)
			{
				if (cachedFreeColonistsAndPrisoner.health.hediffSet.HasHediff(HediffDefOf.PregnancyLabor))
				{
					yield return cachedFreeColonistsAndPrisoner;
				}
			}
		}

		public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
		{
			yield return "RitualTargetChildBirth".Translate();
		}
	}
}
