using System;
using System.Collections.Generic;
using LudeonTK;
using Verse;

namespace RimWorld;

public static class AbilityUtility
{
	public static bool ValidateIsConscious(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (!targetPawn.health.capacities.CanBeAwake)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityCantApplyUnconscious".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateIsAwake(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (!targetPawn.Awake())
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityCantApplyAsleep".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateNoMentalState(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (targetPawn.InMentalState)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityCantApplyToMentallyBroken".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateIsMaddened(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (!targetPawn.InMentalState || targetPawn.MentalStateDef != MentalStateDefOf.Manhunter)
		{
			if (showMessages)
			{
				SendPostProcessedMessage((targetPawn.health.hediffSet.HasHediff(HediffDefOf.Scaria) ? "AbilityCantApplyToScaria" : "AbilityCanOnlyApplyToManhunter").Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateCanWalk(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (targetPawn.Downed)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityCantApplyDowned".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateHasMentalState(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (!targetPawn.InMentalState && !targetPawn.health.hediffSet.HasHediff(HediffDefOf.CatatonicBreakdown))
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityCanOnlyApplyToMentallyBroken".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateHasResistance(Pawn targetPawn, bool showMessages, Ability ability)
	{
		Pawn_GuestTracker guest = targetPawn.guest;
		if (guest != null && guest.resistance <= float.Epsilon)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityCantApplyNoResistance".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateNoInspiration(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (targetPawn.Inspiration != null)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityAlreadyInspired".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateCanGetInspiration(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (targetPawn.mindState.inspirationHandler.GetRandomAvailableInspirationDef() == null)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityCantGetInspiredNow".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateMustBeHuman(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (targetPawn.NonHumanlikeOrWildMan())
		{
			if (showMessages)
			{
				SendPostProcessedMessage((targetPawn.IsWildMan() ? "AbilityMustBeHumanNonWild" : "AbilityMustBeHuman").Translate(), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateMustBeHumanOrWildMan(Pawn targetPawn, bool showMessage, Ability ability)
	{
		if (!targetPawn.RaceProps.Humanlike)
		{
			if (showMessage)
			{
				SendPostProcessedMessage("AbilityMustBeHuman".Translate(), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateMustNotBeBaby(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (targetPawn.DevelopmentalStage.Baby())
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityCantApplyOnBaby".Translate(), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateMustBeAnimal(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (!targetPawn.IsAnimal)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityMustBeAnimal".Translate(), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateNotSameIdeo(Pawn casterPawn, Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (casterPawn.Ideo != null && casterPawn.Ideo == targetPawn.Ideo)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityMustBeNotSameIdeo".Translate(targetPawn, casterPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateSameIdeo(Pawn casterPawn, Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (casterPawn.Ideo != targetPawn.Ideo)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityMustBeSameIdeo".Translate(targetPawn, casterPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateSickOrInjured(Pawn targetPawn, bool showMessage, Ability ability)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (Hediff hediff in targetPawn.health.hediffSet.hediffs)
		{
			flag |= hediff.TryGetComp<HediffComp_Immunizable>() != null;
			flag2 |= hediff is Hediff_Injury hd && !hd.IsPermanent() && hd.CanHealNaturally();
		}
		if (!flag && !flag2)
		{
			if (showMessage)
			{
				SendPostProcessedMessage("AbilityMustBeSickOrInjured".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateNotGuilty(Pawn targetPawn, bool showMessages, Ability ability)
	{
		if (targetPawn.guilt != null && targetPawn.guilt.IsGuilty)
		{
			if (showMessages)
			{
				SendPostProcessedMessage("AbilityMustBeNotGuilty".Translate(targetPawn), targetPawn, ability);
			}
			return false;
		}
		return true;
	}

	public static bool ValidateHasTendableWound(Pawn targetPawn, bool showMessages, Ability ability)
	{
		List<Hediff> hediffs = targetPawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if ((hediffs[i] is Hediff_Injury || hediffs[i] is Hediff_MissingPart) && hediffs[i].TendableNow())
			{
				return true;
			}
		}
		if (showMessages)
		{
			SendPostProcessedMessage("AbilityMustHaveTendableWound".Translate(targetPawn), targetPawn, ability);
		}
		return false;
	}

	private static void SendPostProcessedMessage(string message, LookTargets targets, Ability ability)
	{
		if (ability != null)
		{
			message = "CannotUseAbility".Translate(ability.def.label) + ": " + message;
		}
		Messages.Message(message, targets, MessageTypeDefOf.RejectInput, historical: false);
	}

	public static void DoClamor(IntVec3 cell, float radius, Thing source, ClamorDef clamor)
	{
		if (clamor != null)
		{
			GenClamor.DoClamor(source, cell, radius, clamor);
		}
	}

	public static bool IsIncendiary(this Ability ability)
	{
		return ability.def.ai_IsIncendiary;
	}

	public static Ability MakeAbility(AbilityDef def, Pawn pawn)
	{
		return Activator.CreateInstance(def.abilityClass, pawn, def) as Ability;
	}

	public static Ability MakeAbility(AbilityDef def, Pawn pawn, Precept sourcePrecept)
	{
		return Activator.CreateInstance(def.abilityClass, pawn, sourcePrecept, def) as Ability;
	}

	public static void DoTable_AbilityCosts()
	{
		List<TableDataGetter<AbilityDef>> list = new List<TableDataGetter<AbilityDef>>();
		list.Add(new TableDataGetter<AbilityDef>("name", (AbilityDef a) => a.LabelCap));
		list.Add(new TableDataGetter<AbilityDef>("level", (AbilityDef a) => a.level));
		list.Add(new TableDataGetter<AbilityDef>("heat cost", (AbilityDef a) => a.EntropyGain));
		list.Add(new TableDataGetter<AbilityDef>("psyfocus cost", (AbilityDef a) => (!(a.PsyfocusCostRange.Span <= float.Epsilon)) ? (a.PsyfocusCostRange.min * 100f + "-" + a.PsyfocusCostRange.max * 100f + "%") : a.PsyfocusCostRange.max.ToStringPercent()));
		list.Add(new TableDataGetter<AbilityDef>("max psyfocus recovery time days", (AbilityDef a) => (a.PsyfocusCostRange.Span <= float.Epsilon) ? ((object)(a.PsyfocusCostRange.min / StatDefOf.MeditationFocusGain.defaultBaseValue)) : (a.PsyfocusCostRange.min / StatDefOf.MeditationFocusGain.defaultBaseValue + "-" + a.PsyfocusCostRange.max / StatDefOf.MeditationFocusGain.defaultBaseValue)));
		DebugTables.MakeTablesDialog(DefDatabase<AbilityDef>.AllDefsListForReading, list.ToArray());
	}
}
