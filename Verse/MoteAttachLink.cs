using UnityEngine;

namespace Verse
{
	public struct MoteAttachLink
	{
		private TargetInfo targetInt;

		private Vector3 lastDrawPosInt;

		public bool Linked => targetInt.IsValid;

		public TargetInfo Target => targetInt;

		public Vector3 LastDrawPos => lastDrawPosInt;

		public static MoteAttachLink Invalid => new MoteAttachLink(TargetInfo.Invalid);

		public MoteAttachLink(TargetInfo target)
		{
			targetInt = target;
			lastDrawPosInt = Vector3.zero;
			if (target.IsValid)
			{
				UpdateDrawPos();
			}
		}

		public void UpdateDrawPos()
		{
			if (targetInt.HasThing)
			{
				lastDrawPosInt = targetInt.Thing.DrawPos;
			}
			else
			{
				lastDrawPosInt = targetInt.Cell.ToVector3Shifted();
			}
		}
	}
}
