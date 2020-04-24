using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Cold : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			float statValue = p.GetStatValue(StatDefOf.ComfyTemperatureMin);
			float ambientTemperature = p.AmbientTemperature;
			float num = statValue - ambientTemperature;
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
