using Verse;

namespace RimWorld
{
	public class RitualStage_AnimaTreeLinking : RitualStage
	{
		public static readonly SimpleCurve ProgressPerParticipantCurve = new SimpleCurve
		{
			new CurvePoint(1f, 1f),
			new CurvePoint(2f, 1.2f),
			new CurvePoint(4f, 1.5f),
			new CurvePoint(6f, 2f),
			new CurvePoint(8f, 3f)
		};

		public override TargetInfo GetSecondFocus(LordJob_Ritual ritual)
		{
			return ritual.selectedTarget;
		}

		public override float ProgressPerTick(LordJob_Ritual ritual)
		{
			int num = 1;
			foreach (Pawn item in ritual.assignments.SpectatorsForReading)
			{
				if (ritual.IsParticipating(item))
				{
					num++;
				}
			}
			return ProgressPerParticipantCurve.Evaluate(num);
		}
	}
}
