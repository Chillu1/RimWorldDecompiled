using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class ReloadableUtility
	{
		public static CompReloadable FindSomeReloadableComponent(Pawn pawn, bool allowForcedReload)
		{
			if (pawn.apparel == null)
			{
				return null;
			}
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				CompReloadable compReloadable = wornApparel[i].TryGetComp<CompReloadable>();
				if (compReloadable != null && compReloadable.NeedsReload(allowForcedReload))
				{
					return compReloadable;
				}
			}
			return null;
		}

		public static List<Thing> FindEnoughAmmo(Pawn pawn, IntVec3 rootCell, CompReloadable comp, bool forceReload)
		{
			if (comp == null)
			{
				return null;
			}
			IntRange desiredQuantity = new IntRange(comp.MinAmmoNeeded(forceReload), comp.MaxAmmoNeeded(forceReload));
			return RefuelWorkGiverUtility.FindEnoughReservableThings(pawn, rootCell, desiredQuantity, (Thing t) => t.def == comp.AmmoDef);
		}

		public static IEnumerable<Pair<CompReloadable, Thing>> FindPotentiallyReloadableGear(Pawn pawn, List<Thing> potentialAmmo)
		{
			if (pawn.apparel == null)
			{
				yield break;
			}
			List<Apparel> worn = pawn.apparel.WornApparel;
			for (int i = 0; i < worn.Count; i++)
			{
				CompReloadable comp = worn[i].TryGetComp<CompReloadable>();
				if (comp?.AmmoDef == null)
				{
					continue;
				}
				for (int j = 0; j < potentialAmmo.Count; j++)
				{
					Thing thing = potentialAmmo[j];
					if (thing.def == comp.Props.ammoDef)
					{
						yield return new Pair<CompReloadable, Thing>(comp, thing);
					}
				}
			}
		}

		public static Pawn WearerOf(CompReloadable comp)
		{
			return (comp.ParentHolder as Pawn_ApparelTracker)?.pawn;
		}

		public static int TotalChargesFromQueuedJobs(Pawn pawn, ThingWithComps gear)
		{
			CompReloadable compReloadable = gear.TryGetComp<CompReloadable>();
			int num = 0;
			if (compReloadable != null && pawn != null)
			{
				foreach (Job item in pawn.jobs.AllJobs())
				{
					Verb verbToUse = item.verbToUse;
					if (verbToUse != null && compReloadable == verbToUse.ReloadableCompSource)
					{
						num++;
					}
				}
				return num;
			}
			return num;
		}

		public static bool CanUseConsideringQueuedJobs(Pawn pawn, ThingWithComps gear, bool showMessage = true)
		{
			CompReloadable compReloadable = gear.TryGetComp<CompReloadable>();
			if (compReloadable == null)
			{
				return true;
			}
			string text = null;
			if (!Event.current.shift)
			{
				if (!compReloadable.CanBeUsed)
				{
					text = compReloadable.DisabledReason(compReloadable.MinAmmoNeeded(allowForcedReload: false), compReloadable.MaxAmmoNeeded(allowForcedReload: false));
				}
			}
			else if (TotalChargesFromQueuedJobs(pawn, gear) + 1 > compReloadable.RemainingCharges)
			{
				text = compReloadable.DisabledReason(compReloadable.MaxAmmoAmount(), compReloadable.MaxAmmoAmount());
			}
			if (text != null)
			{
				if (showMessage)
				{
					Messages.Message(text, pawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}
	}
}
