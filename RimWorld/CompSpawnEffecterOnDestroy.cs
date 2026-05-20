using Verse;

namespace RimWorld
{
	public class CompSpawnEffecterOnDestroy : ThingComp
	{
		private CompProperties_SpawnEffecterOnDestroy Props => (CompProperties_SpawnEffecterOnDestroy)props;

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			Props.effect.Spawn(parent.Position, previousMap);
		}
	}
}
