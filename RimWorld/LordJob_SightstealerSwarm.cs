using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_SightstealerSwarm : LordJob_EntitySwarm
{
	public LordJob_SightstealerSwarm()
	{
	}

	public LordJob_SightstealerSwarm(IntVec3 startPos, IntVec3 destPos)
		: base(startPos, destPos)
	{
	}

	protected override LordToil CreateTravelingToil(IntVec3 start, IntVec3 dest)
	{
		return new LordToil_SightstealerSwarm(start, dest);
	}
}
