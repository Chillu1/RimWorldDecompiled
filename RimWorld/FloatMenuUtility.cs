using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
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
			else if (!pawn.IsColonistPlayerControlled)
			{
				failStr = "CannotOrderNonControlledLower".Translate();
			}
			else if (target.IsValid && !pawn.equipment.PrimaryEq.PrimaryVerb.CanHitTarget(target))
			{
				if (!pawn.Position.InHorDistOf(target.Cell, primaryVerb.verbProps.range))
				{
					failStr = "OutOfRange".Translate();
				}
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
			else if (pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				failStr = "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn);
			}
			else
			{
				if (pawn != target.Thing)
				{
					return delegate
					{
						Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
						pawn.jobs.TryTakeOrderedJob(job);
					};
				}
				failStr = "CannotAttackSelf".Translate();
			}
			failStr = failStr.CapitalizeFirst();
			return null;
		}

		public static Action GetMeleeAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
		{
			failStr = "";
			if (!pawn.Drafted)
			{
				failStr = "IsNotDraftedLower".Translate(pawn.LabelShort, pawn);
			}
			else if (!pawn.IsColonistPlayerControlled)
			{
				failStr = "CannotOrderNonControlledLower".Translate();
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
			else
			{
				if (pawn != target.Thing)
				{
					return delegate
					{
						Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
						Pawn pawn2 = target.Thing as Pawn;
						if (pawn2 != null)
						{
							job.killIncappedTarget = pawn2.Downed;
						}
						pawn.jobs.TryTakeOrderedJob(job);
					};
				}
				failStr = "CannotAttackSelf".Translate();
			}
			failStr = failStr.CapitalizeFirst();
			return null;
		}

		public static Action GetAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
		{
			if (pawn.equipment.Primary != null && !pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.IsMeleeAttack)
			{
				return GetRangedAttackAction(pawn, target, out failStr);
			}
			return GetMeleeAttackAction(pawn, target, out failStr);
		}

		public static FloatMenuOption DecoratePrioritizedTask(FloatMenuOption option, Pawn pawn, LocalTargetInfo target, string reservedText = "ReservedBy")
		{
			if (option.action == null)
			{
				return option;
			}
			if (pawn != null && !pawn.CanReserve(target) && pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations: true))
			{
				Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(target, pawn);
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
	}
}
