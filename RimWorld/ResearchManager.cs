using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public sealed class ResearchManager : IExposable
{
	public class KnowledgeCategoryProject : IExposable
	{
		public KnowledgeCategoryDef category;

		public ResearchProjectDef project;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref category, "category");
			Scribe_Defs.Look(ref project, "project");
		}
	}

	private ResearchProjectDef currentProj;

	private List<KnowledgeCategoryProject> currentAnomalyKnowledgeProjects;

	private Dictionary<ResearchProjectDef, float> progress = new Dictionary<ResearchProjectDef, float>();

	private Dictionary<ResearchProjectDef, int> techprints = new Dictionary<ResearchProjectDef, int>();

	private Dictionary<ResearchProjectDef, float> anomalyKnowledge = new Dictionary<ResearchProjectDef, float>();

	private DefMap<ResearchTabDef, bool> tabInfoVisibility;

	public bool gravEngineInspected;

	public const float ResearchPointsPerWorkTick = 0.00825f;

	public const int IntellectualExpPerTechprint = 2000;

	public const string ResearchCompletedSignal = "ResearchCompleted";

	public bool AnyProjectIsAvailable => DefDatabase<ResearchProjectDef>.AllDefsListForReading.Any((ResearchProjectDef x) => x.baseCost > 0f && x.CanStartNow);

	public List<KnowledgeCategoryProject> CurrentAnomalyKnowledgeProjects
	{
		get
		{
			EnsureKnowledgeProjectsInitialized();
			return currentAnomalyKnowledgeProjects;
		}
	}

	public bool TabInfoVisible(ResearchTabDef tab)
	{
		if (tabInfoVisibility == null)
		{
			tabInfoVisibility = new DefMap<ResearchTabDef, bool>();
			foreach (ResearchTabDef allDef in DefDatabase<ResearchTabDef>.AllDefs)
			{
				tabInfoVisibility[allDef] = allDef.visibleByDefault;
			}
		}
		return tabInfoVisibility[tab];
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref currentProj, "currentProj");
		Scribe_Collections.Look(ref progress, "progress", LookMode.Def, LookMode.Value);
		Scribe_Collections.Look(ref techprints, "techprints", LookMode.Def, LookMode.Value);
		Scribe_Collections.Look(ref anomalyKnowledge, "knowledge", LookMode.Def, LookMode.Value);
		Scribe_Collections.Look(ref currentAnomalyKnowledgeProjects, "currentKnowledgeProjects", LookMode.Deep);
		Scribe_Deep.Look(ref tabInfoVisibility, "tabInfoVisibility");
		Scribe_Values.Look(ref gravEngineInspected, "gravEngineInspected", defaultValue: false);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			BackCompatibility.ResearchManagerPostLoadInit();
			if (tabInfoVisibility == null)
			{
				tabInfoVisibility = new DefMap<ResearchTabDef, bool>();
			}
			foreach (ResearchTabDef allDef in DefDatabase<ResearchTabDef>.AllDefs)
			{
				if (allDef.visibleByDefault)
				{
					tabInfoVisibility[allDef] = true;
				}
			}
		}
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			return;
		}
		if (techprints == null)
		{
			techprints = new Dictionary<ResearchProjectDef, int>();
		}
		if (ModsConfig.AnomalyActive)
		{
			if (anomalyKnowledge == null)
			{
				anomalyKnowledge = new Dictionary<ResearchProjectDef, float>();
			}
			EnsureKnowledgeProjectsInitialized();
		}
	}

	public void SetCurrentProject(ResearchProjectDef proj)
	{
		if (proj.baseCost > 0f)
		{
			currentProj = proj;
		}
		if (!ModsConfig.AnomalyActive || proj.knowledgeCategory == null)
		{
			return;
		}
		foreach (KnowledgeCategoryProject currentAnomalyKnowledgeProject in CurrentAnomalyKnowledgeProjects)
		{
			if (currentAnomalyKnowledgeProject.category == proj.knowledgeCategory)
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.AnomalyResearch, KnowledgeAmount.Total);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.StudyingEntities, KnowledgeAmount.Total);
				currentAnomalyKnowledgeProject.project = proj;
				break;
			}
		}
	}

	public void StopProject(ResearchProjectDef proj)
	{
		if (currentProj == proj)
		{
			currentProj = null;
		}
		else
		{
			if (!ModsConfig.AnomalyActive || proj.knowledgeCategory == null)
			{
				return;
			}
			foreach (KnowledgeCategoryProject currentAnomalyKnowledgeProject in CurrentAnomalyKnowledgeProjects)
			{
				if (currentAnomalyKnowledgeProject.category == proj.knowledgeCategory)
				{
					currentAnomalyKnowledgeProject.project = null;
					break;
				}
			}
		}
	}

	public ResearchProjectDef GetProject(KnowledgeCategoryDef category = null)
	{
		if (category == null)
		{
			return currentProj;
		}
		foreach (KnowledgeCategoryProject currentAnomalyKnowledgeProject in CurrentAnomalyKnowledgeProjects)
		{
			if (currentAnomalyKnowledgeProject.category == category)
			{
				return currentAnomalyKnowledgeProject.project;
			}
		}
		return null;
	}

	public bool IsCurrentProject(ResearchProjectDef proj)
	{
		if (proj == currentProj)
		{
			return true;
		}
		if (ModsConfig.AnomalyActive)
		{
			foreach (KnowledgeCategoryProject currentAnomalyKnowledgeProject in CurrentAnomalyKnowledgeProjects)
			{
				if (currentAnomalyKnowledgeProject.project == proj)
				{
					return true;
				}
			}
		}
		return false;
	}

	public float GetProgress(ResearchProjectDef proj)
	{
		if (proj.baseCost > 0f)
		{
			if (progress.TryGetValue(proj, out var value))
			{
				return value;
			}
			progress.Add(proj, 0f);
			return 0f;
		}
		if (ModsConfig.AnomalyActive && proj.knowledgeCost > 0f)
		{
			if (anomalyKnowledge.TryGetValue(proj, out var value2))
			{
				return value2;
			}
			anomalyKnowledge.Add(proj, 0f);
			return 0f;
		}
		return 0f;
	}

	public void AddProgress(ResearchProjectDef proj, float amount, Pawn source = null)
	{
		if (progress.TryGetValue(proj, out var value))
		{
			progress[proj] = value + amount;
		}
		else
		{
			progress.Add(proj, amount);
		}
		progress[proj] = Mathf.Min(progress[proj], proj.Cost);
		if (proj.PrerequisitesCompleted && proj.IsFinished)
		{
			FinishProject(proj, doCompletionDialog: true, source);
		}
	}

	public int GetTechprints(ResearchProjectDef proj)
	{
		if (!techprints.TryGetValue(proj, out var value))
		{
			return 0;
		}
		return value;
	}

	public void ApplyTechprint(ResearchProjectDef proj, Pawn applyingPawn)
	{
		if (!ModLister.CheckRoyalty("Techprint"))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("LetterTechprintAppliedPartIntro".Translate(NamedArgumentUtility.Named(proj, "PROJECT")));
		stringBuilder.AppendLine();
		if (proj.TechprintCount > GetTechprints(proj))
		{
			AddTechprints(proj, 1);
			if (proj.TechprintCount == GetTechprints(proj))
			{
				stringBuilder.AppendLine("LetterTechprintAppliedPartJustUnlocked".Translate(NamedArgumentUtility.Named(proj, "PROJECT")));
				stringBuilder.AppendLine();
			}
			else
			{
				stringBuilder.AppendLine("LetterTechprintAppliedPartNotUnlockedYet".Translate(GetTechprints(proj), proj.TechprintCount.ToString(), NamedArgumentUtility.Named(proj, "PROJECT")));
				stringBuilder.AppendLine();
			}
		}
		else if (proj.IsFinished)
		{
			stringBuilder.AppendLine("LetterTechprintAppliedPartAlreadyResearched".Translate(NamedArgumentUtility.Named(proj, "PROJECT")));
			stringBuilder.AppendLine();
		}
		else if (!proj.IsFinished && proj.baseCost > 0f)
		{
			float num = (proj.baseCost - GetProgress(proj)) * 0.5f;
			stringBuilder.AppendLine("LetterTechprintAppliedPartAlreadyUnlocked".Translate(num, NamedArgumentUtility.Named(proj, "PROJECT")));
			stringBuilder.AppendLine();
			if (!progress.TryGetValue(proj, out var value))
			{
				progress.Add(proj, Mathf.Min(num, proj.baseCost));
			}
			else
			{
				progress[proj] = Mathf.Min(value + num, proj.baseCost);
			}
		}
		if (applyingPawn != null)
		{
			stringBuilder.AppendLine("LetterTechprintAppliedPartExpAwarded".Translate(2000.ToString(), SkillDefOf.Intellectual.label, applyingPawn.Named("PAWN")));
			applyingPawn.skills.Learn(SkillDefOf.Intellectual, 2000f, direct: true, ignoreLearnRate: true);
		}
		if (stringBuilder.Length > 0)
		{
			Find.LetterStack.ReceiveLetter("LetterTechprintAppliedLabel".Translate(NamedArgumentUtility.Named(proj, "PROJECT")), stringBuilder.ToString().TrimEndNewlines(), LetterDefOf.PositiveEvent);
		}
	}

	public void AddTechprints(ResearchProjectDef proj, int amount)
	{
		if (techprints.TryGetValue(proj, out var value))
		{
			value += amount;
			if (value > proj.TechprintCount)
			{
				value = proj.TechprintCount;
			}
			techprints[proj] = value;
		}
		else
		{
			techprints.Add(proj, amount);
		}
	}

	public void ResearchPerformed(float amount, Pawn researcher)
	{
		if (currentProj == null)
		{
			Log.Error("Researched without having an active project.");
			return;
		}
		amount *= 0.00825f;
		amount *= Find.Storyteller.difficulty.researchSpeedFactor;
		if (researcher?.Faction != null)
		{
			amount /= currentProj.CostFactor(researcher.Faction.def.techLevel);
		}
		if (DebugSettings.fastResearch)
		{
			amount *= 500f;
		}
		researcher?.records.AddTo(RecordDefOf.ResearchPointsResearched, amount);
		float num = GetProgress(currentProj);
		num += amount;
		progress[currentProj] = num;
		if (currentProj.IsFinished)
		{
			FinishProject(currentProj, doCompletionDialog: true, researcher);
		}
	}

	public float GetKnowledge(ResearchProjectDef proj)
	{
		if (anomalyKnowledge.TryGetValue(proj, out var value))
		{
			return value;
		}
		return 0f;
	}

	public void ApplyKnowledge(KnowledgeCategoryDef category, float amount)
	{
		if (!ModLister.CheckAnomaly("Knowledge") || amount <= 0f)
		{
			return;
		}
		bool flag = false;
		ResearchProjectDef project = GetProject(category);
		if (project != null)
		{
			if (ApplyKnowledge(project, amount, out var remainder) && remainder > 0f)
			{
				amount = remainder;
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag && category.overflowCategory != null)
		{
			ApplyKnowledge(category.overflowCategory, amount);
		}
	}

	public bool ApplyKnowledge(ResearchProjectDef project, float amount, out float remainder)
	{
		if (anomalyKnowledge.TryGetValue(project, out var value))
		{
			anomalyKnowledge[project] = value + amount;
		}
		else
		{
			anomalyKnowledge.Add(project, amount);
		}
		if (project.PrerequisitesCompleted && project.IsFinished)
		{
			remainder = anomalyKnowledge[project] - project.knowledgeCost;
			anomalyKnowledge[project] = Mathf.Min(anomalyKnowledge[project], project.Cost);
			FinishProject(project, doCompletionDialog: true, null, doCompletionLetter: false);
			return true;
		}
		remainder = 0f;
		return false;
	}

	public void ReapplyAllMods()
	{
		foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
		{
			if (allDef.IsFinished)
			{
				allDef.ReapplyAllMods();
			}
		}
	}

	public void FinishProject(ResearchProjectDef proj, bool doCompletionDialog = false, Pawn researcher = null, bool doCompletionLetter = true)
	{
		if (proj.prerequisites != null)
		{
			for (int i = 0; i < proj.prerequisites.Count; i++)
			{
				if (!proj.prerequisites[i].IsFinished)
				{
					FinishProject(proj.prerequisites[i], doCompletionDialog, researcher, doCompletionLetter);
				}
			}
		}
		int num = GetTechprints(proj);
		if (num < proj.TechprintCount)
		{
			AddTechprints(proj, proj.TechprintCount - num);
		}
		if (proj.RequiredAnalyzedThingCount > 0)
		{
			for (int j = 0; j < proj.requiredAnalyzed.Count; j++)
			{
				CompProperties_CompAnalyzableUnlockResearch compProperties = proj.requiredAnalyzed[j].GetCompProperties<CompProperties_CompAnalyzableUnlockResearch>();
				Find.AnalysisManager.ForceCompleteAnalysisProgress(compProperties.analysisID);
			}
		}
		if (proj.baseCost > 0f)
		{
			progress[proj] = proj.baseCost;
		}
		else if (ModsConfig.AnomalyActive && proj.knowledgeCost > 0f)
		{
			anomalyKnowledge.SetOrAdd(proj, proj.knowledgeCost);
			Find.SignalManager.SendSignal(new Signal("ThingStudied", global: true));
		}
		if (researcher != null)
		{
			TaleRecorder.RecordTale(TaleDefOf.FinishedResearchProject, researcher, proj);
		}
		ReapplyAllMods();
		if (proj.recalculatePower)
		{
			try
			{
				foreach (Map map in Find.Maps)
				{
					foreach (Thing item in map.listerThings.ThingsInGroup(ThingRequestGroup.PowerTrader))
					{
						item.TryGetComp<CompPowerTrader>()?.SetUpPowerVars();
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}
		if (doCompletionDialog)
		{
			DiaNode diaNode = new DiaNode((string)("ResearchFinished".Translate(proj.LabelCap) + "\n\n" + proj.description));
			diaNode.options.Add(DiaOption.DefaultOK);
			DiaOption diaOption = new DiaOption("ResearchScreen".Translate());
			diaOption.resolveTree = true;
			diaOption.action = delegate
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
				if (MainButtonDefOf.Research.TabWindow is MainTabWindow_Research mainTabWindow_Research && proj.tab != null)
				{
					mainTabWindow_Research.CurTab = proj.tab;
				}
			};
			diaNode.options.Add(diaOption);
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true));
		}
		if (doCompletionLetter && !proj.discoveredLetterTitle.NullOrEmpty() && Find.Storyteller.difficulty.AllowedBy(proj.discoveredLetterDisabledWhen))
		{
			Find.LetterStack.ReceiveLetter(proj.discoveredLetterTitle, proj.discoveredLetterText, LetterDefOf.NeutralEvent);
		}
		if (proj.teachConcept != null)
		{
			LessonAutoActivator.TeachOpportunity(proj.teachConcept, OpportunityType.Important);
		}
		if (currentProj == proj)
		{
			currentProj = null;
		}
		else if (ModsConfig.AnomalyActive && proj.knowledgeCategory != null)
		{
			foreach (KnowledgeCategoryProject currentAnomalyKnowledgeProject in CurrentAnomalyKnowledgeProjects)
			{
				if (currentAnomalyKnowledgeProject.project == proj)
				{
					currentAnomalyKnowledgeProject.project = null;
					break;
				}
			}
		}
		foreach (Def unlockedDef in proj.UnlockedDefs)
		{
			if (unlockedDef is ThingDef thingDef)
			{
				thingDef.Notify_UnlockedByResearch();
			}
		}
		Find.SignalManager.SendSignal(new Signal("ResearchCompleted", global: true));
	}

	public void ResetAllProgress()
	{
		progress.Clear();
		techprints.Clear();
		anomalyKnowledge.Clear();
		currentProj = null;
		gravEngineInspected = false;
	}

	private void EnsureKnowledgeProjectsInitialized()
	{
		if (!ModsConfig.AnomalyActive)
		{
			return;
		}
		if (currentAnomalyKnowledgeProjects == null)
		{
			currentAnomalyKnowledgeProjects = new List<KnowledgeCategoryProject>();
		}
		currentAnomalyKnowledgeProjects.RemoveAll((KnowledgeCategoryProject x) => x.category == null);
		foreach (KnowledgeCategoryDef k in DefDatabase<KnowledgeCategoryDef>.AllDefs)
		{
			if (!currentAnomalyKnowledgeProjects.Any((KnowledgeCategoryProject x) => x.category == k))
			{
				currentAnomalyKnowledgeProjects.Add(new KnowledgeCategoryProject
				{
					category = k,
					project = null
				});
			}
		}
	}

	public void Notify_MonolithLevelChanged(int newLevel)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return;
		}
		foreach (ResearchTabDef allDef in DefDatabase<ResearchTabDef>.AllDefs)
		{
			if (!TabInfoVisible(allDef) && allDef.minMonolithLevelVisible > 0 && (newLevel >= allDef.minMonolithLevelVisible || (!Find.Anomaly.GenerateMonolith && Find.Storyteller.difficulty.AnomalyPlaystyleDef.enableAnomalyContent)))
			{
				tabInfoVisibility[allDef] = true;
			}
		}
	}

	public void DebugSetAllProjectsFinished()
	{
		progress.Clear();
		anomalyKnowledge.Clear();
		foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
		{
			if (allDef.baseCost > 0f)
			{
				progress.Add(allDef, allDef.baseCost);
			}
			else if (allDef.knowledgeCost > 0f)
			{
				anomalyKnowledge.Add(allDef, allDef.knowledgeCost);
			}
		}
		ReapplyAllMods();
	}

	public bool AnyProjectsAvailableWithKnowledgeCategory(KnowledgeCategoryDef category)
	{
		return DefDatabase<ResearchProjectDef>.AllDefsListForReading.Any((ResearchProjectDef x) => x.CanStartNow && x.knowledgeCategory == category);
	}
}
