using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_GuiltTracker : IExposable
	{
		private Pawn pawn;

		public int lastGuiltyTick = -99999;

		private const int GuiltyDuration = 60000;

		public bool IsGuilty
		{
			get
			{
				if (TicksUntilInnocent <= 0)
				{
					return pawn.InAggroMentalState;
				}
				return true;
			}
		}

		public int TicksUntilInnocent => Mathf.Max(0, lastGuiltyTick + 60000 - Find.TickManager.TicksGame);

		public Pawn_GuiltTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref lastGuiltyTick, "lastGuiltyTick", -99999);
		}

		public void Notify_Guilty()
		{
			lastGuiltyTick = Find.TickManager.TicksGame;
		}
	}
}
