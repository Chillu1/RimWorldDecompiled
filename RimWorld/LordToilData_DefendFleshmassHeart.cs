using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordToilData_DefendFleshmassHeart : LordToilData
{
	public Building_FleshmassHeart heart;

	public override void ExposeData()
	{
		Scribe_References.Look(ref heart, "heart");
	}
}
