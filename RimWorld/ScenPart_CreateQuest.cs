using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ScenPart_CreateQuest : ScenPart
{
	private QuestScriptDef questDef;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref questDef, "questDef");
	}

	public override void DoEditInterface(Listing_ScenEdit listing)
	{
		Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
		string label = ((questDef == null) ? ((string)"ScenPart_SelectQuestDef".Translate()) : ((!string.IsNullOrEmpty(questDef.label)) ? ((string)questDef.LabelCap) : questDef.defName));
		if (!Widgets.ButtonText(scenPartRect, label))
		{
			return;
		}
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		foreach (QuestScriptDef allDef in DefDatabase<QuestScriptDef>.AllDefs)
		{
			QuestScriptDef localFd = allDef;
			string label2 = ((!string.IsNullOrEmpty(allDef.label)) ? ((string)allDef.LabelCap) : allDef.defName);
			list.Add(new FloatMenuOption(label2, delegate
			{
				questDef = localFd;
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	public override void PostGameStart()
	{
		if (questDef != null)
		{
			QuestUtility.GenerateQuestAndMakeAvailable(questDef, 0f);
		}
	}
}
