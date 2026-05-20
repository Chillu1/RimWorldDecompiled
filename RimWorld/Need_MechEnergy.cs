using System.Text;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class Need_MechEnergy : Need
{
	private bool selfShutdown;

	public Building_MechCharger currentCharger;

	public const float BaseFallPerDayActive = 10f;

	public const float BaseFallPerDayIdle = 3f;

	public const float BaseGainPerDaySelfShutdown = 1f;

	public const float ShutdownUntil = 15f;

	public override float MaxLevel => pawn.RaceProps.maxMechEnergy;

	public bool IsSelfShutdown
	{
		get
		{
			if (pawn.CurJobDef == JobDefOf.SelfShutdown)
			{
				return pawn.GetPosture().Laying();
			}
			return false;
		}
	}

	public bool IsLowEnergySelfShutdown
	{
		get
		{
			if (IsSelfShutdown)
			{
				return selfShutdown;
			}
			return false;
		}
	}

	public override int GUIChangeArrow
	{
		get
		{
			if (FallPerDay > 0f)
			{
				return -1;
			}
			if (IsSelfShutdown || pawn.IsCharging())
			{
				return 1;
			}
			return 0;
		}
	}

	private float BaseFallPerDay
	{
		get
		{
			if (pawn.mindState != null && !pawn.mindState.IsIdle && !pawn.IsGestating())
			{
				return 10f;
			}
			return 3f;
		}
	}

	public float FallPerDay
	{
		get
		{
			if (pawn.Downed)
			{
				return 0f;
			}
			if (!pawn.Awake())
			{
				return 0f;
			}
			if (IsSelfShutdown)
			{
				return 0f;
			}
			if (currentCharger != null)
			{
				return 0f;
			}
			if (pawn.IsCaravanMember())
			{
				return 0f;
			}
			return BaseFallPerDay * pawn.GetStatValue(StatDefOf.MechEnergyUsageFactor);
		}
	}

	public Need_MechEnergy(Pawn pawn)
		: base(pawn)
	{
	}

	public override void NeedInterval()
	{
		float num = 400f;
		if (!IsSelfShutdown)
		{
			CurLevel -= FallPerDay / num;
		}
		else
		{
			CurLevel += 1f / num;
		}
		if (CurLevel <= 0f)
		{
			selfShutdown = true;
		}
		else if (CurLevel >= 15f || pawn.CurJobDef == JobDefOf.MechCharge)
		{
			selfShutdown = false;
		}
		Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.SelfShutdown);
		if (firstHediffOfDef != null && !selfShutdown)
		{
			pawn.health.RemoveHediff(firstHediffOfDef);
			pawn.jobs?.CheckForJobOverride();
		}
		else if (firstHediffOfDef == null && selfShutdown)
		{
			pawn.health.AddHediff(HediffDefOf.SelfShutdown);
		}
		if (selfShutdown && pawn.Spawned && !pawn.Downed && pawn.CurJobDef != JobDefOf.SelfShutdown && pawn.jobs != null)
		{
			IntVec3 result = pawn.Position;
			RCellFinder.TryFindNearbyMechSelfShutdownSpot(pawn.Position, pawn, pawn.Map, out result, allowForbidden: true);
			if (pawn.Drafted)
			{
				pawn.drafter.Drafted = false;
			}
			pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.SelfShutdown, result), JobCondition.InterruptForced, null, resumeCurJobAfterwards: false, cancelBusyStances: true, null, JobTag.SatisfyingNeeds);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref selfShutdown, "selfShutdown", defaultValue: false);
		Scribe_References.Look(ref currentCharger, "currentCharger");
	}

	public override string GetTipString()
	{
		StringBuilder stringBuilder = new StringBuilder(base.GetTipString());
		stringBuilder.AppendInNewLine("CurrentMechEnergyFallPerDay".Translate() + ": " + (FallPerDay / 100f).ToStringPercent());
		return stringBuilder.ToString();
	}
}
