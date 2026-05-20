using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetAnimalKindByPoints : QuestNode
{
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
		float points = slate.Get("points", 0f);
		if (DefDatabase<PawnKindDef>.AllDefs.Where((PawnKindDef x) => x.RaceProps.Animal && !x.RaceProps.Dryad && !x.RaceProps.neverIncludeInQuests && x.combatPower < points).TryRandomElement(out var result))
		{
			slate.Set("animalKindDef", result);
			return true;
		}
		return false;
	}
}
