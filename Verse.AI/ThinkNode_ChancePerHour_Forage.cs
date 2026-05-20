namespace Verse.AI;

public class ThinkNode_ChancePerHour_Forage : ThinkNode_ChancePerHour
{
	protected override float MtbHours(Pawn pawn)
	{
		if (pawn.Map.Biome.forageability == 0f)
		{
			return 0f;
		}
		return 72f / pawn.Map.Biome.forageability;
	}
}
