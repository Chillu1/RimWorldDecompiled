using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class PawnTextureAtlas
	{
		private RenderTexture texture;

		private Dictionary<Pawn, PawnTextureAtlasFrameSet> frameAssignments = new Dictionary<Pawn, PawnTextureAtlasFrameSet>();

		private List<PawnTextureAtlasFrameSet> freeFrameSets = new List<PawnTextureAtlasFrameSet>();

		private static List<Pawn> tmpPawnsToFree = new List<Pawn>();

		private const int AtlasSize = 2048;

		public const int FrameSize = 128;

		private const int PawnsHeldPerAtlas = 32;

		private const int FramesPerPawn = 8;

		public RenderTexture RawTexture => texture;

		public int FreeCount => freeFrameSets.Count;

		public PawnTextureAtlas()
		{
			texture = new RenderTexture(2048, 2048, 24, RenderTextureFormat.ARGB32, 0);
			texture.name = "PawnTextureAtlas_" + 2048;
			List<Rect> list = new List<Rect>();
			for (int i = 0; i < 2048; i += 128)
			{
				for (int j = 0; j < 2048; j += 128)
				{
					list.Add(new Rect((float)i / 2048f, (float)j / 2048f, 0.0625f, 0.0625f));
				}
			}
			while (list.Count >= 8)
			{
				PawnTextureAtlasFrameSet pawnTextureAtlasFrameSet = new PawnTextureAtlasFrameSet();
				pawnTextureAtlasFrameSet.uvRects = new Rect[8]
				{
					list.Pop(),
					list.Pop(),
					list.Pop(),
					list.Pop(),
					list.Pop(),
					list.Pop(),
					list.Pop(),
					list.Pop()
				};
				pawnTextureAtlasFrameSet.meshes = pawnTextureAtlasFrameSet.uvRects.Select((Rect u) => TextureAtlasHelper.CreateMeshForUV(u)).ToArray();
				pawnTextureAtlasFrameSet.atlas = texture;
				freeFrameSets.Add(pawnTextureAtlasFrameSet);
			}
		}

		public bool ContainsFrameSet(Pawn pawn)
		{
			return frameAssignments.ContainsKey(pawn);
		}

		public bool TryGetFrameSet(Pawn pawn, out PawnTextureAtlasFrameSet frameSet, out bool createdNew)
		{
			createdNew = false;
			if (!frameAssignments.TryGetValue(pawn, out frameSet))
			{
				if (FreeCount == 0)
				{
					return false;
				}
				createdNew = true;
				frameSet = freeFrameSets.Pop();
				for (int i = 0; i < frameSet.isDirty.Length; i++)
				{
					frameSet.isDirty[i] = true;
				}
				frameAssignments.Add(pawn, frameSet);
				return true;
			}
			return true;
		}

		public void GC()
		{
			try
			{
				foreach (Pawn key in frameAssignments.Keys)
				{
					if (!key.SpawnedOrAnyParentSpawned)
					{
						tmpPawnsToFree.Add(key);
					}
				}
				foreach (Pawn item in tmpPawnsToFree)
				{
					freeFrameSets.Add(frameAssignments[item]);
					frameAssignments.Remove(item);
				}
			}
			finally
			{
				tmpPawnsToFree.Clear();
			}
		}

		public void Destroy()
		{
			foreach (PawnTextureAtlasFrameSet item in frameAssignments.Values.Concat(freeFrameSets))
			{
				Mesh[] meshes = item.meshes;
				for (int i = 0; i < meshes.Length; i++)
				{
					Object.Destroy(meshes[i]);
				}
			}
			frameAssignments.Clear();
			freeFrameSets.Clear();
			Object.Destroy(texture);
		}
	}
}
