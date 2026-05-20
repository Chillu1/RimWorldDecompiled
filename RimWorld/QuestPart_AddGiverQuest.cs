using System.Linq;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld;

public class QuestPart_AddGiverQuest : QuestPart_AddQuest
{
	public QuestScriptDef questScript;

	public string discoveryMethodTranslationKey;

	public float points;

	private Signal receivedSignal;

	public override QuestScriptDef QuestDef => questScript;

	public override Slate GetSlate()
	{
		Slate slate = new Slate();
		NamedArgument[] args = receivedSignal.args.Args.Select((NamedArgument arg) => arg.arg.Named(arg.label)).ToArray();
		slate.Set("points", points);
		slate.Set("discoveryMethod", discoveryMethodTranslationKey.Translate(args));
		return slate;
	}

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (ModsConfig.OdysseyActive)
		{
			receivedSignal = signal;
			base.Notify_QuestSignalReceived(signal);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref questScript, "questScript");
		Scribe_Values.Look(ref discoveryMethodTranslationKey, "discoveryMethodTranslationKey");
		Scribe_Values.Look(ref points, "points", 0f);
	}
}
