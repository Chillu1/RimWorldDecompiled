using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompRitualEffect_IntervalSpawnDividedCircleEffecter : CompRitualEffect_IntervalSpawnBurst
	{
		protected new CompProperties_RitualEffectIntervalSpawnDividedCircle Props => (CompProperties_RitualEffectIntervalSpawnDividedCircle)props;

		protected override Vector3 SpawnPos(LordJob_Ritual ritual)
		{
			return Vector3.zero;
		}

		public override void OnSetup(RitualVisualEffect parent, LordJob_Ritual ritual, bool loading)
		{
			base.parent = parent;
			float num = 360f / (float)Props.numCopies;
			for (int i = 0; i < Props.numCopies; i++)
			{
				Vector3 vector = Quaternion.AngleAxis(num * (float)i, Vector3.up) * Vector3.forward;
				IntVec3 cell = parent.ritual.selectedTarget.Cell;
				TargetInfo targetInfo = new TargetInfo(cell, parent.ritual.Map);
				Vector3 vector2 = (vector * Props.radius + Props.offset) * ScaleForRoom(ritual);
				Room room = (cell + vector2.ToIntVec3() + Props.roomCheckOffset).GetRoom(ritual.Map);
				if (!props.onlySpawnInSameRoom || room == ritual.GetRoom)
				{
					Effecter effecter = Props.effecterDef.Spawn(cell, parent.ritual.Map, vector2);
					effecter.Trigger(targetInfo, TargetInfo.Invalid);
					parent.AddEffecterToMaintain(targetInfo, effecter);
				}
			}
		}
	}
}
