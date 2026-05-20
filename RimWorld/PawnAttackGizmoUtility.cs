using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class PawnAttackGizmoUtility
{
	public static IEnumerable<Gizmo> GetAttackGizmos(Pawn pawn)
	{
		if (ShouldUseMeleeAttackGizmo(pawn))
		{
			yield return GetMeleeAttackGizmo(pawn);
		}
		if (ShouldUseSquadAttackGizmo())
		{
			yield return GetSquadAttackGizmo(pawn);
		}
	}

	public static bool CanShowEquipmentGizmos()
	{
		return !AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons();
	}

	private static bool ShouldUseSquadAttackGizmo()
	{
		if (AtLeastOneSelectedPlayerPawnHasRangedWeapon())
		{
			return AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons();
		}
		return false;
	}

	private static bool CanOrderPlayerPawn(Pawn pawn)
	{
		if (!pawn.IsColonistPlayerControlled && !pawn.IsColonyMechPlayerControlled)
		{
			return pawn.IsColonySubhumanPlayerControlled;
		}
		return true;
	}

	private static Gizmo GetSquadAttackGizmo(Pawn pawn)
	{
		Command_Target command_Target = new Command_Target();
		command_Target.defaultLabel = "CommandSquadAttack".Translate();
		command_Target.defaultDesc = "CommandSquadAttackDesc".Translate();
		command_Target.hotKey = KeyBindingDefOf.Misc1;
		command_Target.icon = TexCommand.SquadAttack;
		command_Target.targetingParams = TargetingParameters.ForAttackAny();
		command_Target.targetingParams.canTargetLocations = AllSelectedPlayerPawnsCanTargetLocations();
		if (FloatMenuUtility.GetAttackAction(pawn, LocalTargetInfo.Invalid, out var failStr) == null)
		{
			command_Target.Disable(failStr.CapitalizeFirst() + ".");
		}
		command_Target.action = delegate(LocalTargetInfo target)
		{
			foreach (Pawn item in Find.Selector.SelectedObjects.Where((object x) => x is Pawn pawn2 && CanOrderPlayerPawn(pawn2) && pawn2.Drafted).Cast<Pawn>())
			{
				string failStr2;
				Action attackAction = FloatMenuUtility.GetAttackAction(item, target, out failStr2);
				if (attackAction != null)
				{
					attackAction();
				}
				else if (!failStr2.NullOrEmpty())
				{
					Messages.Message(failStr2, target.Thing, MessageTypeDefOf.RejectInput, historical: false);
				}
			}
		};
		command_Target.onUpdate = delegate
		{
			foreach (Pawn item2 in Find.Selector.SelectedObjects.Where((object x) => x is Pawn pawn2 && CanOrderPlayerPawn(pawn2) && pawn2.Drafted).Cast<Pawn>())
			{
				ThingWithComps thingWithComps = item2.equipment.AllEquipmentListForReading.FirstOrDefault((ThingWithComps w) => w.def.IsRangedWeapon);
				if (thingWithComps != null && thingWithComps.TryGetComp<CompEquippable>(out var comp))
				{
					Verb verb = comp.AllVerbs.FirstOrDefault((Verb v) => v.verbProps.hasStandardCommand);
					verb?.verbProps.DrawRadiusRing(item2.Position, verb);
				}
			}
		};
		return command_Target;
	}

	private static bool ShouldUseMeleeAttackGizmo(Pawn pawn)
	{
		if (!pawn.Drafted)
		{
			return false;
		}
		if (!AtLeastOneSelectedPlayerPawnHasRangedWeapon() && !AtLeastOneSelectedPlayerPawnHasNoWeapon())
		{
			return AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons();
		}
		return true;
	}

	public static Gizmo GetMeleeAttackGizmo(Pawn pawn)
	{
		Command_Target command_Target = new Command_Target();
		command_Target.defaultLabel = "CommandMeleeAttack".Translate();
		command_Target.defaultDesc = "CommandMeleeAttackDesc".Translate();
		command_Target.targetingParams = TargetingParameters.ForAttackAny();
		command_Target.hotKey = KeyBindingDefOf.Misc2;
		command_Target.icon = TexCommand.AttackMelee;
		if (FloatMenuUtility.GetMeleeAttackAction(pawn, LocalTargetInfo.Invalid, out var failStr) == null)
		{
			command_Target.Disable(failStr.CapitalizeFirst() + ".");
		}
		command_Target.action = delegate(LocalTargetInfo target)
		{
			foreach (Pawn item in Find.Selector.SelectedObjects.Where((object x) => x is Pawn pawn2 && CanOrderPlayerPawn(pawn2) && pawn2.Drafted).Cast<Pawn>())
			{
				string failStr2;
				Action meleeAttackAction = FloatMenuUtility.GetMeleeAttackAction(item, target, out failStr2);
				if (meleeAttackAction != null)
				{
					meleeAttackAction();
				}
				else if (!failStr2.NullOrEmpty())
				{
					Messages.Message(failStr2, target.Thing, MessageTypeDefOf.RejectInput, historical: false);
				}
			}
		};
		return command_Target;
	}

	private static bool AtLeastOneSelectedPlayerPawnHasRangedWeapon()
	{
		List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
		for (int i = 0; i < selectedObjectsListForReading.Count; i++)
		{
			if (selectedObjectsListForReading[i] is Pawn pawn && CanOrderPlayerPawn(pawn) && pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
			{
				return true;
			}
		}
		return false;
	}

	private static bool AtLeastOneSelectedPlayerPawnHasNoWeapon()
	{
		List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
		for (int i = 0; i < selectedObjectsListForReading.Count; i++)
		{
			if (selectedObjectsListForReading[i] is Pawn pawn && CanOrderPlayerPawn(pawn) && (pawn.equipment == null || pawn.equipment.Primary == null))
			{
				return true;
			}
		}
		return false;
	}

	private static bool AtLeastTwoSelectedPlayerPawnsHaveDifferentWeapons()
	{
		if (Find.Selector.NumSelected <= 1)
		{
			return false;
		}
		ThingDef thingDef = null;
		bool flag = false;
		List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
		for (int i = 0; i < selectedObjectsListForReading.Count; i++)
		{
			if (selectedObjectsListForReading[i] is Pawn pawn && CanOrderPlayerPawn(pawn))
			{
				ThingDef thingDef2 = ((pawn.equipment != null && pawn.equipment.Primary != null) ? pawn.equipment.Primary.def : null);
				if (!flag)
				{
					thingDef = thingDef2;
					flag = true;
				}
				else if (thingDef2 != thingDef)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool AllSelectedPlayerPawnsCanTargetLocations()
	{
		foreach (object selectedObject in Find.Selector.SelectedObjects)
		{
			if (selectedObject is Pawn pawn && CanOrderPlayerPawn(pawn) && pawn.Drafted)
			{
				if (pawn.equipment.Primary == null || pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.IsMeleeAttack)
				{
					return false;
				}
				if (!pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.targetParams.canTargetLocations)
				{
					return false;
				}
			}
		}
		return true;
	}
}
