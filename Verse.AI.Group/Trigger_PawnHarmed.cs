using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_PawnHarmed : Trigger
	{
		public float chance = 1f;

		public bool requireInstigatorWithFaction;

		public Faction requireInstigatorWithSpecificFaction;

		public Trigger_PawnHarmed(float chance = 1f, bool requireInstigatorWithFaction = false, Faction requireInstigatorWithSpecificFaction = null)
		{
			this.chance = chance;
			this.requireInstigatorWithFaction = requireInstigatorWithFaction;
			this.requireInstigatorWithSpecificFaction = requireInstigatorWithSpecificFaction;
		}

		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (!SignalIsHarm(signal))
			{
				return false;
			}
			if (requireInstigatorWithFaction && (signal.dinfo.Instigator == null || signal.dinfo.Instigator.Faction == null))
			{
				return false;
			}
			if (requireInstigatorWithSpecificFaction != null && (signal.dinfo.Instigator == null || signal.dinfo.Instigator.Faction != requireInstigatorWithSpecificFaction))
			{
				return false;
			}
			return Rand.Value < chance;
		}

		public static bool SignalIsHarm(TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnDamaged)
			{
				return signal.dinfo.Def.ExternalViolenceFor(signal.Pawn);
			}
			if (signal.type == TriggerSignalType.PawnLost)
			{
				if (signal.condition != PawnLostCondition.MadePrisoner)
				{
					return signal.condition == PawnLostCondition.IncappedOrKilled;
				}
				return true;
			}
			if (signal.type == TriggerSignalType.PawnArrestAttempted)
			{
				return true;
			}
			return false;
		}
	}
}
