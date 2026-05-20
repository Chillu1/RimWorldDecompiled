using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_RandomWaves : QuestPartActivable
{
	private string outSignal;

	private int lastWaveTick = -99999;

	private float mtbWavesHours;

	private float minRefireWavesHours;

	private float maxRefireWavesHours;

	public QuestPart_RandomWaves()
	{
	}

	public QuestPart_RandomWaves(string inSignalEnable, string outSignal, float mtbWavesHours, float minRefireWavesHours, float maxRefireWavesHours = float.MaxValue)
	{
		base.inSignalEnable = inSignalEnable;
		this.outSignal = outSignal;
		this.mtbWavesHours = mtbWavesHours;
		this.minRefireWavesHours = minRefireWavesHours;
		this.maxRefireWavesHours = maxRefireWavesHours;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag != inSignalEnable))
		{
			lastWaveTick = Find.TickManager.TicksGame;
		}
	}

	public override void QuestPartTick()
	{
		if (((float)Find.TickManager.TicksGame > (float)lastWaveTick + minRefireWavesHours * 2500f && Rand.MTBEventOccurs(mtbWavesHours, 2500f, 1f)) || (float)Find.TickManager.TicksGame > (float)lastWaveTick + maxRefireWavesHours * 2500f)
		{
			Find.SignalManager.SendSignal(new Signal(outSignal));
			lastWaveTick = Find.TickManager.TicksGame;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref outSignal, "outSignal");
		Scribe_Values.Look(ref lastWaveTick, "lastWaveTick", 0);
		Scribe_Values.Look(ref mtbWavesHours, "mtbWavesHours", 40f);
		Scribe_Values.Look(ref minRefireWavesHours, "minRefireWavesHours", 24f);
		Scribe_Values.Look(ref maxRefireWavesHours, "maxRefireWavesHours", 72f);
	}
}
