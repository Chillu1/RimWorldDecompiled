using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class QuestPart_AddQuest : QuestPart
{
	public string inSignal;

	public Pawn acceptee;

	public Quest parent;

	public bool sendAvailableLetter;

	public abstract QuestScriptDef QuestDef { get; }

	public virtual bool CanAdd => true;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignal && CanAdd)
		{
			AddQuest();
			PostAdd();
		}
	}

	private void AddQuest()
	{
		Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(QuestDef, GetSlate());
		quest.parent = parent;
		quest.Accept(acceptee);
		if (sendAvailableLetter)
		{
			QuestUtility.SendLetterQuestAvailable(quest);
		}
	}

	public abstract Slate GetSlate();

	public virtual void PostAdd()
	{
	}

	public override void DoDebugWindowContents(Rect innerRect, ref float curY)
	{
		Rect rect = new Rect(innerRect.x, curY, 500f, 25f);
		if (Widgets.ButtonText(rect, "AddQuest " + QuestDef.defName))
		{
			AddQuest();
		}
		curY += rect.height + 4f;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref acceptee, "acceptee");
		Scribe_References.Look(ref parent, "parent");
		Scribe_Values.Look(ref sendAvailableLetter, "sendAvailableLetter", defaultValue: false);
	}
}
