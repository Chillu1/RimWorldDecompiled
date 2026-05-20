using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetRandomPawnKindForFaction : QuestNode
{
	public class Choice
	{
		public FactionDef factionDef;

		public string categoryTag;

		public List<PawnKindDef> pawnKinds;
	}

	public SlateRef<Thing> factionOf;

	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<List<Choice>> choices;

	public SlateRef<PawnKindDef> fallback;

	protected override bool TestRunInt(Slate slate)
	{
		return SetVars(slate);
	}

	protected override void RunInt()
	{
		SetVars(QuestGen.slate);
	}

	private bool SetVars(Slate slate)
	{
		Thing value = factionOf.GetValue(slate);
		if (value == null)
		{
			return false;
		}
		Faction faction = value.Faction;
		if (faction == null)
		{
			return false;
		}
		List<Choice> value2 = choices.GetValue(slate);
		for (int i = 0; i < value2.Count; i++)
		{
			if (((value2[i].factionDef != null && faction.def == value2[i].factionDef) || (!value2[i].categoryTag.NullOrEmpty() && value2[i].categoryTag == faction.def.categoryTag)) && value2[i].pawnKinds.TryRandomElement(out var result))
			{
				slate.Set(storeAs.GetValue(slate), result);
				return true;
			}
		}
		if (fallback.GetValue(slate) != null)
		{
			slate.Set(storeAs.GetValue(slate), fallback.GetValue(slate));
			return true;
		}
		return false;
	}
}
