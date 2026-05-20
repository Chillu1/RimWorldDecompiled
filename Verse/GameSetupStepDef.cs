namespace Verse;

public class GameSetupStepDef : Def
{
	public float order;

	public bool executeAfterWorldSetup;

	public GameSetupStep setupStep;

	public override void PostLoad()
	{
		base.PostLoad();
		setupStep.def = this;
	}
}
