using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class LessonAutoActivator
{
	private static Dictionary<ConceptDef, float> opportunities = new Dictionary<ConceptDef, float>();

	private static float timeSinceLastLesson = 10000f;

	private static List<ConceptDef> alertingConcepts = new List<ConceptDef>();

	private const float MapStartGracePeriod = 8f;

	private const float KnowledgeDecayRate = 0.00015f;

	private const float OpportunityDecayRate = 0.4f;

	private const float OpportunityMaxDesireAdd = 60f;

	private const int CheckInterval = 15;

	private const float MaxLessonInterval = 900f;

	private static float SecondsSinceLesson => timeSinceLastLesson;

	private static float RelaxDesire => 100f - SecondsSinceLesson * (1f / 9f);

	public static void Reset()
	{
		alertingConcepts.Clear();
	}

	public static void TeachOpportunity(ConceptDef conc, OpportunityType opp)
	{
		TeachOpportunity(conc, null, opp);
	}

	public static void TeachOpportunity(ConceptDef conc, Thing subject, OpportunityType opp)
	{
		if (TutorSystem.AdaptiveTrainingEnabled && !PlayerKnowledgeDatabase.IsComplete(conc))
		{
			float value = 999f;
			switch (opp)
			{
			case OpportunityType.GoodToKnow:
				value = 60f;
				break;
			case OpportunityType.Important:
				value = 80f;
				break;
			case OpportunityType.Critical:
				value = 100f;
				break;
			default:
				Log.Error("Unknown need");
				break;
			}
			opportunities[conc] = value;
			if ((int)opp >= 1 || Find.Tutor.learningReadout.ActiveConceptsCount < 4)
			{
				TryInitiateLesson(conc);
			}
		}
	}

	public static void Notify_KnowledgeDemonstrated(ConceptDef conc)
	{
		if (PlayerKnowledgeDatabase.IsComplete(conc))
		{
			opportunities[conc] = 0f;
		}
	}

	public static void LessonAutoActivatorUpdate()
	{
		if (!TutorSystem.AdaptiveTrainingEnabled || Current.Game == null || Find.Tutor.learningReadout.ShowAllMode)
		{
			return;
		}
		timeSinceLastLesson += RealTime.realDeltaTime;
		if (Current.ProgramState == ProgramState.Playing && (Time.timeSinceLevelLoad < 8f || Find.WindowStack.SecondsSinceClosedGameStartDialog < 8f || Find.TickManager.NotPlaying))
		{
			return;
		}
		for (int num = alertingConcepts.Count - 1; num >= 0; num--)
		{
			if (PlayerKnowledgeDatabase.IsComplete(alertingConcepts[num]))
			{
				alertingConcepts.RemoveAt(num);
			}
		}
		if (Time.frameCount % 15 != 0 || Find.ActiveLesson.Current != null)
		{
			return;
		}
		for (int i = 0; i < DefDatabase<ConceptDef>.AllDefsListForReading.Count; i++)
		{
			ConceptDef conceptDef = DefDatabase<ConceptDef>.AllDefsListForReading[i];
			if (PlayerKnowledgeDatabase.IsComplete(conceptDef))
			{
				continue;
			}
			float knowledge = PlayerKnowledgeDatabase.GetKnowledge(conceptDef);
			knowledge -= 0.00015f * Time.deltaTime * 15f;
			if (knowledge < 0f)
			{
				knowledge = 0f;
			}
			PlayerKnowledgeDatabase.SetKnowledge(conceptDef, knowledge);
			if (conceptDef.opportunityDecays)
			{
				float opportunity = GetOpportunity(conceptDef);
				opportunity -= 0.4f * Time.deltaTime * 15f;
				if (opportunity < 0f)
				{
					opportunity = 0f;
				}
				opportunities[conceptDef] = opportunity;
			}
		}
		if (Find.Tutor.learningReadout.ActiveConceptsCount < 3)
		{
			ConceptDef conceptDef2 = MostDesiredConcept();
			if (conceptDef2 != null)
			{
				float desire = GetDesire(conceptDef2);
				if (desire > 0.1f && RelaxDesire < desire)
				{
					TryInitiateLesson(conceptDef2);
				}
			}
		}
		else
		{
			SetLastLessonTimeToNow();
		}
	}

	private static ConceptDef MostDesiredConcept()
	{
		float num = -9999f;
		ConceptDef result = null;
		List<ConceptDef> allDefsListForReading = DefDatabase<ConceptDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			ConceptDef conceptDef = allDefsListForReading[i];
			float desire = GetDesire(conceptDef);
			if (desire > num && (!conceptDef.needsOpportunity || !(GetOpportunity(conceptDef) < 0.1f)) && !(PlayerKnowledgeDatabase.GetKnowledge(conceptDef) > 0.15f))
			{
				num = desire;
				result = conceptDef;
			}
		}
		return result;
	}

	private static float GetDesire(ConceptDef conc)
	{
		if (PlayerKnowledgeDatabase.IsComplete(conc))
		{
			return 0f;
		}
		if (Find.Tutor.learningReadout.IsActive(conc))
		{
			return 0f;
		}
		if (Current.ProgramState != conc.gameMode)
		{
			return 0f;
		}
		if (conc.needsOpportunity && GetOpportunity(conc) < 0.1f)
		{
			return 0f;
		}
		return (0f + conc.priority + GetOpportunity(conc) / 100f * 60f) * (1f - PlayerKnowledgeDatabase.GetKnowledge(conc));
	}

	private static float GetOpportunity(ConceptDef conc)
	{
		if (opportunities.TryGetValue(conc, out var value))
		{
			return value;
		}
		opportunities[conc] = 0f;
		return 0f;
	}

	private static void TryInitiateLesson(ConceptDef conc)
	{
		if (Find.Tutor.learningReadout.TryActivateConcept(conc))
		{
			SetLastLessonTimeToNow();
		}
	}

	private static void SetLastLessonTimeToNow()
	{
		timeSinceLastLesson = 0f;
	}

	public static void Notify_TutorialEnding()
	{
		SetLastLessonTimeToNow();
	}

	public static string DebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("RelaxDesire: " + RelaxDesire);
		foreach (ConceptDef item in DefDatabase<ConceptDef>.AllDefs.OrderByDescending((ConceptDef co) => GetDesire(co)))
		{
			if (PlayerKnowledgeDatabase.IsComplete(item))
			{
				stringBuilder.AppendLine(item.defName + " complete");
				continue;
			}
			stringBuilder.AppendLine(item.defName + "\n   know " + PlayerKnowledgeDatabase.GetKnowledge(item).ToString("F3") + "\n   need " + opportunities[item].ToString("F3") + "\n   des " + GetDesire(item).ToString("F3"));
		}
		return stringBuilder.ToString();
	}

	public static void DebugForceInitiateBestLessonNow()
	{
		TryInitiateLesson(DefDatabase<ConceptDef>.AllDefs.OrderByDescending((ConceptDef def) => GetDesire(def)).First());
	}
}
