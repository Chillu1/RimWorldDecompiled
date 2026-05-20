using Verse;

namespace RimWorld;

public class GameCondition_DroughtInitial : GameCondition_Drought
{
	private const int DurationUntilFullDrought = 240000;

	public override void GameConditionTick()
	{
		if (!HandleEndDueToRain() && Find.TickManager.TicksGame > startTick + 240000)
		{
			IncidentDef drought = IncidentDefOf.Drought;
			IncidentParms parms = StorytellerUtility.DefaultParmsNow(drought.category, base.SingleMap);
			End();
			drought.Worker.TryExecute(parms);
		}
	}
}
