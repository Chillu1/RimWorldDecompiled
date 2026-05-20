using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_GetPawnCountByPointsWeighted : QuestNode
{
	[NoTranslate]
	public SlateRef<string> storeAs;

	public SlateRef<int> challengeRating;

	public SlateRef<int> maxCountOneStar;

	public SlateRef<int> maxCountTwoStar;

	public SlateRef<int> maxCountThreeStar;

	public SlateRef<SimpleCurve> pointsCurve;

	public SlateRef<SimpleCurve> chancesCurve;

	public SlateRef<bool> roundRandom;

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
		float x = slate.Get("points", 0f);
		float num = pointsCurve.GetValue(slate).Evaluate(x);
		if (roundRandom.GetValue(slate))
		{
			num = GenMath.RoundRandom(num);
		}
		if (challengeRating.TryGetValue(slate, out var value))
		{
			num = value switch
			{
				1 => Mathf.Min(num, maxCountOneStar.GetValue(slate)), 
				2 => Mathf.Min(num, maxCountTwoStar.GetValue(slate)), 
				_ => Mathf.Min(num, maxCountThreeStar.GetValue(slate)), 
			};
		}
		SimpleCurve value2 = chancesCurve.GetValue(slate);
		int num2 = value2.Points.Count - 1;
		while (num2 >= 0)
		{
			if (value2.Points[num2].x > num)
			{
				value2.Points[num2] = new CurvePoint(0f, 0f);
				num2--;
				continue;
			}
			value2.Points.Insert(num2 + 1, new CurvePoint(num + 1f, 0f));
			break;
		}
		float num3 = Rand.ByCurve(value2);
		if (roundRandom.GetValue(slate))
		{
			num3 = GenMath.RoundRandom(num3);
		}
		slate.Set(storeAs.GetValue(slate), Mathf.Clamp(num3, 1f, num));
	}
}
