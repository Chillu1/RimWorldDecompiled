using System;
using RimWorld;
using Verse.AI;

namespace Verse;

public static class ThingListGroupHelper
{
	public static readonly ThingRequestGroup[] AllGroups;

	static ThingListGroupHelper()
	{
		AllGroups = new ThingRequestGroup[Enum.GetValues(typeof(ThingRequestGroup)).Length];
		int num = 0;
		foreach (object value in Enum.GetValues(typeof(ThingRequestGroup)))
		{
			AllGroups[num] = (ThingRequestGroup)value;
			num++;
		}
	}

	public static bool Includes(this ThingRequestGroup group, ThingDef def)
	{
		switch (group)
		{
		case ThingRequestGroup.Undefined:
			return false;
		case ThingRequestGroup.Nothing:
			return false;
		case ThingRequestGroup.Everything:
			return true;
		case ThingRequestGroup.HaulableEver:
			return def.EverHaulable;
		case ThingRequestGroup.HaulableAlways:
			return def.alwaysHaulable;
		case ThingRequestGroup.Plant:
			return def.category == ThingCategory.Plant;
		case ThingRequestGroup.NonStumpPlant:
			if (def.category == ThingCategory.Plant)
			{
				return !def.plant.isStump;
			}
			return false;
		case ThingRequestGroup.HarvestablePlant:
			if (def.category == ThingCategory.Plant)
			{
				return def.plant.Harvestable;
			}
			return false;
		case ThingRequestGroup.FoodSource:
			if (!def.IsNutritionGivingIngestible)
			{
				return def.thingClass == typeof(Building_NutrientPasteDispenser);
			}
			return true;
		case ThingRequestGroup.MechCharger:
			return def.thingClass == typeof(Building_MechCharger);
		case ThingRequestGroup.FoodSourceNotPlantOrTree:
			if (!def.IsNutritionGivingIngestible || (def.ingestible.foodType & ~FoodTypeFlags.Plant & ~FoodTypeFlags.Tree) == 0)
			{
				return def.thingClass == typeof(Building_NutrientPasteDispenser);
			}
			return true;
		case ThingRequestGroup.HasGUIOverlay:
			return def.drawGUIOverlay;
		case ThingRequestGroup.Corpse:
			return typeof(Corpse).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.Blueprint:
			return def.IsBlueprint;
		case ThingRequestGroup.Construction:
			if (!def.IsBlueprint)
			{
				return def.IsFrame;
			}
			return true;
		case ThingRequestGroup.BuildingArtificial:
			return def.IsBuildingArtificial;
		case ThingRequestGroup.BuildingFrame:
			return def.IsFrame;
		case ThingRequestGroup.Pawn:
			return def.category == ThingCategory.Pawn;
		case ThingRequestGroup.PotentialBillGiver:
			return !def.AllRecipes.NullOrEmpty();
		case ThingRequestGroup.Medicine:
			return def.IsMedicine;
		case ThingRequestGroup.Apparel:
			return def.IsApparel;
		case ThingRequestGroup.MinifiedThing:
			return typeof(MinifiedThing).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.Filth:
			return def.filth != null;
		case ThingRequestGroup.AttackTarget:
			return typeof(IAttackTarget).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.Weapon:
			return def.IsWeapon;
		case ThingRequestGroup.Refuelable:
			return def.HasAssignableCompFrom(typeof(CompRefuelable));
		case ThingRequestGroup.Styleable:
			return def.HasAssignableCompFrom(typeof(CompStyleable));
		case ThingRequestGroup.HaulableEverOrMinifiable:
			if (!def.EverHaulable)
			{
				return def.Minifiable;
			}
			return true;
		case ThingRequestGroup.Drug:
			return def.IsDrug;
		case ThingRequestGroup.Shell:
			return def.IsShell;
		case ThingRequestGroup.Bed:
			return def.IsBed;
		case ThingRequestGroup.Grave:
			return typeof(Building_Grave).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.Art:
			return def.HasAssignableCompFrom(typeof(CompArt));
		case ThingRequestGroup.ThingHolder:
			return def.ThisOrAnyCompIsThingHolder();
		case ThingRequestGroup.ActiveTransporter:
			return typeof(IActiveTransporter).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.Transporter:
			return def.HasAssignableCompFrom(typeof(CompTransporter));
		case ThingRequestGroup.LongRangeMineralScanner:
			return def.HasAssignableCompFrom(typeof(CompLongRangeMineralScanner));
		case ThingRequestGroup.AffectsSky:
			return def.HasAssignableCompFrom(typeof(CompAffectsSky));
		case ThingRequestGroup.WindSource:
			return def.HasAssignableCompFrom(typeof(CompWindSource));
		case ThingRequestGroup.AlwaysFlee:
			return def.alwaysFlee;
		case ThingRequestGroup.Fire:
			return typeof(Fire).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.ResearchBench:
			return typeof(Building_ResearchBench).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.Facility:
			return def.HasAssignableCompFrom(typeof(CompFacility));
		case ThingRequestGroup.AffectedByFacilities:
			return def.HasAssignableCompFrom(typeof(CompAffectedByFacilities));
		case ThingRequestGroup.CreatesInfestations:
			return def.HasAssignableCompFrom(typeof(CompCreatesInfestations));
		case ThingRequestGroup.WithCustomRectForSelector:
			return def.hasCustomRectForSelector;
		case ThingRequestGroup.ProjectileInterceptor:
			return def.HasAssignableCompFrom(typeof(CompProjectileInterceptor));
		case ThingRequestGroup.ConditionCauser:
			return def.GetCompProperties<CompProperties_CausesGameCondition>() != null;
		case ThingRequestGroup.MusicalInstrument:
			return typeof(Building_MusicalInstrument).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.MusicSource:
			return def.HasAssignableCompFrom(typeof(CompPlaysMusic));
		case ThingRequestGroup.Throne:
			return typeof(Building_Throne).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.FoodDispenser:
			return def.IsFoodDispenser;
		case ThingRequestGroup.Projectile:
			return def.projectile != null;
		case ThingRequestGroup.MeditationFocus:
			return def.HasAssignableCompFrom(typeof(CompMeditationFocus));
		case ThingRequestGroup.Chunk:
			if (!def.thingCategories.NullOrEmpty())
			{
				if (!def.thingCategories.Contains(ThingCategoryDefOf.Chunks))
				{
					return def.thingCategories.Contains(ThingCategoryDefOf.StoneChunks);
				}
				return true;
			}
			return false;
		case ThingRequestGroup.Seed:
			return def.GetCompProperties<CompProperties_Plantable>() != null;
		case ThingRequestGroup.DryadSpawner:
			return def.GetCompProperties<CompProperties_TreeConnection>() != null;
		case ThingRequestGroup.Studiable:
			if (!def.HasAssignableCompFrom(typeof(CompStudiable)))
			{
				return ThingRequestGroup.EntityHolder.Includes(def);
			}
			return true;
		case ThingRequestGroup.Suppressable:
			if (!def.HasAssignableCompFrom(typeof(CompActivity)))
			{
				return ThingRequestGroup.EntityHolder.Includes(def);
			}
			return true;
		case ThingRequestGroup.ActionDelay:
			return typeof(SignalAction_Delay).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.MechGestator:
			return typeof(Building_MechGestator).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.Dissolving:
			return def.HasAssignableCompFrom(typeof(CompDissolution));
		case ThingRequestGroup.GenepackHolder:
			return def.GetCompProperties<CompProperties_GenepackContainer>() != null;
		case ThingRequestGroup.SubcoreScanner:
			return typeof(Building_SubcoreScanner).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.BossgroupCaller:
			return def.GetCompProperties<CompProperties_Useable_CallBossgroup>() != null;
		case ThingRequestGroup.WakeUpDormant:
			return def.HasAssignableCompFrom(typeof(CompWakeUpDormant));
		case ThingRequestGroup.Atomizer:
			return def.HasAssignableCompFrom(typeof(CompAtomizer));
		case ThingRequestGroup.WasteProducer:
			return def.HasAssignableCompFrom(typeof(CompWasteProducer));
		case ThingRequestGroup.PowerTrader:
			return def.HasAssignableCompFrom(typeof(CompPowerTrader));
		case ThingRequestGroup.Book:
			return def.thingClass.SameOrSubclassOf<Book>();
		case ThingRequestGroup.HoldingPlatformTarget:
			return def.HasAssignableCompFrom(typeof(CompHoldingPlatformTarget));
		case ThingRequestGroup.MapPortal:
			return typeof(MapPortal).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.BuildingGroundSpawner:
			return typeof(BuildingGroundSpawner).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.PsychicRitualSpot:
			return def.HasAssignableCompFrom(typeof(CompPsychicRitualSpot));
		case ThingRequestGroup.EntityHolder:
			return def.HasAssignableCompFrom(typeof(CompEntityHolder));
		case ThingRequestGroup.SubstructureFootprint:
			if (ModsConfig.OdysseyActive)
			{
				return def.HasAssignableCompFrom(typeof(CompSubstructureFootprint));
			}
			return false;
		case ThingRequestGroup.PassengerShuttle:
			return typeof(Building_PassengerShuttle).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.Fish:
			if (ModsConfig.OdysseyActive)
			{
				return def.thingCategories.NotNullAndContains(ThingCategoryDefOf.Fish);
			}
			return false;
		case ThingRequestGroup.Hackable:
			return def.HasAssignableCompFrom(typeof(CompHackable));
		case ThingRequestGroup.Door:
			return def.IsDoor;
		case ThingRequestGroup.CostProvider:
			return typeof(IPathFindCostProvider).IsAssignableFrom(def.thingClass);
		case ThingRequestGroup.ApparelSource:
			return typeof(IHaulSource).IsAssignableFrom(def.thingClass);
		default:
			throw new ArgumentException("group");
		}
	}
}
