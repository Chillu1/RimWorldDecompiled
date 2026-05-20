using Verse;
using Verse.Sound;

namespace RimWorld;

public class QuestPart_PlayOneShotOnCamera : QuestPart
{
	public SoundDef soundDef;

	public string inSignal;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (signal.tag == inSignal)
		{
			soundDef.PlayOneShotOnCamera();
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref soundDef, "soundDef");
		Scribe_Values.Look(ref inSignal, "inSignal");
	}
}
