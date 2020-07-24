using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Verse
{
	public class Pawn_CarryTracker : IThingHolder, IExposable
	{
		public Pawn pawn;

		public ThingOwner<Thing> innerContainer;

		public Thing CarriedThing
		{
			get
			{
				if (innerContainer.Count == 0)
				{
					return null;
				}
				return innerContainer[0];
			}
		}

		public bool Full => AvailableStackSpace(CarriedThing.def) <= 0;

		public IThingHolder ParentHolder => pawn;

		public Pawn_CarryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
		}

		public void ExposeData()
		{
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public int AvailableStackSpace(ThingDef td)
		{
			int num = MaxStackSpaceEver(td);
			if (CarriedThing != null)
			{
				num -= CarriedThing.stackCount;
			}
			return num;
		}

		public int MaxStackSpaceEver(ThingDef td)
		{
			int b = Mathf.RoundToInt(pawn.GetStatValue(StatDefOf.CarryingCapacity) / td.VolumePerUnit);
			return Mathf.Min(td.stackLimit, b);
		}

		public bool TryStartCarry(Thing item)
		{
			if (pawn.Dead || pawn.Downed)
			{
				Log.Error(string.Concat("Dead/downed pawn ", pawn, " tried to start carry item."));
				return false;
			}
			if (innerContainer.TryAdd(item))
			{
				item.def.soundPickup.PlayOneShot(new TargetInfo(item.Position, pawn.Map));
				return true;
			}
			return false;
		}

		public int TryStartCarry(Thing item, int count, bool reserve = true)
		{
			if (pawn.Dead || pawn.Downed)
			{
				Log.Error(string.Concat("Dead/downed pawn ", pawn, " tried to start carry ", item.ToStringSafe()));
				return 0;
			}
			count = Mathf.Min(count, AvailableStackSpace(item.def));
			count = Mathf.Min(count, item.stackCount);
			int num = innerContainer.TryAdd(item.SplitOff(count), count);
			if (num > 0)
			{
				item.def.soundPickup.PlayOneShot(new TargetInfo(item.Position, pawn.Map));
				if (reserve)
				{
					pawn.Reserve(CarriedThing, pawn.CurJob);
				}
			}
			return num;
		}

		public bool TryDropCarriedThing(IntVec3 dropLoc, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			if (innerContainer.TryDrop(CarriedThing, dropLoc, pawn.MapHeld, mode, out resultingThing, placedAction))
			{
				if (resultingThing != null && pawn.Faction.HostileTo(Faction.OfPlayer))
				{
					resultingThing.SetForbidden(value: true, warnOnFail: false);
				}
				return true;
			}
			return false;
		}

		public bool TryDropCarriedThing(IntVec3 dropLoc, int count, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
		{
			if (innerContainer.TryDrop(CarriedThing, dropLoc, pawn.MapHeld, mode, count, out resultingThing, placedAction))
			{
				if (resultingThing != null && pawn.Faction.HostileTo(Faction.OfPlayer))
				{
					resultingThing.SetForbidden(value: true, warnOnFail: false);
				}
				return true;
			}
			return false;
		}

		public void DestroyCarriedThing()
		{
			innerContainer.ClearAndDestroyContents();
		}

		public void CarryHandsTick()
		{
			innerContainer.ThingOwnerTick();
		}
	}
}
