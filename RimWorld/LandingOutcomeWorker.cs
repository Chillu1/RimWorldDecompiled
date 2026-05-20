using RimWorld.Planet;
using Verse;

namespace RimWorld;

public abstract class LandingOutcomeWorker
{
	private LandingOutcomeDef def;

	protected virtual LetterDef LetterDef => def.letterDef;

	protected virtual string LetterLabel => def.letterLabel;

	protected virtual string LetterText => def.letterText;

	public LandingOutcomeWorker(LandingOutcomeDef def)
	{
		this.def = def;
	}

	public virtual void ApplyOutcome(Gravship gravship)
	{
		SendStandardLetter(gravship.Engine, null, gravship.Engine);
	}

	protected void SendStandardLetter(Building_GravEngine Engine, string extraText, LookTargets letterTarget)
	{
		TaggedString text = LetterText.Formatted(Engine.RenamableLabel.Named("GRAVSHIP"), Engine.launchInfo.pilot.Named("PILOT"), Engine.launchInfo.copilot.Named("COPILOT"));
		if (extraText != null)
		{
			text += "\n\n" + extraText;
		}
		text += "\n\n" + "GravshipOutcomeChanceLetterExtra".Translate(GravshipUtility.NegativeLandingOutcomeFromQuality(Engine.launchInfo.quality));
		Find.LetterStack.ReceiveLetter(LetterLabel, text, LetterDef, letterTarget);
	}
}
