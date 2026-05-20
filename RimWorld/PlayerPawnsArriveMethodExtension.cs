using System;
using Verse;

namespace RimWorld;

public static class PlayerPawnsArriveMethodExtension
{
	public static string ToStringHuman(this PlayerPawnsArriveMethod method)
	{
		return method switch
		{
			PlayerPawnsArriveMethod.Standing => "PlayerPawnsArriveMethod_Standing".Translate(), 
			PlayerPawnsArriveMethod.DropPods => "PlayerPawnsArriveMethod_DropPods".Translate(), 
			PlayerPawnsArriveMethod.Gravship => "PlayerPawnsArriveMethod_Gravship".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}
}
