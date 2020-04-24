using System.Collections.Generic;

namespace Verse
{
	public class Effecter
	{
		public EffecterDef def;

		public List<SubEffecter> children = new List<SubEffecter>();

		public Effecter(EffecterDef def)
		{
			this.def = def;
			for (int i = 0; i < def.children.Count; i++)
			{
				children.Add(def.children[i].Spawn(this));
			}
		}

		public void EffectTick(TargetInfo A, TargetInfo B)
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].SubEffectTick(A, B);
			}
		}

		public void Trigger(TargetInfo A, TargetInfo B)
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].SubTrigger(A, B);
			}
		}

		public void Cleanup()
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].SubCleanup();
			}
		}
	}
}
