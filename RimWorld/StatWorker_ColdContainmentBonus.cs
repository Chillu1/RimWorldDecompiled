using Verse;

namespace RimWorld;

public class StatWorker_ColdContainmentBonus : StatWorker
{
	public override bool ShouldShowFor(StatRequest req)
	{
		if (!ContainmentUtility.ShowContainmentStats(req.Thing))
		{
			return false;
		}
		float modifier;
		return ContainmentUtility.TryGetColdContainmentBonus(req.Thing, out modifier);
	}

	public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
	{
		ContainmentUtility.TryGetColdContainmentBonus(req.Thing, out var modifier);
		return modifier - 1f;
	}
}
