using UnityEngine;

namespace Verse
{
	public class PawnTextureAtlasFrameSet
	{
		public RenderTexture atlas;

		public Rect[] uvRects = new Rect[8];

		public Mesh[] meshes = new Mesh[8];

		public bool[] isDirty = new bool[8] { true, true, true, true, true, true, true, true };

		public int GetIndex(Rot4 rotation, PawnDrawMode drawMode)
		{
			if (drawMode == PawnDrawMode.BodyAndHead)
			{
				return rotation.AsInt;
			}
			return 4 + rotation.AsInt;
		}
	}
}
