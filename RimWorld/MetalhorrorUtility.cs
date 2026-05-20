using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class MetalhorrorUtility
{
	public const float MetalhorrorLOS = 29.9f;

	private const int MTBUpdateRateTicks = 300;

	public const int MaxAgeHoursLarva = 24;

	public const int MaxAgeHoursJuvenile = 72;

	private const int LarvaLifeStageIndex = 0;

	private const int JuvenileLifeStageIndex = 1;

	private const int MatureLifeStageIndex = 2;

	private const int SpawnStunTicks = 60;

	private static readonly SimpleCurve NaturalEmergenceMTBCurve = new SimpleCurve
	{
		new CurvePoint(0.19f, 7200000f),
		new CurvePoint(0.2f, 1800000f),
		new CurvePoint(0.4f, 240000f),
		new CurvePoint(0.5f, 1800f)
	};

	private static int cachedFrame;

	private static readonly Dictionary<Map, float> cachedMTB = new Dictionary<Map, float>();

	public static void TryEmerge(Pawn infected, string reasonKey = null, bool sympathetic = false)
	{
		infected.health.hediffSet.GetFirstHediff<Hediff_MetalhorrorImplant>()?.Emerge(reasonKey, sympathetic);
	}

	public static Pawn SpawnMetalhorror(Pawn infected, Hediff_MetalhorrorImplant hediff)
	{
		if (!ModLister.CheckAnomaly("Metalhorror"))
		{
			return null;
		}
		if (hediff == null)
		{
			Log.Error("Attempted to emerged from pawn which did not have a Metalhorror implant hediff");
			return null;
		}
		Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Metalhorror, Faction.OfEntities));
		if (!GenAdj.TryFindRandomAdjacentCell8WayWithRoom(infected.SpawnedParentOrMe, out var result))
		{
			result = infected.PositionHeld;
		}
		CompMetalhorror compMetalhorror = pawn.TryGetComp<CompMetalhorror>();
		compMetalhorror.emergedFrom = infected;
		compMetalhorror.implantSource = hediff.ImplantSource;
		int index = 2;
		int num = hediff.ageTicks / 2500;
		if (num <= 24)
		{
			index = 0;
		}
		else if (num <= 72)
		{
			index = 1;
		}
		pawn.ageTracker.LockCurrentLifeStageIndex(index);
		pawn.ageTracker.AgeBiologicalTicks = hediff.ageTicks;
		pawn.ageTracker.AgeChronologicalTicks = hediff.ageTicks;
		Pawn pawn2 = (Pawn)GenSpawn.Spawn(pawn, result, infected.MapHeld);
		compMetalhorror.FindOrCreateEmergedLord();
		infected.health.RemoveHediff(hediff);
		Find.BattleLog.Add(new BattleLogEntry_Event(infected, RulePackDefOf.Event_MetalhorrorEmerged, pawn2));
		pawn2.stances.stunner.StunFor(60, null, addBattleLog: false);
		return pawn2;
	}

	public static Thing FindTarget(Pawn pawn)
	{
		TargetScanFlags flags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
		return (Thing)AttackTargetFinder.BestAttackTarget(pawn, flags, null, 0f, 29.9f);
	}

	public static void Infect(Pawn pawn, Pawn source = null, string descKey = "InsectImplant", string descResolved = null)
	{
		if (!CanBeInfected(pawn))
		{
			return;
		}
		ImplantSource implantSource = new ImplantSource(source, descKey, descResolved);
		if (!Find.Anomaly.emergedBiosignatures.Contains(implantSource.Biosignature))
		{
			if (source == null)
			{
				Find.Anomaly.Notify_NewMetalhorrorBiosignatureImplanted();
			}
			((Hediff_MetalhorrorImplant)pawn.health.AddHediff(HediffDefOf.MetalhorrorImplant)).ImplantSource = implantSource;
		}
	}

	public static bool IsInfected(Pawn pawn)
	{
		Hediff_MetalhorrorImplant hediff;
		return IsInfected(pawn, out hediff);
	}

	public static bool IsInfected(Pawn pawn, out Hediff_MetalhorrorImplant hediff)
	{
		hediff = null;
		if (!pawn.RaceProps.IsFlesh)
		{
			return false;
		}
		hediff = pawn.health.hediffSet.GetFirstHediff<Hediff_MetalhorrorImplant>();
		return hediff != null;
	}

	public static bool CanBeInfected(Pawn pawn)
	{
		if (ModsConfig.AnomalyActive && pawn.RaceProps.Humanlike && !pawn.DevelopmentalStage.Baby())
		{
			return !IsInfected(pawn);
		}
		return false;
	}

	public static void Detect(Pawn infected, string reason, string subtleMessage, float realisedDetectedChance)
	{
		Hediff_MetalhorrorImplant firstHediff = infected.health.hediffSet.GetFirstHediff<Hediff_MetalhorrorImplant>();
		if (firstHediff != null && !firstHediff.Visible)
		{
			firstHediff.Detect(reason, Rand.Chance(realisedDetectedChance));
			if (!firstHediff.KnowsDetected)
			{
				Find.LetterStack.ReceiveLetter("MetalhorrorDetected".Translate(), subtleMessage, LetterDefOf.ThreatSmall);
			}
		}
	}

	public static bool ShouldRandomEmerge(Pawn pawn, int delta)
	{
		if (!pawn.IsHashIntervalTick(300, delta))
		{
			return false;
		}
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!pawn.SpawnedOrAnyParentSpawned)
		{
			return false;
		}
		Hediff_MetalhorrorImplant firstHediff = pawn.health.hediffSet.GetFirstHediff<Hediff_MetalhorrorImplant>();
		if (firstHediff == null)
		{
			return false;
		}
		if (firstHediff.Emerging)
		{
			return false;
		}
		if (cachedFrame != GenTicks.TicksGame)
		{
			cachedFrame = GenTicks.TicksGame;
			cachedMTB.Clear();
		}
		if (!cachedMTB.ContainsKey(pawn.MapHeld))
		{
			int num = 0;
			int num2 = 0;
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive)
			{
				if (item.RaceProps.Humanlike && (!item.IsMutant || !item.mutant.Def.preventIllnesses) && (item.Faction == Faction.OfPlayer || item.HostFaction == Faction.OfPlayer) && !item.IsPrisoner)
				{
					if (IsInfected(item))
					{
						num++;
					}
					num2++;
				}
			}
			if (num2 == 0)
			{
				cachedMTB[pawn.MapHeld] = -1f;
				return false;
			}
			float x = (float)num / (float)num2;
			cachedMTB[pawn.MapHeld] = NaturalEmergenceMTBCurve.Evaluate(x);
		}
		if (cachedMTB[pawn.MapHeld] >= 0f)
		{
			return Rand.MTBEventOccurs(cachedMTB[pawn.MapHeld], 1f, 300f);
		}
		return false;
	}

	public static bool TryPawnExitMap(Pawn pawn)
	{
		bool flag = pawn.guest != null && pawn.guest.Released;
		if (!pawn.Spawned || !flag || !IsInfected(pawn))
		{
			return true;
		}
		TryEmerge(pawn, "MetalhorrorReasonTriedToLeaveMap".Translate(pawn.Named("INFECTED")));
		return false;
	}
}
