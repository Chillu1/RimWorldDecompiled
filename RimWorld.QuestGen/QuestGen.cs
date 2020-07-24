using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld.QuestGen
{
	public static class QuestGen
	{
		public static Quest quest;

		public static Slate slate = new Slate();

		private static QuestScriptDef root;

		private static bool working;

		private static List<QuestTextRequest> textRequests = new List<QuestTextRequest>();

		private static List<Pawn> generatedPawns = new List<Pawn>();

		private static Dictionary<string, int> generatedSignals = new Dictionary<string, int>();

		private static Dictionary<string, int> generatedTargetQuestTags = new Dictionary<string, int>();

		private static List<Rule> questDescriptionRules = new List<Rule>();

		private static Dictionary<string, string> questDescriptionConstants = new Dictionary<string, string>();

		private static List<Rule> questNameRules = new List<Rule>();

		private static Dictionary<string, string> questNameConstants = new Dictionary<string, string>();

		private static List<string> slateQuestTagsToAddWhenFinished = new List<string>();

		public static QuestScriptDef Root => root;

		public static bool Working => working;

		public static List<Rule> QuestDescriptionRulesReadOnly => questDescriptionRules;

		public static Dictionary<string, string> QuestDescriptionConstantsReadOnly => questDescriptionConstants;

		public static List<Rule> QuestNameRulesReadOnly => questNameRules;

		public static Dictionary<string, string> QuestNameConstantsReadOnly => questNameConstants;

		public static List<QuestTextRequest> TextRequestsReadOnly => textRequests;

		public static string GenerateNewSignal(string signalString, bool ensureUnique = true)
		{
			if (!ensureUnique || !generatedSignals.TryGetValue(signalString, out int value))
			{
				value = 0;
			}
			string result = "Quest" + quest.id + "." + signalString + ((value == 0) ? "" : (value + 1).ToString());
			generatedSignals[signalString] = value + 1;
			return result;
		}

		public static string GenerateNewTargetQuestTag(string targetString, bool ensureUnique = true)
		{
			if (!ensureUnique || !generatedTargetQuestTags.TryGetValue(targetString, out int value))
			{
				value = 0;
			}
			string result = "Quest" + quest.id + "." + targetString + ((value == 0) ? "" : (value + 1).ToString());
			generatedTargetQuestTags[targetString] = value + 1;
			return result;
		}

		private static void ResetIdCounters()
		{
			generatedSignals.Clear();
			generatedTargetQuestTags.Clear();
		}

		public static Quest Generate(QuestScriptDef root, Slate initialVars)
		{
			if (DeepProfiler.enabled)
			{
				DeepProfiler.Start("Generate quest");
			}
			Quest result = null;
			try
			{
				if (working)
				{
					throw new Exception("Called Generate() while already working.");
				}
				working = true;
				QuestGen.root = root;
				slate.Reset();
				slate.SetAll(initialVars);
				quest = Quest.MakeRaw();
				quest.ticksUntilAcceptanceExpiry = (int)(root.expireDaysRange.RandomInRange * 60000f);
				if (root.defaultChallengeRating > 0)
				{
					quest.challengeRating = root.defaultChallengeRating;
				}
				quest.root = root;
				slate.SetIfNone("inSignal", quest.InitiateSignal);
				root.Run();
				try
				{
					QuestNode_ResolveQuestName.Resolve();
				}
				catch (Exception arg)
				{
					Log.Error("Error while generating quest name: " + arg);
				}
				try
				{
					QuestNode_ResolveQuestDescription.Resolve();
				}
				catch (Exception arg2)
				{
					Log.Error("Error while generating quest description: " + arg2);
				}
				try
				{
					QuestNode_ResolveTextRequests.Resolve();
				}
				catch (Exception arg3)
				{
					Log.Error("Error while resolving text requests: " + arg3);
				}
				AddSlateQuestTags();
				bool flag = root.autoAccept;
				if (flag)
				{
					List<QuestPart> partsListForReading = quest.PartsListForReading;
					for (int i = 0; i < partsListForReading.Count; i++)
					{
						if (partsListForReading[i].PreventsAutoAccept)
						{
							flag = false;
							break;
						}
					}
				}
				if (flag)
				{
					quest.SetInitiallyAccepted();
				}
				result = quest;
				return result;
			}
			catch (Exception arg4)
			{
				Log.Error("Error in QuestGen: " + arg4);
				return result;
			}
			finally
			{
				if (DeepProfiler.enabled)
				{
					DeepProfiler.End();
				}
				quest = null;
				QuestGen.root = null;
				working = false;
				generatedPawns.Clear();
				textRequests.Clear();
				slate.Reset();
				questDescriptionRules.Clear();
				questDescriptionConstants.Clear();
				questNameRules.Clear();
				questNameConstants.Clear();
				slateQuestTagsToAddWhenFinished.Clear();
				ResetIdCounters();
			}
		}

		public static void AddToGeneratedPawns(Pawn pawn)
		{
			if (!working)
			{
				Log.Error("Tried to add a pawn to generated pawns while not resolving any quest.");
			}
			else if (!generatedPawns.Contains(pawn))
			{
				generatedPawns.Add(pawn);
			}
		}

		public static bool WasGeneratedForQuestBeingGenerated(Pawn pawn)
		{
			if (!working)
			{
				return false;
			}
			return generatedPawns.Contains(pawn);
		}

		public static void AddQuestDescriptionRules(RulePack rulePack)
		{
			AddQuestDescriptionRules(rulePack.Rules);
		}

		public static void AddQuestDescriptionRules(List<Rule> rules)
		{
			if (!working)
			{
				Log.Error("Tried to add quest description rules while not resolving any quest.");
			}
			else
			{
				questDescriptionRules.AddRange(QuestGenUtility.AppendCurrentPrefix(rules));
			}
		}

		public static void AddQuestDescriptionConstants(Dictionary<string, string> constants)
		{
			if (!working)
			{
				Log.Error("Tried to add quest description constants while not resolving any quest.");
				return;
			}
			foreach (KeyValuePair<string, string> item in QuestGenUtility.AppendCurrentPrefix(constants))
			{
				if (!questDescriptionConstants.ContainsKey(item.Key))
				{
					questDescriptionConstants.Add(item.Key, item.Value);
				}
			}
		}

		public static void AddQuestNameRules(RulePack rulePack)
		{
			AddQuestNameRules(rulePack.Rules);
		}

		public static void AddQuestNameRules(List<Rule> rules)
		{
			if (!working)
			{
				Log.Error("Tried to add quest name rules while not resolving any quest.");
			}
			else
			{
				questNameRules.AddRange(QuestGenUtility.AppendCurrentPrefix(rules));
			}
		}

		public static void AddQuestNameConstants(Dictionary<string, string> constants)
		{
			if (!working)
			{
				Log.Error("Tried to add quest name constants while not resolving any quest.");
				return;
			}
			foreach (KeyValuePair<string, string> item in QuestGenUtility.AppendCurrentPrefix(constants))
			{
				if (!questNameConstants.ContainsKey(item.Key))
				{
					questNameConstants.Add(item.Key, item.Value);
				}
			}
		}

		public static void AddSlateQuestTagToAddWhenFinished(string slateVarNameWithPrefix)
		{
			if (!slateQuestTagsToAddWhenFinished.Contains(slateVarNameWithPrefix))
			{
				slateQuestTagsToAddWhenFinished.Add(slateVarNameWithPrefix);
			}
		}

		public static void AddTextRequest(string localKeyword, Action<string> setter, RulePack extraLocalRules = null)
		{
			AddTextRequest(localKeyword, setter, extraLocalRules?.Rules);
		}

		public static void AddTextRequest(string localKeyword, Action<string> setter, List<Rule> extraLocalRules)
		{
			if (!working)
			{
				Log.Error("Tried to add a text request while not resolving any quest.");
				return;
			}
			QuestTextRequest questTextRequest = new QuestTextRequest();
			questTextRequest.keyword = localKeyword;
			if (!slate.CurrentPrefix.NullOrEmpty())
			{
				questTextRequest.keyword = slate.CurrentPrefix + "/" + questTextRequest.keyword;
			}
			questTextRequest.keyword = QuestGenUtility.NormalizeVarPath(questTextRequest.keyword);
			questTextRequest.setter = setter;
			if (extraLocalRules != null)
			{
				questTextRequest.extraRules = QuestGenUtility.AppendCurrentPrefix(extraLocalRules);
			}
			textRequests.Add(questTextRequest);
		}

		private static void AddSlateQuestTags()
		{
			for (int i = 0; i < slateQuestTagsToAddWhenFinished.Count; i++)
			{
				if (slate.TryGet(slateQuestTagsToAddWhenFinished[i], out object var, isAbsoluteName: true))
				{
					string questTagToAdd = GenerateNewTargetQuestTag(slateQuestTagsToAddWhenFinished[i], ensureUnique: false);
					QuestUtility.AddQuestTag(var, questTagToAdd);
				}
			}
			slateQuestTagsToAddWhenFinished.Clear();
		}
	}
}
