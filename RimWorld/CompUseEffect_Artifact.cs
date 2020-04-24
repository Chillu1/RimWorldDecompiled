using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompUseEffect_Artifact : CompUseEffect
	{
		public CompProperties_UseEffectArtifact Props => (CompProperties_UseEffectArtifact)props;

		public override void DoEffect(Pawn usedBy)
		{
			base.DoEffect(usedBy);
			if (Props.sound != null)
			{
				Props.sound.PlayOneShot(new TargetInfo(parent.Position, usedBy.MapHeld));
			}
			usedBy.records.Increment(RecordDefOf.ArtifactsActivated);
		}
	}
}
