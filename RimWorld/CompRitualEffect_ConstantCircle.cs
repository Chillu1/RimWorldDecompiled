using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompRitualEffect_ConstantCircle : CompRitualEffect_Constant
	{
		protected CompProperties_RitualEffectConstantCircle Props => (CompProperties_RitualEffectConstantCircle)props;

		protected override Vector3 SpawnPos(LordJob_Ritual ritual)
		{
			return Vector3.zero;
		}

		public override void OnSetup(RitualVisualEffect parent, LordJob_Ritual ritual, bool loading)
		{
			base.parent = parent;
			Spawn(ritual);
		}

		protected override void Spawn(LordJob_Ritual ritual)
		{
			float num = 360f / (float)Props.numCopies;
			for (int i = 0; i < Props.numCopies; i++)
			{
				Vector3 vector = Quaternion.AngleAxis(num * (float)i, Vector3.up) * Vector3.forward;
				Mote mote = SpawnMote(ritual, ritual.selectedTarget.Cell.ToVector3Shifted() + vector * Props.radius);
				if (mote != null)
				{
					parent.AddMoteToMaintain(mote);
					if (props.colorOverride.HasValue)
					{
						mote.instanceColor = props.colorOverride.Value;
					}
					else
					{
						mote.instanceColor = parent.def.tintColor;
					}
				}
			}
			spawned = true;
		}
	}
}
