using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public class EffecterDef : Def
	{
		public List<SubEffecterDef> children;

		public float positionRadius;

		public FloatRange offsetTowardsTarget;

		public Effecter Spawn()
		{
			return new Effecter(this);
		}

		public Effecter Spawn(IntVec3 target, Map map, float scale = 1f)
		{
			Effecter effecter = new Effecter(this);
			TargetInfo targetInfo = new TargetInfo(target, map);
			effecter.scale = scale;
			effecter.Trigger(targetInfo, targetInfo);
			return effecter;
		}

		public Effecter Spawn(Thing target, Map map, float scale = 1f)
		{
			Effecter effecter = new Effecter(this);
			effecter.offset = target.TrueCenter() - target.Position.ToVector3Shifted();
			effecter.scale = scale;
			TargetInfo targetInfo = new TargetInfo(target.Position, map);
			effecter.Trigger(targetInfo, targetInfo);
			return effecter;
		}
	}
}
