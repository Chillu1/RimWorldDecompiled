using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class PawnUtility
	{
		private const float HumanFilthFactor = 4f;

		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<string> tmpPawnKindsStr = new List<string>();

		private static HashSet<PawnKindDef> tmpAddedPawnKinds = new HashSet<PawnKindDef>();

		private static List<PawnKindDef> tmpPawnKinds = new List<PawnKindDef>();

		private const float RecruitDifficultyMin = 0.1f;

		private const float RecruitDifficultyMax = 0.99f;

		private const float RecruitDifficultyGaussianWidthFactor = 0.15f;

		private const float RecruitDifficultyOffsetPerTechDiff = 0.16f;

		private static List<Thing> tmpThings = new List<Thing>();

		public static Faction GetFactionLeaderFaction(Pawn pawn)
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (allFactionsListForReading[i].leader == pawn)
				{
					return allFactionsListForReading[i];
				}
			}
			return null;
		}

		public static bool IsFactionLeader(Pawn pawn)
		{
			return GetFactionLeaderFaction(pawn) != null;
		}

		public static bool IsInteractionBlocked(this Pawn pawn, InteractionDef interaction, bool isInitiator, bool isRandom)
		{
			MentalStateDef mentalStateDef = pawn.MentalStateDef;
			if (mentalStateDef != null)
			{
				if (isRandom)
				{
					return mentalStateDef.blockRandomInteraction;
				}
				if (interaction == null)
				{
					return false;
				}
				List<InteractionDef> list = (isInitiator ? mentalStateDef.blockInteractionInitiationExcept : mentalStateDef.blockInteractionRecipientExcept);
				if (list != null)
				{
					return !list.Contains(interaction);
				}
				return false;
			}
			return false;
		}

		public static bool IsKidnappedPawn(Pawn pawn)
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (allFactionsListForReading[i].kidnapped.KidnappedPawnsListForReading.Contains(pawn))
				{
					return true;
				}
			}
			return false;
		}

		public static bool IsTravelingInTransportPodWorldObject(Pawn pawn)
		{
			if (!pawn.IsWorldPawn() || !ThingOwnerUtility.AnyParentIs<ActiveDropPodInfo>(pawn))
			{
				return ThingOwnerUtility.AnyParentIs<TravelingTransportPods>(pawn);
			}
			return true;
		}

		public static bool ForSaleBySettlement(Pawn pawn)
		{
			return pawn.ParentHolder is Settlement_TraderTracker;
		}

		public static bool IsInvisible(this Pawn pawn)
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].TryGetComp<HediffComp_Invisibility>() != null)
				{
					return true;
				}
			}
			return false;
		}

		public static void TryDestroyStartingColonistFamily(Pawn pawn)
		{
			if (!pawn.relations.RelatedPawns.Any((Pawn x) => Find.GameInitData.startingAndOptionalPawns.Contains(x)))
			{
				DestroyStartingColonistFamily(pawn);
			}
		}

		public static void DestroyStartingColonistFamily(Pawn pawn)
		{
			foreach (Pawn item in pawn.relations.RelatedPawns.ToList())
			{
				if (!Find.GameInitData.startingAndOptionalPawns.Contains(item))
				{
					WorldPawnSituation situation = Find.WorldPawns.GetSituation(item);
					if (situation == WorldPawnSituation.Free || situation == WorldPawnSituation.Dead)
					{
						Find.WorldPawns.RemovePawn(item);
						Find.WorldPawns.PassToWorld(item, PawnDiscardDecideMode.Discard);
					}
				}
			}
		}

		public static bool EnemiesAreNearby(Pawn pawn, int regionsToScan = 9, bool passDoors = false)
		{
			TraverseParms tp = (passDoors ? TraverseParms.For(TraverseMode.PassDoors) : TraverseParms.For(pawn));
			bool foundEnemy = false;
			RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(tp, isDestination: false), delegate(Region r)
			{
				List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].HostileTo(pawn))
					{
						foundEnemy = true;
						return true;
					}
				}
				return foundEnemy;
			}, regionsToScan);
			return foundEnemy;
		}

		public static bool WillSoonHaveBasicNeed(Pawn p)
		{
			if (p.needs == null)
			{
				return false;
			}
			if (p.needs.rest != null && p.needs.rest.CurLevel < 0.33f)
			{
				return true;
			}
			if (p.needs.food != null && p.needs.food.CurLevelPercentage < p.needs.food.PercentageThreshHungry + 0.05f)
			{
				return true;
			}
			return false;
		}

		public static float AnimalFilthChancePerCell(ThingDef def, float bodySize)
		{
			return bodySize * 0.00125f * (1f - def.race.petness);
		}

		public static float HumanFilthChancePerCell(ThingDef def, float bodySize)
		{
			return bodySize * 0.00125f * 4f;
		}

		public static bool CanCasuallyInteractNow(this Pawn p, bool twoWayInteraction = false)
		{
			if (p.Drafted)
			{
				return false;
			}
			if (p.IsInvisible())
			{
				return false;
			}
			if (ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(p))
			{
				return false;
			}
			if (p.InAggroMentalState)
			{
				return false;
			}
			if (!p.Awake())
			{
				return false;
			}
			if (p.IsFormingCaravan())
			{
				return false;
			}
			Job curJob = p.CurJob;
			if (curJob != null && twoWayInteraction && (!curJob.def.casualInterruptible || !curJob.playerForced))
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<Pawn> SpawnedMasteredPawns(Pawn master)
		{
			if (Current.ProgramState != ProgramState.Playing || master.Faction == null || !master.RaceProps.Humanlike || !master.Spawned)
			{
				yield break;
			}
			List<Pawn> pawns = master.Map.mapPawns.SpawnedPawnsInFaction(master.Faction);
			for (int i = 0; i < pawns.Count; i++)
			{
				if (pawns[i].playerSettings != null && pawns[i].playerSettings.Master == master)
				{
					yield return pawns[i];
				}
			}
		}

		public static bool InValidState(Pawn p)
		{
			if (p.health == null)
			{
				return false;
			}
			if (!p.Dead && (p.stances == null || p.mindState == null || p.needs == null || p.ageTracker == null))
			{
				return false;
			}
			return true;
		}

		public static PawnPosture GetPosture(this Pawn p)
		{
			if (p.Dead)
			{
				return PawnPosture.LayingOnGroundNormal;
			}
			if (p.Downed)
			{
				if (p.jobs != null && p.jobs.posture.Laying())
				{
					return p.jobs.posture;
				}
				return PawnPosture.LayingOnGroundNormal;
			}
			if (p.jobs == null)
			{
				return PawnPosture.Standing;
			}
			return p.jobs.posture;
		}

		public static void ForceWait(Pawn pawn, int ticks, Thing faceTarget = null, bool maintainPosture = false)
		{
			if (ticks <= 0)
			{
				Log.ErrorOnce("Forcing a wait for zero ticks", 47045639);
			}
			Job job = JobMaker.MakeJob(maintainPosture ? JobDefOf.Wait_MaintainPosture : JobDefOf.Wait, faceTarget);
			job.expiryInterval = ticks;
			pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
		}

		public static void GiveNameBecauseOfNuzzle(Pawn namer, Pawn namee)
		{
			string value = ((namee.Name == null) ? namee.LabelIndefinite() : namee.Name.ToStringFull);
			namee.Name = PawnBioAndNameGenerator.GeneratePawnName(namee);
			if (namer.Faction == Faction.OfPlayer)
			{
				Messages.Message("MessageNuzzledPawnGaveNameTo".Translate(namer.Named("NAMER"), value, namee.Name.ToStringFull, namee.Named("NAMEE")), namee, MessageTypeDefOf.NeutralEvent);
			}
		}

		public static void GainComfortFromCellIfPossible(this Pawn p, bool chairsOnly = false)
		{
			if (Find.TickManager.TicksGame % 10 == 0)
			{
				Building edifice = p.Position.GetEdifice(p.Map);
				if (edifice != null && (!chairsOnly || (edifice.def.category == ThingCategory.Building && edifice.def.building.isSittable)))
				{
					GainComfortFromThingIfPossible(p, edifice);
				}
			}
		}

		public static void GainComfortFromThingIfPossible(Pawn p, Thing from)
		{
			if (Find.TickManager.TicksGame % 10 == 0)
			{
				float statValue = from.GetStatValue(StatDefOf.Comfort);
				if (statValue >= 0f && p.needs != null && p.needs.comfort != null)
				{
					p.needs.comfort.ComfortUsed(statValue);
				}
			}
		}

		public static float BodyResourceGrowthSpeed(Pawn pawn)
		{
			if (pawn.needs != null && pawn.needs.food != null)
			{
				switch (pawn.needs.food.CurCategory)
				{
				case HungerCategory.Fed:
					return 1f;
				case HungerCategory.Hungry:
					return 0.666f;
				case HungerCategory.UrgentlyHungry:
					return 0.333f;
				case HungerCategory.Starving:
					return 0f;
				}
			}
			return 1f;
		}

		public static bool FertileMateTarget(Pawn male, Pawn female)
		{
			if (female.gender != Gender.Female || !female.ageTracker.CurLifeStage.reproductive)
			{
				return false;
			}
			CompEggLayer compEggLayer = female.TryGetComp<CompEggLayer>();
			if (compEggLayer != null)
			{
				return !compEggLayer.FullyFertilized;
			}
			return !female.health.hediffSet.HasHediff(HediffDefOf.Pregnant);
		}

		public static void Mated(Pawn male, Pawn female)
		{
			if (female.ageTracker.CurLifeStage.reproductive)
			{
				CompEggLayer compEggLayer = female.TryGetComp<CompEggLayer>();
				if (compEggLayer != null)
				{
					compEggLayer.Fertilize(male);
				}
				else if (Rand.Value < 0.5f && !female.health.hediffSet.HasHediff(HediffDefOf.Pregnant))
				{
					Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.Pregnant, female);
					hediff_Pregnant.father = male;
					female.health.AddHediff(hediff_Pregnant);
				}
			}
		}

		public static bool PlayerForcedJobNowOrSoon(Pawn pawn)
		{
			if (pawn.jobs == null)
			{
				return false;
			}
			Job curJob = pawn.CurJob;
			if (curJob != null)
			{
				return curJob.playerForced;
			}
			if (pawn.jobs.jobQueue.Any())
			{
				return pawn.jobs.jobQueue.Peek().job.playerForced;
			}
			return false;
		}

		public static bool TrySpawnHatchedOrBornPawn(Pawn pawn, Thing motherOrEgg)
		{
			if (motherOrEgg.SpawnedOrAnyParentSpawned)
			{
				return GenSpawn.Spawn(pawn, motherOrEgg.PositionHeld, motherOrEgg.MapHeld) != null;
			}
			Pawn pawn2 = motherOrEgg as Pawn;
			if (pawn2 != null)
			{
				if (pawn2.IsCaravanMember())
				{
					pawn2.GetCaravan().AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny: true);
					Find.WorldPawns.PassToWorld(pawn);
					return true;
				}
				if (pawn2.IsWorldPawn())
				{
					Find.WorldPawns.PassToWorld(pawn);
					return true;
				}
			}
			else if (motherOrEgg.ParentHolder != null)
			{
				Pawn_InventoryTracker pawn_InventoryTracker = motherOrEgg.ParentHolder as Pawn_InventoryTracker;
				if (pawn_InventoryTracker != null)
				{
					if (pawn_InventoryTracker.pawn.IsCaravanMember())
					{
						pawn_InventoryTracker.pawn.GetCaravan().AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny: true);
						Find.WorldPawns.PassToWorld(pawn);
						return true;
					}
					if (pawn_InventoryTracker.pawn.IsWorldPawn())
					{
						Find.WorldPawns.PassToWorld(pawn);
						return true;
					}
				}
			}
			return false;
		}

		public static ByteGrid GetAvoidGrid(this Pawn p, bool onlyIfLordAllows = true)
		{
			if (!p.Spawned)
			{
				return null;
			}
			if (p.Faction == null)
			{
				return null;
			}
			if (!p.Faction.def.canUseAvoidGrid)
			{
				return null;
			}
			if (p.Faction == Faction.OfPlayer || !p.Faction.HostileTo(Faction.OfPlayer))
			{
				return null;
			}
			if (onlyIfLordAllows)
			{
				Lord lord = p.GetLord();
				if (lord != null && lord.CurLordToil.useAvoidGrid)
				{
					return lord.Map.avoidGrid.Grid;
				}
				return null;
			}
			return p.Map.avoidGrid.Grid;
		}

		public static bool ShouldCollideWithPawns(Pawn p)
		{
			if (p.Downed || p.Dead)
			{
				return false;
			}
			if (!p.mindState.anyCloseHostilesRecently)
			{
				return false;
			}
			return true;
		}

		public static bool AnyPawnBlockingPathAt(IntVec3 c, Pawn forPawn, bool actAsIfHadCollideWithPawnsJob = false, bool collideOnlyWithStandingPawns = false, bool forPathFinder = false)
		{
			return PawnBlockingPathAt(c, forPawn, actAsIfHadCollideWithPawnsJob, collideOnlyWithStandingPawns, forPathFinder) != null;
		}

		public static Pawn PawnBlockingPathAt(IntVec3 c, Pawn forPawn, bool actAsIfHadCollideWithPawnsJob = false, bool collideOnlyWithStandingPawns = false, bool forPathFinder = false)
		{
			List<Thing> thingList = c.GetThingList(forPawn.Map);
			if (thingList.Count == 0)
			{
				return null;
			}
			bool flag = false;
			if (actAsIfHadCollideWithPawnsJob)
			{
				flag = true;
			}
			else
			{
				Job curJob = forPawn.CurJob;
				if (curJob != null && (curJob.collideWithPawns || curJob.def.collideWithPawns || forPawn.jobs.curDriver.collideWithPawns))
				{
					flag = true;
				}
				else if (forPawn.Drafted)
				{
					_ = forPawn.pather.Moving;
				}
			}
			for (int i = 0; i < thingList.Count; i++)
			{
				Pawn pawn = thingList[i] as Pawn;
				if (pawn == null || pawn == forPawn || pawn.Downed || (collideOnlyWithStandingPawns && (pawn.pather.MovingNow || (pawn.pather.Moving && pawn.pather.MovedRecently(60)))) || PawnsCanShareCellBecauseOfBodySize(pawn, forPawn))
				{
					continue;
				}
				if (pawn.HostileTo(forPawn))
				{
					return pawn;
				}
				if (flag && (forPathFinder || !forPawn.Drafted || !pawn.RaceProps.Animal))
				{
					Job curJob2 = pawn.CurJob;
					if (curJob2 != null && (curJob2.collideWithPawns || curJob2.def.collideWithPawns || pawn.jobs.curDriver.collideWithPawns))
					{
						return pawn;
					}
				}
			}
			return null;
		}

		private static bool PawnsCanShareCellBecauseOfBodySize(Pawn p1, Pawn p2)
		{
			if (p1.BodySize >= 1.5f || p2.BodySize >= 1.5f)
			{
				return false;
			}
			float num = p1.BodySize / p2.BodySize;
			if (num < 1f)
			{
				num = 1f / num;
			}
			return num > 3.57f;
		}

		public static bool KnownDangerAt(IntVec3 c, Map map, Pawn forPawn)
		{
			return c.GetEdifice(map)?.IsDangerousFor(forPawn) ?? false;
		}

		public static bool ShouldSendNotificationAbout(Pawn p)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return false;
			}
			if (PawnGenerator.IsBeingGenerated(p))
			{
				return false;
			}
			if (p.IsWorldPawn() && (!p.IsCaravanMember() || !p.GetCaravan().IsPlayerControlled) && !IsTravelingInTransportPodWorldObject(p) && !p.IsBorrowedByAnyFaction() && p.Corpse.DestroyedOrNull())
			{
				return false;
			}
			if (p.Faction != Faction.OfPlayer)
			{
				if (p.HostFaction != Faction.OfPlayer)
				{
					return false;
				}
				if (p.RaceProps.Humanlike && p.guest.Released && !p.Downed && !p.InBed())
				{
					return false;
				}
				if (p.CurJob != null && p.CurJob.exitMapOnArrival && !PrisonBreakUtility.IsPrisonBreaking(p))
				{
					return false;
				}
			}
			return true;
		}

		public static bool ShouldGetThoughtAbout(Pawn pawn, Pawn subject)
		{
			if (pawn.Faction != subject.Faction)
			{
				if (!subject.IsWorldPawn())
				{
					return !pawn.IsWorldPawn();
				}
				return false;
			}
			return true;
		}

		public static bool IsTeetotaler(this Pawn pawn)
		{
			if (pawn.story != null)
			{
				return pawn.story.traits.DegreeOfTrait(TraitDefOf.DrugDesire) < 0;
			}
			return false;
		}

		public static bool IsProsthophobe(this Pawn pawn)
		{
			if (pawn.story != null)
			{
				return pawn.story.traits.HasTrait(TraitDefOf.BodyPurist);
			}
			return false;
		}

		public static bool IsPrisonerInPrisonCell(this Pawn pawn)
		{
			if (pawn.IsPrisoner && pawn.Spawned)
			{
				return pawn.Position.IsInPrisonCell(pawn.Map);
			}
			return false;
		}

		public static string PawnKindsToCommaList(IEnumerable<Pawn> pawns, bool useAnd = false)
		{
			tmpPawns.Clear();
			tmpPawns.AddRange(pawns);
			if (tmpPawns.Count >= 2)
			{
				tmpPawns.SortBy((Pawn x) => !x.RaceProps.Humanlike, (Pawn x) => x.GetKindLabelPlural());
			}
			tmpAddedPawnKinds.Clear();
			tmpPawnKindsStr.Clear();
			for (int i = 0; i < tmpPawns.Count; i++)
			{
				if (tmpAddedPawnKinds.Contains(tmpPawns[i].kindDef))
				{
					continue;
				}
				tmpAddedPawnKinds.Add(tmpPawns[i].kindDef);
				int num = 0;
				for (int j = 0; j < tmpPawns.Count; j++)
				{
					if (tmpPawns[j].kindDef == tmpPawns[i].kindDef)
					{
						num++;
					}
				}
				if (num == 1)
				{
					tmpPawnKindsStr.Add("1 " + tmpPawns[i].KindLabel);
				}
				else
				{
					tmpPawnKindsStr.Add(num + " " + tmpPawns[i].GetKindLabelPlural(num));
				}
			}
			tmpPawns.Clear();
			return tmpPawnKindsStr.ToCommaList(useAnd);
		}

		public static List<string> PawnKindsToList(IEnumerable<PawnKindDef> pawnKinds)
		{
			tmpPawnKinds.Clear();
			tmpPawnKinds.AddRange(pawnKinds);
			if (tmpPawnKinds.Count >= 2)
			{
				tmpPawnKinds.SortBy((PawnKindDef x) => !x.RaceProps.Humanlike, (PawnKindDef x) => GenLabel.BestKindLabel(x, Gender.None, plural: true));
			}
			tmpAddedPawnKinds.Clear();
			tmpPawnKindsStr.Clear();
			for (int i = 0; i < tmpPawnKinds.Count; i++)
			{
				if (tmpAddedPawnKinds.Contains(tmpPawnKinds[i]))
				{
					continue;
				}
				tmpAddedPawnKinds.Add(tmpPawnKinds[i]);
				int num = 0;
				for (int j = 0; j < tmpPawnKinds.Count; j++)
				{
					if (tmpPawnKinds[j] == tmpPawnKinds[i])
					{
						num++;
					}
				}
				if (num == 1)
				{
					tmpPawnKindsStr.Add("1 " + GenLabel.BestKindLabel(tmpPawnKinds[i], Gender.None));
				}
				else
				{
					tmpPawnKindsStr.Add(num + " " + GenLabel.BestKindLabel(tmpPawnKinds[i], Gender.None, plural: true, num));
				}
			}
			return tmpPawnKindsStr;
		}

		public static string PawnKindsToLineList(IEnumerable<PawnKindDef> pawnKinds, string prefix)
		{
			PawnKindsToList(pawnKinds);
			return tmpPawnKindsStr.ToLineList(prefix);
		}

		public static string PawnKindsToLineList(IEnumerable<PawnKindDef> pawnKinds, string prefix, Color color)
		{
			PawnKindsToList(pawnKinds);
			for (int i = 0; i < tmpPawnKindsStr.Count; i++)
			{
				tmpPawnKindsStr[i] = tmpPawnKindsStr[i].Colorize(color);
			}
			return tmpPawnKindsStr.ToLineList(prefix);
		}

		public static string PawnKindsToCommaList(IEnumerable<PawnKindDef> pawnKinds, bool useAnd = false)
		{
			PawnKindsToList(pawnKinds);
			return tmpPawnKindsStr.ToCommaList(useAnd);
		}

		public static LocomotionUrgency ResolveLocomotion(Pawn pawn, LocomotionUrgency secondPriority)
		{
			if (!pawn.Dead && pawn.mindState.duty != null && pawn.mindState.duty.locomotion != 0)
			{
				return pawn.mindState.duty.locomotion;
			}
			return secondPriority;
		}

		public static LocomotionUrgency ResolveLocomotion(Pawn pawn, LocomotionUrgency secondPriority, LocomotionUrgency thirdPriority)
		{
			LocomotionUrgency locomotionUrgency = ResolveLocomotion(pawn, secondPriority);
			if (locomotionUrgency != 0)
			{
				return locomotionUrgency;
			}
			return thirdPriority;
		}

		public static Danger ResolveMaxDanger(Pawn pawn, Danger secondPriority)
		{
			if (!pawn.Dead && pawn.mindState.duty != null && pawn.mindState.duty.maxDanger != 0)
			{
				return pawn.mindState.duty.maxDanger;
			}
			return secondPriority;
		}

		public static Danger ResolveMaxDanger(Pawn pawn, Danger secondPriority, Danger thirdPriority)
		{
			Danger danger = ResolveMaxDanger(pawn, secondPriority);
			if (danger != 0)
			{
				return danger;
			}
			return thirdPriority;
		}

		public static bool IsFighting(this Pawn pawn)
		{
			if (pawn.CurJob != null)
			{
				if (pawn.CurJob.def != JobDefOf.AttackMelee && pawn.CurJob.def != JobDefOf.AttackStatic && pawn.CurJob.def != JobDefOf.Wait_Combat)
				{
					return pawn.CurJob.def == JobDefOf.PredatorHunt;
				}
				return true;
			}
			return false;
		}

		public static Hediff_Psylink GetMainPsylinkSource(this Pawn pawn)
		{
			return (Hediff_Psylink)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier);
		}

		public static int GetPsylinkLevel(this Pawn pawn)
		{
			return pawn.GetMainPsylinkSource()?.level ?? 0;
		}

		public static int GetMaxPsylinkLevel(this Pawn pawn)
		{
			return (int)HediffDefOf.PsychicAmplifier.maxSeverity;
		}

		public static RoyalTitle GetMaxPsylinkLevelTitle(this Pawn pawn)
		{
			if (pawn.royalty == null)
			{
				return null;
			}
			int num = 0;
			RoyalTitle result = null;
			foreach (RoyalTitle item in pawn.royalty.AllTitlesInEffectForReading)
			{
				if (num < item.def.maxPsylinkLevel)
				{
					num = item.def.maxPsylinkLevel;
					result = item;
				}
			}
			return result;
		}

		public static int GetMaxPsylinkLevelByTitle(this Pawn pawn)
		{
			return pawn.GetMaxPsylinkLevelTitle()?.def.maxPsylinkLevel ?? 0;
		}

		public static void ChangePsylinkLevel(this Pawn pawn, int levelOffset, bool sendLetter = true)
		{
			Hediff_Psylink mainPsylinkSource = pawn.GetMainPsylinkSource();
			if (mainPsylinkSource == null)
			{
				mainPsylinkSource = (Hediff_Psylink)HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn);
				try
				{
					mainPsylinkSource.suppressPostAddLetter = !sendLetter;
					pawn.health.AddHediff(mainPsylinkSource, pawn.health.hediffSet.GetBrain());
				}
				finally
				{
					mainPsylinkSource.suppressPostAddLetter = false;
				}
			}
			else
			{
				mainPsylinkSource.ChangeLevel(levelOffset, sendLetter);
			}
		}

		public static float RecruitDifficulty(this Pawn pawn, Faction recruiterFaction)
		{
			float baseRecruitDifficulty = pawn.kindDef.baseRecruitDifficulty;
			Rand.PushState();
			Rand.Seed = pawn.HashOffset();
			baseRecruitDifficulty += Rand.Gaussian(0f, 0.15f);
			Rand.PopState();
			if (pawn.Faction != null)
			{
				int num = Mathf.Min((int)pawn.Faction.def.techLevel, 4);
				int num2 = Mathf.Min((int)recruiterFaction.def.techLevel, 4);
				int num3 = Mathf.Abs(num - num2);
				baseRecruitDifficulty += (float)num3 * 0.16f;
			}
			if (pawn.royalty != null)
			{
				RoyalTitle mostSeniorTitle = pawn.royalty.MostSeniorTitle;
				if (mostSeniorTitle != null)
				{
					baseRecruitDifficulty += mostSeniorTitle.def.recruitmentDifficultyOffset;
				}
			}
			return Mathf.Clamp(baseRecruitDifficulty, 0.1f, 0.99f);
		}

		public static void GiveAllStartingPlayerPawnsThought(ThoughtDef thought)
		{
			foreach (Pawn startingAndOptionalPawn in Find.GameInitData.startingAndOptionalPawns)
			{
				if (startingAndOptionalPawn.needs.mood == null)
				{
					continue;
				}
				if (thought.IsSocial)
				{
					foreach (Pawn startingAndOptionalPawn2 in Find.GameInitData.startingAndOptionalPawns)
					{
						if (startingAndOptionalPawn2 != startingAndOptionalPawn)
						{
							startingAndOptionalPawn.needs.mood.thoughts.memories.TryGainMemory(thought, startingAndOptionalPawn2);
						}
					}
				}
				else
				{
					startingAndOptionalPawn.needs.mood.thoughts.memories.TryGainMemory(thought);
				}
			}
		}

		public static IntVec3 DutyLocation(this Pawn pawn)
		{
			if (pawn.mindState.duty != null && pawn.mindState.duty.focus.IsValid)
			{
				return pawn.mindState.duty.focus.Cell;
			}
			return pawn.Position;
		}

		public static bool EverBeenColonistOrTameAnimal(Pawn pawn)
		{
			return pawn.records.GetAsInt(RecordDefOf.TimeAsColonistOrColonyAnimal) > 0;
		}

		public static bool EverBeenPrisoner(Pawn pawn)
		{
			return pawn.records.GetAsInt(RecordDefOf.TimeAsPrisoner) > 0;
		}

		public static bool EverBeenQuestLodger(Pawn pawn)
		{
			return pawn.records.GetAsInt(RecordDefOf.TimeAsQuestLodger) > 0;
		}

		public static void RecoverFromUnwalkablePositionOrKill(IntVec3 c, Map map)
		{
			if (!c.InBounds(map) || c.Walkable(map))
			{
				return;
			}
			tmpThings.Clear();
			tmpThings.AddRange(c.GetThingList(map));
			for (int i = 0; i < tmpThings.Count; i++)
			{
				Pawn pawn = tmpThings[i] as Pawn;
				if (pawn == null)
				{
					continue;
				}
				if (CellFinder.TryFindBestPawnStandCell(pawn, out var cell))
				{
					pawn.Position = cell;
					pawn.Notify_Teleported(endCurrentJob: true, resetTweenedPos: false);
					continue;
				}
				DamageInfo damageInfo = new DamageInfo(DamageDefOf.Crush, 99999f, 999f, -1f, null, pawn.health.hediffSet.GetBrain(), null, DamageInfo.SourceCategory.Collapse);
				pawn.TakeDamage(damageInfo);
				if (!pawn.Dead)
				{
					pawn.Kill(damageInfo);
				}
			}
		}

		public static float GetManhunterOnDamageChance(Pawn pawn, float distance, Thing instigator)
		{
			float manhunterOnDamageChance = GetManhunterOnDamageChance(pawn.kindDef);
			manhunterOnDamageChance *= GenMath.LerpDoubleClamped(1f, 30f, 3f, 1f, distance);
			if (instigator != null)
			{
				manhunterOnDamageChance *= 1f - instigator.GetStatValue(StatDefOf.HuntingStealth);
			}
			return manhunterOnDamageChance;
		}

		public static float GetManhunterOnDamageChance(Pawn pawn, Thing instigator = null)
		{
			if (instigator != null)
			{
				return GetManhunterOnDamageChance(pawn, pawn.Position.DistanceTo(instigator.Position), instigator);
			}
			return GetManhunterOnDamageChance(pawn.kindDef);
		}

		public static float GetManhunterOnDamageChance(PawnKindDef kind)
		{
			return kind.RaceProps.manhunterOnDamageChance * Find.Storyteller.difficultyValues.manhunterChanceOnDamageFactor;
		}

		public static float GetManhunterOnDamageChance(RaceProperties race)
		{
			return race.manhunterOnDamageChance * Find.Storyteller.difficultyValues.manhunterChanceOnDamageFactor;
		}
	}
}
