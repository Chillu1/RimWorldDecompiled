using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class Effecter
	{
		public EffecterDef def;

		public List<SubEffecter> children = new List<SubEffecter>();

		public int ticksLeft = -1;

		public Vector3 offset;

		public float scale = 1f;

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
