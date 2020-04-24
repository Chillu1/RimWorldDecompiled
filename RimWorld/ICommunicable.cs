using Verse;

namespace RimWorld
{
	public interface ICommunicable
	{
		string GetCallLabel();

		string GetInfoText();

		void TryOpenComms(Pawn negotiator);

		Faction GetFaction();

		FloatMenuOption CommFloatMenuOption(Building_CommsConsole console, Pawn negotiator);
	}
}
