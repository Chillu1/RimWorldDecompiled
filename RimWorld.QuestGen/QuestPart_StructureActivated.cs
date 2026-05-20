using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_StructureActivated : QuestPart
{
	private string activatedSignal;

	private string destroyedSignal;

	private string outSignal;

	private int structureCount;

	private int activatedStructureCount;

	private int stage;

	public QuestPart_StructureActivated()
	{
	}

	public QuestPart_StructureActivated(int structureCount, string activatedSignal, string destroyedSignal, string outSignal, int stage)
	{
		this.structureCount = structureCount;
		this.activatedSignal = activatedSignal;
		this.destroyedSignal = destroyedSignal;
		this.outSignal = outSignal;
		this.stage = stage;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if ((!(signal.tag == activatedSignal) && !(signal.tag == destroyedSignal)) || !signal.args.TryGetArg("SUBJECT", out Thing arg))
		{
			return;
		}
		signal.args.TryGetArg("ACTIVATOR", out Pawn arg2);
		activatedStructureCount++;
		Messages.Message("StructureActivatedMessage".Translate(arg2.Named("PAWN"), arg.Named("STRUCTURE")) + ((structureCount > 1) ? $" ({activatedStructureCount}/{structureCount})." : "."), arg, MessageTypeDefOf.PositiveEvent);
		if (activatedStructureCount == structureCount)
		{
			Signal signal2 = new Signal(outSignal);
			signal2.args.Add(arg2.Named("ACTIVATOR"));
			Find.SignalManager.SendSignal(signal2);
			switch (stage)
			{
			case 1:
				Find.LetterStack.ReceiveLetter("VoidAwakeningStageOneStructuresActivatedLabel".Translate(), "VoidAwakeningStageOneStructuresActivatedText".Translate(), LetterDefOf.NegativeEvent);
				break;
			case 2:
				Find.LetterStack.ReceiveLetter("VoidAwakeningStageTwoStructuresActivatedLabel".Translate(), "VoidAwakeningStageTwoStructuresActivatedText".Translate(), LetterDefOf.NegativeEvent);
				break;
			case 3:
				break;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref activatedSignal, "studiedSignal");
		Scribe_Values.Look(ref destroyedSignal, "destroyedSignal");
		Scribe_Values.Look(ref outSignal, "outSignal");
		Scribe_Values.Look(ref structureCount, "structureCount", 0);
		Scribe_Values.Look(ref activatedStructureCount, "studiedStructureCount", 0);
		Scribe_Values.Look(ref stage, "wave", 0);
	}
}
