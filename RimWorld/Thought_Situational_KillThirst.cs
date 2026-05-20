using Verse;

namespace RimWorld;

public class Thought_Situational_KillThirst : Thought_Situational
{
	private static readonly SimpleCurve MoodOffsetCurve = new SimpleCurve
	{
		new CurvePoint(0.301f, 0f),
		new CurvePoint(0.3f, -4f),
		new CurvePoint(0f, -18f)
	};

	public override float MoodOffset()
	{
		if (pawn.needs == null || !pawn.needs.TryGetNeed(out Need_KillThirst need))
		{
			return 0f;
		}
		return MoodOffsetCurve.Evaluate(need.CurLevel);
	}
}
