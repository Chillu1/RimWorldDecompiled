using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetRandomElementByWeight : QuestNode
{
	public class Option
	{
		public SlateRef<object> element;

		public float weight;
	}

	[NoTranslate]
	public SlateRef<string> storeAs;

	public List<Option> options = new List<Option>();

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
		if (options.TryRandomElementByWeight((Option x) => x.weight, out var result))
		{
			slate.Set(storeAs.GetValue(slate), result.element.GetValue(slate));
		}
	}
}
