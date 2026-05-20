using Verse;

namespace RimWorld;

public class QuestPart_DescriptionPart : QuestPartActivable
{
	public string descriptionPart;

	protected string resolvedDescriptionPart;

	public override string DescriptionPart => resolvedDescriptionPart;

	protected override void Enable(SignalArgs receivedArgs)
	{
		base.Enable(receivedArgs);
		resolvedDescriptionPart = receivedArgs.GetFormattedText(descriptionPart).Resolve();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref descriptionPart, "descriptionPart");
		Scribe_Values.Look(ref resolvedDescriptionPart, "resolvedDescriptionPart");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		descriptionPart = "Debug description part.";
	}
}
