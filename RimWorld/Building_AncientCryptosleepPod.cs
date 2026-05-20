using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Building_AncientCryptosleepPod : Building_AncientCryptosleepCasket
	{
		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			return Enumerable.Empty<FloatMenuOption>();
		}
	}
}
