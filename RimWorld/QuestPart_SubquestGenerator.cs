using System.Collections.Generic;
using System.Linq;
using RimWorld.QuestGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class QuestPart_SubquestGenerator : QuestPartActivable
{
	public List<QuestScriptDef> subquestDefs = new List<QuestScriptDef>();

	public IntRange interval;

	public int maxActiveSubquests = 2;

	public string expiryInfoPartKey;

	public int maxSuccessfulSubquests = -1;

	private int? currentInterval;

	protected virtual bool CanGenerateSubquest
	{
		get
		{
			if (PendingSubquestCount < maxActiveSubquests)
			{
				if (maxSuccessfulSubquests >= 0)
				{
					return SuccessfulSubquestCount + PendingSubquestCount < maxSuccessfulSubquests;
				}
				return true;
			}
			return false;
		}
	}

	protected int SuccessfulSubquestCount => quest.GetSubquests(QuestState.EndedSuccess).Count();

	private int PendingSubquestCount => quest.GetSubquests().Count((Quest q) => q.State == QuestState.Ongoing || q.State == QuestState.NotYetAccepted);

	protected bool Paused => Find.AnyPlayerHomeMap == null;

	public override string ExpiryInfoPart
	{
		get
		{
			if (expiryInfoPartKey.NullOrEmpty())
			{
				return null;
			}
			return $"{expiryInfoPartKey.Translate()} {SuccessfulSubquestCount} / {maxSuccessfulSubquests}";
		}
	}

	public override void QuestPartTick()
	{
		if (subquestDefs.Count == 0)
		{
			return;
		}
		if (maxSuccessfulSubquests > 0 && SuccessfulSubquestCount >= maxSuccessfulSubquests)
		{
			Complete();
		}
		else
		{
			if (Paused)
			{
				return;
			}
			if (currentInterval.HasValue)
			{
				currentInterval--;
				if (currentInterval < 0)
				{
					if (!TryGenerateSubquest())
					{
						Log.Warning("Failed to generate subquest for quest " + quest.root.defName);
					}
					currentInterval = null;
				}
			}
			if (!currentInterval.HasValue && CanGenerateSubquest)
			{
				currentInterval = interval.RandomInRange;
			}
		}
	}

	protected abstract QuestScriptDef GetNextSubquestDef();

	protected abstract Slate InitSlate();

	protected virtual bool TryGenerateSubquest()
	{
		QuestScriptDef nextSubquestDef = GetNextSubquestDef();
		if (nextSubquestDef == null)
		{
			return false;
		}
		Slate vars = InitSlate();
		Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(nextSubquestDef, vars);
		quest.parent = base.quest;
		if (!quest.hidden && quest.root.sendAvailableLetter)
		{
			QuestUtility.SendLetterQuestAvailable(quest);
		}
		return true;
	}

	public override void DoDebugWindowContents(Rect innerRect, ref float curY)
	{
		if (base.State != QuestPartState.Enabled)
		{
			return;
		}
		Rect rect = new Rect(innerRect.x, curY, 500f, 25f);
		if (Widgets.ButtonText(rect, "Generate random subquest") && CanGenerateSubquest)
		{
			TryGenerateSubquest();
		}
		curY += rect.height + 4f;
		Rect rect2 = new Rect(innerRect.x, curY, 500f, 25f);
		if (Widgets.ButtonText(rect2, "Generate specific subquest"))
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (QuestScriptDef questDef in subquestDefs)
			{
				list.Add(new FloatMenuOption(questDef.defName, delegate
				{
					Slate vars = InitSlate();
					Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(questDef, vars);
					quest.parent = base.quest;
					if (!quest.hidden && quest.root.sendAvailableLetter)
					{
						QuestUtility.SendLetterQuestAvailable(quest);
					}
				}));
			}
			if (list.Any())
			{
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}
		curY += rect2.height + 4f;
		Rect rect3 = new Rect(innerRect.x, curY, 500f, 25f);
		if (Widgets.ButtonText(rect3, "Remove active subquests"))
		{
			foreach (Quest subquest in quest.GetSubquests())
			{
				Find.QuestManager.Remove(subquest);
			}
		}
		curY += rect3.height + 4f;
		Rect rect4 = new Rect(innerRect.x, curY, 500f, 25f);
		if (Widgets.ButtonText(rect4, "Complete quest"))
		{
			Complete();
		}
		curY += rect4.height + 4f;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref currentInterval, "currentInterval");
		Scribe_Collections.Look(ref subquestDefs, "subquestDefs", LookMode.Def);
		Scribe_Values.Look(ref interval, "interval");
		Scribe_Values.Look(ref maxActiveSubquests, "maxActiveSubquests", 0);
		Scribe_Values.Look(ref maxSuccessfulSubquests, "maxSuccessfulSubquests", -1);
		Scribe_Values.Look(ref expiryInfoPartKey, "expiryInfoPartKey");
	}
}
