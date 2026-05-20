using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetFreeColonistsCount : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<Map> onlyThisMap;

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
		slate.Set(var: (onlyThisMap.GetValue(slate) == null) ? PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists.Count : onlyThisMap.GetValue(slate).mapPawns.FreeColonistsCount, name: storeAs.GetValue(slate));
	}
}
