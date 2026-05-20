namespace Verse.AI;

public class ThinkNode_ConditionalAnimalShouldForage : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		return pawn.playerSettings.animalForage;
	}
}
