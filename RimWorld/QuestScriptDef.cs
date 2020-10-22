using System;
using System.Collections.Generic;
using RimWorld.QuestGen;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class QuestScriptDef : Def
	{
		public QuestNode root;

		public float rootSelectionWeight;

		public SimpleCurve rootSelectionWeightFactorFromPointsCurve;

		public float rootMinPoints;

		public float rootMinProgressScore;

		public bool rootIncreasesPopulation;

		public float decreeSelectionWeight;

		public List<string> decreeTags;

		public RulePack questDescriptionRules;

		public RulePack questNameRules;

		public RulePack questDescriptionAndNameRules;

		public RulePack questContentRules;

		public bool autoAccept;

		public FloatRange expireDaysRange = new FloatRange(-1f, -1f);

		public bool nameMustBeUnique;

		public int defaultChallengeRating = -1;

		public bool defaultHidden;

		public bool isRootSpecial;

		public bool canGiveRoyalFavor;

		public LetterDef questAvailableLetterDef;

		public bool hideFactionInfoInWindow;

		public bool affectedByPopulation;

		public bool affectedByPoints = true;

		public bool IsRootRandomSelected => rootSelectionWeight != 0f;

		public bool IsRootDecree => decreeSelectionWeight != 0f;

		public bool IsRootAny
		{
			get
			{
				if (!IsRootRandomSelected && !IsRootDecree)
				{
					return isRootSpecial;
				}
				return true;
			}
		}

		public void Run()
		{
			if (questDescriptionRules != null)
			{
				RimWorld.QuestGen.QuestGen.AddQuestDescriptionRules(questDescriptionRules);
			}
			if (questNameRules != null)
			{
				RimWorld.QuestGen.QuestGen.AddQuestNameRules(questNameRules);
			}
			if (questDescriptionAndNameRules != null)
			{
				RimWorld.QuestGen.QuestGen.AddQuestDescriptionRules(questDescriptionAndNameRules);
				RimWorld.QuestGen.QuestGen.AddQuestNameRules(questDescriptionAndNameRules);
			}
			if (questContentRules != null)
			{
				RimWorld.QuestGen.QuestGen.AddQuestContentRules(questContentRules);
			}
			root.Run();
		}

		public bool CanRun(Slate slate)
		{
			try
			{
				return root.TestRun(slate.DeepCopy());
			}
			catch (Exception arg)
			{
				Log.Error("Error while checking if can generate quest: " + arg);
			}
			return false;
		}

		public bool CanRun(float points)
		{
			Slate slate = new Slate();
			slate.Set("points", points);
			return CanRun(slate);
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			if (rootSelectionWeight > 0f && !autoAccept && expireDaysRange.TrueMax <= 0f)
			{
				yield return "rootSelectionWeight > 0 but expireDaysRange not set";
			}
			if (autoAccept && expireDaysRange.TrueMax > 0f)
			{
				yield return "autoAccept but there is an expireDaysRange set";
			}
			if (defaultChallengeRating > 0 && !IsRootAny)
			{
				yield return "non-root quest has defaultChallengeRating";
			}
		}
	}
}
