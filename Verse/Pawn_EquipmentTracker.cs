using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class Pawn_EquipmentTracker : IThingHolder, IExposable
{
	public Pawn pawn;

	private ThingOwner<ThingWithComps> equipment;

	public Thing bondedWeapon;

	private static List<KeyBindingDef> tmpKeybindings = new List<KeyBindingDef>();

	public ThingWithComps Primary
	{
		get
		{
			for (int i = 0; i < equipment.Count; i++)
			{
				if (equipment[i].def.equipmentType == EquipmentType.Primary)
				{
					return equipment[i];
				}
			}
			return null;
		}
		private set
		{
			if (Primary == value)
			{
				return;
			}
			if (value != null && value.def.equipmentType != EquipmentType.Primary)
			{
				Log.Error("Tried to set non-primary equipment as primary.");
				return;
			}
			if (Primary != null)
			{
				equipment.Remove(Primary);
			}
			if (value != null)
			{
				equipment.TryAdd(value);
			}
			if (pawn.drafter != null)
			{
				pawn.drafter.Notify_PrimaryWeaponChanged();
			}
		}
	}

	public CompEquippable PrimaryEq => Primary?.GetComp<CompEquippable>();

	public List<ThingWithComps> AllEquipmentListForReading => equipment.InnerListForReading;

	public IEnumerable<Verb> AllEquipmentVerbs
	{
		get
		{
			List<ThingWithComps> list = AllEquipmentListForReading;
			for (int i = 0; i < list.Count; i++)
			{
				ThingWithComps thingWithComps = list[i];
				List<Verb> verbs = thingWithComps.GetComp<CompEquippable>().AllVerbs;
				for (int j = 0; j < verbs.Count; j++)
				{
					yield return verbs[j];
				}
			}
		}
	}

	public IThingHolder ParentHolder => pawn;

	public Pawn_EquipmentTracker(Pawn newPawn)
	{
		pawn = newPawn;
		equipment = new ThingOwner<ThingWithComps>(this);
	}

	public void ExposeData()
	{
		Scribe_Deep.Look(ref equipment, "equipment", this);
		Scribe_References.Look(ref bondedWeapon, "bondedWeapon");
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		List<ThingWithComps> allEquipmentListForReading = AllEquipmentListForReading;
		for (int i = 0; i < allEquipmentListForReading.Count; i++)
		{
			foreach (Verb allVerb in allEquipmentListForReading[i].GetComp<CompEquippable>().AllVerbs)
			{
				allVerb.caster = pawn;
			}
		}
	}

	public void EquipmentTrackerTick()
	{
		List<ThingWithComps> allEquipmentListForReading = AllEquipmentListForReading;
		for (int i = 0; i < allEquipmentListForReading.Count; i++)
		{
			ThingWithComps thingWithComps = allEquipmentListForReading[i];
			if (thingWithComps.def.tickerType != TickerType.Normal)
			{
				thingWithComps.GetComp<CompEquippable>().verbTracker.VerbsTick();
			}
		}
	}

	public bool HasAnything()
	{
		return equipment.Any;
	}

	public void MakeRoomFor(ThingWithComps eq)
	{
		MakeRoomFor(eq, out var _);
	}

	public void MakeRoomFor(ThingWithComps eq, out ThingWithComps dropped)
	{
		dropped = null;
		if (eq.def.equipmentType != EquipmentType.Primary || Primary == null)
		{
			return;
		}
		if (TryDropEquipment(Primary, out dropped, pawn.Position))
		{
			if (dropped != null)
			{
				dropped.SetForbidden(value: false);
			}
		}
		else
		{
			Log.Error(pawn?.ToString() + " couldn't make room for equipment " + eq);
		}
	}

	public void Remove(ThingWithComps eq)
	{
		equipment.Remove(eq);
	}

	public bool TryDropEquipment(ThingWithComps eq, out ThingWithComps resultingEq, IntVec3 pos, bool forbid = true)
	{
		if (!pos.IsValid)
		{
			Log.Error(pawn?.ToString() + " tried to drop " + eq?.ToString() + " at invalid cell.");
			resultingEq = null;
			return false;
		}
		if (equipment.TryDrop(eq, pos, pawn.MapHeld, ThingPlaceMode.Near, out resultingEq))
		{
			if (resultingEq != null)
			{
				resultingEq.SetForbidden(forbid, warnOnFail: false);
			}
			return true;
		}
		return false;
	}

	public void DropAllEquipment(IntVec3 pos, bool forbid = true, bool rememberPrimary = false)
	{
		for (int num = equipment.Count - 1; num >= 0; num--)
		{
			bool flag = equipment[num] == Primary;
			if (TryDropEquipment(equipment[num], out var resultingEq, pos, forbid) && rememberPrimary && flag)
			{
				pawn.mindState.droppedWeapon = resultingEq;
			}
		}
	}

	public bool TryTransferEquipmentToContainer(ThingWithComps eq, ThingOwner container)
	{
		return equipment.TryTransferToContainer(eq, container);
	}

	public void DestroyEquipment(ThingWithComps eq)
	{
		if (!equipment.Contains(eq))
		{
			Log.Warning("Tried to destroy equipment " + eq?.ToString() + " but it's not here.");
			return;
		}
		Remove(eq);
		eq.Destroy();
	}

	public void DestroyAllEquipment(DestroyMode mode = DestroyMode.Vanish)
	{
		equipment.ClearAndDestroyContents(mode);
	}

	public bool Contains(Thing eq)
	{
		return equipment.Contains(eq);
	}

	internal void Notify_PrimaryDestroyed()
	{
		if (Primary != null)
		{
			Remove(Primary);
		}
		if (pawn.Spawned)
		{
			pawn.stances.CancelBusyStanceSoft();
		}
	}

	public void AddEquipment(ThingWithComps newEq)
	{
		if (newEq.def.equipmentType == EquipmentType.Primary && Primary != null)
		{
			Log.Error("Pawn " + pawn.LabelCap + " got primaryInt equipment " + newEq?.ToString() + " while already having primaryInt equipment " + Primary);
		}
		else if (equipment.TryAdd(newEq) && newEq.def.equipmentType == EquipmentType.Primary)
		{
			pawn.mindState.droppedWeapon = null;
		}
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		if (Primary != null)
		{
			foreach (Gizmo item in PrimaryEq.CompGetEquippedGizmosExtra())
			{
				yield return item;
			}
		}
		if (!PawnAttackGizmoUtility.CanShowEquipmentGizmos())
		{
			yield break;
		}
		try
		{
			tmpKeybindings.Add(KeyBindingDefOf.Misc1);
			tmpKeybindings.Add(KeyBindingDefOf.Misc2);
			tmpKeybindings.Add(KeyBindingDefOf.Misc3);
			List<ThingWithComps> list = AllEquipmentListForReading;
			ThingWithComps primaryMelee = list.FirstOrDefault((ThingWithComps w) => w.def.IsMeleeWeapon);
			ThingWithComps primaryRanged = list.FirstOrDefault((ThingWithComps w) => w.def.IsRangedWeapon);
			if (primaryMelee != null)
			{
				KeyBindingDef misc = KeyBindingDefOf.Misc2;
				foreach (Gizmo item2 in YieldGizmos(primaryMelee, misc))
				{
					yield return item2;
				}
			}
			if (primaryRanged != null)
			{
				KeyBindingDef misc2 = KeyBindingDefOf.Misc1;
				foreach (Gizmo item3 in YieldGizmos(primaryRanged, misc2))
				{
					yield return item3;
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				ThingWithComps thingWithComps = list[i];
				if (thingWithComps == primaryMelee || thingWithComps == primaryRanged)
				{
					continue;
				}
				foreach (Gizmo item4 in YieldGizmos(thingWithComps))
				{
					yield return item4;
				}
			}
		}
		finally
		{
			tmpKeybindings.Clear();
		}
		static IEnumerable<Gizmo> YieldGizmos(ThingWithComps eq, KeyBindingDef preferredHotKey = null)
		{
			foreach (Command verbsCommand in eq.GetComp<CompEquippable>().GetVerbsCommands())
			{
				if (tmpKeybindings.Count > 0)
				{
					if (preferredHotKey != null && tmpKeybindings.Contains(preferredHotKey))
					{
						verbsCommand.hotKey = preferredHotKey;
						tmpKeybindings.Remove(preferredHotKey);
					}
					else
					{
						verbsCommand.hotKey = tmpKeybindings.Pop();
					}
				}
				yield return verbsCommand;
			}
		}
	}

	public void Notify_EquipmentAdded(ThingWithComps eq)
	{
		foreach (Verb allVerb in eq.GetComp<CompEquippable>().AllVerbs)
		{
			allVerb.caster = pawn;
			allVerb.Notify_PickedUp();
		}
		eq.Notify_Equipped(pawn);
		if (ModsConfig.RoyaltyActive && eq.def.equipmentType == EquipmentType.Primary && bondedWeapon != null && !bondedWeapon.Destroyed)
		{
			bondedWeapon.TryGetComp<CompBladelinkWeapon>()?.Notify_WieldedOtherWeapon();
		}
	}

	public void Notify_EquipmentRemoved(ThingWithComps eq)
	{
		eq.Notify_Unequipped(pawn);
		if (ModsConfig.RoyaltyActive)
		{
			eq.TryGetComp<CompBladelinkWeapon>()?.Notify_EquipmentLost(pawn);
		}
		if (ModsConfig.OdysseyActive)
		{
			eq.TryGetComp<CompUniqueWeapon>()?.Notify_EquipmentLost(pawn);
		}
	}

	public void Notify_AbilityUsed(Ability ability)
	{
		if (PrimaryEq is CompEquippableAbility compEquippableAbility && ability == compEquippableAbility.AbilityForReading)
		{
			compEquippableAbility.UsedOnce();
		}
	}

	public void Notify_PawnSpawned()
	{
		if (!pawn.BeingTransportedOnGravship && HasAnything() && pawn.Downed && !pawn.GetPosture().InBed())
		{
			if (pawn.kindDef.destroyGearOnDrop)
			{
				DestroyAllEquipment();
			}
			else
			{
				DropAllEquipment(pawn.Position);
			}
		}
	}

	public void Notify_PawnDied()
	{
		if (ModsConfig.RoyaltyActive && bondedWeapon != null)
		{
			bondedWeapon.TryGetComp<CompBladelinkWeapon>()?.UnCode();
		}
	}

	public void Notify_KilledPawn()
	{
		foreach (ThingWithComps item in equipment)
		{
			item.Notify_KilledPawn(pawn);
		}
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return equipment;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}
}
