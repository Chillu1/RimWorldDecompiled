using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class TransferableUtility
	{
		private static List<Thing> tmpThings = new List<Thing>();

		public static void Transfer(List<Thing> things, int count, Action<Thing, IThingHolder> transferred)
		{
			if (count <= 0)
			{
				return;
			}
			tmpThings.Clear();
			tmpThings.AddRange(things);
			int num = count;
			for (int i = 0; i < tmpThings.Count; i++)
			{
				Thing thing = tmpThings[i];
				int num2 = Mathf.Min(num, thing.stackCount);
				if (num2 > 0)
				{
					IThingHolder parentHolder = thing.ParentHolder;
					Thing thing2 = thing.SplitOff(num2);
					num -= num2;
					if (thing2 == thing)
					{
						things.Remove(thing);
					}
					transferred(thing2, parentHolder);
					if (num <= 0)
					{
						break;
					}
				}
			}
			tmpThings.Clear();
			if (num > 0)
			{
				Log.Error("Can't transfer things because there is nothing left.");
			}
		}

		public static void TransferNoSplit(List<Thing> things, int count, Action<Thing, int> transfer, bool removeIfTakingEntireThing = true, bool errorIfNotEnoughThings = true)
		{
			if (count <= 0)
			{
				return;
			}
			tmpThings.Clear();
			tmpThings.AddRange(things);
			int num = count;
			for (int i = 0; i < tmpThings.Count; i++)
			{
				Thing thing = tmpThings[i];
				int num2 = Mathf.Min(num, thing.stackCount);
				if (num2 > 0)
				{
					num -= num2;
					if (removeIfTakingEntireThing && num2 >= thing.stackCount)
					{
						things.Remove(thing);
					}
					transfer(thing, num2);
					if (num <= 0)
					{
						break;
					}
				}
			}
			tmpThings.Clear();
			if (num > 0 && errorIfNotEnoughThings)
			{
				Log.Error("Can't transfer things because there is nothing left.");
			}
		}

		public static bool TransferAsOne(Thing a, Thing b, TransferAsOneMode mode)
		{
			if (a == b)
			{
				return true;
			}
			if (a.def != b.def)
			{
				return false;
			}
			a = a.GetInnerIfMinified();
			b = b.GetInnerIfMinified();
			if (a.def.tradeNeverStack || b.def.tradeNeverStack)
			{
				return false;
			}
			if (!CanStack(a) || !CanStack(b))
			{
				return false;
			}
			if (a.def != b.def || a.Stuff != b.Stuff)
			{
				return false;
			}
			if (mode == TransferAsOneMode.PodsOrCaravanPacking)
			{
				float num = -1f;
				CompRottable compRottable = a.TryGetComp<CompRottable>();
				if (compRottable != null)
				{
					num = compRottable.RotProgressPct;
				}
				float num2 = -1f;
				CompRottable compRottable2 = b.TryGetComp<CompRottable>();
				if (compRottable2 != null)
				{
					num2 = compRottable2.RotProgressPct;
				}
				if (Mathf.Abs(num - num2) > 0.1f)
				{
					return false;
				}
			}
			if (a is Corpse && b is Corpse)
			{
				Pawn innerPawn = ((Corpse)a).InnerPawn;
				Pawn innerPawn2 = ((Corpse)b).InnerPawn;
				if (innerPawn.def != innerPawn2.def)
				{
					return false;
				}
				if (innerPawn.kindDef != innerPawn2.kindDef)
				{
					return false;
				}
				if (innerPawn.RaceProps.Humanlike || innerPawn2.RaceProps.Humanlike)
				{
					return false;
				}
				if ((innerPawn.Name != null && !innerPawn.Name.Numerical) || (innerPawn2.Name != null && !innerPawn2.Name.Numerical))
				{
					return false;
				}
				return true;
			}
			if (a.def.category == ThingCategory.Pawn)
			{
				if (b.def != a.def)
				{
					return false;
				}
				Pawn pawn = (Pawn)a;
				Pawn pawn2 = (Pawn)b;
				if (pawn.kindDef != pawn2.kindDef)
				{
					return false;
				}
				if (pawn.gender != pawn2.gender)
				{
					return false;
				}
				if (pawn.ageTracker.CurLifeStageIndex != pawn2.ageTracker.CurLifeStageIndex)
				{
					return false;
				}
				if (Mathf.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - pawn2.ageTracker.AgeBiologicalYearsFloat) > 1f)
				{
					return false;
				}
				return true;
			}
			Apparel apparel = a as Apparel;
			Apparel apparel2 = b as Apparel;
			if (apparel != null && apparel2 != null && apparel.WornByCorpse != apparel2.WornByCorpse)
			{
				return false;
			}
			if (mode != TransferAsOneMode.InactiveTradeable && a.def.useHitPoints && a.def.healthAffectsPrice && Mathf.Abs(a.HitPoints - b.HitPoints) >= 10)
			{
				return false;
			}
			if (a.TryGetQuality(out QualityCategory qc) && b.TryGetQuality(out QualityCategory qc2) && qc != qc2)
			{
				return false;
			}
			if (a.def.category == ThingCategory.Item)
			{
				return a.CanStackWith(b);
			}
			if (a.def.category == ThingCategory.Building)
			{
				return true;
			}
			Log.Error(string.Concat("Unknown TransferAsOne pair: ", a, ", ", b));
			return false;
		}

		public static bool CanStack(Thing thing)
		{
			if (thing.def.category == ThingCategory.Pawn)
			{
				if (thing.def.race.Humanlike)
				{
					return false;
				}
				Pawn pawn = (Pawn)thing;
				if (pawn.health.summaryHealth.SummaryHealthPercent < 0.9999f)
				{
					return false;
				}
				if (pawn.Name != null && !pawn.Name.Numerical)
				{
					return false;
				}
				if (pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null)
				{
					return false;
				}
				if (pawn.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true))
				{
					return false;
				}
			}
			return true;
		}

		public static T TransferableMatching<T>(Thing thing, List<T> transferables, TransferAsOneMode mode) where T : Transferable
		{
			if (thing == null || transferables == null)
			{
				return null;
			}
			for (int i = 0; i < transferables.Count; i++)
			{
				T val = transferables[i];
				if (val.HasAnyThing && TransferAsOne(thing, val.AnyThing, mode))
				{
					return val;
				}
			}
			return null;
		}

		public static Tradeable TradeableMatching(Thing thing, List<Tradeable> tradeables)
		{
			if (thing == null || tradeables == null)
			{
				return null;
			}
			for (int i = 0; i < tradeables.Count; i++)
			{
				Tradeable tradeable = tradeables[i];
				if (tradeable.HasAnyThing)
				{
					TransferAsOneMode mode = (!tradeable.TraderWillTrade) ? TransferAsOneMode.InactiveTradeable : TransferAsOneMode.Normal;
					if (TransferAsOne(thing, tradeable.AnyThing, mode))
					{
						return tradeable;
					}
				}
			}
			return null;
		}

		public static TransferableOneWay TransferableMatchingDesperate(Thing thing, List<TransferableOneWay> transferables, TransferAsOneMode mode)
		{
			if (thing == null || transferables == null)
			{
				return null;
			}
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (transferableOneWay.HasAnyThing && transferableOneWay.things.Contains(thing))
				{
					return transferableOneWay;
				}
			}
			for (int j = 0; j < transferables.Count; j++)
			{
				TransferableOneWay transferableOneWay2 = transferables[j];
				if (transferableOneWay2.HasAnyThing && TransferAsOne(thing, transferableOneWay2.AnyThing, mode))
				{
					return transferableOneWay2;
				}
			}
			if (!(thing is Pawn))
			{
				for (int k = 0; k < transferables.Count; k++)
				{
					TransferableOneWay transferableOneWay3 = transferables[k];
					if (transferableOneWay3.HasAnyThing && transferableOneWay3.ThingDef == thing.def)
					{
						return transferableOneWay3;
					}
				}
			}
			return null;
		}

		public static List<Pawn> GetPawnsFromTransferables(List<TransferableOneWay> transferables)
		{
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < transferables.Count; i++)
			{
				if (transferables[i].CountToTransfer > 0 && transferables[i].AnyThing is Pawn)
				{
					for (int j = 0; j < transferables[i].CountToTransfer; j++)
					{
						Pawn item = (Pawn)transferables[i].things[j];
						list.Add(item);
					}
				}
			}
			return list;
		}

		public static void SimulateTradeableTransfer(List<Thing> all, List<Tradeable> tradeables, List<ThingCount> outThingsAfterTransfer)
		{
			outThingsAfterTransfer.Clear();
			for (int i = 0; i < all.Count; i++)
			{
				outThingsAfterTransfer.Add(new ThingCount(all[i], all[i].stackCount));
			}
			for (int j = 0; j < tradeables.Count; j++)
			{
				int countToTransferToSource = tradeables[j].CountToTransferToSource;
				int countToTransferToDestination = tradeables[j].CountToTransferToDestination;
				if (countToTransferToSource > 0)
				{
					TransferNoSplit(tradeables[j].thingsTrader, countToTransferToSource, delegate(Thing originalThing, int toTake)
					{
						outThingsAfterTransfer.Add(new ThingCount(originalThing, toTake));
					}, removeIfTakingEntireThing: false, errorIfNotEnoughThings: false);
				}
				else
				{
					if (countToTransferToDestination <= 0)
					{
						continue;
					}
					TransferNoSplit(tradeables[j].thingsColony, countToTransferToDestination, delegate(Thing originalThing, int toTake)
					{
						int num = 0;
						ThingCount thingCount;
						while (true)
						{
							if (num >= outThingsAfterTransfer.Count)
							{
								return;
							}
							thingCount = outThingsAfterTransfer[num];
							if (thingCount.Thing == originalThing)
							{
								break;
							}
							num++;
						}
						outThingsAfterTransfer[num] = thingCount.WithCount(thingCount.Count - toTake);
					}, removeIfTakingEntireThing: false, errorIfNotEnoughThings: false);
				}
			}
		}
	}
}
