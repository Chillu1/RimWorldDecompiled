using Verse;

namespace RimWorld
{
	public class CompProperties_SmokeCloudMaker : CompProperties
	{
		public EffecterDef sourceStreamEffect;

		public float cloudRadius;

		public FleckDef cloudFleck;

		public float fleckScale = 1f;

		public float fleckSpawnMTB;

		public CompProperties_SmokeCloudMaker()
		{
			compClass = typeof(CompSmokeCloudMaker);
		}
	}
}
