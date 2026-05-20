using System.Collections.Generic;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld;

[StaticConstructorOnStartup]
public class BookOutcomeDoer_GiveQuest : BookOutcomeDoer
{
	private bool hasQuest;

	private QuestScriptDef questDef;

	private Quest quest;

	private bool giveNext;

	private const int ReceiveQuestMTBTicks = 12500;

	private static readonly Texture2D ViewQuestCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/ViewQuest");

	public new BookOutcomeProperties_GiveQuest Props => (BookOutcomeProperties_GiveQuest)props;

	private bool QuestGiven => quest != null;

	public override bool BenefitDetailsCanChange(Pawn reader = null)
	{
		if (!QuestGiven)
		{
			return hasQuest;
		}
		return false;
	}

	public override string GetBenefitsString(Pawn reader = null)
	{
		if (!hasQuest)
		{
			return null;
		}
		TaggedString ts = " - " + "BookHasQuest".Translate();
		if (QuestGiven)
		{
			ts += string.Format(" ({0})", "DiscoveredQuest".Translate());
			ts = ts.Colorize(Color.gray);
		}
		return ts.Resolve();
	}

	public override bool DoesProvidesOutcome(Pawn reader)
	{
		if (hasQuest)
		{
			return !QuestGiven;
		}
		return false;
	}

	public override void OnBookGenerated(Pawn author = null)
	{
		if (ModsConfig.OdysseyActive)
		{
			hasQuest = Rand.Chance(Props.questChance);
			if (hasQuest)
			{
				questDef = GetQuestDef();
			}
			if (questDef == null)
			{
				hasQuest = false;
			}
		}
	}

	private QuestScriptDef GetQuestDef()
	{
		IEnumerable<QuestScriptDef> giverQuests = QuestUtility.GetGiverQuests(QuestGiverTag.Reading);
		if (giverQuests.EnumerableNullOrEmpty())
		{
			return null;
		}
		return giverQuests.RandomElementByWeight((QuestScriptDef q) => q.rootSelectionWeight);
	}

	public override void OnReadingTick(Pawn reader, float factor)
	{
		if (hasQuest && !QuestGiven && (Rand.MTBEventOccurs(12500f, 1f, 1f) || giveNext))
		{
			GenerateQuest(reader);
		}
	}

	private void GenerateQuest(Pawn reader)
	{
		Slate slate = new Slate();
		slate.Set("points", StorytellerUtility.DefaultThreatPointsNow(reader.Map));
		slate.Set("discoveryMethod", "QuestDiscoveredFromBook".Translate(base.Book.Named("BOOK"), reader.Named("READER")));
		if (questDef == null)
		{
			questDef = GetQuestDef();
		}
		if (questDef != null)
		{
			quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, slate);
			Messages.Message("MessageBookGaveQuest".Translate(quest.name, base.Book.Named("BOOK"), reader.Named("READER")), MessageTypeDefOf.PositiveEvent);
			if (!quest.hidden && quest.root.sendAvailableLetter)
			{
				QuestUtility.SendLetterQuestAvailable(quest, slate.Get<string>("discoveryMethod"));
			}
		}
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		if (QuestGiven)
		{
			yield return new Command_Action
			{
				defaultLabel = "CommandViewQuest".Translate(quest.name),
				defaultDesc = "CommandViewQuestDesc".Translate(),
				icon = ViewQuestCommandTex,
				action = delegate
				{
					Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
					((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
				}
			};
		}
		else if (DebugSettings.godMode)
		{
			yield return new Command_Action
			{
				defaultLabel = "DEV: Give quest",
				icon = TexUI.Placeholder,
				action = delegate
				{
					giveNext = true;
				}
			};
		}
	}

	public override IEnumerable<RulePack> GetTopicRulePacks()
	{
		if (hasQuest)
		{
			yield return questDef.questSubjectRules;
		}
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref hasQuest, "hasQuest", defaultValue: false);
		Scribe_Defs.Look(ref questDef, "questDef");
		Scribe_References.Look(ref quest, "quest");
	}
}
