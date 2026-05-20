using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_DarkenMap : QuestPart
{
	public string inSignal;

	public MapParent mapParent;

	public QuestPart_DarkenMap()
	{
	}

	public QuestPart_DarkenMap(string inSignal, MapParent mapParent)
	{
		this.inSignal = inSignal;
		this.mapParent = mapParent;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (mapParent.Map != null && signal.tag == inSignal)
		{
			mapParent.Map.gameConditionManager.SetTargetBrightness(0f);
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		if (mapParent.Map != null)
		{
			GameCondition_UnnaturalDarkness activeCondition = mapParent.Map.GameConditionManager.GetActiveCondition<GameCondition_UnnaturalDarkness>();
			if (activeCondition != null)
			{
				activeCondition.Permanent = false;
				activeCondition.TicksLeft = 301;
			}
			mapParent.Map.gameConditionManager.SetTargetBrightness(1f);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref mapParent, "mapParent");
	}
}
