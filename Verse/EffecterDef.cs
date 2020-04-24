using System.Collections.Generic;

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
	}
}
