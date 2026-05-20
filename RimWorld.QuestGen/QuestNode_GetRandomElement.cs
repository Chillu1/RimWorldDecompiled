using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetRandomElement : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public List<SlateRef<object>> options;

	protected override bool TestRunInt(Slate slate)
	{
		SetVars(slate);
		return true;
	}

	protected override void RunInt()
	{
		SetVars(QuestGen.slate);
	}

	private void SetVars(Slate slate)
	{
		if (options.TryRandomElement(out var result))
		{
			slate.Set(storeAs.GetValue(slate), result.GetValue(slate));
		}
	}
}
