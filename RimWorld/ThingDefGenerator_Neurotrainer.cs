using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ThingDefGenerator_Neurotrainer
{
	public static string NeurotrainerDefPrefix = "Neurotrainer";

	public static string PsytrainerDefPrefix = "Psytrainer";

	private const int MaxAbilityLevel = 6;

	public static IEnumerable<ThingDef> ImpliedThingDefs(bool hotReload = false)
	{
		foreach (AbilityDef allDef in DefDatabase<AbilityDef>.AllDefs)
		{
			if (allDef.IsPsycast)
			{
				ThingDef thingDef = BaseNeurotrainer(PsytrainerDefPrefix + "_" + allDef.defName, hotReload);
				thingDef.label = "PsycastNeurotrainerLabel".Translate(allDef.label);
				thingDef.description = "PsycastNeurotrainerDescription".Translate(allDef.Named("PSYCAST"), allDef.description.Named("PSYCASTDESCRIPTION"));
				thingDef.comps.Add(new CompProperties_Usable
				{
					useJob = JobDefOf.UseNeurotrainer,
					useLabel = "PsycastNeurotrainerUseLabel".Translate(allDef.label),
					showUseGizmo = true
				});
				thingDef.comps.Add(new CompProperties_UseEffect_GainAbility
				{
					ability = allDef
				});
				thingDef.statBases.Add(new StatModifier
				{
					stat = StatDefOf.MarketValue,
					value = Mathf.Round(Mathf.Lerp(100f, 1000f, (float)allDef.level / 6f))
				});
				thingDef.thingCategories = new List<ThingCategoryDef> { ThingCategoryDefOf.NeurotrainersPsycast };
				thingDef.thingSetMakerTags = new List<string> { "RewardStandardLowFreq" };
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
			ThingDef thingDef2 = BaseNeurotrainer(NeurotrainerDefPrefix + "_" + allDef2.defName, hotReload);
			thingDef2.label = "SkillNeurotrainerLabel".Translate(allDef2.label);
			thingDef2.description = "SkillNeurotrainerDescription".Translate();
			thingDef2.comps.Add(new CompProperties_Usable
			{
				useJob = JobDefOf.UseNeurotrainer,
				useLabel = "SkillNeurotrainerUseLabel".Translate(allDef2.label),
				showUseGizmo = true
			});
			thingDef2.comps.Add(new CompProperties_UseEffect_LearnSkill
			{
				skill = allDef2
			});
			thingDef2.statBases.Add(new StatModifier
			{
				stat = StatDefOf.MarketValue,
				value = 750f
			});
			thingDef2.thingCategories = new List<ThingCategoryDef> { ThingCategoryDefOf.NeurotrainersSkill };
			thingDef2.thingSetMakerTags = new List<string> { "RewardStandardMidFreq", "SkillNeurotrainer" };
			thingDef2.modContentPack = allDef2.modContentPack;
			yield return thingDef2;
		}
	}

	private static ThingDef BaseNeurotrainer(string defName, bool hotReload = false)
	{
		ThingDef obj = (hotReload ? (DefDatabase<ThingDef>.GetNamed(defName, errorOnFail: false) ?? new ThingDef()) : new ThingDef());
		obj.defName = defName;
		obj.category = ThingCategory.Item;
		obj.selectable = true;
		obj.thingClass = typeof(ThingWithComps);
		obj.comps = new List<CompProperties>
		{
			new CompProperties_UseEffectPlaySound
			{
				soundOnUsed = SoundDefOf.MechSerumUsed
			},
			new CompProperties_UseEffectDestroySelf
			{
				compClass = typeof(CompUseEffect_DestroySelf)
			},
			new CompProperties_Forbiddable()
		};
		obj.graphicData = new GraphicData
		{
			texPath = "Things/Item/Special/MechSerumNeurotrainer",
			graphicClass = typeof(Graphic_Single)
		};
		obj.drawGUIOverlay = false;
		obj.statBases = new List<StatModifier>
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
		};
		obj.techLevel = TechLevel.Ultra;
		obj.altitudeLayer = AltitudeLayer.Item;
		obj.alwaysHaulable = true;
		obj.rotatable = false;
		obj.pathCost = 14;
		obj.tradeTags = new List<string> { "ExoticMisc" };
		obj.stackLimit = 1;
		obj.tradeNeverStack = true;
		obj.forceDebugSpawnable = true;
		obj.drawerType = DrawerType.MapMeshOnly;
		return obj;
	}
}
