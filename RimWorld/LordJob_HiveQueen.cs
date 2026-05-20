using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class LordJob_HiveQueen : LordJob_StructureThreatCluster
{
	public LordJob_HiveQueen()
	{
	}

	public LordJob_HiveQueen(Faction faction, IntVec3 position, float wanderRadius, bool sendWokenUpMessage = true, bool awakeOnClamor = false)
		: base(faction, position, wanderRadius, sendWokenUpMessage, awakeOnClamor)
	{
	}

	protected override LordToil GetIdleToil()
	{
		return new LordToil_WanderNest();
	}
}
