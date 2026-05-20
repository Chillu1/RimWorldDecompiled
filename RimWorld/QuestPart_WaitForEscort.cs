using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class QuestPart_WaitForEscort : QuestPart_MakeLord
{
	public IntVec3 point;

	public bool addFleeToil = true;

	protected override Lord MakeLord()
	{
		return LordMaker.MakeNewLord(faction, new LordJob_WaitForEscort(point, addFleeToil), base.Map);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref point, "point");
		Scribe_Values.Look(ref addFleeToil, "addFleeToil", defaultValue: false);
	}
}
