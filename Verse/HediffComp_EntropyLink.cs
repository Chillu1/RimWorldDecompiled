using RimWorld;

namespace Verse
{
	public class HediffComp_EntropyLink : HediffComp
	{
		public HediffCompProperties_EntropyLink Props => (HediffCompProperties_EntropyLink)props;

		public override void Notify_EntropyGained(float baseAmount, float finalAmount, Thing source = null)
		{
			base.Notify_EntropyGained(baseAmount, finalAmount, source);
			HediffComp_Link hediffComp_Link = parent.TryGetComp<HediffComp_Link>();
			if (hediffComp_Link != null && hediffComp_Link.other != source && hediffComp_Link.other.psychicEntropy != null)
			{
				hediffComp_Link.other.psychicEntropy.TryAddEntropy(baseAmount * Props.entropyTransferAmount, parent.pawn, scale: false);
				MoteMaker.MakeInteractionOverlay(ThingDefOf.Mote_PsychicLinkPulse, parent.pawn, hediffComp_Link.other);
			}
		}
	}
}
