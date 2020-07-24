using RimWorld;
using System;
using Verse.AI;

namespace Verse
{
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
			case ThingRequestGroup.FoodSourceNotPlantOrTree:
				if (!def.IsNutritionGivingIngestible || (def.ingestible.foodType & ~FoodTypeFlags.Plant & ~FoodTypeFlags.Tree) == 0)
				{
					return def.thingClass == typeof(Building_NutrientPasteDispenser);
				}
				return true;
			case ThingRequestGroup.HasGUIOverlay:
				return def.drawGUIOverlay;
			case ThingRequestGroup.Corpse:
				return def.thingClass == typeof(Corpse);
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
				return def.HasComp(typeof(CompRefuelable));
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
				return def.HasComp(typeof(CompArt));
			case ThingRequestGroup.ThingHolder:
				return def.ThisOrAnyCompIsThingHolder();
			case ThingRequestGroup.ActiveDropPod:
				return typeof(IActiveDropPod).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.Transporter:
				return def.HasComp(typeof(CompTransporter));
			case ThingRequestGroup.LongRangeMineralScanner:
				return def.HasComp(typeof(CompLongRangeMineralScanner));
			case ThingRequestGroup.AffectsSky:
				return def.HasComp(typeof(CompAffectsSky));
			case ThingRequestGroup.WindSource:
				return def.HasComp(typeof(CompWindSource));
			case ThingRequestGroup.AlwaysFlee:
				return def.alwaysFlee;
			case ThingRequestGroup.Fire:
				return typeof(Fire).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.ResearchBench:
				return typeof(Building_ResearchBench).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.Facility:
				return def.HasComp(typeof(CompFacility));
			case ThingRequestGroup.AffectedByFacilities:
				return def.HasComp(typeof(CompAffectedByFacilities));
			case ThingRequestGroup.CreatesInfestations:
				return def.HasComp(typeof(CompCreatesInfestations));
			case ThingRequestGroup.WithCustomRectForSelector:
				return def.hasCustomRectForSelector;
			case ThingRequestGroup.ProjectileInterceptor:
				return def.HasComp(typeof(CompProjectileInterceptor));
			case ThingRequestGroup.ConditionCauser:
				return def.GetCompProperties<CompProperties_CausesGameCondition>() != null;
			case ThingRequestGroup.MusicalInstrument:
				return typeof(Building_MusicalInstrument).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.Throne:
				return typeof(Building_Throne).IsAssignableFrom(def.thingClass);
			case ThingRequestGroup.FoodDispenser:
				return def.IsFoodDispenser;
			case ThingRequestGroup.Projectile:
				return def.projectile != null;
			case ThingRequestGroup.MeditationFocus:
				return def.HasComp(typeof(CompMeditationFocus));
			default:
				throw new ArgumentException("group");
			}
		}
	}
}
