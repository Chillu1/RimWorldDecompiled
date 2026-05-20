using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class QuestPart_DefendPoint : QuestPart_MakeLord
{
	public IntVec3 point;

	public float? wanderRadius;

	public float? defendRadius;

	public bool isCaravanSendable;

	public bool addFleeToil = true;

	protected override Lord MakeLord()
	{
		return LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(point, wanderRadius, defendRadius, isCaravanSendable, addFleeToil), base.Map);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref point, "point");
		Scribe_Values.Look(ref wanderRadius, "wanderRadius");
		Scribe_Values.Look(ref defendRadius, "defendRadius");
		Scribe_Values.Look(ref isCaravanSendable, "isCaravanSendable", defaultValue: false);
		Scribe_Values.Look(ref addFleeToil, "addFleeToil", defaultValue: false);
	}
}
