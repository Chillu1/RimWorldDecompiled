namespace RimWorld;

public class IncidentWorker_MakeInitialDrought : IncidentWorker_MakeGameCondition
{
	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		GameConditionManager gameConditionManager = parms.target.GameConditionManager;
		GameCondition cond = GameConditionMaker.MakeConditionPermanent(GetGameConditionDef(parms));
		gameConditionManager.RegisterCondition(cond);
		return true;
	}
}
