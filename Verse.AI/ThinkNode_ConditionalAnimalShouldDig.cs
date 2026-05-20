namespace Verse.AI;

public class ThinkNode_ConditionalAnimalShouldDig : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		return pawn.playerSettings.animalDig;
	}
}
