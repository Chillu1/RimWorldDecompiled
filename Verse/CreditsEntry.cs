using UnityEngine;

namespace Verse
{
	public abstract class CreditsEntry
	{
		public abstract float DrawHeight(float width);

		public abstract void Draw(Rect rect);
	}
}
