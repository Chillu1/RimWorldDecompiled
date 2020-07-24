using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

			private const float StartingMinTemperature = 12f;

			private const float TargetMinTemperature = -40f;

			private const float StartingMaxTemperature = 32f;

			private const float TargetMaxTemperature = 30f;

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
					if (aps[i].thing.apparel.bodyPartGroups.Contains(bp))
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
				if (mustBeSafe && !GenTemperature.SafeTemperatureRange(raceDef, aps).Includes(mapTemperature))
				{
					return false;
				}
				switch (warmth)
				{
				case NeededWarmth.Cool:
					return aps.Sum((ThingStuffPair a) => a.InsulationHeat) >= -2f;
				case NeededWarmth.Warm:
					return aps.Sum((ThingStuffPair a) => a.InsulationCold) >= 52f;
				default:
					throw new NotImplementedException();
				}
			}

			public void AddFreeWarmthAsNeeded(NeededWarmth warmth, float mapTemperature)
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
				for (int i = 0; i < 3; i++)
				{
					if (!SatisfiesNeededWarmth(warmth, mustBeSafe: true, mapTemperature))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							debugSb.AppendLine("Checking to give free torso-cover at max price " + freeWarmParkaMaxPrice);
						}
						Predicate<ThingStuffPair> parkaPairValidator = delegate(ThingStuffPair pa)
						{
							if (pa.Price > freeWarmParkaMaxPrice)
							{
								return false;
							}
							if (pa.InsulationCold <= 0f)
							{
								return false;
							}
							if (!pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
							{
								return false;
							}
							if (!pa.thing.apparel.canBeGeneratedToSatisfyWarmth)
							{
								return false;
							}
							return (!(GetReplacedInsulationCold(pa) >= pa.InsulationCold)) ? true : false;
						};
						for (int j = 0; j < 2; j++)
						{
							ThingStuffPair candidate;
							if (j == 0)
							{
								if (!allApparelPairs.Where((ThingStuffPair pa) => parkaPairValidator(pa) && pa.InsulationCold < 40f).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out candidate))
								{
									continue;
								}
							}
							else if (!allApparelPairs.Where((ThingStuffPair pa) => parkaPairValidator(pa)).TryMaxBy((ThingStuffPair x) => x.InsulationCold - GetReplacedInsulationCold(x), out candidate))
							{
								continue;
							}
							if (DebugViewSettings.logApparelGeneration)
							{
								debugSb.AppendLine(string.Concat("Giving free torso-cover: ", candidate, " insulation=", candidate.InsulationCold));
								foreach (ThingStuffPair item in aps.Where((ThingStuffPair a) => !ApparelUtility.CanWearTogether(a.thing, candidate.thing, body)))
								{
									debugSb.AppendLine("    -replaces " + item.ToString() + " InsulationCold=" + item.InsulationCold);
								}
							}
							aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, candidate.thing, body));
							aps.Add(candidate);
							break;
						}
					}
					if (GenTemperature.SafeTemperatureRange(raceDef, aps).Includes(mapTemperature))
					{
						break;
					}
				}
				if (!SatisfiesNeededWarmth(warmth, mustBeSafe: true, mapTemperature))
				{
					if (DebugViewSettings.logApparelGeneration)
					{
						debugSb.AppendLine("Checking to give free hat at max price " + freeWarmHatMaxPrice);
					}
					Predicate<ThingStuffPair> hatPairValidator = delegate(ThingStuffPair pa)
					{
						if (pa.Price > freeWarmHatMaxPrice)
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
						return (!(GetReplacedInsulationCold(pa) >= pa.InsulationCold)) ? true : false;
					};
					if (allApparelPairs.Where((ThingStuffPair pa) => hatPairValidator(pa)).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out ThingStuffPair hatPair))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							debugSb.AppendLine(string.Concat("Giving free hat: ", hatPair, " insulation=", hatPair.InsulationCold));
							foreach (ThingStuffPair item2 in aps.Where((ThingStuffPair a) => !ApparelUtility.CanWearTogether(a.thing, hatPair.thing, body)))
							{
								debugSb.AppendLine("    -replaces " + item2.ToString() + " InsulationCold=" + item2.InsulationCold);
							}
						}
						aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, hatPair.thing, body));
						aps.Add(hatPair);
					}
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					debugSb.AppendLine("New TotalInsulationCold: " + TotalInsulationCold);
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
							Log.Error(string.Concat(pawn, " generated with apparel that cannot be worn together: ", aps[j], ", ", aps[k]));
							return;
						}
					}
				}
			}

			private float GetReplacedInsulationCold(ThingStuffPair newAp)
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

			public override string ToString()
			{
				string str = "[";
				for (int i = 0; i < aps.Count; i++)
				{
					str = str + aps[i].ToString() + ", ";
				}
				return str + "]";
			}
		}

		private static List<ThingStuffPair> allApparelPairs;

		private static float freeWarmParkaMaxPrice;

		private static float freeWarmHatMaxPrice;

		private static PossibleApparelSet workingSet;

		private static StringBuilder debugSb;

		private const int PracticallyInfinity = 9999999;

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
		}

		public static void GenerateStartingApparelFor(Pawn pawn, PawnGenerationRequest request)
		{
			if (!pawn.RaceProps.ToolUser || !pawn.RaceProps.IsFlesh)
			{
				return;
			}
			pawn.apparel.DestroyAll();
			float randomInRange = pawn.kindDef.apparelMoney.RandomInRange;
			float mapTemperature;
			NeededWarmth neededWarmth = ApparelWarmthNeededNow(pawn, request, out mapTemperature);
			bool allowHeadgear = Rand.Value < pawn.kindDef.apparelAllowHeadgearChance;
			debugSb = null;
			if (DebugViewSettings.logApparelGeneration)
			{
				debugSb = new StringBuilder();
				debugSb.AppendLine("Generating apparel for " + pawn);
				debugSb.AppendLine("Money: " + randomInRange.ToString("F0"));
				debugSb.AppendLine("Needed warmth: " + neededWarmth);
				debugSb.AppendLine("Headgear allowed: " + allowHeadgear);
			}
			int @int = Rand.Int;
			tmpApparelCandidates.Clear();
			for (int i = 0; i < allApparelPairs.Count; i++)
			{
				ThingStuffPair thingStuffPair = allApparelPairs[i];
				if (CanUsePair(thingStuffPair, pawn, randomInRange, allowHeadgear, @int))
				{
					tmpApparelCandidates.Add(thingStuffPair);
				}
			}
			if (randomInRange < 0.001f)
			{
				GenerateWorkingPossibleApparelSetFor(pawn, randomInRange, tmpApparelCandidates);
			}
			else
			{
				int num = 0;
				while (true)
				{
					GenerateWorkingPossibleApparelSetFor(pawn, randomInRange, tmpApparelCandidates);
					if (DebugViewSettings.logApparelGeneration)
					{
						debugSb.Append(num.ToString().PadRight(5) + "Trying: " + workingSet.ToString());
					}
					if (num < 10 && Rand.Value < 0.85f && randomInRange < 9999999f)
					{
						float num2 = Rand.Range(0.45f, 0.8f);
						float totalPrice = workingSet.TotalPrice;
						if (totalPrice < randomInRange * num2)
						{
							if (DebugViewSettings.logApparelGeneration)
							{
								debugSb.AppendLine(" -- Failed: Spent $" + totalPrice.ToString("F0") + ", < " + (num2 * 100f).ToString("F0") + "% of money.");
							}
							goto IL_037d;
						}
					}
					if (num < 20 && Rand.Value < 0.97f && !workingSet.Covers(BodyPartGroupDefOf.Torso))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							debugSb.AppendLine(" -- Failed: Does not cover torso.");
						}
					}
					else if (num < 30 && Rand.Value < 0.8f && workingSet.CoatButNoShirt())
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							debugSb.AppendLine(" -- Failed: Coat but no shirt.");
						}
					}
					else
					{
						if (num < 50)
						{
							bool mustBeSafe = num < 17;
							if (!workingSet.SatisfiesNeededWarmth(neededWarmth, mustBeSafe, mapTemperature))
							{
								if (DebugViewSettings.logApparelGeneration)
								{
									debugSb.AppendLine(" -- Failed: Wrong warmth.");
								}
								goto IL_037d;
							}
						}
						if (num >= 80 || !workingSet.IsNaked(pawn.gender))
						{
							break;
						}
						if (DebugViewSettings.logApparelGeneration)
						{
							debugSb.AppendLine(" -- Failed: Naked.");
						}
					}
					goto IL_037d;
					IL_037d:
					num++;
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					debugSb.Append(" -- Approved! Total price: $" + workingSet.TotalPrice.ToString("F0") + ", TotalInsulationCold: " + workingSet.TotalInsulationCold);
				}
			}
			if ((!pawn.kindDef.apparelIgnoreSeasons || request.ForceAddFreeWarmLayerIfNeeded) && !workingSet.SatisfiesNeededWarmth(neededWarmth, mustBeSafe: true, mapTemperature))
			{
				workingSet.AddFreeWarmthAsNeeded(neededWarmth, mapTemperature);
			}
			if (DebugViewSettings.logApparelGeneration)
			{
				Log.Message(debugSb.ToString());
			}
			workingSet.GiveToPawn(pawn);
			workingSet.Reset(null, null);
			if (pawn.kindDef.apparelColor != Color.white)
			{
				List<Apparel> wornApparel = pawn.apparel.WornApparel;
				for (int j = 0; j < wornApparel.Count; j++)
				{
					wornApparel[j].SetColor(pawn.kindDef.apparelColor, reportFailure: false);
				}
			}
			List<SpecificApparelRequirement> specificApparelRequirements = pawn.kindDef.specificApparelRequirements;
			if (specificApparelRequirements != null)
			{
				foreach (SpecificApparelRequirement item in specificApparelRequirements.Where((SpecificApparelRequirement x) => x.Color != default(Color)))
				{
					List<Apparel> wornApparel2 = pawn.apparel.WornApparel;
					for (int k = 0; k < wornApparel2.Count; k++)
					{
						if (ApparelRequirementHandlesThing(item, wornApparel2[k].def))
						{
							wornApparel2[k].SetColor(item.Color, reportFailure: false);
						}
					}
				}
			}
			foreach (Apparel item2 in pawn.apparel.WornApparel)
			{
				CompBiocodable compBiocodable = item2.TryGetComp<CompBiocodable>();
				if (compBiocodable != null && Rand.Chance(request.BiocodeApparelChance))
				{
					compBiocodable.CodeFor(pawn);
				}
			}
		}

		private static void GenerateWorkingPossibleApparelSetFor(Pawn pawn, float money, List<ThingStuffPair> apparelCandidates)
		{
			workingSet.Reset(pawn.RaceProps.body, pawn.def);
			float num = money;
			List<ThingDef> reqApparel = pawn.kindDef.apparelRequired;
			if (reqApparel != null)
			{
				int j;
				for (j = 0; j < reqApparel.Count; j++)
				{
					ThingStuffPair pair = allApparelPairs.Where((ThingStuffPair pa) => pa.thing == reqApparel[j] && CanUseStuff(pawn, pa)).RandomElementByWeight((ThingStuffPair pa) => pa.Commonality);
					workingSet.Add(pair);
					num -= pair.Price;
				}
			}
			List<SpecificApparelRequirement> att = pawn.kindDef.specificApparelRequirements;
			if (att != null)
			{
				int i;
				for (i = 0; i < att.Count; i++)
				{
					if ((!att[i].RequiredTag.NullOrEmpty() || !att[i].AlternateTagChoices.NullOrEmpty()) && allApparelPairs.Where((ThingStuffPair pa) => ApparelRequirementTagsMatch(att[i], pa.thing) && ApparelRequirementHandlesThing(att[i], pa.thing) && CanUseStuff(pawn, pa) && pa.thing.apparel.CorrectGenderForWearing(pawn.gender) && !workingSet.PairOverlapsAnything(pa)).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality, out ThingStuffPair result))
					{
						workingSet.Add(result);
						num -= result.Price;
					}
				}
			}
			usableApparel.Clear();
			for (int k = 0; k < apparelCandidates.Count; k++)
			{
				if (!workingSet.PairOverlapsAnything(apparelCandidates[k]))
				{
					usableApparel.Add(apparelCandidates[k]);
				}
			}
			ThingStuffPair result2;
			while ((!(Rand.Value < 0.1f) || !(money < 9999999f)) && usableApparel.Where((ThingStuffPair pa) => CanUseStuff(pawn, pa)).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality, out result2))
			{
				workingSet.Add(result2);
				num -= result2.Price;
				for (int num2 = usableApparel.Count - 1; num2 >= 0; num2--)
				{
					if (usableApparel[num2].Price > num || workingSet.PairOverlapsAnything(usableApparel[num2]))
					{
						usableApparel.RemoveAt(num2);
					}
				}
			}
		}

		private static bool CanUseStuff(Pawn pawn, ThingStuffPair pair)
		{
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
			if (pair.stuff != null && pawn.Faction != null && !pawn.Faction.def.CanUseStuffForApparel(pair.stuff))
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
				return req.AlternateTagChoices.Where((SpecificApparelRequirement.TagChance x) => thing.apparel.tags.Contains(x.tag) && Rand.Value < x.chance).Any();
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
			if (!pair.thing.apparel.CorrectGenderForWearing(pawn.gender))
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
			if (pair.thing.generateAllowChance < 1f && !Rand.ChanceSeeded(pair.thing.generateAllowChance, fixedSeed ^ pair.thing.shortHash ^ 0x3D28557))
			{
				return false;
			}
			return true;
		}

		public static bool IsHeadgear(ThingDef td)
		{
			if (!td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
			{
				return td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead);
			}
			return true;
		}

		private static NeededWarmth ApparelWarmthNeededNow(Pawn pawn, PawnGenerationRequest request, out float mapTemperature)
		{
			int tile = request.Tile;
			if (tile == -1)
			{
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				if (anyPlayerHomeMap != null)
				{
					tile = anyPlayerHomeMap.Tile;
				}
			}
			if (tile == -1)
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
				if (neededWarmth2 != 0)
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

		public static NeededWarmth CalculateNeededWarmth(Pawn pawn, int tile, Twelfth twelfth)
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
