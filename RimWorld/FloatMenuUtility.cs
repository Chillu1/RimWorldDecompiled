using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class FloatMenuUtility
{
	public static void MakeMenu<T>(IEnumerable<T> objects, Func<T, string> labelGetter, Func<T, Action> actionGetter)
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (T @object in objects)
		{
			list.Add(new FloatMenuOption(labelGetter(@object), actionGetter(@object)));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public static Action GetRangedAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
	{
		failStr = "";
		if (pawn.equipment.Primary == null)
		{
			return null;
		}
		Verb primaryVerb = pawn.equipment.PrimaryEq.PrimaryVerb;
		if (primaryVerb.verbProps.IsMeleeAttack)
		{
			return null;
		}
		if (!pawn.Drafted)
		{
			failStr = "IsNotDraftedLower".Translate(pawn.LabelShort, pawn);
		}
		else if (!pawn.IsColonistPlayerControlled && !pawn.IsColonyMech && !pawn.IsColonySubhumanPlayerControlled)
		{
			failStr = "CannotOrderNonControlledLower".Translate();
		}
		else if (pawn.IsColonyMechPlayerControlled && target.IsValid && !MechanitorUtility.InMechanitorCommandRange(pawn, target))
		{
			failStr = "OutOfCommandRange".Translate();
		}
		else if (target.IsValid && !pawn.equipment.PrimaryEq.PrimaryVerb.CanHitTarget(target))
		{
			if (!pawn.Position.InHorDistOf(target.Cell, primaryVerb.EffectiveRange))
			{
				failStr = "OutOfRange".Translate();
			}
			else
			{
				float num = primaryVerb.verbProps.EffectiveMinRange(target, pawn);
				if ((float)pawn.Position.DistanceToSquared(target.Cell) < num * num)
				{
					failStr = "TooClose".Translate();
				}
				else
				{
					failStr = "CannotHitTarget".Translate();
				}
			}
		}
		else if (pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			failStr = "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn);
		}
		else if (pawn == target.Thing)
		{
			failStr = "CannotAttackSelf".Translate();
		}
		else if (target.Thing is Pawn target2 && (pawn.InSameExtraFaction(target2, ExtraFactionType.HomeFaction) || pawn.InSameExtraFaction(target2, ExtraFactionType.MiniFaction)))
		{
			failStr = "CannotAttackSameFactionMember".Translate();
		}
		else if (target.Thing is Pawn victim && HistoryEventUtility.IsKillingInnocentAnimal(pawn, victim) && !new HistoryEvent(HistoryEventDefOf.KilledInnocentAnimal, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
		{
			failStr = "IdeoligionForbids".Translate();
		}
		else
		{
			if (!(target.Thing is Pawn pawn2) || pawn.Ideo == null || !pawn.Ideo.IsVeneratedAnimal(pawn2) || new HistoryEvent(HistoryEventDefOf.HuntedVeneratedAnimal, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
			{
				return delegate
				{
					Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				};
			}
			failStr = "IdeoligionForbids".Translate();
		}
		failStr = failStr.CapitalizeFirst();
		return null;
	}

	public static Action GetMeleeAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr, bool ignoreControlled = false)
	{
		failStr = "";
		if (!pawn.Drafted && !ignoreControlled)
		{
			failStr = "IsNotDraftedLower".Translate(pawn.LabelShort, pawn);
		}
		else if (!pawn.IsColonistPlayerControlled && !pawn.IsColonyMech && !pawn.IsColonySubhumanPlayerControlled && !ignoreControlled)
		{
			failStr = "CannotOrderNonControlledLower".Translate();
		}
		else if (pawn.IsColonyMechPlayerControlled && target.IsValid && !MechanitorUtility.InMechanitorCommandRange(pawn, target))
		{
			failStr = "OutOfCommandRange".Translate();
		}
		else if (target.IsValid && !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
		{
			failStr = "NoPath".Translate();
		}
		else if (pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			failStr = "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn);
		}
		else if (pawn.meleeVerbs.TryGetMeleeVerb(target.Thing) == null)
		{
			failStr = "Incapable".Translate();
		}
		else if (pawn == target.Thing)
		{
			failStr = "CannotAttackSelf".Translate();
		}
		else if (target.Thing is Pawn target2 && (pawn.InSameExtraFaction(target2, ExtraFactionType.HomeFaction) || pawn.InSameExtraFaction(target2, ExtraFactionType.MiniFaction)))
		{
			failStr = "CannotAttackSameFactionMember".Translate();
		}
		else
		{
			if (!(target.Thing is Pawn pawn2) || !pawn2.RaceProps.Animal || !HistoryEventUtility.IsKillingInnocentAnimal(pawn, pawn2) || new HistoryEvent(HistoryEventDefOf.KilledInnocentAnimal, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
			{
				return delegate
				{
					Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
					if (target.Thing is Pawn pawn3)
					{
						job.killIncappedTarget = pawn3.Downed;
					}
					pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
				};
			}
			failStr = "IdeoligionForbids".Translate();
		}
		failStr = failStr.CapitalizeFirst();
		return null;
	}

	public static bool UseRangedAttack(Pawn pawn)
	{
		if (pawn.equipment.Primary != null)
		{
			return !pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.IsMeleeAttack;
		}
		return false;
	}

	public static Action GetAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
	{
		if (UseRangedAttack(pawn))
		{
			return GetRangedAttackAction(pawn, target, out failStr);
		}
		return GetMeleeAttackAction(pawn, target, out failStr);
	}

	public static FloatMenuOption DecoratePrioritizedTask(FloatMenuOption option, Pawn pawn, LocalTargetInfo target, string reservedText = "ReservedBy", ReservationLayerDef layer = null)
	{
		if (option.action == null)
		{
			return option;
		}
		if (pawn != null && !pawn.CanReserve(target, 1, -1, layer) && pawn.CanReserve(target, 1, -1, layer, ignoreOtherReservations: true))
		{
			Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(target, pawn, layer);
			if (pawn2 == null)
			{
				pawn2 = pawn.Map.physicalInteractionReservationManager.FirstReserverOf(target);
			}
			if (pawn2 != null)
			{
				option.Label = option.Label + ": " + reservedText.Translate(pawn2.LabelShort, pawn2);
			}
		}
		if (option.revalidateClickTarget != null && option.revalidateClickTarget != target.Thing)
		{
			Log.ErrorOnce($"Click target mismatch; {option.revalidateClickTarget} vs {target.Thing} in {option.Label}", 52753118);
		}
		option.revalidateClickTarget = target.Thing;
		return option;
	}

	public static void ValidateTakeToBedOption(Pawn pawn, Pawn target, FloatMenuOption option, string cannot, GuestStatus? guestStatus = null)
	{
		Building_Bed building_Bed = RestUtility.FindBedFor(target, pawn, checkSocialProperness: false, ignoreOtherReservations: false, guestStatus);
		if (building_Bed != null)
		{
			return;
		}
		building_Bed = RestUtility.FindBedFor(target, pawn, checkSocialProperness: false, ignoreOtherReservations: true, guestStatus);
		if (building_Bed != null)
		{
			if (pawn.MapHeld.reservationManager.TryGetReserver(building_Bed, pawn.Faction, out var reserver))
			{
				option.Label = option.Label + " (" + building_Bed.def.label + " " + "ReservedBy".Translate(reserver.LabelShort, reserver).Resolve().StripTags() + ")";
			}
		}
		else
		{
			option.Disabled = true;
			option.Label = cannot;
		}
	}
}
