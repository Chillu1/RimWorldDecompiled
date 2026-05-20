using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class SlaveRebellionUtility
{
	private enum SlaveRebellionType
	{
		GrandRebellion,
		LocalRebellion,
		SingleRebellion
	}

	private static List<float> computeCombinedMTBs = new List<float>();

	private static Dictionary<Pawn, CachedRebellionMtb> cachedSlaveRebellionMTB = new Dictionary<Pawn, CachedRebellionMtb>();

	private static Dictionary<Map, CachedRebellionMtb> cachedCombinedSlaveRebellionMTB = new Dictionary<Map, CachedRebellionMtb>();

	private static int pawnMtbCacheNextWipeTick = 0;

	private static int mapMtbCacheNextWipeTick = 0;

	private const int cacheWipeIntervalTicks = 60000;

	private const int MaxRegionsForDoorCount = 25;

	private const float WontRebelMTB = -1f;

	private const float BaseInitiateSlaveRebellionMtbDays = 45f;

	private const float WeaponProximityMultiplier = 4f;

	private const float WeaponMaxDistanceForProximityMultiplier = 6.9f;

	private const float RoomTouchesMapEdge = 1.7f;

	private const float UnattendedByColonistsMultiplier = 20f;

	private const float BlissLobotomyMultiplier = 10f;

	private static readonly SimpleCurve MovingCapacityFactorCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.01f),
		new CurvePoint(0.5f, 0.5f),
		new CurvePoint(1f, 1f)
	};

	private static readonly SimpleCurve SuppresionRebellionFactorCurve = new SimpleCurve
	{
		new CurvePoint(0f, 5f),
		new CurvePoint(0.333f, 1.5f),
		new CurvePoint(0.5f, 1f),
		new CurvePoint(1f, 0.25f)
	};

	private static readonly SimpleCurve MoodRebellionFactorCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1.5f),
		new CurvePoint(0.5f, 1f),
		new CurvePoint(1f, 0.8f)
	};

	private static readonly SimpleCurve SlaveCountFactorCurve = new SimpleCurve
	{
		new CurvePoint(1f, 1f),
		new CurvePoint(2f, 0.75f),
		new CurvePoint(5f, 0.5f),
		new CurvePoint(10f, 0.3f),
		new CurvePoint(20f, 0.2f)
	};

	private const float SapperChance = 0.5f;

	private const float LocalRebellionSearchDistance = 35f;

	private const float AggressiveRebellionChance = 0.5f;

	private const float BaseMeleeSlaveSuppressionPct = 0.02f;

	private const float DamageSuppressionMultiplier = 0.2f;

	private static readonly SimpleCurve CurrentSuppressionFactorCurve = new SimpleCurve
	{
		new CurvePoint(0f, 2f),
		new CurvePoint(0.5f, 1f),
		new CurvePoint(1f, 0.5f)
	};

	private static List<Pawn> rebellingSlaves = new List<Pawn>();

	private static List<Pawn> allPossibleRebellingSlaves = new List<Pawn>();

	private static List<Pawn> tmpSlaves = new List<Pawn>();

	public static float InitiateSlaveRebellionMtbDays(Pawn pawn)
	{
		int ticksGame = Find.TickManager.TicksGame;
		if (ticksGame >= pawnMtbCacheNextWipeTick)
		{
			cachedSlaveRebellionMTB.Clear();
			pawnMtbCacheNextWipeTick = ticksGame + 60000;
		}
		if (!pawn.IsSlave)
		{
			return -1f;
		}
		CachedRebellionMtb cachedRebellionMtb = cachedSlaveRebellionMTB.TryGetValue(pawn);
		if (cachedRebellionMtb != null && cachedRebellionMtb.gameTick == ticksGame)
		{
			return cachedRebellionMtb.mtb;
		}
		if (cachedRebellionMtb == null)
		{
			cachedRebellionMtb = new CachedRebellionMtb();
			cachedSlaveRebellionMTB[pawn] = cachedRebellionMtb;
		}
		cachedRebellionMtb.gameTick = ticksGame;
		cachedRebellionMtb.mtb = InitiateSlaveRebellionMtbDaysHelper(pawn);
		return cachedRebellionMtb.mtb;
	}

	private static float InitiateSlaveRebellionMtbDaysHelper(Pawn pawn)
	{
		if (!CanParticipateInSlaveRebellion(pawn))
		{
			return -1f;
		}
		if (!pawn.needs.TryGetNeed(out Need_Suppression need))
		{
			return -1f;
		}
		float num = 45f;
		num /= MovingCapacityFactorCurve.Evaluate(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
		num /= SuppresionRebellionFactorCurve.Evaluate(need.CurLevelPercentage);
		num /= SlaveCountFactorCurve.Evaluate(pawn.Map.mapPawns.SlavesOfColonySpawned.Count);
		if (pawn.needs.mood != null)
		{
			num /= MoodRebellionFactorCurve.Evaluate(pawn.needs.mood.CurLevelPercentage);
		}
		if (InRoomTouchingMapEdge(pawn))
		{
			num /= 1.7f;
		}
		if (CanApplyWeaponFactor(pawn))
		{
			num /= 4f;
		}
		if (IsUnattendedByColonists(pawn.Map))
		{
			num /= 20f;
		}
		if (BlissLobotomized(pawn))
		{
			num *= 10f;
		}
		return num;
	}

	public static void IncrementInteractionSuppression(Pawn initiator, Pawn recipient)
	{
		if (initiator != null && recipient != null && recipient.needs != null && recipient.needs.TryGetNeed(out Need_Suppression need))
		{
			float statValue = initiator.GetStatValue(StatDefOf.SuppressionPower);
			float num = CurrentSuppressionFactorCurve.Evaluate(need.CurLevel);
			IncrementSuppression(need, initiator, recipient, statValue * num * need.MaxLevel);
		}
	}

	public static void IncrementMeleeSuppression(Pawn initiator, Pawn recipient, float damageDealt)
	{
		if (initiator != null && recipient != null && recipient.needs != null && recipient.needs.TryGetNeed(out Need_Suppression need))
		{
			float num = CurrentSuppressionFactorCurve.Evaluate(need.CurLevel);
			float num2 = 0.2f * damageDealt;
			IncrementSuppression(need, initiator, recipient, 0.02f * num * need.MaxLevel * num2);
		}
	}

	public static void IncrementSuppression(Need_Suppression suppressionNeed, Pawn initiator, Pawn recipient, float suppressionIncrementPct)
	{
		if (suppressionNeed != null && initiator != null && recipient != null)
		{
			suppressionNeed.CurLevelPercentage += suppressionIncrementPct;
			TaggedString taggedString = "TextMote_SuppressionIncreased".Translate(suppressionNeed.CurLevel.ToStringPercent());
			MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, taggedString, 4f);
		}
	}

	private static bool InRoomTouchingMapEdge(Pawn pawn)
	{
		return pawn.GetRoom()?.TouchesMapEdge ?? false;
	}

	private static bool CanApplyWeaponFactor(Pawn pawn)
	{
		if (pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return false;
		}
		ThingWithComps primary = pawn.equipment.Primary;
		if (primary == null || !primary.def.IsWeapon || !WeaponUsableInRebellion(primary))
		{
			return GoodWeaponInSameRoom(pawn);
		}
		return true;
	}

	public static bool IsUnattendedByColonists(Map map)
	{
		foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
		{
			if (!item.IsSlave && !item.Downed && !item.Dead)
			{
				return false;
			}
		}
		return true;
	}

	public static bool BlissLobotomized(Pawn p)
	{
		if (ModsConfig.AnomalyActive)
		{
			return p.health.hediffSet.HasHediff(HediffDefOf.BlissLobotomy);
		}
		return false;
	}

	public static string GetAnySlaveRebellionExplanation(Pawn pawn)
	{
		int count = pawn.Map.mapPawns.SlavesOfColonySpawned.Count;
		if (count > 1 && RebellionForAnySlaveInMapMtbDays(pawn.Map) != -1f)
		{
			int numTicks = (int)(RebellionForAnySlaveInMapMtbDays(pawn.Map) * 60000f);
			return string.Format("\n{0}", "SuppressionRebellionAnySlave".Translate(count, numTicks.ToStringTicksToPeriod()));
		}
		return "";
	}

	public static string GetSlaveRebellionMtbCalculationExplanation(Pawn pawn)
	{
		if (!pawn.needs.TryGetNeed(out Need_Suppression need) || !CanParticipateInSlaveRebellion(pawn))
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Format("{0}: {1}", "SuppressionBaseInterval".Translate(), 2700000.ToStringTicksToPeriodVague()));
		float f = 1f / MovingCapacityFactorCurve.Evaluate(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving));
		stringBuilder.AppendLine(string.Format("{0}: x{1}", "SuppressionMovingCapacityFactor".Translate(), f.ToStringPercent()));
		float f2 = 1f / SuppresionRebellionFactorCurve.Evaluate(need.CurLevelPercentage);
		stringBuilder.AppendLine(string.Format("{0}: x{1}", "SuppressionFactor".Translate(), f2.ToStringPercent()));
		float f3 = 1f / MoodRebellionFactorCurve.Evaluate(pawn.needs.mood.CurLevelPercentage);
		stringBuilder.AppendLine(string.Format("{0}: x{1}", "SuppressionMoodFactor".Translate(), f3.ToStringPercent()));
		int count = pawn.Map.mapPawns.SlavesOfColonySpawned.Count;
		float f4 = 1f / SlaveCountFactorCurve.Evaluate(count);
		stringBuilder.AppendLine(string.Format("{0}: x{1}", "SuppressionSlaveCountFactor".Translate(), f4.ToStringPercent()));
		if (InRoomTouchingMapEdge(pawn))
		{
			float f5 = 0.58823526f;
			stringBuilder.AppendLine(string.Format("{0}: x{1}", "SuppressionEscapeFactor".Translate(), f5.ToStringPercent()));
		}
		if (CanApplyWeaponFactor(pawn))
		{
			float f6 = 0.25f;
			stringBuilder.AppendLine(string.Format("{0}: x{1}", "SuppressionWeaponProximityFactor".Translate(), f6.ToStringPercent()));
		}
		if (IsUnattendedByColonists(pawn.Map))
		{
			float f7 = 0.05f;
			stringBuilder.AppendLine(string.Format("{0}: x{1}", "SuppressionUnattendedByColonists".Translate(), f7.ToStringPercent()));
		}
		if (BlissLobotomized(pawn))
		{
			stringBuilder.AppendLine(string.Format("{0}: x{1}", "BlissLobotomy".Translate(), 10f.ToStringPercent()));
		}
		stringBuilder.Append(string.Format("{0}: {1}", "SuppressionFinalInterval".Translate(), ((int)(InitiateSlaveRebellionMtbDays(pawn) * 60000f)).ToStringTicksToPeriod()));
		return stringBuilder.ToString();
	}

	private static bool IsDamagelessWeapon(Thing t)
	{
		ProjectileProperties projectileProperties = t?.def?.Verbs?.FirstOrDefault((VerbProperties v) => v.isPrimary)?.defaultProjectile?.projectile;
		if (projectileProperties == null || projectileProperties.GetDamageAmount(null) != 0)
		{
			if (projectileProperties == null)
			{
				return false;
			}
			return projectileProperties.damageDef?.harmsHealth == false;
		}
		return true;
	}

	public static bool WeaponUsableInRebellion(Thing weapon)
	{
		if (weapon != null && !IsDamagelessWeapon(weapon) && weapon.def != ThingDefOf.WoodLog)
		{
			return !PawnWeaponGenerator.IsDerpWeapon(weapon.def, weapon.Stuff);
		}
		return false;
	}

	public static float RebellionForAnySlaveInMapMtbDays(Map m)
	{
		int ticksGame = Find.TickManager.TicksGame;
		if (ticksGame >= mapMtbCacheNextWipeTick)
		{
			cachedCombinedSlaveRebellionMTB.Clear();
			mapMtbCacheNextWipeTick = ticksGame + 60000;
		}
		CachedRebellionMtb cachedRebellionMtb = cachedCombinedSlaveRebellionMTB.TryGetValue(m);
		if (cachedRebellionMtb != null && cachedRebellionMtb.gameTick == ticksGame)
		{
			return cachedRebellionMtb.mtb;
		}
		if (cachedRebellionMtb == null)
		{
			cachedRebellionMtb = new CachedRebellionMtb();
			cachedCombinedSlaveRebellionMTB[m] = cachedRebellionMtb;
		}
		cachedRebellionMtb.gameTick = ticksGame;
		cachedRebellionMtb.mtb = RebellionForAnySlaveInMapMtbDaysHelper(m);
		return cachedRebellionMtb.mtb;
	}

	private static float RebellionForAnySlaveInMapMtbDaysHelper(Map m)
	{
		computeCombinedMTBs.Clear();
		foreach (Pawn item in m.mapPawns.SlavesOfColonySpawned)
		{
			float num = InitiateSlaveRebellionMtbDays(item);
			if (num != -1f)
			{
				computeCombinedMTBs.Add(num);
			}
		}
		if (computeCombinedMTBs.Count <= 0)
		{
			return -1f;
		}
		return Rand.CombineMTBs(computeCombinedMTBs);
	}

	private static bool GoodWeaponInSameRoom(Pawn pawn)
	{
		Room room = pawn.GetRoom();
		if (room == null || room.PsychologicallyOutdoors)
		{
			return false;
		}
		ThingRequest thingReq = ThingRequest.ForGroup(ThingRequestGroup.Weapon);
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, thingReq, PathEndMode.Touch, TraverseParms.For(pawn), 6.9f, (Thing t) => EquipmentUtility.CanEquip(t, pawn) && WeaponUsableInRebellion(t) && t.GetRoom() == room) != null;
	}

	public static bool CanParticipateInSlaveRebellion(Pawn pawn)
	{
		if (!pawn.Spawned)
		{
			return false;
		}
		if (pawn.Map.IsPocketMap && pawn.Map.generatorDef.pocketMapProperties.preventPrisonerEscape)
		{
			return false;
		}
		if (pawn.Map.Biome.inVacuum)
		{
			return false;
		}
		if (!pawn.Downed && pawn.Spawned && pawn.IsSlave && !pawn.InMentalState && pawn.Awake())
		{
			return !IsRebelling(pawn);
		}
		return false;
	}

	public static bool IsRebelling(Pawn pawn)
	{
		Lord lord = pawn.GetLord();
		if (lord != null)
		{
			return lord.LordJob is LordJob_SlaveRebellion;
		}
		return false;
	}

	public static bool StartSlaveRebellion(Pawn initiator, bool forceAggressive = false)
	{
		if (StartSlaveRebellion(initiator, out var letterText, out var letterLabel, out var letterDef, out var lookTargets, forceAggressive))
		{
			Find.LetterStack.ReceiveLetter(letterLabel, letterText, letterDef, lookTargets);
			return true;
		}
		return false;
	}

	public static bool StartSlaveRebellion(Pawn initiator, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets, bool forceAggressive = false)
	{
		letterText = null;
		letterLabel = null;
		letterDef = null;
		lookTargets = null;
		if (!ModLister.CheckIdeology("Slave rebellion"))
		{
			return false;
		}
		rebellingSlaves.Clear();
		rebellingSlaves.Add(initiator);
		allPossibleRebellingSlaves.Clear();
		List<Pawn> slavesOfColonySpawned = initiator.Map.mapPawns.SlavesOfColonySpawned;
		for (int i = 0; i < slavesOfColonySpawned.Count; i++)
		{
			Pawn pawn = slavesOfColonySpawned[i];
			if (pawn != initiator && CanParticipateInSlaveRebellion(pawn))
			{
				allPossibleRebellingSlaves.Add(pawn);
			}
		}
		SlaveRebellionType slaveRebellionType = DecideSlaveRebellionType();
		switch (slaveRebellionType)
		{
		case SlaveRebellionType.GrandRebellion:
		{
			for (int k = 0; k < allPossibleRebellingSlaves.Count; k++)
			{
				rebellingSlaves.Add(allPossibleRebellingSlaves[k]);
			}
			break;
		}
		case SlaveRebellionType.LocalRebellion:
		{
			for (int j = 0; j < allPossibleRebellingSlaves.Count; j++)
			{
				Pawn pawn2 = allPossibleRebellingSlaves[j];
				if (!(initiator.Position.DistanceTo(pawn2.Position) > 35f))
				{
					rebellingSlaves.Add(pawn2);
				}
			}
			break;
		}
		}
		if (rebellingSlaves.Count == 1)
		{
			slaveRebellionType = SlaveRebellionType.SingleRebellion;
		}
		else if (rebellingSlaves.Count == allPossibleRebellingSlaves.Count)
		{
			slaveRebellionType = SlaveRebellionType.GrandRebellion;
		}
		if (!RCellFinder.TryFindRandomExitSpot(initiator, out var spot, TraverseMode.PassDoors))
		{
			return false;
		}
		if (!PrisonBreakUtility.TryFindGroupUpLoc(rebellingSlaves, spot, out var groupUpLoc))
		{
			return false;
		}
		bool flag = forceAggressive || Rand.Chance(0.5f);
		switch (slaveRebellionType)
		{
		case SlaveRebellionType.GrandRebellion:
			if (flag)
			{
				letterLabel = "LetterLabelGrandSlaveRebellion".Translate();
				letterText = "LetterGrandSlaveRebellion".Translate(GenLabel.ThingsLabel(rebellingSlaves));
			}
			else
			{
				letterLabel = "LetterLabelGrandSlaveEscape".Translate();
				letterText = "LetterGrandSlaveEscape".Translate(GenLabel.ThingsLabel(rebellingSlaves));
			}
			break;
		case SlaveRebellionType.LocalRebellion:
			if (flag)
			{
				letterLabel = "LetterLabelLocalSlaveRebellion".Translate();
				letterText = "LetterLocalSlaveRebellion".Translate(initiator, GenLabel.ThingsLabel(rebellingSlaves));
			}
			else
			{
				letterLabel = "LetterLabelLocalSlaveEscape".Translate();
				letterText = "LetterLocalSlaveEscape".Translate(initiator, GenLabel.ThingsLabel(rebellingSlaves));
			}
			break;
		case SlaveRebellionType.SingleRebellion:
			if (flag)
			{
				letterLabel = "LetterLabelSingleSlaveRebellion".Translate() + (": " + initiator.LabelShort);
				letterText = "LetterSingleSlaveRebellion".Translate(initiator);
			}
			else
			{
				letterLabel = "LetterLabelSingleSlaveEscape".Translate() + (": " + initiator.LabelShort);
				letterText = "LetterSingleSlaveEscape".Translate(initiator);
			}
			break;
		default:
			Log.Error($"Unkown slave rebellion type {slaveRebellionType}");
			break;
		}
		letterText += "\n\n" + "SlaveRebellionSuppressionExplanation".Translate();
		lookTargets = new LookTargets(rebellingSlaves);
		letterDef = LetterDefOf.ThreatBig;
		int sapperThingID = -1;
		if (Rand.Value < 0.5f)
		{
			sapperThingID = initiator.thingIDNumber;
		}
		for (int l = 0; l < rebellingSlaves.Count; l++)
		{
			rebellingSlaves[l].GetLord()?.Notify_PawnLost(rebellingSlaves[l], PawnLostCondition.ForcedToJoinOtherLord);
		}
		LordMaker.MakeNewLord(rebellingSlaves[0].Faction, new LordJob_SlaveRebellion(groupUpLoc, spot, sapperThingID, !flag), initiator.Map, rebellingSlaves);
		for (int m = 0; m < rebellingSlaves.Count; m++)
		{
			if (!rebellingSlaves[m].Awake())
			{
				RestUtility.WakeUp(rebellingSlaves[m]);
			}
			rebellingSlaves[m].drafter.Drafted = false;
			if (rebellingSlaves[m].CurJob != null)
			{
				rebellingSlaves[m].jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			rebellingSlaves[m].Map.attackTargetsCache.UpdateTarget(rebellingSlaves[m]);
			if (rebellingSlaves[m].carryTracker.CarriedThing != null)
			{
				rebellingSlaves[m].carryTracker.TryDropCarriedThing(rebellingSlaves[m].Position, ThingPlaceMode.Near, out var _);
			}
		}
		rebellingSlaves.Clear();
		return true;
	}

	private static SlaveRebellionType DecideSlaveRebellionType()
	{
		return Enum.GetValues(typeof(SlaveRebellionType)).Cast<SlaveRebellionType>().RandomElement();
	}

	public static Pawn FindSlaveForRebellion(Pawn pawn)
	{
		if (!pawn.Spawned)
		{
			return null;
		}
		tmpSlaves.Clear();
		IReadOnlyList<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			Pawn pawn2 = allPawnsSpawned[i];
			if (pawn2.IsSlave && pawn2.SlaveFaction == pawn.SlaveFaction && pawn2 != pawn && !pawn2.Downed && !pawn2.InMentalState && !pawn2.IsBurning() && pawn2.Awake() && CanParticipateInSlaveRebellion(pawn2) && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly))
			{
				tmpSlaves.Add(pawn2);
			}
		}
		if (!tmpSlaves.Any())
		{
			return null;
		}
		Pawn result = tmpSlaves.RandomElement();
		tmpSlaves.Clear();
		return result;
	}

	public static void ClearCache()
	{
		cachedSlaveRebellionMTB.Clear();
		cachedCombinedSlaveRebellionMTB.Clear();
		pawnMtbCacheNextWipeTick = 0;
		mapMtbCacheNextWipeTick = 0;
	}
}
