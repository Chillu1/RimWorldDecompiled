using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class LordToil_Party : LordToil_Gathering
	{
		private float joyPerTick = 3.5E-05f;

		public const float DefaultJoyPerTick = 3.5E-05f;

		private LordToilData_Party Data => (LordToilData_Party)data;

		public LordToil_Party(IntVec3 spot, GatheringDef gatheringDef, float joyPerTick = 3.5E-05f)
			: base(spot, gatheringDef)
		{
			this.joyPerTick = joyPerTick;
			data = new LordToilData_Party();
		}

		public override void LordToilTick()
		{
			List<Pawn> ownedPawns = lord.ownedPawns;
			for (int i = 0; i < ownedPawns.Count; i++)
			{
				if (GatheringsUtility.InGatheringArea(ownedPawns[i].Position, spot, base.Map))
				{
					ownedPawns[i].needs.joy.GainJoy(joyPerTick, JoyKindDefOf.Social);
					if (!Data.presentForTicks.ContainsKey(ownedPawns[i]))
					{
						Data.presentForTicks.Add(ownedPawns[i], 0);
					}
					Data.presentForTicks[ownedPawns[i]]++;
				}
			}
		}
	}
}
