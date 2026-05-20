using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompRitualEffect_StaticAreaRandomMote : CompRitualEffect_Constant
	{
		protected CompProperties_RitualEffectStaticAreaRandomMote Props => (CompProperties_RitualEffectStaticAreaRandomMote)props;

		protected override ThingDef MoteDef => Props.moteDefs.RandomElement();

		protected override Vector3 SpawnPos(LordJob_Ritual ritual)
		{
			return Vector3.zero;
		}

		public override void OnSetup(RitualVisualEffect parent, LordJob_Ritual ritual, bool loading)
		{
			base.parent = parent;
			CellRect cellRect = CellRect.CenteredOn(ritual.selectedTarget.Cell, Props.area.x / 2, Props.area.z / 2).ClipInsideMap(ritual.Map);
			List<IntVec3> list = new List<IntVec3>();
			for (int i = 0; i < Props.spawnCount; i++)
			{
				IntVec3 pos = IntVec3.Invalid;
				for (int j = 0; j < 15; j++)
				{
					pos = cellRect.RandomCell;
					if (!list.Any((IntVec3 c) => c.InHorDistOf(pos, Props.minDist)))
					{
						break;
					}
				}
				if (pos.IsValid)
				{
					Mote mote = SpawnMote(ritual, pos.ToVector3Shifted() + Props.offset);
					if (mote != null)
					{
						parent.AddMoteToMaintain(mote);
					}
					list.Add(pos);
				}
			}
		}
	}
}
