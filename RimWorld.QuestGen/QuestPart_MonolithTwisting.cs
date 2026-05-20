using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_MonolithTwisting : QuestPart_Alert_StructuresArriving
{
	private Building_VoidMonolith monolith;

	private int intensifyTick;

	private Effecter effecter;

	private Effecter intenseEffecter;

	private int intensifiedDelay;

	public QuestPart_MonolithTwisting()
	{
	}

	public QuestPart_MonolithTwisting(string label, string explanation, string inSignalEnable, string inSignalDisable, int delay, Building_VoidMonolith monolith, int intensifiedDelay)
		: base(label, explanation, inSignalEnable, inSignalDisable, delay, monolith)
	{
		this.monolith = monolith;
		this.intensifiedDelay = intensifiedDelay;
	}

	protected override void Enable(SignalArgs receivedArgs)
	{
		intensifyTick = Find.TickManager.TicksGame + intensifiedDelay;
		base.Enable(receivedArgs);
	}

	public override void QuestPartTick()
	{
		base.QuestPartTick();
		if (effecter == null)
		{
			effecter = EffecterDefOf.MonolithTwisting.Spawn();
		}
		if (Find.TickManager.TicksGame >= intensifyTick && intenseEffecter == null)
		{
			intenseEffecter = EffecterDefOf.MonolithTwistingIntense.SpawnMaintained(monolith, monolith.MapHeld);
		}
		effecter.EffectTick(monolith, monolith);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref monolith, "monolith");
		Scribe_Values.Look(ref intensifyTick, "intensifyTick", 0);
	}
}
