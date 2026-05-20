using Verse;

namespace RimWorld;

public class RoomContents_CorpseRoom : RoomContents_DeadBodyLabyrinth
{
	protected override IntRange CorpseRange => new IntRange(5, 10);

	protected override IntRange BloodFilthRange => new IntRange(4, 10);
}
