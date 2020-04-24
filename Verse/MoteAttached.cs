using RimWorld;
using UnityEngine;

namespace Verse
{
	public class MoteAttached : Mote
	{
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			exactPosition += def.mote.attachedDrawOffset;
		}

		protected override void TimeInterval(float deltaTime)
		{
			base.TimeInterval(deltaTime);
			if (!link1.Linked)
			{
				return;
			}
			if (!link1.Target.ThingDestroyed)
			{
				link1.UpdateDrawPos();
			}
			Vector3 b = def.mote.attachedDrawOffset;
			if (def.mote.attachedToHead)
			{
				Pawn pawn = link1.Target.Thing as Pawn;
				if (pawn != null && pawn.story != null)
				{
					b = pawn.Drawer.renderer.BaseHeadOffsetAt((pawn.GetPosture() == PawnPosture.Standing) ? Rot4.North : pawn.Drawer.renderer.LayingFacing()).RotatedBy(pawn.Drawer.renderer.BodyAngle());
				}
			}
			exactPosition = link1.LastDrawPos + b;
		}
	}
}
