namespace Verse.AI;

public abstract class ThinkNode_ChancePerHour : ThinkNode_Priority
{
	public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
	{
		if (Find.TickManager.TicksGame < GetLastTryTick(pawn) + 2500)
		{
			return ThinkResult.NoJob;
		}
		SetLastTryTick(pawn, Find.TickManager.TicksGame);
		float num = MtbHours(pawn);
		if (num <= 0f)
		{
			return ThinkResult.NoJob;
		}
		Rand.PushState();
		int salt = Gen.HashCombineInt(base.UniqueSaveKey, 26504059);
		Rand.Seed = pawn.RandSeedForHour(salt);
		bool num2 = Rand.MTBEventOccurs(num, 2500f, 2500f);
		Rand.PopState();
		if (num2)
		{
			return base.TryIssueJobPackage(pawn, jobParams);
		}
		return ThinkResult.NoJob;
	}

	protected abstract float MtbHours(Pawn pawn);

	private int GetLastTryTick(Pawn pawn)
	{
		if (pawn.mindState.thinkData.TryGetValue(base.UniqueSaveKey, out var value))
		{
			return value;
		}
		return -99999;
	}

	private void SetLastTryTick(Pawn pawn, int val)
	{
		pawn.mindState.thinkData[base.UniqueSaveKey] = val;
	}
}
