using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class IdeoDevelopmentUtility
{
	private static readonly SimpleCurve AnytimeAndFuneralRitualDevelopmentPointsOverOutcomeIndex = new SimpleCurve
	{
		new CurvePoint(0f, 0f),
		new CurvePoint(1f, 0f),
		new CurvePoint(2f, 1f),
		new CurvePoint(3f, 2f)
	};

	private static readonly SimpleCurve[] DateRitualDevelopmentCurvesByCount = new SimpleCurve[5]
	{
		new SimpleCurve
		{
			new CurvePoint(0f, 2f),
			new CurvePoint(1f, 4f),
			new CurvePoint(2f, 6f),
			new CurvePoint(3f, 7f)
		},
		new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 2f),
			new CurvePoint(2f, 3f),
			new CurvePoint(3f, 4f)
		},
		new SimpleCurve
		{
			new CurvePoint(0f, 1f),
			new CurvePoint(1f, 1f),
			new CurvePoint(2f, 2f),
			new CurvePoint(3f, 3f)
		},
		new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1f, 1f),
			new CurvePoint(2f, 1f),
			new CurvePoint(3f, 2f)
		},
		new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(1f, 1f),
			new CurvePoint(2f, 1f),
			new CurvePoint(3f, 1f)
		}
	};

	private static List<Precept> toRemovePrecepts = new List<Precept>();

	public static int DevelopmentPointsForQuestSuccess(Ideo ideo, QuestScriptDef root)
	{
		int num = 0;
		if (root.successHistoryEvent == null || !ideo.Fluid)
		{
			return num;
		}
		List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
		for (int i = 0; i < preceptsListForReading.Count; i++)
		{
			for (int j = 0; j < preceptsListForReading[i].def.comps.Count; j++)
			{
				if (preceptsListForReading[i].def.comps[j] is PreceptComp_DevelopmentPoints preceptComp_DevelopmentPoints && preceptComp_DevelopmentPoints.eventDef == root.successHistoryEvent)
				{
					num += preceptComp_DevelopmentPoints.points;
				}
			}
		}
		return num;
	}

	public static SimpleCurve GetDevelopmentPointsOverOutcomeIndexCurveForRitual(Ideo ideo, Precept_Ritual ritual)
	{
		if (!(ritual.outcomeEffect is RitualOutcomeEffectWorker_FromQuality { GivesDevelopmentPoints: not false }))
		{
			return null;
		}
		if (ritual.isAnytime || ritual.def == PreceptDefOf.Funeral)
		{
			return AnytimeAndFuneralRitualDevelopmentPointsOverOutcomeIndex;
		}
		if (ritual.IsDateTriggered)
		{
			int num = Mathf.Clamp(ideo.PreceptsListForReading.Count((Precept p) => p is Precept_Ritual precept_Ritual && precept_Ritual.IsDateTriggered) - 1, 0, DateRitualDevelopmentCurvesByCount.Length - 1);
			return DateRitualDevelopmentCurvesByCount[num];
		}
		return null;
	}

	public static void GetAllRitualsThatGiveDevelopmentPoints(Ideo ideo, List<Precept_Ritual> rituals)
	{
		if (!ideo.Fluid)
		{
			return;
		}
		List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
		for (int i = 0; i < preceptsListForReading.Count; i++)
		{
			if (preceptsListForReading[i] is Precept_Ritual precept_Ritual && GetDevelopmentPointsOverOutcomeIndexCurveForRitual(ideo, precept_Ritual) != null)
			{
				rituals.Add(precept_Ritual);
			}
		}
	}

	public static void GetAllQuestSuccessEventsThatGiveDevelopmentPoints(Ideo ideo, List<HistoryEventDef> successEvents)
	{
		if (!ideo.Fluid)
		{
			return;
		}
		foreach (QuestScriptDef allDef in DefDatabase<QuestScriptDef>.AllDefs)
		{
			if (!successEvents.Contains(allDef.successHistoryEvent) && DevelopmentPointsForQuestSuccess(ideo, allDef) > 0)
			{
				successEvents.Add(allDef.successHistoryEvent);
			}
		}
	}

	public static void ConfirmChangesToIdeo(Ideo ideo, Ideo newIdeo, Action confirmCallback)
	{
		toRemovePrecepts.Clear();
		GetPreceptsToRemove(ideo, newIdeo, toRemovePrecepts);
		string text = "";
		for (int i = 0; i < toRemovePrecepts.Count; i++)
		{
			if (toRemovePrecepts[i].TryGetLostByReformingWarning(out var warning))
			{
				text += warning;
			}
		}
		if (!text.NullOrEmpty())
		{
			text += "\n\n" + "ReformIdeoContinue".Translate();
			Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, confirmCallback));
		}
		else
		{
			confirmCallback();
		}
		toRemovePrecepts.Clear();
	}

	public static void ApplyChangesToIdeo(Ideo ideo, Ideo newIdeo)
	{
		toRemovePrecepts.Clear();
		GetPreceptsToRemove(ideo, newIdeo, toRemovePrecepts);
		ideo.development.Notify_PreReform(newIdeo);
		newIdeo.CopyTo(ideo);
		UpdateAllStyleReferences(ideo);
		ideo.development.Notify_Reformed();
		for (int i = 0; i < toRemovePrecepts.Count; i++)
		{
			toRemovePrecepts[i].Notify_RemovedByReforming();
		}
		toRemovePrecepts.Clear();
	}

	private static void UpdateAllStyleReferences(Ideo ideo)
	{
		foreach (Map map in Find.Maps)
		{
			foreach (Building item in map.listerBuildings.allBuildingsColonist)
			{
				UpdateThingStyleReference(item, ideo);
			}
		}
	}

	private static void UpdateThingStyleReference(Thing thing, Ideo ideo)
	{
		if (thing.StyleSourcePrecept == null || thing.StyleSourcePrecept.ideo != ideo)
		{
			return;
		}
		List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
		Precept_ThingStyle precept_ThingStyle = (Precept_ThingStyle)preceptsListForReading.FirstOrDefault((Precept p) => p.Id == thing.StyleSourcePrecept.Id);
		if (precept_ThingStyle != null)
		{
			thing.StyleSourcePrecept = precept_ThingStyle;
		}
		else
		{
			precept_ThingStyle = (Precept_ThingStyle)preceptsListForReading.FirstOrDefault((Precept p) => p is Precept_ThingStyle precept_ThingStyle2 && precept_ThingStyle2.ThingDef == thing.def);
			thing.StyleSourcePrecept = precept_ThingStyle;
		}
		if (thing.Spawned)
		{
			thing.DirtyMapMesh(thing.Map);
		}
	}

	private static void GetPreceptsToRemove(Ideo ideo, Ideo newIdeo, List<Precept> preceptsOut)
	{
		List<Precept> preceptsListForReading = ideo.PreceptsListForReading;
		List<Precept> preceptsListForReading2 = newIdeo.PreceptsListForReading;
		for (int i = 0; i < preceptsListForReading.Count; i++)
		{
			bool flag = false;
			for (int j = 0; j < preceptsListForReading2.Count; j++)
			{
				if (preceptsListForReading2[j].Id == preceptsListForReading[i].Id)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				preceptsOut.Add(preceptsListForReading[i]);
			}
		}
	}
}
