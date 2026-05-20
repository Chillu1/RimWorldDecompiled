using Verse;

namespace RimWorld;

public class IncidentWorker_DeathPall : IncidentWorker_MakeGameCondition
{
	public override GameConditionDef GetGameConditionDef(IncidentParms parms)
	{
		return GameConditionDefOf.DeathPall;
	}
}
