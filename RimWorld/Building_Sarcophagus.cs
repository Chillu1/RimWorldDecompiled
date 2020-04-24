using Verse;

namespace RimWorld
{
	public class Building_Sarcophagus : Building_Grave
	{
		private bool everNonEmpty;

		private bool thisIsFirstBodyEver;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref everNonEmpty, "everNonEmpty", defaultValue: false);
		}

		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				thisIsFirstBodyEver = !everNonEmpty;
				everNonEmpty = true;
				return true;
			}
			return false;
		}

		public override void Notify_CorpseBuried(Pawn worker)
		{
			base.Notify_CorpseBuried(worker);
			if (thisIsFirstBodyEver && worker.IsColonist && base.Corpse.InnerPawn.def.race.Humanlike && !base.Corpse.everBuriedInSarcophagus)
			{
				base.Corpse.everBuriedInSarcophagus = true;
				foreach (Pawn freeColonist in base.Map.mapPawns.FreeColonists)
				{
					if (freeColonist.needs.mood != null)
					{
						freeColonist.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.KnowBuriedInSarcophagus);
					}
				}
			}
		}
	}
}
