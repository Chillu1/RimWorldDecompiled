using UnityEngine;

namespace Verse
{
	public class CreditRecord_Space : CreditsEntry
	{
		private float height = 10f;

		public CreditRecord_Space()
		{
		}

		public CreditRecord_Space(float height)
		{
			this.height = height;
		}

		public override float DrawHeight(float width)
		{
			return height;
		}

		public override void Draw(Rect rect)
		{
		}
	}
}
