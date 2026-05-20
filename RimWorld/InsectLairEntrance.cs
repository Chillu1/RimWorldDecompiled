using Verse;
using Verse.Sound;

namespace RimWorld;

public class InsectLairEntrance : MapPortal
{
	public bool spawnGravcore;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref spawnGravcore, "spawnGravcore", defaultValue: false);
	}

	public override void OnEntered(Pawn pawn)
	{
		if (!beenEntered)
		{
			TaggedString text = "EnteredMegahiveText".Translate(pawn.Named("PAWN"));
			if (spawnGravcore)
			{
				text += "\n\n" + "EnteredMegahiveGravcoreExtra".Translate();
			}
			Find.LetterStack.ReceiveLetter("EnteredMegahiveLabel".Translate(), text, LetterDefOf.ThreatBig, exit);
		}
		base.OnEntered(pawn);
	}

	protected override void ReceiveCompSignal(string signal)
	{
		base.ReceiveCompSignal(signal);
		if (!(signal != "Sealed"))
		{
			EffecterDefOf.ImpactSmallDustCloud.Spawn(base.Position, base.Map).Cleanup();
			SoundDefOf.Crater_FilledIn.PlayOneShot(this);
		}
	}
}
