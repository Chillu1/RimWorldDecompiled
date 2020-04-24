using Verse;

namespace RimWorld
{
	public abstract class SketchBuildable : SketchEntity
	{
		public abstract BuildableDef Buildable
		{
			get;
		}

		public abstract ThingDef Stuff
		{
			get;
		}

		public override string Label => GenLabel.ThingLabel(Buildable, Stuff);

		public override bool LostImportantReferences => Buildable == null;

		public abstract Thing GetSpawnedBlueprintOrFrame(IntVec3 at, Map map);

		public override bool IsSameSpawnedOrBlueprintOrFrame(IntVec3 at, Map map)
		{
			if (!at.InBounds(map))
			{
				return false;
			}
			if (IsSameSpawned(at, map))
			{
				return true;
			}
			return GetSpawnedBlueprintOrFrame(at, map) != null;
		}
	}
}
