using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class Section
	{
		public IntVec3 botLeft;

		public Map map;

		public MapMeshFlag dirtyFlags;

		private List<SectionLayer> layers = new List<SectionLayer>();

		private bool foundRect;

		private CellRect calculatedRect;

		public const int Size = 17;

		public CellRect CellRect
		{
			get
			{
				if (!foundRect)
				{
					calculatedRect = new CellRect(botLeft.x, botLeft.z, 17, 17);
					calculatedRect.ClipInsideMap(map);
					foundRect = true;
				}
				return calculatedRect;
			}
		}

		public Section(IntVec3 sectCoords, Map map)
		{
			botLeft = sectCoords * 17;
			this.map = map;
			foreach (Type item in typeof(SectionLayer).AllSubclassesNonAbstract())
			{
				layers.Add((SectionLayer)Activator.CreateInstance(item, this));
			}
		}

		public void DrawSection(bool drawSunShadowsOnly)
		{
			int count = layers.Count;
			for (int i = 0; i < count; i++)
			{
				if (!drawSunShadowsOnly || layers[i] is SectionLayer_SunShadows)
				{
					layers[i].DrawLayer();
				}
			}
			if (!drawSunShadowsOnly && DebugViewSettings.drawSectionEdges)
			{
				GenDraw.DrawLineBetween(botLeft.ToVector3(), botLeft.ToVector3() + new Vector3(0f, 0f, 17f));
				GenDraw.DrawLineBetween(botLeft.ToVector3(), botLeft.ToVector3() + new Vector3(17f, 0f, 0f));
			}
		}

		public void RegenerateAllLayers()
		{
			for (int i = 0; i < layers.Count; i++)
			{
				if (layers[i].Visible)
				{
					try
					{
						layers[i].Regenerate();
					}
					catch (Exception ex)
					{
						Log.Error("Could not regenerate layer " + layers[i].ToStringSafe() + ": " + ex);
					}
				}
			}
		}

		public void RegenerateLayers(MapMeshFlag changeType)
		{
			for (int i = 0; i < layers.Count; i++)
			{
				SectionLayer sectionLayer = layers[i];
				if ((sectionLayer.relevantChangeTypes & changeType) != 0)
				{
					try
					{
						sectionLayer.Regenerate();
					}
					catch (Exception ex)
					{
						Log.Error("Could not regenerate layer " + sectionLayer.ToStringSafe() + ": " + ex);
					}
				}
			}
		}

		public SectionLayer GetLayer(Type type)
		{
			return layers.Where((SectionLayer sect) => sect.GetType() == type).FirstOrDefault();
		}
	}
}
