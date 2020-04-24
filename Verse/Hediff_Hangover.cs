using RimWorld;

namespace Verse
{
	public class Hediff_Hangover : HediffWithComps
	{
		public override bool Visible
		{
			get
			{
				if (pawn.health.hediffSet.HasHediff(HediffDefOf.AlcoholHigh))
				{
					return false;
				}
				return base.Visible;
			}
		}
	}
}
