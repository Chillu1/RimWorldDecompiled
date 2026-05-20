using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;
using Verse.Sound;

namespace Verse;

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
			Log.Error("Dead/downed/deathresting pawn " + pawn?.ToString() + " tried to start carry item.");
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
			Log.Error("Dead/downed/deathresting pawn " + pawn?.ToString() + " tried to start carry " + item.ToStringSafe());
			return 0;
		}
		count = Mathf.Min(count, AvailableStackSpace(item.def));
		count = Mathf.Min(count, item.stackCount);
		bool flag = Find.Selector.IsSelected(item);
		Thing thing = item.SplitOff(count);
		int num = innerContainer.TryAdd(thing, count);
		if (num > 0 && thing != item)
		{
			TryUpdateTransferables(thing);
		}
		if (num > 0)
		{
			item.def.soundPickup.PlayOneShot(new TargetInfo(item.Position, pawn.Map));
			if (reserve)
			{
				pawn.Reserve(CarriedThing, pawn.CurJob);
			}
			if (flag)
			{
				if (!thing.Destroyed)
				{
					Find.Selector.Select(thing);
				}
				Find.Selector.Select(CarriedThing);
			}
			pawn.MapHeld.resourceCounter.UpdateResourceCounts();
		}
		return num;
	}

	private void TryUpdateTransferables(Thing splitStack)
	{
		if (splitStack != null && pawn.jobs?.curDriver is JobDriver_HaulToTransporter jobDriver_HaulToTransporter)
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(splitStack, jobDriver_HaulToTransporter.Transporter?.leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay != null && !transferableOneWay.things.Contains(splitStack) && transferableOneWay.MaxCount + splitStack.stackCount <= transferableOneWay.CountToTransfer)
			{
				transferableOneWay.things.Add(splitStack);
			}
		}
	}

	public bool TryDropCarriedThing(IntVec3 dropLoc, ThingPlaceMode mode, out Thing resultingThing, Action<Thing, int> placedAction = null)
	{
		if (innerContainer.TryDrop(CarriedThing, dropLoc, pawn.MapHeld, mode, out resultingThing, placedAction))
		{
			if (resultingThing != null && pawn.Faction.HostileTo(Faction.OfPlayer))
			{
				resultingThing.SetForbidden(value: true, warnOnFail: false);
			}
			pawn.MapHeld.resourceCounter.UpdateResourceCounts();
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
			pawn.MapHeld.resourceCounter.UpdateResourceCounts();
			return true;
		}
		return false;
	}

	public int CarriedCount(ThingDef def)
	{
		int num = 0;
		foreach (Thing item in innerContainer)
		{
			if (item.def == def)
			{
				num += item.stackCount;
			}
		}
		return num;
	}

	public void DestroyCarriedThing()
	{
		innerContainer.ClearAndDestroyContents();
	}

	public void CarryHandsTickInterval(int delta)
	{
		if (CarriedThing is Pawn pawn && pawn.DevelopmentalStage.Baby())
		{
			pawn.ideo?.IncreaseIdeoExposureIfBabyTick(this.pawn.Ideo, delta);
		}
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		Gizmo gizmo = ContainingSelectionUtility.SelectCarriedThingGizmo(pawn, CarriedThing);
		if (gizmo != null)
		{
			yield return gizmo;
		}
		if (pawn.Drafted && CarriedThing is Pawn)
		{
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandDropPawn".Translate(CarriedThing);
			command_Action.defaultDesc = "CommandDropPawnDesc".Translate();
			command_Action.action = delegate
			{
				pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
			};
			command_Action.icon = TexCommand.DropCarriedPawn;
			yield return command_Action;
		}
		if (!ModsConfig.BiotechActive || !DebugSettings.ShowDevGizmos)
		{
			yield break;
		}
		CompDissolution compDissolution = CarriedThing.TryGetComp<CompDissolution>();
		if (compDissolution != null)
		{
			Command_Action command_Action2 = new Command_Action();
			command_Action2.defaultLabel = "DEV: Dissolution event";
			command_Action2.action = delegate
			{
				compDissolution.TriggerDissolutionEvent();
			};
			yield return command_Action2;
		}
	}
}
