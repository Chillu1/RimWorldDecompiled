using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class LordToil_Party : LordToil_Gathering
{
	private float joyPerTick = 3.5E-05f;

	public const float DefaultJoyPerTick = 3.5E-05f;

	public LordToil_Party(IntVec3 spot, GatheringDef gatheringDef, float joyPerTick = 3.5E-05f)
		: base(spot, gatheringDef)
	{
		this.joyPerTick = joyPerTick;
	}

	public override void LordToilTick()
	{
		base.LordToilTick();
		List<Pawn> ownedPawns = lord.ownedPawns;
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			if (GatheringsUtility.InGatheringArea(ownedPawns[i].Position, spot, base.Map))
			{
				ownedPawns[i].needs.joy?.GainJoy(joyPerTick, JoyKindDefOf.Social);
			}
		}
	}
}
