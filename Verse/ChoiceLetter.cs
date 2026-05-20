using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public abstract class ChoiceLetter : LetterWithTimeout
{
	public string title;

	private TaggedString text;

	public bool radioMode;

	public Quest quest;

	public List<ThingDef> hyperlinkThingDefs;

	public List<HediffDef> hyperlinkHediffDefs;

	public TaggedString Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value.CapitalizeFirst();
		}
	}

	public abstract IEnumerable<DiaOption> Choices { get; }

	protected DiaOption Option_Close => new DiaOption("Close".Translate())
	{
		action = delegate
		{
			Find.LetterStack.RemoveLetter(this);
		},
		resolveTree = true
	};

	protected DiaOption Option_JumpToLocation
	{
		get
		{
			GlobalTargetInfo target = lookTargets.TryGetPrimaryTarget();
			DiaOption diaOption = new DiaOption("JumpToLocation".Translate());
			diaOption.action = delegate
			{
				CameraJumper.TryJumpAndSelect(target);
				Find.LetterStack.RemoveLetter(this);
			};
			diaOption.resolveTree = true;
			if (!CameraJumper.CanJump(target))
			{
				diaOption.Disable(null);
			}
			return diaOption;
		}
	}

	protected DiaOption Option_JumpToLocationAndPostpone
	{
		get
		{
			GlobalTargetInfo target = lookTargets.TryGetPrimaryTarget();
			DiaOption diaOption = new DiaOption("JumpToLocation".Translate());
			diaOption.action = delegate
			{
				CameraJumper.TryJumpAndSelect(target);
			};
			diaOption.resolveTree = true;
			if (!CameraJumper.CanJump(target))
			{
				diaOption.Disable(null);
			}
			return diaOption;
		}
	}

	protected DiaOption Option_Reject => new DiaOption("RejectLetter".Translate())
	{
		action = delegate
		{
			Find.LetterStack.RemoveLetter(this);
		},
		resolveTree = true
	};

	protected DiaOption Option_Postpone
	{
		get
		{
			DiaOption diaOption = new DiaOption("PostponeLetter".Translate());
			diaOption.resolveTree = true;
			if (LastTickBeforeTimeout)
			{
				diaOption.Disable(null);
			}
			return diaOption;
		}
	}

	protected DiaOption Option_ViewInQuestsTab(string labelKey = "ViewRelatedQuest", bool postpone = false)
	{
		TaggedString taggedString = labelKey.Translate();
		if (title != quest.name && !quest.hidden)
		{
			taggedString += ": " + quest.name;
		}
		DiaOption diaOption = new DiaOption(taggedString);
		diaOption.action = delegate
		{
			if (quest != null)
			{
				Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Quests);
				((MainTabWindow_Quests)MainButtonDefOf.Quests.TabWindow).Select(quest);
				if (!postpone)
				{
					Find.LetterStack.RemoveLetter(this);
				}
			}
		};
		diaOption.resolveTree = true;
		if (quest == null || quest.hidden)
		{
			diaOption.Disable(null);
		}
		return diaOption;
	}

	protected DiaOption Option_ViewInfoCard(int index)
	{
		int num = ((hyperlinkThingDefs != null) ? hyperlinkThingDefs.Count : 0);
		if (index >= num)
		{
			return new DiaOption(new Dialog_InfoCard.Hyperlink(hyperlinkHediffDefs[index - num]));
		}
		return new DiaOption(new Dialog_InfoCard.Hyperlink(hyperlinkThingDefs[index]));
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref title, "title");
		Scribe_Values.Look(ref text, "text");
		Scribe_Values.Look(ref radioMode, "radioMode", defaultValue: false);
		Scribe_References.Look(ref quest, "quest");
		Scribe_Collections.Look(ref hyperlinkThingDefs, "hyperlinkThingDefs", LookMode.Def);
		Scribe_Collections.Look(ref hyperlinkHediffDefs, "hyperlinkHediffDefs", LookMode.Def);
	}

	protected override string GetMouseoverText()
	{
		return text.Resolve();
	}

	public override void OpenLetter()
	{
		DiaNode diaNode = new DiaNode(text);
		diaNode.options.AddRange(Choices);
		Dialog_NodeTreeWithFactionInfo window = new Dialog_NodeTreeWithFactionInfo(diaNode, relatedFaction, delayInteractivity: false, radioMode, title);
		Find.WindowStack.Add(window);
	}
}
