using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetRandomInRangeForChallengeRating : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<FloatRange> oneStarRange;

	public SlateRef<FloatRange> twoStarRange;

	public SlateRef<FloatRange> threeStarRange;

	public SlateRef<bool> roundRandom;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		float randomInRange = GetRangeFromRating().RandomInRange;
		slate.Set(storeAs.GetValue(slate), roundRandom.GetValue(slate) ? ((float)GenMath.RoundRandom(randomInRange)) : randomInRange);
	}

	public FloatRange GetRangeFromRating()
	{
		int challengeRating = QuestGen.quest.challengeRating;
		Slate slate = QuestGen.slate;
		return challengeRating switch
		{
			3 => threeStarRange.GetValue(slate), 
			2 => twoStarRange.GetValue(slate), 
			_ => oneStarRange.GetValue(slate), 
		};
	}

	protected override bool TestRunInt(Slate slate)
	{
		slate.Set(storeAs.GetValue(slate), 0);
		return true;
	}
}
