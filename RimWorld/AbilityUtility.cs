using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class AbilityUtility
	{
		public static bool ValidateIsConscious(Pawn targetPawn, bool showMessages)
		{
			if (!targetPawn.health.capacities.CanBeAwake)
			{
				if (showMessages)
				{
					Messages.Message("AbilityCantApplyUnconscious".Translate(targetPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public static bool ValidateIsAwake(Pawn targetPawn, bool showMessages)
		{
			if (!targetPawn.Awake())
			{
				if (showMessages)
				{
					Messages.Message("AbilityCantApplyAsleep".Translate(targetPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public static bool ValidateNoMentalState(Pawn targetPawn, bool showMessages)
		{
			if (targetPawn.InMentalState)
			{
				if (showMessages)
				{
					Messages.Message("AbilityCantApplyToMentallyBroken".Translate(targetPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public static bool ValidateHasMentalState(Pawn targetPawn, bool showMessages)
		{
			if (!targetPawn.InMentalState)
			{
				if (showMessages)
				{
					Messages.Message("AbilityCanOnlyApplyToMentallyBroken".Translate(targetPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public static bool ValidateHasResistance(Pawn targetPawn, bool showMessages)
		{
			Pawn_GuestTracker guest = targetPawn.guest;
			if (guest != null && guest.resistance <= float.Epsilon)
			{
				if (showMessages)
				{
					Messages.Message("AbilityCantApplyNoResistance".Translate(targetPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public static bool ValidateNoInspiration(Pawn targetPawn, bool showMessages)
		{
			if (targetPawn.Inspiration != null)
			{
				if (showMessages)
				{
					Messages.Message("AbilityAlreadyInspired".Translate(targetPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public static bool ValidateCanGetInspiration(Pawn targetPawn, bool showMessages)
		{
			if (targetPawn.mindState.inspirationHandler.GetRandomAvailableInspirationDef() == null)
			{
				if (showMessages)
				{
					Messages.Message("AbilityCantGetInspiredNow".Translate(targetPawn), targetPawn, MessageTypeDefOf.RejectInput, historical: false);
				}
				return false;
			}
			return true;
		}

		public static void DoClamor(IntVec3 cell, float radius, Thing source, ClamorDef clamor)
		{
			if (clamor != null)
			{
				GenClamor.DoClamor(source, cell, radius, clamor);
			}
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
}
