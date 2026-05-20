using Verse;

namespace RimWorld
{
	public class CompMoteEmitterRandomColor : CompMoteEmitter
	{
		public CompProperties_MoteEmitterRandomColor Props => (CompProperties_MoteEmitterRandomColor)props;

		public override void Emit()
		{
			base.Emit();
			mote.instanceColor = Props.colors.RandomElement();
		}
	}
}
