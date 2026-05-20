using Verse;

namespace RimWorld;

public class SkillNeed_Curve : SkillNeed
{
	public SimpleCurve curve;

	public override float ValueFor(Pawn pawn)
	{
		if (pawn.skills == null)
		{
			return curve.MinY;
		}
		return curve.Evaluate(pawn.skills.GetSkill(skill).Level);
	}
}
