using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Hot : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			float statValue = p.GetStatValue(StatDefOf.ComfyTemperatureMax);
			float num = p.AmbientTemperature - statValue;
			if (num <= 0f)
			{
				return ThoughtState.Inactive;
			}
			if (num < 10f)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (num < 20f)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			if (num < 30f)
			{
				return ThoughtState.ActiveAtStage(2);
			}
			return ThoughtState.ActiveAtStage(3);
		}
	}
}
