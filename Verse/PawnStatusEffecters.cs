using System.Collections.Generic;

namespace Verse
{
	internal struct PawnStatusEffecters
	{
		private class LiveEffecter : IFullPoolable
		{
			public EffecterDef def;

			public Effecter effecter;

			public int lastMaintainTick;

			public bool Expired => Find.TickManager.TicksGame > lastMaintainTick;

			public void Cleanup()
			{
				if (effecter != null)
				{
					effecter.Cleanup();
				}
				FullPool<LiveEffecter>.Return(this);
			}

			public void Reset()
			{
				def = null;
				effecter = null;
				lastMaintainTick = -1;
			}

			public void Maintain()
			{
				lastMaintainTick = Find.TickManager.TicksGame;
			}

			public void Tick(Pawn pawn)
			{
				if (effecter == null)
				{
					effecter = def.Spawn();
				}
				effecter.EffectTick(pawn, null);
			}
		}

		public Pawn pawn;

		private List<LiveEffecter> pairs;

		public PawnStatusEffecters(Pawn pawn)
		{
			this.pawn = pawn;
			pairs = new List<LiveEffecter>();
		}

		public void EffectersTick()
		{
			List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				HediffComp_Effecter hediffComp_Effecter = hediffs[i].TryGetComp<HediffComp_Effecter>();
				if (hediffComp_Effecter != null)
				{
					EffecterDef effecterDef = hediffComp_Effecter.CurrentStateEffecter();
					if (effecterDef != null)
					{
						AddOrMaintain(effecterDef);
					}
				}
			}
			if (pawn.mindState.mentalStateHandler.CurState != null)
			{
				EffecterDef effecterDef2 = pawn.mindState.mentalStateHandler.CurState.CurrentStateEffecter();
				if (effecterDef2 != null)
				{
					AddOrMaintain(effecterDef2);
				}
			}
			for (int num = pairs.Count - 1; num >= 0; num--)
			{
				if (pairs[num].Expired)
				{
					pairs[num].Cleanup();
					pairs.RemoveAt(num);
				}
				else
				{
					pairs[num].Tick(pawn);
				}
			}
		}

		private void AddOrMaintain(EffecterDef def)
		{
			for (int i = 0; i < pairs.Count; i++)
			{
				if (pairs[i].def == def)
				{
					pairs[i].Maintain();
					return;
				}
			}
			LiveEffecter liveEffecter = FullPool<LiveEffecter>.Get();
			liveEffecter.def = def;
			liveEffecter.Maintain();
			pairs.Add(liveEffecter);
		}
	}
}
