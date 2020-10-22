using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class NeurotrainerDefGenerator
	{
		public static string NeurotrainerDefPrefix = "Neurotrainer";

		public static string PsytrainerDefPrefix = "Psytrainer";

		private const int MaxAbilityLevel = 6;

		public static IEnumerable<ThingDef> ImpliedThingDefs()
		{
			foreach (AbilityDef allDef in DefDatabase<AbilityDef>.AllDefs)
			{
				if (typeof(Psycast).IsAssignableFrom(allDef.abilityClass))
				{
					ThingDef thingDef = BaseNeurotrainer();
					thingDef.defName = PsytrainerDefPrefix + "_" + allDef.defName;
					thingDef.label = "PsycastNeurotrainerLabel".Translate(allDef.label);
					thingDef.description = "PsycastNeurotrainerDescription".Translate();
					thingDef.comps.Add(new CompProperties_Neurotrainer
					{
						compClass = typeof(CompNeurotrainer),
						useJob = JobDefOf.UseNeurotrainer,
						useLabel = "PsycastNeurotrainerUseLabel".Translate(allDef.label),
						ability = allDef
					});
					thingDef.comps.Add(new CompProperties_UseEffect
					{
						compClass = typeof(CompUseEffect_GainAbility)
					});
					thingDef.statBases.Add(new StatModifier
					{
						stat = StatDefOf.MarketValue,
						value = Mathf.Round(Mathf.Lerp(100f, 1000f, (float)allDef.level / 6f))
					});
					thingDef.thingCategories = new List<ThingCategoryDef>
					{
						ThingCategoryDefOf.NeurotrainersPsycast
					};
					thingDef.thingSetMakerTags = new List<string>
					{
						"RewardStandardLowFreq"
					};
					thingDef.modContentPack = allDef.modContentPack;
					thingDef.descriptionHyperlinks = new List<DefHyperlink>
					{
						new DefHyperlink(allDef)
					};
					yield return thingDef;
				}
			}
			foreach (SkillDef allDef2 in DefDatabase<SkillDef>.AllDefs)
			{
				ThingDef thingDef2 = BaseNeurotrainer();
				thingDef2.defName = NeurotrainerDefPrefix + "_" + allDef2.defName;
				thingDef2.label = "SkillNeurotrainerLabel".Translate(allDef2.label);
				thingDef2.description = "SkillNeurotrainerDescription".Translate();
				thingDef2.comps.Add(new CompProperties_Neurotrainer
				{
					compClass = typeof(CompNeurotrainer),
					useJob = JobDefOf.UseNeurotrainer,
					useLabel = "SkillNeurotrainerUseLabel".Translate(allDef2.label),
					skill = allDef2
				});
				thingDef2.comps.Add(new CompProperties_UseEffect
				{
					compClass = typeof(CompUseEffect_LearnSkill)
				});
				thingDef2.statBases.Add(new StatModifier
				{
					stat = StatDefOf.MarketValue,
					value = 750f
				});
				thingDef2.thingCategories = new List<ThingCategoryDef>
				{
					ThingCategoryDefOf.NeurotrainersSkill
				};
				thingDef2.thingSetMakerTags = new List<string>
				{
					"RewardStandardHighFreq",
					"SkillNeurotrainer"
				};
				thingDef2.modContentPack = allDef2.modContentPack;
				yield return thingDef2;
			}
		}

		private static ThingDef BaseNeurotrainer()
		{
			return new ThingDef
			{
				category = ThingCategory.Item,
				selectable = true,
				thingClass = typeof(ThingWithComps),
				comps = new List<CompProperties>
				{
					new CompProperties_UseEffectPlaySound
					{
						soundOnUsed = SoundDefOf.MechSerumUsed
					},
					new CompProperties_UseEffect
					{
						compClass = typeof(CompUseEffect_DestroySelf)
					},
					new CompProperties_Forbiddable()
				},
				graphicData = new GraphicData
				{
					texPath = "Things/Item/Special/MechSerumNeurotrainer",
					graphicClass = typeof(Graphic_Single)
				},
				drawGUIOverlay = false,
				statBases = new List<StatModifier>
				{
					new StatModifier
					{
						stat = StatDefOf.MaxHitPoints,
						value = 80f
					},
					new StatModifier
					{
						stat = StatDefOf.Mass,
						value = 0.2f
					},
					new StatModifier
					{
						stat = StatDefOf.DeteriorationRate,
						value = 2f
					},
					new StatModifier
					{
						stat = StatDefOf.Flammability,
						value = 0.2f
					}
				},
				techLevel = TechLevel.Ultra,
				altitudeLayer = AltitudeLayer.Item,
				alwaysHaulable = true,
				rotatable = false,
				pathCost = DefGenerator.StandardItemPathCost,
				tradeTags = new List<string>
				{
					"ExoticMisc"
				},
				stackLimit = 1,
				tradeNeverStack = true,
				forceDebugSpawnable = true
			};
		}
	}
}
