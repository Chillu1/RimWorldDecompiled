using System;
using System.Xml;
using RimWorld;
using Verse.AI;

namespace Verse;

public class BackCompatibilityConverter_1_4 : BackCompatibilityConverter
{
	public override bool AppliesToVersion(int majorVer, int minorVer)
	{
		return majorVer switch
		{
			1 => minorVer <= 4, 
			0 => true, 
			_ => false, 
		};
	}

	public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
	{
		if (defType == typeof(ThingDef) && defName == "AncientMechDetritus")
		{
			return "ChunkMechanoidSlag";
		}
		return null;
	}

	public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
	{
		if (providedClassName == "QuestPart_Filter_ThingStudied")
		{
			return typeof(QuestPart_Filter_ThingAnalyzed);
		}
		if (providedClassName == "Building" && node["def"].InnerText == "AncientMechDetritus")
		{
			return typeof(Thing);
		}
		return null;
	}

	public override void PostExposeData(object obj)
	{
		if (Scribe.mode == LoadSaveMode.LoadingVars && obj is Game game)
		{
			game.relationshipRecords = new RelationshipRecords();
			if (game.readingPolicyDatabase == null)
			{
				game.readingPolicyDatabase = new ReadingPolicyDatabase();
			}
		}
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (obj is Thing thing)
		{
			if (thing.def == ThingDefOf.ChunkMechanoidSlag)
			{
				thing.HitPoints = thing.MaxHitPoints;
			}
			if (thing.def == ThingDefOf.VitalsMonitor && thing.Rotation.IsHorizontal)
			{
				thing.Rotation = thing.Rotation.Opposite;
			}
		}
		if (obj is Pawn pawn)
		{
			if (pawn.reading == null && pawn.RaceProps.Humanlike)
			{
				pawn.reading = new Pawn_ReadingTracker(pawn);
			}
			if (pawn.workSettings != null && pawn.workSettings.Initialized && ModsConfig.AnomalyActive && !pawn.WorkTypeIsDisabled(WorkTypeDefOf.DarkStudy) && pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDefOf.DarkStudy) >= 6f)
			{
				pawn.workSettings.SetPriority(WorkTypeDefOf.DarkStudy, 3);
			}
		}
		if (obj is Map map)
		{
			map.enrouteManager = new EnrouteManager(map);
			if (map.generatorDef == null)
			{
				map.generatorDef = MapGeneratorDefOf.Base_Player;
			}
		}
		if (obj is PlaySettings playSettings)
		{
			playSettings.defaultCareForColonist = MedicalCareCategory.Best;
			playSettings.defaultCareForPrisoner = MedicalCareCategory.HerbalOrWorse;
			playSettings.defaultCareForSlave = MedicalCareCategory.HerbalOrWorse;
			playSettings.defaultCareForTamedAnimal = MedicalCareCategory.HerbalOrWorse;
			playSettings.defaultCareForFriendlyFaction = MedicalCareCategory.HerbalOrWorse;
			playSettings.defaultCareForNeutralFaction = MedicalCareCategory.HerbalOrWorse;
			playSettings.defaultCareForHostileFaction = MedicalCareCategory.HerbalOrWorse;
			playSettings.defaultCareForNoFaction = MedicalCareCategory.HerbalOrWorse;
			playSettings.defaultCareForWildlife = MedicalCareCategory.HerbalOrWorse;
			playSettings.defaultCareForEntities = MedicalCareCategory.NoMeds;
			playSettings.defaultCareForGhouls = MedicalCareCategory.NoMeds;
		}
	}
}
