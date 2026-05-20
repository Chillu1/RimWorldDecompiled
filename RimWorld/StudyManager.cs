using System.Collections.Generic;
using Verse;

namespace RimWorld;

public sealed class StudyManager : IExposable
{
	public const float DefaultStudyPerTick = 0.08f;

	public const float DefaultStudyPerInteraction = 0.87f;

	public const int DefaultStudyInteractions = 5;

	public const string ThingStudiedSignal = "ThingStudied";

	private Dictionary<Map, HashSet<Thing>> studiableThingsCache = new Dictionary<Map, HashSet<Thing>>();

	public Dictionary<ThingDef, float> backCompatStudyProgress = new Dictionary<ThingDef, float>();

	public void UpdateStudiableCache(Thing thing, Map map)
	{
		if (map == null)
		{
			return;
		}
		if (!studiableThingsCache.ContainsKey(map))
		{
			studiableThingsCache.Add(map, new HashSet<Thing>());
		}
		if (thing.def.IsStudiable)
		{
			CompStudiable compStudiable = thing.TryGetComp<CompStudiable>();
			if (compStudiable != null && compStudiable.EverStudiableCached())
			{
				goto IL_005d;
			}
		}
		if (!ModsConfig.AnomalyActive || !(thing is Building_HoldingPlatform { HeldPawn: not null }))
		{
			if (studiableThingsCache[map].Contains(thing))
			{
				studiableThingsCache[map].Remove(thing);
				if (studiableThingsCache[map].NullOrEmpty())
				{
					studiableThingsCache.Remove(map);
				}
			}
			return;
		}
		goto IL_005d;
		IL_005d:
		if (!studiableThingsCache[map].Contains(thing))
		{
			studiableThingsCache[map].Add(thing);
		}
	}

	public HashSet<Thing> GetStudiableThingsAndPlatforms(Map map)
	{
		if (!studiableThingsCache.ContainsKey(map))
		{
			return new HashSet<Thing>();
		}
		return studiableThingsCache[map];
	}

	public void Study(Thing studiedThing, Pawn studier, float studyAmount)
	{
		if (!studiedThing.def.IsStudiable)
		{
			Log.Error("Tried to study " + studiedThing.def.label + " which is not studiable.");
			return;
		}
		CompStudiable compStudiable = studiedThing.TryGetComp<CompStudiable>();
		compStudiable.studyPoints += studyAmount;
		if (compStudiable.Props.studyAmountToComplete > 0f && compStudiable.studyPoints >= compStudiable.Props.studyAmountToComplete)
		{
			compStudiable.studyPoints = compStudiable.Props.studyAmountToComplete;
		}
		studiedThing.Notify_Studied(studier, studyAmount);
		Find.SignalManager.SendSignal(new Signal("ThingStudied", global: true));
	}

	public void StudyAnomaly(Thing studiedThing, Pawn studier, float knowledgeAmount, KnowledgeCategoryDef knowledgeCategory)
	{
		if (ModsConfig.AnomalyActive && !(knowledgeAmount <= 0f))
		{
			Thing thing = studiedThing;
			if (thing.Map == null)
			{
				thing = thing.ParentHolder as Thing;
			}
			if (thing != null)
			{
				MoteMaker.ThrowText(thing.DrawPos, thing.Map, $"{knowledgeCategory.LabelCap} +{knowledgeAmount:0.00}", 3f);
			}
			Find.ResearchManager.ApplyKnowledge(knowledgeCategory, knowledgeAmount);
			studiedThing.Notify_Studied(studier, knowledgeAmount, knowledgeCategory);
			Find.SignalManager.SendSignal(new Signal("ThingStudied", global: true));
		}
	}

	public float GetKnowledgeAmount(KnowledgeCategoryDef knowledgeCategory)
	{
		float num = 0f;
		foreach (ResearchProjectDef item in DefDatabase<ResearchProjectDef>.AllDefsListForReading)
		{
			if (item.knowledgeCategory == knowledgeCategory)
			{
				num += Find.ResearchManager.GetKnowledge(item);
			}
		}
		return num;
	}

	public void ExposeData()
	{
		if (Scribe.mode != LoadSaveMode.LoadingVars)
		{
			return;
		}
		Dictionary<ThingDef, float> dict = new Dictionary<ThingDef, float>();
		Scribe_Collections.Look(ref dict, "studyProgress", LookMode.Def, LookMode.Value);
		if (dict == null)
		{
			return;
		}
		foreach (ThingDef key in dict.Keys)
		{
			if (key.GetCompProperties<CompProperties_Studiable>() != null)
			{
				backCompatStudyProgress[key] = dict[key];
			}
			CompProperties_CompAnalyzableUnlockResearch compProperties = key.GetCompProperties<CompProperties_CompAnalyzableUnlockResearch>();
			if (compProperties != null && dict[key] >= 1f)
			{
				Find.AnalysisManager?.ForceCompleteAnalysisProgress(compProperties.analysisID);
			}
		}
	}
}
