using System.Collections.Generic;
using System.Text;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_OptimizeApparel : ThinkNode_JobGiver
	{
		private static NeededWarmth neededWarmth;

		private static StringBuilder debugSb;

		private static readonly List<float> wornApparelScores = new List<float>();

		private static readonly List<Thing> tmpApparelList = new List<Thing>();

		private const int ApparelOptimizeCheckIntervalMin = 6000;

		private const int ApparelOptimizeCheckIntervalMax = 9000;

		private const float MinScoreGainToCare = 0.05f;

		private const float ScoreFactorIfNotReplacing = 10f;

		private static readonly SimpleCurve InsulationColdScoreFactorCurve_NeedWarm = new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(30f, 8f)
		};

		private static readonly SimpleCurve HitPointsPercentScoreFactorCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(0.2f, 0.2f),
			new CurvePoint(0.22f, 0.3f),
			new CurvePoint(0.5f, 0.3f),
			new CurvePoint(0.52f, 1f)
		};

		private static readonly List<LocalTargetInfo> tmpQueueDye = new List<LocalTargetInfo>();

		private static readonly List<LocalTargetInfo> tmpQueueApparel = new List<LocalTargetInfo>();

		private static readonly List<Apparel> tmpApparelToRecolor = new List<Apparel>();

		private static void SetNextOptimizeTick(Pawn pawn)
		{
			pawn.mindState.nextApparelOptimizeTick = Find.TickManager.TicksGame + Rand.Range(6000, 9000);
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.outfits == null)
			{
				Log.ErrorOnce($"{pawn} tried to run JobGiver_OptimizeApparel without an OutfitTracker", 5643897);
				return null;
			}
			if (pawn.Faction != Faction.OfPlayer)
			{
				Log.ErrorOnce($"Non-colonist {pawn} tried to optimize apparel.", 764323);
				return null;
			}
			if (pawn.IsMutant && pawn.mutant.Def.disableApparel)
			{
				return null;
			}
			if (pawn.IsQuestLodger())
			{
				return null;
			}
			if (!DebugViewSettings.debugApparelOptimize)
			{
				if (Find.TickManager.TicksGame < pawn.mindState.nextApparelOptimizeTick)
				{
					return null;
				}
			}
			else
			{
				debugSb = new StringBuilder();
				debugSb.AppendLine($"Scanning for {pawn} at {pawn.Position}");
			}
			if (ModsConfig.IdeologyActive && TryCreateRecolorJob(pawn, out var job))
			{
				return job;
			}
			ApparelPolicy currentApparelPolicy = pawn.outfits.CurrentApparelPolicy;
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			for (int num = wornApparel.Count - 1; num >= 0; num--)
			{
				bool flag = false;
				Apparel apparel = wornApparel[num];
				if (pawn.MapHeld.Biome.inVacuum && pawn.Position.GetVacuum(pawn.MapHeld) >= 0.5f)
				{
					flag = apparel.GetStatValue(StatDefOf.VacuumResistance, applyPostProcess: true, 60) > 0f;
				}
				if (!flag && !currentApparelPolicy.filter.Allows(apparel) && pawn.outfits.forcedHandler.AllowedToAutomaticallyDrop(apparel) && !pawn.apparel.IsLocked(apparel))
				{
					Job job2 = JobMaker.MakeJob(JobDefOf.RemoveApparel, apparel);
					job2.haulDroppedApparel = true;
					return job2;
				}
			}
			Thing thing = null;
			float num2 = 0f;
			tmpApparelList.Clear();
			pawn.Map.listerThings.GetAllThings(in tmpApparelList, ThingRequestGroup.Apparel, null, lookInHaulSources: true);
			foreach (IHaulSource item2 in pawn.Map.haulDestinationManager.AllHaulSourcesListForReading)
			{
				foreach (Thing item3 in (IEnumerable<Thing>)item2.GetDirectlyHeldThings())
				{
					if (item3 is Apparel item)
					{
						tmpApparelList.Add(item);
					}
				}
			}
			if (tmpApparelList.Count == 0)
			{
				SetNextOptimizeTick(pawn);
				return null;
			}
			neededWarmth = PawnApparelGenerator.CalculateNeededWarmth(pawn, pawn.Map.TileInfo.tile, GenLocalDate.Twelfth(pawn));
			wornApparelScores.Clear();
			for (int i = 0; i < wornApparel.Count; i++)
			{
				wornApparelScores.Add(ApparelScoreRaw(pawn, wornApparel[i]));
			}
			for (int j = 0; j < tmpApparelList.Count; j++)
			{
				Apparel apparel2 = (Apparel)tmpApparelList[j];
				if (!currentApparelPolicy.filter.Allows(apparel2) || !apparel2.IsInAnyStorage() || apparel2.IsForbidden(pawn) || apparel2.IsBurning() || (apparel2.def.apparel.gender != Gender.None && apparel2.def.apparel.gender != pawn.gender))
				{
					continue;
				}
				float num3 = ApparelScoreGain(pawn, apparel2, wornApparelScores);
				if (DebugViewSettings.debugApparelOptimize)
				{
					debugSb.AppendLine($"{apparel2.LabelCap}: {num3:F2}");
				}
				if (num3 < 0.05f || num3 < num2 || (CompBiocodable.IsBiocoded(apparel2) && !CompBiocodable.IsBiocodedFor(apparel2, pawn)) || !ApparelUtility.HasPartsToWear(pawn, apparel2.def))
				{
					continue;
				}
				LocalTargetInfo target = apparel2;
				if (apparel2.ParentHolder is IApparelSource apparelSource && apparelSource is Thing thing2)
				{
					if (!apparelSource.ApparelSourceEnabled)
					{
						continue;
					}
					target = thing2;
				}
				if (pawn.CanReserveAndReach(target, PathEndMode.OnCell, pawn.NormalMaxDanger()) && apparel2.def.apparel.developmentalStageFilter.Has(pawn.DevelopmentalStage))
				{
					thing = apparel2;
					num2 = num3;
				}
			}
			tmpApparelList.Clear();
			if (DebugViewSettings.debugApparelOptimize)
			{
				debugSb.AppendLine($"BEST: {thing}");
				Log.Message(debugSb.ToString());
				debugSb = null;
			}
			if (thing == null)
			{
				SetNextOptimizeTick(pawn);
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Wear, thing);
		}

		public static bool TryCreateRecolorJob(Pawn pawn, out Job job, bool dryRun = false)
		{
			if (!ModLister.CheckIdeology("Apparel recoloring"))
			{
				job = null;
				return false;
			}
			if (pawn.apparel.AnyApparelNeedsRecoloring)
			{
				Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.StylingStation), PathEndMode.Touch, TraverseParms.For(pawn), 9999f, (Thing t) => !t.IsForbidden(pawn) && pawn.CanReserve(t) && pawn.CanReserveSittableOrSpot(t.InteractionCell));
				if (thing != null)
				{
					try
					{
						foreach (Apparel item in pawn.apparel.WornApparel)
						{
							if (item.DesiredColor.HasValue)
							{
								tmpApparelToRecolor.Add(item);
							}
						}
						List<Thing> list = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Dye);
						if (tmpApparelToRecolor.Count > 0)
						{
							list.SortBy((Thing t) => t.Position.DistanceToSquared(pawn.Position));
							foreach (Thing item2 in list)
							{
								if (!pawn.CanReach(item2, PathEndMode.Touch, Danger.Some) || item2.IsForbidden(pawn))
								{
									continue;
								}
								for (int num = 0; num < item2.stackCount && pawn.CanReserve(item2, 1, num + 1); num++)
								{
									tmpQueueApparel.Add(tmpApparelToRecolor[tmpApparelToRecolor.Count - 1]);
									if (!tmpQueueDye.Contains(item2))
									{
										tmpQueueDye.Add(item2);
									}
									tmpApparelToRecolor.RemoveAt(tmpApparelToRecolor.Count - 1);
									if (tmpApparelToRecolor.Count == 0)
									{
										break;
									}
								}
								if (tmpApparelToRecolor.Count == 0)
								{
									break;
								}
							}
							if (tmpQueueApparel.Count > 0)
							{
								if (dryRun)
								{
									job = null;
								}
								else
								{
									job = JobMaker.MakeJob(JobDefOf.RecolorApparel);
									List<LocalTargetInfo> targetQueue = job.GetTargetQueue(TargetIndex.A);
									List<LocalTargetInfo> targetQueue2 = job.GetTargetQueue(TargetIndex.B);
									targetQueue.AddRange(tmpQueueDye);
									targetQueue2.AddRange(tmpQueueApparel);
									job.SetTarget(TargetIndex.C, thing);
									job.count = tmpQueueApparel.Count;
								}
								return true;
							}
						}
					}
					finally
					{
						tmpApparelToRecolor.Clear();
						tmpQueueApparel.Clear();
						tmpQueueDye.Clear();
					}
				}
			}
			job = null;
			return false;
		}

		public static float ApparelScoreGain(Pawn pawn, Apparel ap, List<float> wornScoresCache)
		{
			if (ap.def == ThingDefOf.Apparel_ShieldBelt && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsWeaponUsingProjectiles)
			{
				return -1000f;
			}
			if (ap.def.apparel.ignoredByNonViolent && pawn.WorkTagIsDisabled(WorkTags.Violent))
			{
				return -1000f;
			}
			float num = ApparelScoreRaw(pawn, ap);
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			bool flag = false;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (!ApparelUtility.CanWearTogether(wornApparel[i].def, ap.def, pawn.RaceProps.body))
				{
					if (!pawn.outfits.forcedHandler.AllowedToAutomaticallyDrop(wornApparel[i]) || pawn.apparel.IsLocked(wornApparel[i]))
					{
						return -1000f;
					}
					num -= wornScoresCache[i];
					flag = true;
				}
			}
			if (!flag)
			{
				num *= 10f;
			}
			return num;
		}

		public static float ApparelScoreRaw(Pawn pawn, Apparel ap)
		{
			if (!ap.PawnCanWear(pawn, ignoreGender: true))
			{
				return -10f;
			}
			if (ap.def.apparel.blocksVision)
			{
				return -10f;
			}
			if (ap.def.apparel.slaveApparel && !pawn.IsSlave)
			{
				return -10f;
			}
			if (ap.def.apparel.mechanitorApparel && pawn.mechanitor == null)
			{
				return -10f;
			}
			float num = 0.1f + ap.def.apparel.scoreOffset;
			float num2 = ap.GetStatValue(StatDefOf.ArmorRating_Sharp) + ap.GetStatValue(StatDefOf.ArmorRating_Blunt);
			num += num2;
			if (ap.def.useHitPoints)
			{
				float x = (float)ap.HitPoints / (float)ap.MaxHitPoints;
				num *= HitPointsPercentScoreFactorCurve.Evaluate(x);
			}
			num += ap.GetSpecialApparelScoreOffset();
			float num3 = 1f;
			if (neededWarmth == NeededWarmth.Warm)
			{
				float statValue = ap.GetStatValue(StatDefOf.Insulation_Cold);
				num3 *= InsulationColdScoreFactorCurve_NeedWarm.Evaluate(statValue);
			}
			num *= num3;
			if (ap.WornByCorpse && (pawn == null || ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.DeadMansApparel, checkIfNullified: true)))
			{
				num -= 0.5f;
				if (num > 0f)
				{
					num *= 0.1f;
				}
			}
			if (ap.Stuff == ThingDefOf.Human.race.leatherDef)
			{
				if (pawn.Ideo != null && pawn.Ideo.LikesHumanLeatherApparel)
				{
					num += 0.12f;
				}
				else
				{
					if (pawn == null || ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.HumanLeatherApparelSad, checkIfNullified: true))
					{
						num -= 0.5f;
						if (num > 0f)
						{
							num *= 0.1f;
						}
					}
					if (pawn != null && ThoughtUtility.CanGetThought(pawn, ThoughtDefOf.HumanLeatherApparelHappy, checkIfNullified: true))
					{
						num += 0.12f;
					}
				}
			}
			if (pawn != null && !ap.def.apparel.CorrectGenderForWearing(pawn.gender))
			{
				num *= 0.01f;
			}
			bool flag = false;
			if (pawn != null)
			{
				foreach (ApparelRequirementWithSource allRequirement in pawn.apparel.AllRequirements)
				{
					foreach (BodyPartGroupDef item in allRequirement.requirement.bodyPartGroupsMatchAny)
					{
						if (ap.def.apparel.bodyPartGroups.Contains(item))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				bool flag2 = false;
				bool flag3 = false;
				foreach (ApparelRequirementWithSource allRequirement2 in pawn.apparel.AllRequirements)
				{
					if (allRequirement2.requirement.RequiredForPawn(pawn, ap.def))
					{
						flag2 = true;
					}
					if (allRequirement2.requirement.AllowedForPawn(pawn, ap.def))
					{
						flag3 = true;
					}
				}
				if (flag2)
				{
					num *= 25f;
				}
				else if (flag3)
				{
					num *= 10f;
				}
			}
			if (pawn != null && pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Count > 0)
			{
				QualityCategory qualityCategory = QualityCategory.Awful;
				foreach (RoyalTitle item2 in pawn.royalty.AllTitlesInEffectForReading)
				{
					if ((int)item2.def.requiredMinimumApparelQuality > (int)qualityCategory)
					{
						qualityCategory = item2.def.requiredMinimumApparelQuality;
					}
				}
				if (ap.TryGetQuality(out var qc) && (int)qc < (int)qualityCategory)
				{
					num *= 0.25f;
				}
			}
			return num;
		}
	}
}
