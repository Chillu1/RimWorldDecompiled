using RimWorld;

namespace Verse.AI.Group;

public class Trigger_PawnHarmed : Trigger
{
	public float chance = 1f;

	public bool requireInstigatorWithFaction;

	public Faction requireInstigatorWithSpecificFaction;

	public DutyDef skipDuty;

	public int? minTicks;

	private int? minTick;

	public Trigger_PawnHarmed(float chance = 1f, bool requireInstigatorWithFaction = false, Faction requireInstigatorWithSpecificFaction = null, DutyDef skipDuty = null, int? minTicks = null)
	{
		this.chance = chance;
		this.requireInstigatorWithFaction = requireInstigatorWithFaction;
		this.requireInstigatorWithSpecificFaction = requireInstigatorWithSpecificFaction;
		this.skipDuty = skipDuty;
		this.minTicks = minTicks;
	}

	public override void SourceToilBecameActive(Transition transition, LordToil previousToil)
	{
		base.SourceToilBecameActive(transition, previousToil);
		if (minTicks.HasValue)
		{
			minTick = GenTicks.TicksGame + minTicks.Value;
		}
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
		if (signal.dinfo.IntendedTarget is Pawn pawn && pawn.mindState?.duty?.def == skipDuty)
		{
			return false;
		}
		if (minTick.HasValue && GenTicks.TicksGame < minTick.Value)
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
			if (signal.condition != PawnLostCondition.MadePrisoner && signal.condition != PawnLostCondition.Incapped)
			{
				return signal.condition == PawnLostCondition.Killed;
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
