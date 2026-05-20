using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace Verse.AI;

public abstract class MentalState_BabyFit : MentalState
{
	public const float BabyScreamRadius = 9.9f;

	public const int ScreamInterval = 150;

	private float lastScreamTick = -1f;

	private List<Pawn> alreadyHeard = new List<Pawn>(32);

	protected abstract void AuraEffect(Thing source, Pawn hearer);

	public override RandomSocialMode SocialModeMax()
	{
		return RandomSocialMode.Normal;
	}

	public override void MentalStateTick(int delta)
	{
		base.MentalStateTick(delta);
		if ((float)Find.TickManager.TicksGame <= lastScreamTick + 150f || pawn.IsWorldPawn())
		{
			return;
		}
		Caravan caravan = pawn.GetCaravan();
		if (caravan != null)
		{
			foreach (Pawn item in caravan.PawnsListForReading)
			{
				DoPawnHear(pawn, item);
			}
		}
		else
		{
			GenClamor.DoClamor(pawn, 9.9f, DoPawnHear);
		}
		lastScreamTick = Find.TickManager.TicksGame;
	}

	private void DoPawnHear(Thing source, Pawn hearer)
	{
		if (hearer != source && !alreadyHeard.Contains(hearer))
		{
			alreadyHeard.Add(hearer);
			AuraEffect(source, hearer);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastScreamTick, "lastScreamTick", 0f);
		Scribe_Collections.Look(ref alreadyHeard, "alreadyHeard", LookMode.Reference);
	}
}
