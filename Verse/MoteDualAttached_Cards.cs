using UnityEngine;

namespace Verse;

public class MoteDualAttached_Cards : MoteDualAttached
{
	protected override void DrawAt(Vector3 drawLoc, bool flip = false)
	{
		if ((!link1.Linked || !link1.Target.HasThing || !(link2.Target.Thing is Pawn pawn) || !(pawn.Rotation == Rot4.North)) && (!link2.Linked || !link2.Target.HasThing || !(link2.Target.Thing is Pawn pawn2) || !(pawn2.Rotation == Rot4.North)))
		{
			base.DrawAt(drawLoc, flip);
		}
	}
}
