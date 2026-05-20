using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class IncidentWorker_SightstealerArrival : IncidentWorker
{
	public static readonly SimpleCurve SightstealerPointsFactorCurve = new SimpleCurve
	{
		new CurvePoint(4000f, 1f),
		new CurvePoint(10000f, 0.6f)
	};

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Slate slate = new Slate();
		Map map = QuestGen_Get.GetMap();
		slate.Set("map", map);
		slate.Set("points", parms.points * SightstealerPointsFactorCurve.Evaluate(parms.points));
		if (!QuestScriptDefOf.SightstealerArrival.CanRun(slate, map))
		{
			return false;
		}
		QuestUtility.GenerateQuestAndMakeAvailable(QuestScriptDefOf.SightstealerArrival, slate);
		return true;
	}
}
