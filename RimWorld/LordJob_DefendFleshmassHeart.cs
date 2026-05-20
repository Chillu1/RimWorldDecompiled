using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_DefendFleshmassHeart : LordJob
{
	private Building_FleshmassHeart heart;

	public LordJob_DefendFleshmassHeart()
	{
	}

	public LordJob_DefendFleshmassHeart(Building_FleshmassHeart heart)
	{
		this.heart = heart;
	}

	public override StateGraph CreateGraph()
	{
		StateGraph stateGraph = new StateGraph();
		stateGraph.AddToil(new LordToil_DefendFleshmassHeart(heart));
		return stateGraph;
	}

	public override void ExposeData()
	{
		Scribe_References.Look(ref heart, "heart");
	}
}
