using RimWorld;

namespace Verse;

public static class BloodRainUtility
{
	public static bool ExposedToBloodRain(Pawn pawn)
	{
		if (pawn.MapHeld == null)
		{
			return false;
		}
		if (pawn.SpawnedParentOrMe is PawnFlyer)
		{
			return false;
		}
		GameCondition_BloodRain activeCondition = pawn.MapHeld.gameConditionManager.GetActiveCondition<GameCondition_BloodRain>();
		if (activeCondition == null || activeCondition.HiddenByOtherCondition(pawn.MapHeld))
		{
			return false;
		}
		if (activeCondition.TicksPassed > 2000 && pawn.Map != null)
		{
			return !pawn.Position.Roofed(pawn.Map);
		}
		return false;
	}

	public static void BloodRainTick(Pawn pawn)
	{
		if (ModsConfig.AnomalyActive && ExposedToBloodRain(pawn) && (pawn.RaceProps.Humanlike || pawn.IsAnimal) && !pawn.RaceProps.Dryad && !pawn.health.hediffSet.HasHediff(HediffDefOf.BloodRage))
		{
			pawn.health.AddHediff(HediffDefOf.BloodRage);
		}
	}

	public static bool TryTriggerBerserkShort(Pawn pawn)
	{
		if (ModsConfig.AnomalyActive && pawn.health.hediffSet.HasHediff(HediffDefOf.BloodRage))
		{
			bool num = MentalBreakDefOf.BerserkShort.Worker.TryStart(pawn, "MentalBreakReason_BloodRage".Translate(), causedByMood: false);
			if (num && MessagesRepeatAvoider.MessageShowAllowed("MessageBerserkBloodrain-" + pawn.LabelShort, 240f))
			{
				Messages.Message("Berserk_BloodRain".Translate(pawn), pawn, MessageTypeDefOf.NeutralEvent, historical: false);
			}
			return num;
		}
		return false;
	}
}
