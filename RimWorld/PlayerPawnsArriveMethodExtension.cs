using System;
using Verse;

namespace RimWorld
{
	public static class PlayerPawnsArriveMethodExtension
	{
		public static string ToStringHuman(this PlayerPawnsArriveMethod method)
		{
			switch (method)
			{
			case PlayerPawnsArriveMethod.Standing:
				return "PlayerPawnsArriveMethod_Standing".Translate();
			case PlayerPawnsArriveMethod.DropPods:
				return "PlayerPawnsArriveMethod_DropPods".Translate();
			default:
				throw new NotImplementedException();
			}
		}
	}
}
