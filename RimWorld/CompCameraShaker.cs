using Verse;

namespace RimWorld
{
	public class CompCameraShaker : ThingComp
	{
		public CompProperties_CameraShaker Props => (CompProperties_CameraShaker)props;

		public override void CompTick()
		{
			base.CompTick();
			if (parent.Spawned && parent.Map == Find.CurrentMap)
			{
				Find.CameraDriver.shaker.SetMinShake(Props.mag);
			}
		}
	}
}
