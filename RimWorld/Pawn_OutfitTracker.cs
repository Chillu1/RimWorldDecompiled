using Verse;

namespace RimWorld
{
	public class Pawn_OutfitTracker : IExposable
	{
		public Pawn pawn;

		private Outfit curOutfit;

		public OutfitForcedHandler forcedHandler = new OutfitForcedHandler();

		public Outfit CurrentOutfit
		{
			get
			{
				if (curOutfit == null)
				{
					curOutfit = Current.Game.outfitDatabase.DefaultOutfit();
				}
				return curOutfit;
			}
			set
			{
				if (curOutfit != value)
				{
					curOutfit = value;
					if (pawn.mindState != null)
					{
						pawn.mindState.Notify_OutfitChanged();
					}
				}
			}
		}

		public Pawn_OutfitTracker()
		{
		}

		public Pawn_OutfitTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref curOutfit, "curOutfit");
			Scribe_Deep.Look(ref forcedHandler, "overrideHandler");
		}
	}
}
