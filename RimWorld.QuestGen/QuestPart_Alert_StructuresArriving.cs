using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_Alert_StructuresArriving : QuestPart_Alert
{
	private int arriveTick;

	private int delay;

	private int TicksRemaining => arriveTick - Find.TickManager.TicksGame;

	public override string AlertLabel => base.AlertLabel + ": " + TicksRemaining.ToStringTicksToPeriod(allowSeconds: false);

	public QuestPart_Alert_StructuresArriving()
	{
	}

	public QuestPart_Alert_StructuresArriving(string label, string explanation, string inSignalEnable, string inSignalDisable, int delay, LookTargets lookTargets = null)
	{
		QuestGen.AddTextRequest("root", delegate(string x)
		{
			base.label = x;
		}, QuestGenUtility.MergeRules(null, label, "root"));
		QuestGen.AddTextRequest("root", delegate(string x)
		{
			base.explanation = x;
		}, QuestGenUtility.MergeRules(null, explanation, "root"));
		base.inSignalEnable = inSignalEnable;
		base.inSignalDisable = inSignalDisable;
		this.delay = delay;
		base.lookTargets = lookTargets;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref arriveTick, "arriveTick", 0);
		Scribe_Values.Look(ref delay, "delay", 0);
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignalEnable)
		{
			arriveTick = Find.TickManager.TicksGame + delay;
		}
		base.Notify_QuestSignalReceived(signal);
	}
}
