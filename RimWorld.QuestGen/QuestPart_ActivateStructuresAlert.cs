using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_ActivateStructuresAlert : QuestPart_Alert
{
	private List<Thing> structures;

	public override string AlertLabel => string.Concat("StructuresActivatedAlertLabel".Translate() + ": ", structures.Count((Thing s) => s?.TryGetComp<CompVoidStructure>()?.Active == true).ToString(), "/", structures.Count.ToString());

	public override string AlertExplanation => "StructuresActivatedAlertDescription".Translate();

	public QuestPart_ActivateStructuresAlert()
	{
	}

	public QuestPart_ActivateStructuresAlert(List<Thing> structures, string inSignalEnable, string inSignalDisable)
	{
		this.structures = structures;
		base.inSignalEnable = inSignalEnable;
		base.inSignalDisable = inSignalDisable;
		lookTargets = structures;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref structures, "structures", LookMode.Reference);
	}

	public override void Cleanup()
	{
		base.Cleanup();
		structures.Clear();
	}
}
