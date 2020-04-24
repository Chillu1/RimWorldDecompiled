using UnityEngine;

namespace Verse
{
	public class MoteDualAttached : Mote
	{
		protected MoteAttachLink link2 = MoteAttachLink.Invalid;

		public void Attach(TargetInfo a, TargetInfo b)
		{
			link1 = new MoteAttachLink(a);
			link2 = new MoteAttachLink(b);
		}

		public override void Draw()
		{
			UpdatePositionAndRotation();
			base.Draw();
		}

		protected void UpdatePositionAndRotation()
		{
			if (link1.Linked)
			{
				if (link2.Linked)
				{
					if (!link1.Target.ThingDestroyed)
					{
						link1.UpdateDrawPos();
					}
					if (!link2.Target.ThingDestroyed)
					{
						link2.UpdateDrawPos();
					}
					exactPosition = (link1.LastDrawPos + link2.LastDrawPos) * 0.5f;
					if (def.mote.rotateTowardsTarget)
					{
						exactRotation = link1.LastDrawPos.AngleToFlat(link2.LastDrawPos) + 90f;
					}
					if (def.mote.scaleToConnectTargets)
					{
						exactScale = new Vector3(def.graphicData.drawSize.y, 1f, (link2.LastDrawPos - link1.LastDrawPos).MagnitudeHorizontal());
					}
				}
				else
				{
					if (!link1.Target.ThingDestroyed)
					{
						link1.UpdateDrawPos();
					}
					exactPosition = link1.LastDrawPos + def.mote.attachedDrawOffset;
				}
			}
			exactPosition.y = def.altitudeLayer.AltitudeFor();
		}
	}
}
