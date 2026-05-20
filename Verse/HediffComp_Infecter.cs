using RimWorld;

namespace Verse;

public class HediffComp_Infecter : HediffComp
{
	private int ticksUntilInfect = -1;

	private float infectionChanceFactorFromTendRoom = 1f;

	public bool fromScaria;

	private const int UninitializedValue = -1;

	private const int WillNotInfectValue = -2;

	private const int FailedToMakeInfectionValue = -3;

	private const int AlreadyMadeInfectionValue = -4;

	private static readonly SimpleCurve InfectionChanceFactorFromTendQualityCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.7f),
		new CurvePoint(1f, 0.4f)
	};

	private static readonly SimpleCurve InfectionChanceFactorFromSeverityCurve = new SimpleCurve
	{
		new CurvePoint(1f, 0.1f),
		new CurvePoint(12f, 1f)
	};

	public HediffCompProperties_Infecter Props => (HediffCompProperties_Infecter)props;

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		if (parent.IsPermanent())
		{
			ticksUntilInfect = -2;
			return;
		}
		if (parent.Part != null)
		{
			if (parent.Part.def.IsSolid(parent.Part, base.Pawn.health.hediffSet.hediffs))
			{
				ticksUntilInfect = -2;
				return;
			}
			if (base.Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(parent.Part))
			{
				ticksUntilInfect = -2;
				return;
			}
		}
		float num = Props.infectionChance;
		if (base.Pawn.RaceProps.Animal)
		{
			num *= 0.1f;
		}
		if (Rand.Value <= num)
		{
			ticksUntilInfect = HealthTuning.InfectionDelayRange.RandomInRange;
		}
		else
		{
			ticksUntilInfect = -2;
		}
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref infectionChanceFactorFromTendRoom, "infectionChanceFactor", 0f);
		Scribe_Values.Look(ref ticksUntilInfect, "ticksUntilInfect", -2);
		Scribe_Values.Look(ref fromScaria, "fromScaria", defaultValue: false);
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (ticksUntilInfect > 0)
		{
			ticksUntilInfect -= delta;
			if (ticksUntilInfect <= 0)
			{
				CheckMakeInfection();
			}
		}
	}

	public override void CompTended(float quality, float maxQuality, int batchPosition = 0)
	{
		base.CompTended(quality, maxQuality, batchPosition);
		if (base.Pawn.Spawned)
		{
			Room room = base.Pawn.GetRoom();
			if (room != null)
			{
				infectionChanceFactorFromTendRoom = room.GetStat(RoomStatDefOf.InfectionChanceFactor);
			}
		}
	}

	private void CheckMakeInfection()
	{
		if (base.Pawn.health.immunity.DiseaseContractChanceFactor(HediffDefOf.WoundInfection, parent.Part) <= 0.001f)
		{
			ticksUntilInfect = -3;
			return;
		}
		if (base.Pawn.health.hediffSet.HasRegeneration)
		{
			ticksUntilInfect = -3;
			return;
		}
		float num = 1f;
		HediffComp_TendDuration hediffComp_TendDuration = parent.TryGetComp<HediffComp_TendDuration>();
		if (hediffComp_TendDuration != null && hediffComp_TendDuration.IsTended)
		{
			num *= infectionChanceFactorFromTendRoom;
			num *= InfectionChanceFactorFromTendQualityCurve.Evaluate(hediffComp_TendDuration.tendQuality);
		}
		num *= InfectionChanceFactorFromSeverityCurve.Evaluate(parent.Severity);
		if (base.Pawn.Faction == Faction.OfPlayer)
		{
			num *= Find.Storyteller.difficulty.playerPawnInfectionChanceFactor;
		}
		if (Rand.Value < num)
		{
			ticksUntilInfect = -4;
			base.Pawn.health.AddHediff(fromScaria ? HediffDefOf.ScariaInfection : HediffDefOf.WoundInfection, parent.Part);
		}
		else
		{
			ticksUntilInfect = -3;
		}
	}

	public override string CompDebugString()
	{
		if (ticksUntilInfect <= 0)
		{
			if (ticksUntilInfect == -4)
			{
				return "already created infection";
			}
			if (ticksUntilInfect == -3)
			{
				return "failed to make infection";
			}
			if (ticksUntilInfect == -2)
			{
				return "will not make infection";
			}
			if (ticksUntilInfect == -1)
			{
				return "uninitialized data!";
			}
			return "unexpected ticksUntilInfect = " + ticksUntilInfect;
		}
		return "infection may appear in: " + ticksUntilInfect + " ticks\ninfectChnceFactorFromTendRoom: " + infectionChanceFactorFromTendRoom.ToStringPercent();
	}
}
