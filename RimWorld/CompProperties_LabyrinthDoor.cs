using Verse;

namespace RimWorld;

public class CompProperties_LabyrinthDoor : CompProperties_Interactable
{
	public GraphicData jammed;

	public float unlockedChance = 0.3f;

	public CompProperties_LabyrinthDoor()
	{
		compClass = typeof(CompLabyrinthDoor);
	}
}
