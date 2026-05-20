using Verse;

namespace RimWorld;

public class SignalAction_Letter : SignalAction
{
	public string letterLabelKey;

	public string letterMessageKey;

	public LetterDef letterDef;

	public LookTargets lookTargets;

	public Pawn fixedPawnReference;

	private string letterLabel;

	private string letterMessage;

	public bool requireLookTargetsNotDestroyed;

	private Letter tmpLetter;

	protected override void DoAction(SignalArgs args)
	{
		if (requireLookTargetsNotDestroyed && lookTargets.PrimaryTarget.ThingDestroyed)
		{
			return;
		}
		Pawn pawn;
		if (args.TryGetArg("SUBJECT", out Pawn arg))
		{
			pawn = fixedPawnReference ?? arg;
			if (!lookTargets.IsValid())
			{
				lookTargets = arg;
			}
		}
		else
		{
			pawn = fixedPawnReference;
		}
		(string, string) labelMessage = GetLabelMessage(pawn);
		Find.LetterStack.ReceiveLetter(labelMessage.Item1, labelMessage.Item2, letterDef, lookTargets);
	}

	private (string label, string message) GetLabelMessage(Pawn pawn = null)
	{
		string item = ((!string.IsNullOrEmpty(letterLabel)) ? letterLabel : ((pawn == null) ? ((string)letterLabelKey.Translate()) : ((string)letterLabelKey.Translate(pawn.Named("PAWN")))));
		string item2 = ((!string.IsNullOrEmpty(letterMessage)) ? letterMessage : ((pawn == null) ? ((string)letterMessageKey.Translate()) : ((string)letterMessageKey.Translate(pawn.Named("PAWN")))));
		return (label: item, message: item2);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref letterLabelKey, "letterLabelKey");
		Scribe_Values.Look(ref letterLabel, "letterLabel");
		Scribe_Values.Look(ref requireLookTargetsNotDestroyed, "requireLookTargetsSpawned", defaultValue: false);
		Scribe_Values.Look(ref letterMessageKey, "letterMessageKey");
		Scribe_Values.Look(ref letterMessage, "letterMessage");
		Scribe_Deep.Look(ref lookTargets, "lookTargets");
		Scribe_Defs.Look(ref letterDef, "letterDef");
		Scribe_References.Look(ref fixedPawnReference, "fixedPawnReference", saveDestroyedThings: true);
		Scribe_Deep.Look(ref tmpLetter, "letter");
		if (Scribe.mode == LoadSaveMode.PostLoadInit && tmpLetter != null)
		{
			letterLabel = tmpLetter.Label;
			lookTargets = tmpLetter.lookTargets;
			letterDef = tmpLetter.def;
			if (tmpLetter is ChoiceLetter choiceLetter)
			{
				letterMessage = choiceLetter.Text;
			}
		}
	}
}
