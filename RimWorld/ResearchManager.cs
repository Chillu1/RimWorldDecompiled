using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class ResearchManager : IExposable
	{
		public ResearchProjectDef currentProj;

		private Dictionary<ResearchProjectDef, float> progress = new Dictionary<ResearchProjectDef, float>();

		private Dictionary<ResearchProjectDef, int> techprints = new Dictionary<ResearchProjectDef, int>();

		private float ResearchPointsPerWorkTick = 0.00825f;

		public const int IntellectualExpPerTechprint = 2000;

		public bool AnyProjectIsAvailable => DefDatabase<ResearchProjectDef>.AllDefsListForReading.Find((ResearchProjectDef x) => x.CanStartNow) != null;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref currentProj, "currentProj");
			Scribe_Collections.Look(ref progress, "progress", LookMode.Def, LookMode.Value);
			Scribe_Collections.Look(ref techprints, "techprints", LookMode.Def, LookMode.Value);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.ResearchManagerPostLoadInit();
			}
			if (Scribe.mode != LoadSaveMode.Saving && techprints == null)
			{
				techprints = new Dictionary<ResearchProjectDef, int>();
			}
		}

		public float GetProgress(ResearchProjectDef proj)
		{
			if (progress.TryGetValue(proj, out float value))
			{
				return value;
			}
			progress.Add(proj, 0f);
			return 0f;
		}

		public int GetTechprints(ResearchProjectDef proj)
		{
			if (!techprints.TryGetValue(proj, out int value))
			{
				return 0;
			}
			return value;
		}

		public void ApplyTechprint(ResearchProjectDef proj, Pawn applyingPawn)
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Techprints are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it.", 657212);
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("LetterTechprintAppliedPartIntro".Translate(NamedArgumentUtility.Named(proj, "PROJECT")));
			stringBuilder.AppendLine();
			if (proj.techprintCount > GetTechprints(proj))
			{
				AddTechprints(proj, 1);
				if (proj.techprintCount == GetTechprints(proj))
				{
					stringBuilder.AppendLine("LetterTechprintAppliedPartJustUnlocked".Translate(NamedArgumentUtility.Named(proj, "PROJECT")));
					stringBuilder.AppendLine();
				}
				else
				{
					stringBuilder.AppendLine("LetterTechprintAppliedPartNotUnlockedYet".Translate(GetTechprints(proj), proj.techprintCount.ToString(), NamedArgumentUtility.Named(proj, "PROJECT")));
					stringBuilder.AppendLine();
				}
			}
			else if (proj.IsFinished)
			{
				stringBuilder.AppendLine("LetterTechprintAppliedPartAlreadyResearched".Translate(NamedArgumentUtility.Named(proj, "PROJECT")));
				stringBuilder.AppendLine();
			}
			else if (!proj.IsFinished)
			{
				float num = (proj.baseCost - GetProgress(proj)) * 0.5f;
				stringBuilder.AppendLine("LetterTechprintAppliedPartAlreadyUnlocked".Translate(num, NamedArgumentUtility.Named(proj, "PROJECT")));
				stringBuilder.AppendLine();
				if (!progress.TryGetValue(proj, out float value))
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
				applyingPawn.skills.Learn(SkillDefOf.Intellectual, 2000f);
			}
			if (stringBuilder.Length > 0)
			{
				Find.LetterStack.ReceiveLetter("LetterTechprintAppliedLabel".Translate(NamedArgumentUtility.Named(proj, "PROJECT")), stringBuilder.ToString().TrimEndNewlines(), LetterDefOf.PositiveEvent);
			}
		}

		public void AddTechprints(ResearchProjectDef proj, int amount)
		{
			if (techprints.TryGetValue(proj, out int value))
			{
				value += amount;
				if (value > proj.techprintCount)
				{
					value = proj.techprintCount;
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
			amount *= ResearchPointsPerWorkTick;
			amount *= Find.Storyteller.difficulty.researchSpeedFactor;
			if (researcher != null && researcher.Faction != null)
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

		public void FinishProject(ResearchProjectDef proj, bool doCompletionDialog = false, Pawn researcher = null)
		{
			if (proj.prerequisites != null)
			{
				for (int i = 0; i < proj.prerequisites.Count; i++)
				{
					if (!proj.prerequisites[i].IsFinished)
					{
						FinishProject(proj.prerequisites[i]);
					}
				}
			}
			int num = GetTechprints(proj);
			if (num < proj.techprintCount)
			{
				AddTechprints(proj, proj.techprintCount - num);
			}
			progress[proj] = proj.baseCost;
			if (researcher != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.FinishedResearchProject, researcher, currentProj);
			}
			ReapplyAllMods();
			if (doCompletionDialog)
			{
				DiaNode diaNode = new DiaNode((string)("ResearchFinished".Translate(currentProj.LabelCap) + "\n\n" + currentProj.description));
				diaNode.options.Add(DiaOption.DefaultOK);
				DiaOption diaOption = new DiaOption("ResearchScreen".Translate());
				diaOption.resolveTree = true;
				diaOption.action = delegate
				{
					Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
				};
				diaNode.options.Add(diaOption);
				Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true));
			}
			if (!proj.discoveredLetterTitle.NullOrEmpty() && Find.Storyteller.difficulty.difficulty >= proj.discoveredLetterMinDifficulty)
			{
				Find.LetterStack.ReceiveLetter(proj.discoveredLetterTitle, proj.discoveredLetterText, LetterDefOf.NeutralEvent);
			}
			if (proj.unlockExtremeDifficulty && Find.Storyteller.difficulty.difficulty >= DifficultyDefOf.Rough.difficulty)
			{
				Prefs.ExtremeDifficultyUnlocked = true;
				Prefs.Save();
			}
			if (currentProj == proj)
			{
				currentProj = null;
			}
		}

		public void DebugSetAllProjectsFinished()
		{
			progress.Clear();
			foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				progress.Add(allDef, allDef.baseCost);
			}
			ReapplyAllMods();
		}
	}
}
