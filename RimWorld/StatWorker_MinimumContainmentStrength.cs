using Verse;

namespace RimWorld;

public class StatWorker_MinimumContainmentStrength : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		return ContainmentUtility.ShowContainmentStats(req.Thing);
	}
}
