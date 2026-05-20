using Verse;

namespace RimWorld
{
	public class FactionCurve
	{
		public FactionDef faction;

		private SimpleCurve selectionWeightPerPointsCurve;

		public float Evaluate(float x)
		{
			return selectionWeightPerPointsCurve.Evaluate(x);
		}
	}
}
