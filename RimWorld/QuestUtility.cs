using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class QuestUtility
	{
		public const string QuestTargetSignalPart_MapGenerated = "MapGenerated";

		public const string QuestTargetSignalPart_MapRemoved = "MapRemoved";

		public const string QuestTargetSignalPart_Spawned = "Spawned";

		public const string QuestTargetSignalPart_Despawned = "Despawned";

		public const string QuestTargetSignalPart_Destroyed = "Destroyed";

		public const string QuestTargetSignalPart_Killed = "Killed";

		public const string QuestTargetSignalPart_ChangedFaction = "ChangedFaction";

		public const string QuestTargetSignalPart_LeftMap = "LeftMap";

		public const string QuestTargetSignalPart_SurgeryViolation = "SurgeryViolation";

		public const string QuestTargetSignalPart_Arrested = "Arrested";

		public const string QuestTargetSignalPart_Recruited = "Recruited";

		public const string QuestTargetSignalPart_Kidnapped = "Kidnapped";

		public const string QuestTargetSignalPart_ChangedHostFaction = "ChangedHostFaction";

		public const string QuestTargetSignalPart_NoLongerFactionLeader = "NoLongerFactionLeader";

		public const string QuestTargetSignalPart_TitleChanged = "TitleChanged";

		public const string QuestTargetSignalPart_ShuttleSentSatisfied = "SentSatisfied";

		public const string QuestTargetSignalPart_ShuttleSentUnsatisfied = "SentUnsatisfied";

		public const string QuestTargetSignalPart_ShuttleSentWithExtraColonists = "SentWithExtraColonists";

		public const string QuestTargetSignalPart_AllEnemiesDefeated = "AllEnemiesDefeated";

		public const string QuestTargetSignalPart_TradeRequestFulfilled = "TradeRequestFulfilled";

		public const string QuestTargetSignalPart_PeaceTalksResolved = "Resolved";

		public const string QuestTargetSignalPart_LaunchedShip = "LaunchedShip";

		public const string QuestTargetSignalPart_MonumentCompleted = "MonumentCompleted";

		public const string QuestTargetSignalPart_MonumentDestroyed = "MonumentDestroyed";

		public const string QuestTargetSignalPart_MonumentCancelled = "MonumentCancelled";

		public const string QuestTargetSignalPart_AllHivesDestroyed = "AllHivesDestroyed";

		public const string QuestTargetSignalPart_ExitMentalState = "ExitMentalState";

		public const string QuestTargetSignalPart_FactionBecameHostileToPlayer = "BecameHostileToPlayer";

		private static List<ExtraFaction> tmpExtraFactions = new List<ExtraFaction>();

		private static List<QuestPart> tmpQuestParts = new List<QuestPart>();

		public static Quest GenerateQuestAndMakeAvailable(QuestScriptDef root, float points)
		{
			Slate slate = new Slate();
			slate.Set("points", points);
			return GenerateQuestAndMakeAvailable(root, slate);
		}

		public static Quest GenerateQuestAndMakeAvailable(QuestScriptDef root, Slate vars)
		{
			Quest quest = RimWorld.QuestGen.QuestGen.Generate(root, vars);
			Find.QuestManager.Add(quest);
			return quest;
		}

		public static void SendLetterQuestAvailable(Quest quest)
		{
			TaggedString label = IncidentDefOf.GiveQuest_Random.letterLabel + ": " + quest.name;
			TaggedString text;
			if (quest.initiallyAccepted)
			{
				label = "LetterQuestAutomaticallyAcceptedTitle".Translate(quest.name);
				text = "LetterQuestBecameActive".Translate(quest.name);
				int questTicksRemaining = GetQuestTicksRemaining(quest);
				if (questTicksRemaining > 0)
				{
					text += " " + "LetterQuestActiveNowTime".Translate(questTicksRemaining.ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false));
				}
			}
			else
			{
				text = "LetterQuestBecameAvailable".Translate(quest.name);
				if (quest.ticksUntilAcceptanceExpiry >= 0)
				{
					text += "\n\n" + "LetterQuestRequiresAcceptance".Translate(quest.ticksUntilAcceptanceExpiry.ToStringTicksToPeriod(allowSeconds: false, shortForm: false, canUseDecimals: false));
				}
			}
			ChoiceLetter choiceLetter = LetterMaker.MakeLetter(label, text, IncidentDefOf.GiveQuest_Random.letterDef, LookTargets.Invalid, null, quest);
			choiceLetter.title = quest.name;
			Find.LetterStack.ReceiveLetter(choiceLetter);
		}

		public static int GetQuestTicksRemaining(Quest quest)
		{
			foreach (QuestPart item in quest.PartsListForReading)
			{
				QuestPart_Delay questPart_Delay = item as QuestPart_Delay;
				if (questPart_Delay != null && questPart_Delay.State == QuestPartState.Enabled && questPart_Delay.isBad && !questPart_Delay.expiryInfoPart.NullOrEmpty())
				{
					return questPart_Delay.TicksLeft;
				}
			}
			return 0;
		}

		public static void GenerateBackCompatibilityNameFor(Quest quest)
		{
			quest.name = NameGenerator.GenerateName(RulePackDefOf.NamerQuestDefault, Find.QuestManager.QuestsListForReading.Select((Quest x) => x.name), appendNumberIfNameUsed: false, "defaultQuestName");
		}

		public static bool CanPawnAcceptQuest(Pawn p, Quest quest)
		{
			for (int i = 0; i < quest.PartsListForReading.Count; i++)
			{
				QuestPart_RequirementsToAccept questPart_RequirementsToAccept = quest.PartsListForReading[i] as QuestPart_RequirementsToAccept;
				if (questPart_RequirementsToAccept != null && !questPart_RequirementsToAccept.CanPawnAccept(p))
				{
					return false;
				}
			}
			if (!p.Destroyed && p.IsFreeColonist && !p.Downed && !p.Suspended)
			{
				return !p.IsQuestLodger();
			}
			return false;
		}

		public static bool CanAcceptQuest(Quest quest)
		{
			for (int i = 0; i < quest.PartsListForReading.Count; i++)
			{
				QuestPart_RequirementsToAccept questPart_RequirementsToAccept = quest.PartsListForReading[i] as QuestPart_RequirementsToAccept;
				if (questPart_RequirementsToAccept != null && !questPart_RequirementsToAccept.CanAccept().Accepted)
				{
					return false;
				}
			}
			return true;
		}

		public static Vector2 GetLocForDates()
		{
			if (Find.AnyPlayerHomeMap != null)
			{
				return Find.WorldGrid.LongLatOf(Find.AnyPlayerHomeMap.Tile);
			}
			return default(Vector2);
		}

		public static void SendQuestTargetSignals(List<string> questTags, string signalPart)
		{
			SendQuestTargetSignals(questTags, signalPart, default(SignalArgs));
		}

		public static void SendQuestTargetSignals(List<string> questTags, string signalPart, NamedArgument arg1)
		{
			SendQuestTargetSignals(questTags, signalPart, new SignalArgs(arg1));
		}

		public static void SendQuestTargetSignals(List<string> questTags, string signalPart, NamedArgument arg1, NamedArgument arg2)
		{
			SendQuestTargetSignals(questTags, signalPart, new SignalArgs(arg1, arg2));
		}

		public static void SendQuestTargetSignals(List<string> questTags, string signalPart, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3)
		{
			SendQuestTargetSignals(questTags, signalPart, new SignalArgs(arg1, arg2, arg3));
		}

		public static void SendQuestTargetSignals(List<string> questTags, string signalPart, NamedArgument arg1, NamedArgument arg2, NamedArgument arg3, NamedArgument arg4)
		{
			SendQuestTargetSignals(questTags, signalPart, new SignalArgs(arg1, arg2, arg3, arg4));
		}

		public static void SendQuestTargetSignals(List<string> questTags, string signalPart, NamedArgument[] args)
		{
			SendQuestTargetSignals(questTags, signalPart, new SignalArgs(args));
		}

		public static void SendQuestTargetSignals(List<string> questTags, string signalPart, SignalArgs args)
		{
			if (questTags != null)
			{
				for (int i = 0; i < questTags.Count; i++)
				{
					Find.SignalManager.SendSignal(new Signal(questTags[i] + "." + signalPart, args));
				}
			}
		}

		public static void AddQuestTag(ref List<string> questTags, string questTagToAdd)
		{
			if (questTags == null)
			{
				questTags = new List<string>();
			}
			if (!questTags.Contains(questTagToAdd))
			{
				questTags.Add(questTagToAdd);
			}
		}

		public static void AddQuestTag(object obj, string questTagToAdd)
		{
			if (!questTagToAdd.NullOrEmpty())
			{
				if (obj is Thing)
				{
					AddQuestTag(ref ((Thing)obj).questTags, questTagToAdd);
				}
				else if (obj is WorldObject)
				{
					AddQuestTag(ref ((WorldObject)obj).questTags, questTagToAdd);
				}
				else if (obj is Map)
				{
					AddQuestTag(ref ((Map)obj).Parent.questTags, questTagToAdd);
				}
				else if (obj is Lord)
				{
					AddQuestTag(ref ((Lord)obj).questTags, questTagToAdd);
				}
				else if (obj is Faction)
				{
					AddQuestTag(ref ((Faction)obj).questTags, questTagToAdd);
				}
				else if (obj is IEnumerable)
				{
					foreach (object item in (IEnumerable)obj)
					{
						if (item is Thing)
						{
							AddQuestTag(ref ((Thing)item).questTags, questTagToAdd);
						}
						else if (item is WorldObject)
						{
							AddQuestTag(ref ((WorldObject)item).questTags, questTagToAdd);
						}
						else if (item is Map)
						{
							AddQuestTag(ref ((Map)item).Parent.questTags, questTagToAdd);
						}
						else if (item is Faction)
						{
							AddQuestTag(ref ((Faction)item).questTags, questTagToAdd);
						}
					}
				}
			}
		}

		public static bool AnyMatchingTags(List<string> first, List<string> second)
		{
			if (first.NullOrEmpty() || second.NullOrEmpty())
			{
				return false;
			}
			for (int i = 0; i < first.Count; i++)
			{
				for (int j = 0; j < second.Count; j++)
				{
					if (first[i] == second[j])
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool IsReservedByQuestOrQuestBeingGenerated(Pawn pawn)
		{
			if (!Find.QuestManager.IsReservedByAnyQuest(pawn) && (RimWorld.QuestGen.QuestGen.quest == null || !RimWorld.QuestGen.QuestGen.quest.QuestReserves(pawn)))
			{
				return RimWorld.QuestGen.QuestGen.WasGeneratedForQuestBeingGenerated(pawn);
			}
			return true;
		}

		public static bool IsQuestLodger(this Pawn p)
		{
			return p.HasExtraHomeFaction();
		}

		public static bool IsQuestHelper(this Pawn p)
		{
			if (!p.IsQuestLodger())
			{
				return false;
			}
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if (questsListForReading[i].State != QuestState.Ongoing)
				{
					continue;
				}
				List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
				for (int j = 0; j < partsListForReading.Count; j++)
				{
					QuestPart_ExtraFaction questPart_ExtraFaction;
					if ((questPart_ExtraFaction = (partsListForReading[j] as QuestPart_ExtraFaction)) != null && questPart_ExtraFaction.affectedPawns.Contains(p) && questPart_ExtraFaction.areHelpers)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool HasExtraHomeFaction(this Pawn p, Quest forQuest = null)
		{
			return p.GetExtraHomeFaction(forQuest) != null;
		}

		public static bool HasExtraHomeFaction(this Pawn p, Faction faction)
		{
			tmpExtraFactions.Clear();
			GetExtraFactionsFromQuestParts(p, tmpExtraFactions);
			for (int i = 0; i < tmpExtraFactions.Count; i++)
			{
				if (tmpExtraFactions[i].factionType == ExtraFactionType.HomeFaction && tmpExtraFactions[i].faction == faction)
				{
					tmpExtraFactions.Clear();
					return true;
				}
			}
			tmpExtraFactions.Clear();
			return false;
		}

		public static Faction GetExtraHomeFaction(this Pawn p, Quest forQuest = null)
		{
			return p.GetExtraFaction(ExtraFactionType.HomeFaction, forQuest);
		}

		public static Faction GetExtraHostFaction(this Pawn p, Quest forQuest = null)
		{
			return p.GetExtraFaction(ExtraFactionType.HostFaction, forQuest);
		}

		public static Faction GetExtraFaction(this Pawn p, ExtraFactionType extraFactionType, Quest forQuest = null)
		{
			tmpExtraFactions.Clear();
			GetExtraFactionsFromQuestParts(p, tmpExtraFactions, forQuest);
			for (int i = 0; i < tmpExtraFactions.Count; i++)
			{
				if (tmpExtraFactions[i].factionType == extraFactionType)
				{
					Faction faction = tmpExtraFactions[i].faction;
					tmpExtraFactions.Clear();
					return faction;
				}
			}
			tmpExtraFactions.Clear();
			return null;
		}

		public static void GetExtraFactionsFromQuestParts(Pawn pawn, List<ExtraFaction> outExtraFactions, Quest forQuest = null)
		{
			outExtraFactions.Clear();
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if (questsListForReading[i].State != QuestState.Ongoing && questsListForReading[i] != forQuest)
				{
					continue;
				}
				List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
				for (int j = 0; j < partsListForReading.Count; j++)
				{
					QuestPart_ExtraFaction questPart_ExtraFaction = partsListForReading[j] as QuestPart_ExtraFaction;
					if (questPart_ExtraFaction != null && questPart_ExtraFaction.affectedPawns.Contains(pawn))
					{
						outExtraFactions.Add(questPart_ExtraFaction.extraFaction);
					}
				}
			}
		}

		public static bool IsBorrowedByAnyFaction(this Pawn pawn)
		{
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if (questsListForReading[i].State != QuestState.Ongoing)
				{
					continue;
				}
				List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
				for (int j = 0; j < partsListForReading.Count; j++)
				{
					QuestPart_LendColonistsToFaction questPart_LendColonistsToFaction;
					if ((questPart_LendColonistsToFaction = (partsListForReading[j] as QuestPart_LendColonistsToFaction)) != null && questPart_LendColonistsToFaction.LentColonistsListForReading.Contains(pawn))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static int TotalBorrowedColonistCount()
		{
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			int num = 0;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if (questsListForReading[i].State != QuestState.Ongoing)
				{
					continue;
				}
				List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
				for (int j = 0; j < partsListForReading.Count; j++)
				{
					QuestPart_LendColonistsToFaction questPart_LendColonistsToFaction;
					if ((questPart_LendColonistsToFaction = (partsListForReading[j] as QuestPart_LendColonistsToFaction)) != null)
					{
						num += questPart_LendColonistsToFaction.LentColonistsListForReading.Count;
					}
				}
			}
			return num;
		}

		public static IEnumerable<T> GetAllQuestPartsOfType<T>(bool ongoingOnly = true) where T : class
		{
			List<Quest> quests = Find.QuestManager.QuestsListForReading;
			for (int j = 0; j < quests.Count; j++)
			{
				if (ongoingOnly && quests[j].State != QuestState.Ongoing)
				{
					continue;
				}
				List<QuestPart> partList = quests[j].PartsListForReading;
				for (int i = 0; i < partList.Count; i++)
				{
					T val = partList[i] as T;
					if (val != null)
					{
						yield return val;
					}
				}
			}
		}

		public static void AppendInspectStringsFromQuestParts(StringBuilder sb, ISelectable target)
		{
			AppendInspectStringsFromQuestParts(sb, target, out int _);
		}

		public static void AppendInspectStringsFromQuestParts(StringBuilder sb, ISelectable target, out int count)
		{
			AppendInspectStringsFromQuestParts(delegate(string str, Quest quest)
			{
				if (sb.Length != 0)
				{
					sb.AppendLine();
				}
				sb.Append(str);
			}, target, out count);
		}

		public static void AppendInspectStringsFromQuestParts(Action<string, Quest> func, ISelectable target, out int count)
		{
			count = 0;
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if (questsListForReading[i].State != QuestState.Ongoing)
				{
					continue;
				}
				_ = questsListForReading[i].State;
				tmpQuestParts.Clear();
				tmpQuestParts.AddRange(questsListForReading[i].PartsListForReading);
				tmpQuestParts.SortBy((QuestPart x) => (x is QuestPartActivable) ? ((QuestPartActivable)x).EnableTick : 0);
				for (int j = 0; j < tmpQuestParts.Count; j++)
				{
					QuestPartActivable questPartActivable = tmpQuestParts[j] as QuestPartActivable;
					if (questPartActivable != null && questPartActivable.State == QuestPartState.Enabled)
					{
						string str = questPartActivable.ExtraInspectString(target);
						if (!str.NullOrEmpty())
						{
							func(str.Formatted(target.Named("TARGET")), questsListForReading[i]);
							count++;
						}
					}
				}
				tmpQuestParts.Clear();
			}
		}

		public static IEnumerable<Gizmo> GetQuestRelatedGizmos(Thing thing)
		{
			if (Find.Selector.SelectedObjects.Count == 1)
			{
				Quest linkedQuest = Find.QuestManager.QuestsListForReading.FirstOrDefault((Quest q) => !q.Historical && !q.dismissed && (q.QuestLookTargets.Contains(thing) || q.QuestSelectTargets.Contains(thing)));
				if (linkedQuest != null)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "CommandOpenLinkedQuest".Translate(linkedQuest.name);
					command_Action.defaultDesc = "CommandOpenLinkedQuestDesc".Translate();
					command_Action.icon = TexCommand.OpenLinkedQuestTex;
					command_Action.action = delegate
					{
						Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
						((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(linkedQuest);
					};
					yield return command_Action;
				}
			}
		}

		public static Gizmo GetSelectMonumentMarkerGizmo(Thing thing)
		{
			if (!thing.Spawned || !ModsConfig.RoyaltyActive)
			{
				return null;
			}
			List<Thing> list = thing.Map.listerThings.ThingsOfDef(ThingDefOf.MonumentMarker);
			for (int j = 0; j < list.Count; j++)
			{
				MonumentMarker i = (MonumentMarker)list[j];
				if (i.IsPart(thing))
				{
					return new Command_Action
					{
						defaultLabel = "CommandSelectMonumentMarker".Translate(),
						defaultDesc = "CommandSelectMonumentMarkerDesc".Translate(),
						icon = ThingDefOf.MonumentMarker.uiIcon,
						iconAngle = ThingDefOf.MonumentMarker.uiIconAngle,
						iconOffset = ThingDefOf.MonumentMarker.uiIconOffset,
						action = delegate
						{
							CameraJumper.TrySelect(i);
						}
					};
				}
			}
			return null;
		}

		public static bool AnyQuestDisablesRandomMoodCausedMentalBreaksFor(Pawn p)
		{
			List<Quest> questsListForReading = Find.QuestManager.QuestsListForReading;
			for (int i = 0; i < questsListForReading.Count; i++)
			{
				if (questsListForReading[i].State != QuestState.Ongoing)
				{
					continue;
				}
				List<QuestPart> partsListForReading = questsListForReading[i].PartsListForReading;
				for (int j = 0; j < partsListForReading.Count; j++)
				{
					QuestPart_DisableRandomMoodCausedMentalBreaks questPart_DisableRandomMoodCausedMentalBreaks;
					if ((questPart_DisableRandomMoodCausedMentalBreaks = (partsListForReading[j] as QuestPart_DisableRandomMoodCausedMentalBreaks)) != null && questPart_DisableRandomMoodCausedMentalBreaks.State == QuestPartState.Enabled && questPart_DisableRandomMoodCausedMentalBreaks.pawns.Contains(p))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
