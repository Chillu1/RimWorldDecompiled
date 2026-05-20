using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PawnApparelGenerator
	{
		private class PossibleApparelSet
		{
			private List<ThingStuffPair> aps = new List<ThingStuffPair>();

			private HashSet<ApparelUtility.LayerGroupPair> lgps = new HashSet<ApparelUtility.LayerGroupPair>();

			private BodyDef body;

			private ThingDef raceDef;

			private Pawn pawn;

			private const float StartingMinTemperature = 12f;

			private const float TargetMinTemperature = -40f;

			private const float StartingMaxTemperature = 32f;

			private const float TargetMaxTemperature = 30f;

			private const float MinToxicEnvironmentResistanceForFreeApparel = 0.25f;

			private const float MinVacuumResistanceForFreeApparel = 0.9f;

			private const float MinToxicEnvironmentResistanceImprovement = 0.15f;

			private const float MinVacuumResistanceImprovement = 0.15f;

			private static readonly SimpleCurve ToxicEnvironmentResistanceOverPollutionCurve = new SimpleCurve
			{
				new CurvePoint(0f, 0f),
				new CurvePoint(0.5f, 0.5f),
				new CurvePoint(1f, 0.85f)
			};

			private const float DesiredVacuumResistance = 0.9f;

			private static readonly List<BodyPartRecord> tmpParts = new List<BodyPartRecord>();

			public int Count => aps.Count;

			public float TotalPrice => aps.Sum((ThingStuffPair pa) => pa.Price);

			public float TotalInsulationCold => aps.Sum((ThingStuffPair a) => a.InsulationCold);

			public List<ThingStuffPair> ApparelsForReading => aps;

			public void Reset(BodyDef body, ThingDef raceDef)
			{
				aps.Clear();
				lgps.Clear();
				this.body = body;
				this.raceDef = raceDef;
				pawn = null;
			}

			public void Reset(Pawn pawn)
			{
				aps.Clear();
				lgps.Clear();
				this.pawn = pawn;
				body = pawn?.RaceProps?.body;
				raceDef = pawn?.def;
			}

			public void Add(ThingStuffPair pair)
			{
				aps.Add(pair);
				for (int i = 0; i < pair.thing.apparel.layers.Count; i++)
				{
					ApparelLayerDef layer = pair.thing.apparel.layers[i];
					BodyPartGroupDef[] interferingBodyPartGroups = pair.thing.apparel.GetInterferingBodyPartGroups(body);
					for (int j = 0; j < interferingBodyPartGroups.Length; j++)
					{
						lgps.Add(new ApparelUtility.LayerGroupPair(layer, interferingBodyPartGroups[j]));
					}
				}
			}

			public bool PairOverlapsAnything(ThingStuffPair pair)
			{
				if (!lgps.Any())
				{
					return false;
				}
				for (int i = 0; i < pair.thing.apparel.layers.Count; i++)
				{
					ApparelLayerDef layer = pair.thing.apparel.layers[i];
					BodyPartGroupDef[] interferingBodyPartGroups = pair.thing.apparel.GetInterferingBodyPartGroups(body);
					for (int j = 0; j < interferingBodyPartGroups.Length; j++)
					{
						if (lgps.Contains(new ApparelUtility.LayerGroupPair(layer, interferingBodyPartGroups[j])))
						{
							return true;
						}
					}
				}
				return false;
			}

			public bool CoatButNoShirt()
			{
				bool flag = false;
				bool flag2 = false;
				for (int i = 0; i < aps.Count; i++)
				{
					if (!aps[i].thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						continue;
					}
					for (int j = 0; j < aps[i].thing.apparel.layers.Count; j++)
					{
						ApparelLayerDef apparelLayerDef = aps[i].thing.apparel.layers[j];
						if (apparelLayerDef == ApparelLayerDefOf.OnSkin)
						{
							flag2 = true;
						}
						if (apparelLayerDef == ApparelLayerDefOf.Shell || apparelLayerDef == ApparelLayerDefOf.Middle)
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					return !flag2;
				}
				return false;
			}

			public bool Covers(BodyPartGroupDef bp)
			{
				for (int i = 0; i < aps.Count; i++)
				{
					if ((bp != BodyPartGroupDefOf.Legs || !aps[i].thing.apparel.legsNakedUnlessCoveredBySomethingElse) && aps[i].thing.apparel.bodyPartGroups.Contains(bp))
					{
						return true;
					}
				}
				return false;
			}

			public bool IsNaked(Gender gender)
			{
				switch (gender)
				{
				case Gender.Male:
					return !Covers(BodyPartGroupDefOf.Legs);
				case Gender.Female:
					if (Covers(BodyPartGroupDefOf.Legs))
					{
						return !Covers(BodyPartGroupDefOf.Torso);
					}
					return true;
				case Gender.None:
					return false;
				default:
					return false;
				}
			}

			public bool SatisfiesNeededWarmth(NeededWarmth warmth, bool mustBeSafe = false, float mapTemperature = 21f)
			{
				if (warmth == NeededWarmth.Any)
				{
					return true;
				}
				if (mustBeSafe && !SafeTemperature(mapTemperature))
				{
					return false;
				}
				return warmth switch
				{
					NeededWarmth.Cool => aps.Sum((ThingStuffPair a) => a.InsulationHeat) >= -2f, 
					NeededWarmth.Warm => aps.Sum((ThingStuffPair a) => a.InsulationCold) >= 52f, 
					_ => throw new NotImplementedException(), 
				};
			}

			private bool SafeTemperature(float temp)
			{
				if (pawn != null)
				{
					return pawn.SafeTemperatureRange(aps).Includes(temp);
				}
				return GenTemperature.SafeTemperatureRange(raceDef, aps).Includes(temp);
			}

			public void AddFreeWarmthAsNeeded(NeededWarmth warmth, float mapTemperature, Pawn pawn)
			{
				if (warmth == NeededWarmth.Any || warmth == NeededWarmth.Cool)
				{
					return;
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					debugSb.AppendLine();
					debugSb.AppendLine("Trying to give free warm layer.");
				}
				FactionDef homeFaction = pawn.HomeFaction?.def;
				for (int i = 0; i < 3; i++)
				{
					if (!SatisfiesNeededWarmth(warmth, mustBeSafe: true, mapTemperature))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							debugSb.AppendLine($"Checking to give free torso-cover at max price {freeWarmParkaMaxPrice}");
						}
						for (int j = 0; j < 2; j++)
						{
							ThingStuffPair candidate;
							if (j == 0)
							{
								if (!allApparelPairs.Where((ThingStuffPair pa) => ParkaPairValidator(pa) && pa.InsulationCold < 40f).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out candidate))
								{
									continue;
								}
							}
							else if (!allApparelPairs.Where(ParkaPairValidator).TryMaxBy((ThingStuffPair x) => x.InsulationCold - GetReplacedInsulationCold(x), out candidate))
							{
								continue;
							}
							if (DebugViewSettings.logApparelGeneration)
							{
								debugSb.AppendLine($"Giving free torso-cover: {candidate} insulation={candidate.InsulationCold}");
								foreach (ThingStuffPair item in aps.Where((ThingStuffPair a) => !ApparelUtility.CanWearTogether(a.thing, candidate.thing, body)))
								{
									debugSb.AppendLine($"    -replaces {item} InsulationCold={item.InsulationCold}");
								}
							}
							aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, candidate.thing, body));
							aps.Add(candidate);
							break;
						}
					}
					if (SafeTemperature(mapTemperature))
					{
						break;
					}
				}
				if (!SatisfiesNeededWarmth(warmth, mustBeSafe: true, mapTemperature))
				{
					if (DebugViewSettings.logApparelGeneration)
					{
						debugSb.AppendLine($"Checking to give free hat at max price {freeWarmHatMaxPrice}");
					}
					if (allApparelPairs.Where(HatPairValidator).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out var hatPair))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							debugSb.AppendLine($"Giving free hat: {hatPair} insulation={hatPair.InsulationCold}");
							foreach (ThingStuffPair item2 in aps.Where((ThingStuffPair a) => !ApparelUtility.CanWearTogether(a.thing, hatPair.thing, body)))
							{
								debugSb.AppendLine($"    -replaces {item2} InsulationCold={item2.InsulationCold}");
							}
						}
						aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, hatPair.thing, body));
						aps.Add(hatPair);
					}
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					debugSb.AppendLine($"New TotalInsulationCold: {TotalInsulationCold}");
				}
				bool HatPairValidator(ThingStuffPair pa)
				{
					if (pa.Price > freeWarmHatMaxPrice)
					{
						return false;
					}
					if (!pa.thing.apparel.canBeGeneratedToSatisfyWarmth)
					{
						return false;
					}
					if (pa.InsulationCold < 7f)
					{
						return false;
					}
					if (!pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) && !pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead))
					{
						return false;
					}
					if (GetReplacedInsulationCold(pa) >= pa.InsulationCold)
					{
						return false;
					}
					if (!CorrectFactionForApparel(homeFaction, pa.thing))
					{
						return false;
					}
					return true;
				}
				bool ParkaPairValidator(ThingStuffPair pa)
				{
					if (pa.Price > freeWarmParkaMaxPrice)
					{
						return false;
					}
					if (pa.InsulationCold <= 0f)
					{
						return false;
					}
					if (!pa.thing.apparel.canBeGeneratedToSatisfyWarmth)
					{
						return false;
					}
					if (!pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						return false;
					}
					if (!pa.thing.apparel.CorrectAgeForWearing(pawn))
					{
						return false;
					}
					if (GetReplacedInsulationCold(pa) >= pa.InsulationCold)
					{
						return false;
					}
					if (!CorrectFactionForApparel(homeFaction, pa.thing))
					{
						return false;
					}
					return true;
				}
			}

			public bool SatisfiesNeededToxicEnvironmentResistance(float pollution)
			{
				if (pollution <= 0f)
				{
					return true;
				}
				return aps.Sum((ThingStuffPair ap) => ap.ToxicEnvironmentResistance) >= ToxicEnvironmentResistanceOverPollutionCurve.Evaluate(pollution);
			}

			public void AddFreeVacuumResistanceAsNeeded(Pawn pawn)
			{
				if (!ModsConfig.OdysseyActive)
				{
					return;
				}
				float desired = 0.9f - pawn.GetStatValue(StatDefOf.VacuumResistance);
				if (desired <= 0f)
				{
					return;
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					debugSb.AppendLine();
					debugSb.AppendLine("Trying to give free vacuum resistance.");
				}
				for (int i = 0; i < 10; i++)
				{
					if (Protected())
					{
						break;
					}
					if (!allApparelPairs.Where(VacuumValidator).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out var pair))
					{
						continue;
					}
					if (DebugViewSettings.logApparelGeneration)
					{
						debugSb.AppendLine($"Giving free vacuum resistance: {pair} VacuumResistance={pair.VacuumResistance}");
						foreach (ThingStuffPair item in aps.Where((ThingStuffPair a) => !ApparelUtility.CanWearTogether(a.thing, pair.thing, body)))
						{
							debugSb.AppendLine($"    -replaces {item} VacuumResistance={item.VacuumResistance}");
						}
					}
					for (int num = aps.Count - 1; num >= 0; num--)
					{
						ThingStuffPair thingStuffPair = aps[num];
						if (!ApparelUtility.CanWearTogether(thingStuffPair.thing, pair.thing, body))
						{
							if (DebugViewSettings.logApparelGeneration)
							{
								debugSb.AppendLine($"    -replaces {thingStuffPair} VacuumResistance={thingStuffPair.VacuumResistance}");
							}
							desired += thingStuffPair.VacuumResistance;
							aps.RemoveAt(num);
						}
					}
					aps.Add(pair);
					desired -= pair.VacuumResistance;
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					debugSb.AppendLine($"New VacuumResistance: {aps.Sum((ThingStuffPair a) => a.VacuumResistance)}");
				}
				bool Protected()
				{
					if (desired < 0.9f)
					{
						return false;
					}
					return GetUnprotectedVacuumParts().Empty();
				}
				bool VacuumValidator(ThingStuffPair thingStuffPair2)
				{
					if (!thingStuffPair2.thing.apparel.canBeGeneratedToSatisfyVacuumResistance)
					{
						return false;
					}
					if (thingStuffPair2.VacuumResistance < 0.1f)
					{
						return false;
					}
					if (thingStuffPair2.Price > freeVacuumResistanceApparelMaxPrice)
					{
						return false;
					}
					if (!thingStuffPair2.thing.apparel.CorrectAgeForWearing(pawn))
					{
						return false;
					}
					if (!CoversUnprotectedVacuumPart(thingStuffPair2.thing))
					{
						return false;
					}
					for (int j = 0; j < aps.Count; j++)
					{
						if (!ApparelUtility.CanWearTogether(aps[j].thing, thingStuffPair2.thing, body) && thingStuffPair2.VacuumResistance - aps[j].VacuumResistance <= 0.15f)
						{
							return false;
						}
					}
					return true;
				}
			}

			private bool CoversUnprotectedVacuumPart(ThingDef thing)
			{
				foreach (BodyPartRecord unprotectedVacuumPart in GetUnprotectedVacuumParts())
				{
					for (int i = 0; i < unprotectedVacuumPart.groups.Count; i++)
					{
						BodyPartGroupDef bodyPartGroupDef = unprotectedVacuumPart.groups[i];
						for (int j = 0; j < thing.apparel.bodyPartGroups.Count; j++)
						{
							BodyPartGroupDef bodyPartGroupDef2 = thing.apparel.bodyPartGroups[j];
							if (bodyPartGroupDef == bodyPartGroupDef2)
							{
								return true;
							}
						}
					}
				}
				return false;
			}

			private List<BodyPartRecord> GetUnprotectedVacuumParts()
			{
				tmpParts.AddRange(pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Outside));
				for (int num = tmpParts.Count - 1; num >= 0; num--)
				{
					BodyPartRecord bodyPartRecord = tmpParts[num];
					if (!bodyPartRecord.def.canBeVacuumBurnt || !bodyPartRecord.def.IsSkinCovered(bodyPartRecord, pawn.health.hediffSet))
					{
						tmpParts.RemoveAt(num);
					}
					else
					{
						for (int i = 0; i < aps.Count; i++)
						{
							ThingStuffPair thingStuffPair = aps[i];
							if (thingStuffPair.VacuumResistance < 0.1f)
							{
								continue;
							}
							for (int j = 0; j < bodyPartRecord.groups.Count; j++)
							{
								BodyPartGroupDef bodyPartGroupDef = bodyPartRecord.groups[j];
								int num2 = 0;
								while (num2 < thingStuffPair.thing.apparel.bodyPartGroups.Count)
								{
									BodyPartGroupDef bodyPartGroupDef2 = thingStuffPair.thing.apparel.bodyPartGroups[num2];
									if (bodyPartGroupDef != bodyPartGroupDef2)
									{
										num2++;
										continue;
									}
									goto IL_0121;
								}
							}
							continue;
							IL_0121:
							tmpParts.RemoveAt(num);
							break;
						}
					}
				}
				return tmpParts;
			}

			public void AddFreeToxicEnvironmentResistanceAsNeeded(float pollution, Func<ThingStuffPair, bool> extraValidator = null)
			{
				for (int i = 0; i < 5; i++)
				{
					if (SatisfiesNeededToxicEnvironmentResistance(pollution))
					{
						break;
					}
					if (!allApparelPairs.Where(PollutionApparelValidator).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out var pollutionPair))
					{
						continue;
					}
					if (DebugViewSettings.logApparelGeneration)
					{
						debugSb.AppendLine($"Giving free toxic environment resistance: {pollutionPair} ToxicEnvironmentResistance={pollutionPair.ToxicEnvironmentResistance}");
						foreach (ThingStuffPair item in aps.Where((ThingStuffPair a) => !ApparelUtility.CanWearTogether(a.thing, pollutionPair.thing, body)))
						{
							debugSb.AppendLine($"    -replaces {item} ToxicEnvironmentResistance={item.ToxicEnvironmentResistance}");
						}
					}
					aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, pollutionPair.thing, body));
					aps.Add(pollutionPair);
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					debugSb.AppendLine($"New ToxicEnvironmentResistance: {aps.Sum((ThingStuffPair a) => a.ToxicEnvironmentResistance)}");
				}
				bool PollutionApparelValidator(ThingStuffPair pa)
				{
					if (!pa.thing.apparel.canBeGeneratedToSatisfyToxicEnvironmentResistance)
					{
						return false;
					}
					if (pa.ToxicEnvironmentResistance <= 0.25f)
					{
						return false;
					}
					if (pa.Price > freeToxicEnvironmentResistanceApparelMaxPrice)
					{
						return false;
					}
					if (!pa.thing.apparel.CorrectAgeForWearing(pawn))
					{
						return false;
					}
					if (extraValidator != null && !extraValidator(pa))
					{
						return false;
					}
					for (int j = 0; j < aps.Count; j++)
					{
						if (!ApparelUtility.CanWearTogether(aps[j].thing, pa.thing, body) && aps[j].ToxicEnvironmentResistance >= 0.25f && pa.ToxicEnvironmentResistance - aps[j].ToxicEnvironmentResistance <= 0.15f)
						{
							return false;
						}
					}
					return true;
				}
			}

			public void GiveToPawn(Pawn pawn)
			{
				for (int i = 0; i < aps.Count; i++)
				{
					Apparel apparel = (Apparel)ThingMaker.MakeThing(aps[i].thing, aps[i].stuff);
					PawnGenerator.PostProcessGeneratedGear(apparel, pawn);
					if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
					{
						pawn.apparel.Wear(apparel, dropReplacedApparel: false);
					}
				}
				for (int j = 0; j < aps.Count; j++)
				{
					for (int k = 0; k < aps.Count; k++)
					{
						if (j != k && !ApparelUtility.CanWearTogether(aps[j].thing, aps[k].thing, pawn.RaceProps.body))
						{
							Log.Error($"{pawn} generated with apparel that cannot be worn together: {aps[j]}, {aps[k]}");
							return;
						}
					}
				}
			}

			public float GetReplacedInsulationCold(ThingStuffPair newAp)
			{
				float num = 0f;
				for (int i = 0; i < aps.Count; i++)
				{
					if (!ApparelUtility.CanWearTogether(aps[i].thing, newAp.thing, body))
					{
						num += aps[i].InsulationCold;
					}
				}
				return num;
			}

			private bool CorrectFactionForApparel(FactionDef faction, ThingDef apparel)
			{
				if (faction != null)
				{
					if (apparel.apparel.anyTechLevelCanUseForWarmth)
					{
						return true;
					}
					if ((int)faction.techLevel >= 4 && apparel.techLevel == TechLevel.Neolithic)
					{
						return false;
					}
					if (faction.techLevel == TechLevel.Neolithic && (int)apparel.techLevel >= 4)
					{
						return false;
					}
				}
				return true;
			}

			public override string ToString()
			{
				string text = "[";
				for (int i = 0; i < aps.Count; i++)
				{
					text += $"{aps[i]}, ";
				}
				return text + "]";
			}
		}

		private static List<ThingStuffPair> allApparelPairs;

		private static float freeWarmParkaMaxPrice;

		private static float freeWarmHatMaxPrice;

		private static float freeVacuumResistanceApparelMaxPrice;

		private static float freeToxicEnvironmentResistanceApparelMaxPrice;

		private static PossibleApparelSet workingSet;

		private static StringBuilder debugSb;

		private const int PracticallyInfinity = 9999999;

		private const float MinMapPollutionForFreeToxicResistanceApparel = 0.05f;

		private static List<ThingStuffPair> tmpApparelCandidates;

		private static List<ThingStuffPair> usableApparel;

		static PawnApparelGenerator()
		{
			allApparelPairs = new List<ThingStuffPair>();
			workingSet = new PossibleApparelSet();
			debugSb = null;
			tmpApparelCandidates = new List<ThingStuffPair>();
			usableApparel = new List<ThingStuffPair>();
			Reset();
		}

		public static void Reset()
		{
			allApparelPairs = ThingStuffPair.AllWith((ThingDef td) => td.IsApparel);
			freeWarmParkaMaxPrice = (int)(StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_Parka, ThingDefOf.Cloth) * 1.3f);
			freeWarmHatMaxPrice = (int)(StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_Tuque, ThingDefOf.Cloth) * 1.3f);
			freeToxicEnvironmentResistanceApparelMaxPrice = (ModsConfig.BiotechActive ? ((int)(StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_GasMask) * 1.3f)) : 0);
			if (ModsConfig.OdysseyActive)
			{
				int num = (int)StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_Vacsuit);
				int num2 = (int)StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_VacsuitHelmet);
				freeVacuumResistanceApparelMaxPrice = (float)(num + num2) * 1.3f;
			}
			else
			{
				freeVacuumResistanceApparelMaxPrice = 0f;
			}
		}

		public static void GenerateStartingApparelFor(Pawn pawn, PawnGenerationRequest request)
		{
			if (!pawn.RaceProps.ToolUser || !pawn.RaceProps.IsFlesh || pawn.RaceProps.IsAnomalyEntity)
			{
				return;
			}
			pawn.apparel.DestroyAll();
			pawn.outfits?.forcedHandler?.Reset();
			float randomInRange = pawn.kindDef.apparelMoney.RandomInRange;
			float mapTemperature;
			NeededWarmth neededWarmth = ApparelWarmthNeededNow(pawn, request, out mapTemperature);
			bool flag = NeedVacuumResistance(pawn, request);
			bool flag2 = Rand.Value < pawn.kindDef.apparelAllowHeadgearChance;
			float num = ApparelToxicEnvironmentToAddress(pawn, request);
			debugSb = null;
			if (DebugViewSettings.logApparelGeneration)
			{
				debugSb = new StringBuilder();
				debugSb.AppendLine($"Generating apparel for {pawn}");
				debugSb.AppendLine($"Money: {randomInRange:F0}");
				debugSb.AppendLine($"Needed warmth: {neededWarmth}");
				debugSb.AppendLine($"Needs vacuum resistance: {flag}");
				debugSb.AppendLine($"Needed toxic environment resistance: {num}");
				debugSb.AppendLine($"Headgear allowed: {flag2}");
			}
			int fixedSeed = Rand.Int;
			tmpApparelCandidates.Clear();
			for (int i = 0; i < allApparelPairs.Count; i++)
			{
				ThingStuffPair thingStuffPair = allApparelPairs[i];
				if (CanUsePair(thingStuffPair, pawn, randomInRange, flag2, fixedSeed))
				{
					tmpApparelCandidates.Add(thingStuffPair);
				}
			}
			if (!pawn.IsColonist && Rand.Chance(pawn.kindDef.nakedChance))
			{
				debugSb?.AppendLine("Apparel overridden by nakedChance in PawnKindDef");
				workingSet.Reset(pawn);
				GenerateSpecificRequiredApparel(pawn, randomInRange, onlyGenerateIgnoreNaked: true);
			}
			else if (randomInRange < 0.001f)
			{
				GenerateWorkingPossibleApparelSetFor(pawn, randomInRange, tmpApparelCandidates);
			}
			else
			{
				int num2 = 0;
				while (true)
				{
					GenerateWorkingPossibleApparelSetFor(pawn, randomInRange, tmpApparelCandidates);
					debugSb?.Append(num2.ToString().PadRight(5) + "Trying: " + workingSet);
					if (num2 < 10 && Rand.Value < 0.85f && randomInRange < 9999999f)
					{
						float num3 = Rand.Range(0.45f, 0.8f);
						float totalPrice = workingSet.TotalPrice;
						if (totalPrice < randomInRange * num3)
						{
							debugSb?.AppendLine(" -- Failed: Spent $" + totalPrice.ToString("F0") + ", < " + (num3 * 100f).ToString("F0") + "% of money.");
							goto IL_04e5;
						}
					}
					if (num2 < 20 && Rand.Value < 0.97f && !workingSet.Covers(BodyPartGroupDefOf.Torso))
					{
						debugSb?.AppendLine(" -- Failed: Does not cover torso.");
					}
					else if (num2 < 30 && Rand.Value < 0.8f && workingSet.CoatButNoShirt())
					{
						debugSb?.AppendLine(" -- Failed: Coat but no shirt.");
					}
					else
					{
						if (num2 < 50)
						{
							bool mustBeSafe = num2 < 17;
							if (!workingSet.SatisfiesNeededWarmth(neededWarmth, mustBeSafe, mapTemperature))
							{
								debugSb?.AppendLine(" -- Failed: Wrong warmth.");
								goto IL_04e5;
							}
						}
						if (ModsConfig.BiotechActive && num2 < 10 && !workingSet.SatisfiesNeededToxicEnvironmentResistance(num))
						{
							debugSb?.AppendLine(" -- Failed: Wrong toxic environment resistance.");
						}
						else
						{
							if (num2 >= 80 || !workingSet.IsNaked(pawn.gender))
							{
								break;
							}
							debugSb?.AppendLine(" -- Failed: Naked.");
						}
					}
					goto IL_04e5;
					IL_04e5:
					num2++;
				}
				debugSb?.Append(" -- Approved! Total price: $" + workingSet.TotalPrice.ToString("F0") + ", TotalInsulationCold: " + workingSet.TotalInsulationCold);
			}
			if ((!pawn.kindDef.apparelIgnoreSeasons || request.ForceAddFreeWarmLayerIfNeeded) && !workingSet.SatisfiesNeededWarmth(neededWarmth, mustBeSafe: true, mapTemperature))
			{
				workingSet.AddFreeWarmthAsNeeded(neededWarmth, mapTemperature, pawn);
			}
			if (ModsConfig.BiotechActive && !pawn.kindDef.apparelIgnorePollution && num > 0.05f && !workingSet.SatisfiesNeededToxicEnvironmentResistance(num))
			{
				workingSet.AddFreeToxicEnvironmentResistanceAsNeeded(num, delegate(ThingStuffPair pa)
				{
					if (!pa.thing.apparel.CorrectAgeForWearing(pawn))
					{
						return false;
					}
					if (pawn.kindDef.apparelIgnoreSeasons && !request.ForceAddFreeWarmLayerIfNeeded)
					{
						return true;
					}
					return !(workingSet.GetReplacedInsulationCold(pa) > pa.InsulationCold);
				});
			}
			if (flag)
			{
				workingSet.AddFreeVacuumResistanceAsNeeded(pawn);
			}
			if (DebugViewSettings.logApparelGeneration && debugSb != null && debugSb.Length > 0)
			{
				Log.Message(debugSb.ToString());
			}
			workingSet.GiveToPawn(pawn);
			workingSet.Reset(null, null);
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				PostProcessApparel(item, pawn);
				CompBiocodable compBiocodable = item.TryGetComp<CompBiocodable>();
				if (compBiocodable != null && !compBiocodable.Biocoded && Rand.Chance(request.BiocodeApparelChance))
				{
					compBiocodable.CodeFor(pawn);
				}
			}
		}

		public static void PostProcessApparel(Apparel apparel, Pawn pawn)
		{
			if (pawn.kindDef.apparelColor != Color.white)
			{
				apparel.SetColor(pawn.kindDef.apparelColor, reportFailure: false);
			}
			ThingStyleDef thingStyleDef = pawn.Ideo?.GetStyleFor(apparel.def);
			if (thingStyleDef != null)
			{
				apparel.SetStyleDef(thingStyleDef);
			}
			List<SpecificApparelRequirement> specificApparelRequirements = pawn.kindDef.specificApparelRequirements;
			if (specificApparelRequirements == null)
			{
				return;
			}
			for (int i = 0; i < specificApparelRequirements.Count; i++)
			{
				if (!ApparelRequirementHandlesThing(specificApparelRequirements[i], apparel.def))
				{
					continue;
				}
				Color color = specificApparelRequirements[i].GetColor();
				if (color != default(Color))
				{
					apparel.SetColor(color, reportFailure: false);
				}
				if (specificApparelRequirements[i].UseRandomStyleDef)
				{
					if (!apparel.def.randomStyle.NullOrEmpty() && Rand.Chance(apparel.def.randomStyleChance))
					{
						apparel.SetStyleDef(apparel.def.randomStyle.RandomElementByWeight((ThingStyleChance x) => x.Chance).StyleDef);
					}
				}
				else if (specificApparelRequirements[i].StyleDef != null)
				{
					apparel.SetStyleDef(specificApparelRequirements[i].StyleDef);
				}
				if (specificApparelRequirements[i].Locked)
				{
					pawn.apparel.Lock(apparel);
				}
				if (specificApparelRequirements[i].Biocode)
				{
					apparel.TryGetComp<CompBiocodable>()?.CodeFor(pawn);
				}
			}
		}

		public static Apparel GenerateApparelOfDefFor(Pawn pawn, ThingDef apparelDef)
		{
			if (!allApparelPairs.Where((ThingStuffPair pa) => pa.thing == apparelDef && CanUseStuff(pawn, pa)).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality, out var result) && !allApparelPairs.Where((ThingStuffPair pa) => pa.thing == apparelDef).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality, out result))
			{
				return null;
			}
			return (Apparel)ThingMaker.MakeThing(result.thing, result.stuff);
		}

		private static void GenerateWorkingPossibleApparelSetFor(Pawn pawn, float money, List<ThingStuffPair> apparelCandidates)
		{
			workingSet.Reset(pawn);
			float moneyLeft = money;
			moneyLeft = GenerateSpecificRequiredApparel(pawn, moneyLeft, onlyGenerateIgnoreNaked: false);
			List<ThingDef> reqApparel = pawn.kindDef.apparelRequired;
			if (reqApparel != null)
			{
				int i;
				for (i = 0; i < reqApparel.Count; i++)
				{
					if (reqApparel[i].apparel.CorrectAgeForWearing(pawn) && allApparelPairs.Where((ThingStuffPair pa) => pa.thing == reqApparel[i] && CanUseStuff(pawn, pa) && !workingSet.PairOverlapsAnything(pa)).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality, out var result))
					{
						workingSet.Add(result);
						moneyLeft -= result.Price;
					}
				}
			}
			usableApparel.Clear();
			for (int num = 0; num < apparelCandidates.Count; num++)
			{
				if (!workingSet.PairOverlapsAnything(apparelCandidates[num]))
				{
					usableApparel.Add(apparelCandidates[num]);
				}
			}
			ThingStuffPair result2;
			while ((pawn.Ideo == null || !pawn.Ideo.IdeoPrefersNudityForGender(pawn.gender) || (pawn.Faction != null && pawn.Faction.IsPlayer)) && (pawn.IsColonist || pawn.story?.traits == null || !pawn.story.traits.HasTrait(TraitDefOf.Nudist)) && (!(Rand.Value < 0.1f) || !(money < 9999999f)) && usableApparel.Where((ThingStuffPair pa) => CanUseStuff(pawn, pa)).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality, out result2))
			{
				workingSet.Add(result2);
				moneyLeft -= result2.Price;
				for (int num2 = usableApparel.Count - 1; num2 >= 0; num2--)
				{
					if (usableApparel[num2].Price > moneyLeft || workingSet.PairOverlapsAnything(usableApparel[num2]))
					{
						usableApparel.RemoveAt(num2);
					}
				}
			}
		}

		private static float GenerateSpecificRequiredApparel(Pawn pawn, float moneyLeft, bool onlyGenerateIgnoreNaked)
		{
			List<SpecificApparelRequirement> att = pawn.kindDef.specificApparelRequirements;
			if (att != null)
			{
				int i;
				for (i = 0; i < att.Count; i++)
				{
					if ((!att[i].RequiredTag.NullOrEmpty() || (!att[i].AlternateTagChoices.NullOrEmpty() && (!onlyGenerateIgnoreNaked || att[i].IgnoreNaked))) && allApparelPairs.Where((ThingStuffPair pa) => ApparelRequirementTagsMatch(att[i], pa.thing) && ApparelRequirementHandlesThing(att[i], pa.thing) && CanUseStuff(pawn, pa) && pa.thing.apparel.PawnCanWear(pawn) && !workingSet.PairOverlapsAnything(pa)).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality, out var result))
					{
						workingSet.Add(result);
						moneyLeft -= result.Price;
					}
				}
			}
			return moneyLeft;
		}

		private static bool CanUseStuff(Pawn pawn, ThingStuffPair pair)
		{
			if (pair.stuff != null && !pair.stuff.stuffProps.allowedInStuffGeneration)
			{
				return false;
			}
			List<SpecificApparelRequirement> specificApparelRequirements = pawn.kindDef.specificApparelRequirements;
			if (specificApparelRequirements != null)
			{
				for (int i = 0; i < specificApparelRequirements.Count; i++)
				{
					if (!ApparelRequirementCanUseStuff(specificApparelRequirements[i], pair))
					{
						return false;
					}
				}
			}
			if (pair.stuff != null && pawn.Faction != null && !pawn.kindDef.ignoreFactionApparelStuffRequirements && !pawn.Faction.def.CanUseStuffForApparel(pair.stuff))
			{
				return false;
			}
			return true;
		}

		public static bool IsDerpApparel(ThingDef thing, ThingDef stuff)
		{
			if (stuff == null)
			{
				return false;
			}
			if (!thing.IsApparel)
			{
				return false;
			}
			bool flag = false;
			for (int i = 0; i < thing.stuffCategories.Count; i++)
			{
				if (thing.stuffCategories[i] != StuffCategoryDefOf.Woody && thing.stuffCategories[i] != StuffCategoryDefOf.Stony)
				{
					flag = true;
					break;
				}
			}
			if (flag && (stuff.stuffProps.categories.Contains(StuffCategoryDefOf.Woody) || stuff.stuffProps.categories.Contains(StuffCategoryDefOf.Stony)) && (thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso) || thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs)))
			{
				return true;
			}
			return false;
		}

		public static bool ApparelRequirementHandlesThing(SpecificApparelRequirement req, ThingDef thing)
		{
			if (req.BodyPartGroup != null && !thing.apparel.bodyPartGroups.Contains(req.BodyPartGroup))
			{
				return false;
			}
			if (req.ApparelLayer != null && !thing.apparel.layers.Contains(req.ApparelLayer))
			{
				return false;
			}
			if (req.ApparelDef != null && thing != req.ApparelDef)
			{
				return false;
			}
			return true;
		}

		public static bool ApparelRequirementTagsMatch(SpecificApparelRequirement req, ThingDef thing)
		{
			if (!req.RequiredTag.NullOrEmpty() && thing.apparel.tags.Contains(req.RequiredTag))
			{
				return true;
			}
			if (!req.AlternateTagChoices.NullOrEmpty())
			{
				return req.AlternateTagChoices.Any((SpecificApparelRequirement.TagChance x) => thing.apparel.tags.Contains(x.tag) && Rand.Value < x.chance);
			}
			return false;
		}

		private static bool ApparelRequirementCanUseStuff(SpecificApparelRequirement req, ThingStuffPair pair)
		{
			if (req.Stuff == null)
			{
				return true;
			}
			if (!ApparelRequirementHandlesThing(req, pair.thing))
			{
				return true;
			}
			if (pair.stuff != null)
			{
				return req.Stuff == pair.stuff;
			}
			return false;
		}

		private static bool CanUsePair(ThingStuffPair pair, Pawn pawn, float moneyLeft, bool allowHeadgear, int fixedSeed)
		{
			if (pair.Price > moneyLeft)
			{
				return false;
			}
			if (!allowHeadgear && IsHeadgear(pair.thing))
			{
				return false;
			}
			if (!pair.thing.apparel.PawnCanWear(pawn))
			{
				return false;
			}
			if (!pawn.kindDef.apparelTags.NullOrEmpty())
			{
				bool flag = false;
				for (int i = 0; i < pawn.kindDef.apparelTags.Count; i++)
				{
					for (int j = 0; j < pair.thing.apparel.tags.Count; j++)
					{
						if (pawn.kindDef.apparelTags[i] == pair.thing.apparel.tags[j])
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
				if (!flag)
				{
					return false;
				}
			}
			if (!pawn.kindDef.apparelDisallowTags.NullOrEmpty())
			{
				for (int k = 0; k < pawn.kindDef.apparelDisallowTags.Count; k++)
				{
					if (pair.thing.apparel.tags.Contains(pawn.kindDef.apparelDisallowTags[k]))
					{
						return false;
					}
				}
			}
			if (!pawn.kindDef.ignoreApparelAllowChance && pair.thing.generateAllowChance < 1f && !Rand.ChanceSeeded(pair.thing.generateAllowChance, fixedSeed ^ pair.thing.shortHash ^ 0x3D28557))
			{
				return false;
			}
			return true;
		}

		public static bool IsHeadgear(ThingDef td)
		{
			if (!td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) && !td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead))
			{
				return td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Eyes);
			}
			return true;
		}

		private static bool NeedVacuumResistance(Pawn pawn, PawnGenerationRequest request)
		{
			if (!ModsConfig.OdysseyActive)
			{
				return false;
			}
			PlanetTile tile = request.Tile;
			if (!tile.Valid)
			{
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				if (anyPlayerHomeMap != null)
				{
					tile = anyPlayerHomeMap.Tile;
				}
			}
			if (!tile.Valid || !tile.Tile.PrimaryBiome.inVacuum)
			{
				return false;
			}
			return pawn.ConcernedByVacuum;
		}

		private static NeededWarmth ApparelWarmthNeededNow(Pawn pawn, PawnGenerationRequest request, out float mapTemperature)
		{
			PlanetTile tile = request.Tile;
			if (!tile.Valid)
			{
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				if (anyPlayerHomeMap != null)
				{
					tile = anyPlayerHomeMap.Tile;
				}
			}
			if (!tile.Valid)
			{
				mapTemperature = 21f;
				return NeededWarmth.Any;
			}
			NeededWarmth neededWarmth = NeededWarmth.Any;
			Twelfth twelfth = GenLocalDate.Twelfth(tile);
			mapTemperature = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, twelfth);
			for (int i = 0; i < 2; i++)
			{
				NeededWarmth neededWarmth2 = CalculateNeededWarmth(pawn, tile, twelfth);
				if (neededWarmth2 != NeededWarmth.Any)
				{
					neededWarmth = neededWarmth2;
					break;
				}
				twelfth = twelfth.NextTwelfth();
			}
			if (pawn.kindDef.apparelIgnoreSeasons)
			{
				if (request.ForceAddFreeWarmLayerIfNeeded && neededWarmth == NeededWarmth.Warm)
				{
					return neededWarmth;
				}
				return NeededWarmth.Any;
			}
			return neededWarmth;
		}

		public static NeededWarmth CalculateNeededWarmth(Pawn pawn, PlanetTile tile, Twelfth twelfth)
		{
			float num = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, twelfth);
			if (num < pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin) - 4f)
			{
				return NeededWarmth.Warm;
			}
			if (num > pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin) + 4f)
			{
				return NeededWarmth.Cool;
			}
			return NeededWarmth.Any;
		}

		private static float ApparelToxicEnvironmentToAddress(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn.kindDef.apparelIgnorePollution)
			{
				return 0f;
			}
			PlanetTile tile = request.Tile;
			if (!tile.Valid)
			{
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				if (anyPlayerHomeMap != null)
				{
					tile = anyPlayerHomeMap.Tile;
				}
			}
			if (!tile.Valid)
			{
				return 0f;
			}
			return Mathf.Clamp01(Find.WorldGrid[tile].pollution);
		}

		[DebugOutput]
		private static void ApparelPairs()
		{
			DebugTables.MakeTablesDialog(allApparelPairs.OrderByDescending((ThingStuffPair p) => p.thing.defName), new TableDataGetter<ThingStuffPair>("thing", (ThingStuffPair p) => p.thing.defName), new TableDataGetter<ThingStuffPair>("stuff", (ThingStuffPair p) => (p.stuff == null) ? "" : p.stuff.defName), new TableDataGetter<ThingStuffPair>("price", (ThingStuffPair p) => p.Price.ToString()), new TableDataGetter<ThingStuffPair>("commonality", (ThingStuffPair p) => (p.Commonality * 100f).ToString("F4")), new TableDataGetter<ThingStuffPair>("generateCommonality", (ThingStuffPair p) => p.thing.generateCommonality.ToString("F4")), new TableDataGetter<ThingStuffPair>("insulationCold", (ThingStuffPair p) => (p.InsulationCold != 0f) ? p.InsulationCold.ToString() : ""), new TableDataGetter<ThingStuffPair>("headgear", (ThingStuffPair p) => (!IsHeadgear(p.thing)) ? "" : "*"), new TableDataGetter<ThingStuffPair>("derp", (ThingStuffPair p) => (!IsDerpApparel(p.thing, p.stuff)) ? "" : "D"));
		}

		[DebugOutput]
		private static void ApparelPairsByThing()
		{
			DebugOutputsGeneral.MakeTablePairsByThing(allApparelPairs);
		}
	}
}
