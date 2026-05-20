namespace Verse.AI;

public class MentalState_Slaughterer : MentalState_SlaughterThing
{
	protected override bool SlaughterTargetAvailable => SlaughtererMentalStateUtility.FindAnimal(pawn) != null;
}
