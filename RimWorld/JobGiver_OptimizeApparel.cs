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
			new CurvePoint(0.22f, 0.6f),
			new CurvePoint(0.5f, 0.6f),
			new CurvePoint(0.52f, 1f)
		};

		private static HashSet<BodyPartGroupDef> tmpBodyPartGroupsWithRequirement = new HashSet<BodyPartGroupDef>();

		private static HashSet<ThingDef> tmpAllowedApparels = new HashSet<ThingDef>();

		private static HashSet<ThingDef> tmpRequiredApparels = new HashSet<ThingDef>();

		private void SetNextOptimizeTick(Pawn pawn)
		{
			pawn.mindState.nextApparelOptimizeTick = Find.TickManager.TicksGame + Rand.Range(6000, 9000);
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.outfits == null)
			{
				Log.ErrorOnce(pawn + " tried to run JobGiver_OptimizeApparel without an OutfitTracker", 5643897);
				return null;
			}
			if (pawn.Faction != Faction.OfPlayer)
			{
				Log.ErrorOnce("Non-colonist " + pawn + " tried to optimize apparel.", 764323);
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
				debugSb.AppendLine("Scanning for " + pawn + " at " + pawn.Position);
			}
			Outfit currentOutfit = pawn.outfits.CurrentOutfit;
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			for (int num = wornApparel.Count - 1; num >= 0; num--)
			{
				if (!currentOutfit.filter.Allows(wornApparel[num]) && pawn.outfits.forcedHandler.AllowedToAutomaticallyDrop(wornApparel[num]) && !pawn.apparel.IsLocked(wornApparel[num]))
				{
					Job job = JobMaker.MakeJob(JobDefOf.RemoveApparel, wornApparel[num]);
					job.haulDroppedApparel = true;
					return job;
				}
			}
			Thing thing = null;
			float num2 = 0f;
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel);
			if (list.Count == 0)
			{
				SetNextOptimizeTick(pawn);
				return null;
			}
			neededWarmth = PawnApparelGenerator.CalculateNeededWarmth(pawn, pawn.Map.Tile, GenLocalDate.Twelfth(pawn));
			for (int i = 0; i < list.Count; i++)
			{
				Apparel apparel = (Apparel)list[i];
				if (currentOutfit.filter.Allows(apparel) && apparel.IsInAnyStorage() && !apparel.IsForbidden(pawn) && !apparel.IsBurning() && (apparel.def.apparel.gender == Gender.None || apparel.def.apparel.gender == pawn.gender) && (!apparel.def.apparel.tags.Contains("Royal") || pawn.royalty.AllTitlesInEffectForReading.Count != 0))
				{
					float num3 = ApparelScoreGain(pawn, apparel);
					if (DebugViewSettings.debugApparelOptimize)
					{
						debugSb.AppendLine(apparel.LabelCap + ": " + num3.ToString("F2"));
					}
					if (!(num3 < 0.05f) && !(num3 < num2) && (!EquipmentUtility.IsBiocoded(apparel) || EquipmentUtility.IsBiocodedFor(apparel, pawn)) && ApparelUtility.HasPartsToWear(pawn, apparel.def) && pawn.CanReserveAndReach(apparel, PathEndMode.OnCell, pawn.NormalMaxDanger()))
					{
						thing = apparel;
						num2 = num3;
					}
				}
			}
			if (DebugViewSettings.debugApparelOptimize)
			{
				debugSb.AppendLine("BEST: " + thing);
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

		public static float ApparelScoreGain(Pawn pawn, Apparel ap)
		{
			if (ap is ShieldBelt && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsWeaponUsingProjectiles)
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
					num -= ApparelScoreRaw(pawn, wornApparel[i]);
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
			float num = 0.1f;
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
			if (ap.WornByCorpse && (pawn == null || ThoughtUtility.CanGetThought_NewTemp(pawn, ThoughtDefOf.DeadMansApparel, checkIfNullified: true)))
			{
				num -= 0.5f;
				if (num > 0f)
				{
					num *= 0.1f;
				}
			}
			if (ap.Stuff == ThingDefOf.Human.race.leatherDef)
			{
				if (pawn == null || ThoughtUtility.CanGetThought_NewTemp(pawn, ThoughtDefOf.HumanLeatherApparelSad, checkIfNullified: true))
				{
					num -= 0.5f;
					if (num > 0f)
					{
						num *= 0.1f;
					}
				}
				if (pawn != null && ThoughtUtility.CanGetThought_NewTemp(pawn, ThoughtDefOf.HumanLeatherApparelHappy, checkIfNullified: true))
				{
					num += 0.12f;
				}
			}
			if (pawn != null && !ap.def.apparel.CorrectGenderForWearing(pawn.gender))
			{
				num *= 0.01f;
			}
			if (pawn != null && pawn.royalty != null && pawn.royalty.AllTitlesInEffectForReading.Count > 0)
			{
				tmpAllowedApparels.Clear();
				tmpRequiredApparels.Clear();
				tmpBodyPartGroupsWithRequirement.Clear();
				QualityCategory qualityCategory = QualityCategory.Awful;
				foreach (RoyalTitle item in pawn.royalty.AllTitlesInEffectForReading)
				{
					if (item.def.requiredApparel != null)
					{
						for (int i = 0; i < item.def.requiredApparel.Count; i++)
						{
							tmpAllowedApparels.AddRange(item.def.requiredApparel[i].AllAllowedApparelForPawn(pawn, ignoreGender: false, includeWorn: true));
							tmpRequiredApparels.AddRange(item.def.requiredApparel[i].AllRequiredApparelForPawn(pawn, ignoreGender: false, includeWorn: true));
							tmpBodyPartGroupsWithRequirement.AddRange(item.def.requiredApparel[i].bodyPartGroupsMatchAny);
						}
					}
					if ((int)item.def.requiredMinimumApparelQuality > (int)qualityCategory)
					{
						qualityCategory = item.def.requiredMinimumApparelQuality;
					}
				}
				bool num4 = ap.def.apparel.bodyPartGroups.Any((BodyPartGroupDef bp) => tmpBodyPartGroupsWithRequirement.Contains(bp));
				if (ap.TryGetQuality(out QualityCategory qc) && (int)qc < (int)qualityCategory)
				{
					num *= 0.25f;
				}
				if (num4)
				{
					foreach (ThingDef tmpRequiredApparel in tmpRequiredApparels)
					{
						tmpAllowedApparels.Remove(tmpRequiredApparel);
					}
					if (tmpAllowedApparels.Contains(ap.def))
					{
						num *= 10f;
					}
					if (tmpRequiredApparels.Contains(ap.def))
					{
						num *= 25f;
					}
				}
			}
			return num;
		}
	}
}
