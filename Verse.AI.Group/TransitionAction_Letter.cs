namespace Verse.AI.Group;

public class TransitionAction_Letter : TransitionAction
{
	public string title;

	public string message;

	public LetterDef type;

	public TransitionAction_Letter(string title, string message, LetterDef type)
	{
		this.title = title;
		this.message = message;
		this.type = type;
	}

	public override void DoAction(Transition trans)
	{
		Find.LetterStack.ReceiveLetter(title, message, type, trans.target.lord.ownedPawns);
	}
}
