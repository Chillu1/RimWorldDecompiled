using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class DamageGraphicData
	{
		public bool enabled = true;

		public Rect rectN;

		public Rect rectE;

		public Rect rectS;

		public Rect rectW;

		public Rect rect;

		[NoTranslate]
		public List<string> scratches;

		[NoTranslate]
		public string cornerTL;

		[NoTranslate]
		public string cornerTR;

		[NoTranslate]
		public string cornerBL;

		[NoTranslate]
		public string cornerBR;

		[NoTranslate]
		public string edgeLeft;

		[NoTranslate]
		public string edgeRight;

		[NoTranslate]
		public string edgeTop;

		[NoTranslate]
		public string edgeBot;

		[Unsaved(false)]
		public List<Material> scratchMats;

		[Unsaved(false)]
		public Material cornerTLMat;

		[Unsaved(false)]
		public Material cornerTRMat;

		[Unsaved(false)]
		public Material cornerBLMat;

		[Unsaved(false)]
		public Material cornerBRMat;

		[Unsaved(false)]
		public Material edgeLeftMat;

		[Unsaved(false)]
		public Material edgeRightMat;

		[Unsaved(false)]
		public Material edgeTopMat;

		[Unsaved(false)]
		public Material edgeBotMat;

		public void ResolveReferencesSpecial()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				if (scratches != null)
				{
					scratchMats = new List<Material>();
					for (int i = 0; i < scratches.Count; i++)
					{
						scratchMats[i] = MaterialPool.MatFrom(scratches[i], ShaderDatabase.Transparent);
					}
				}
				if (cornerTL != null)
				{
					cornerTLMat = MaterialPool.MatFrom(cornerTL, ShaderDatabase.Transparent);
				}
				if (cornerTR != null)
				{
					cornerTRMat = MaterialPool.MatFrom(cornerTR, ShaderDatabase.Transparent);
				}
				if (cornerBL != null)
				{
					cornerBLMat = MaterialPool.MatFrom(cornerBL, ShaderDatabase.Transparent);
				}
				if (cornerBR != null)
				{
					cornerBRMat = MaterialPool.MatFrom(cornerBR, ShaderDatabase.Transparent);
				}
				if (edgeTop != null)
				{
					edgeTopMat = MaterialPool.MatFrom(edgeTop, ShaderDatabase.Transparent);
				}
				if (edgeBot != null)
				{
					edgeBotMat = MaterialPool.MatFrom(edgeBot, ShaderDatabase.Transparent);
				}
				if (edgeLeft != null)
				{
					edgeLeftMat = MaterialPool.MatFrom(edgeLeft, ShaderDatabase.Transparent);
				}
				if (edgeRight != null)
				{
					edgeRightMat = MaterialPool.MatFrom(edgeRight, ShaderDatabase.Transparent);
				}
			});
		}
	}
}
