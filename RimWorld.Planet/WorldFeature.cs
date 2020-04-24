using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldFeature : IExposable, ILoadReferenceable
	{
		public int uniqueID;

		public FeatureDef def;

		public string name;

		public Vector3 drawCenter;

		public float drawAngle;

		public float maxDrawSizeInTiles;

		public float alpha;

		protected static SimpleCurve EffectiveDrawSizeCurve = new SimpleCurve
		{
			new CurvePoint(10f, 15f),
			new CurvePoint(25f, 40f),
			new CurvePoint(50f, 90f),
			new CurvePoint(100f, 150f),
			new CurvePoint(200f, 200f)
		};

		[TweakValue("Interface.World", 0f, 40f)]
		protected static float FeatureSizePoint10 = 15f;

		[TweakValue("Interface.World", 0f, 100f)]
		protected static float FeatureSizePoint25 = 40f;

		[TweakValue("Interface.World", 0f, 200f)]
		protected static float FeatureSizePoint50 = 90f;

		[TweakValue("Interface.World", 0f, 400f)]
		protected static float FeatureSizePoint100 = 150f;

		[TweakValue("Interface.World", 0f, 800f)]
		protected static float FeatureSizePoint200 = 200f;

		public float EffectiveDrawSize => EffectiveDrawSizeCurve.Evaluate(maxDrawSizeInTiles);

		public IEnumerable<int> Tiles
		{
			get
			{
				WorldGrid worldGrid = Find.WorldGrid;
				int tilesCount = worldGrid.TilesCount;
				for (int i = 0; i < tilesCount; i++)
				{
					if (worldGrid[i].feature == this)
					{
						yield return i;
					}
				}
			}
		}

		protected static void FeatureSizePoint10_Changed()
		{
			TweakChanged();
		}

		protected static void FeatureSizePoint25_Changed()
		{
			TweakChanged();
		}

		protected static void FeatureSizePoint50_Changed()
		{
			TweakChanged();
		}

		protected static void FeatureSizePoint100_Changed()
		{
			TweakChanged();
		}

		protected static void FeatureSizePoint200_Changed()
		{
			TweakChanged();
		}

		private static void TweakChanged()
		{
			Find.WorldFeatures.textsCreated = false;
			EffectiveDrawSizeCurve[0] = new CurvePoint(EffectiveDrawSizeCurve[0].x, FeatureSizePoint10);
			EffectiveDrawSizeCurve[1] = new CurvePoint(EffectiveDrawSizeCurve[1].x, FeatureSizePoint25);
			EffectiveDrawSizeCurve[2] = new CurvePoint(EffectiveDrawSizeCurve[2].x, FeatureSizePoint50);
			EffectiveDrawSizeCurve[3] = new CurvePoint(EffectiveDrawSizeCurve[3].x, FeatureSizePoint100);
			EffectiveDrawSizeCurve[4] = new CurvePoint(EffectiveDrawSizeCurve[4].x, FeatureSizePoint200);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref uniqueID, "uniqueID", 0);
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref name, "name");
			Scribe_Values.Look(ref drawCenter, "drawCenter");
			Scribe_Values.Look(ref drawAngle, "drawAngle", 0f);
			Scribe_Values.Look(ref maxDrawSizeInTiles, "maxDrawSizeInTiles", 0f);
			BackCompatibility.PostExposeData(this);
		}

		public string GetUniqueLoadID()
		{
			return "WorldFeature_" + uniqueID;
		}
	}
}
