using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class Section
{
	public IntVec3 botLeft;

	public readonly Map map;

	private CellRect calculatedRect;

	private CellRect bounds;

	public ulong dirtyFlags;

	private bool anyLayerDirty;

	private readonly List<SectionLayer> layers = new List<SectionLayer>();

	private readonly List<SectionLayer_Dynamic> dynamic = new List<SectionLayer_Dynamic>();

	public const int Size = 17;

	public CellRect Bounds => bounds;

	public CellRect CellRect
	{
		get
		{
			if (calculatedRect == default(CellRect))
			{
				calculatedRect = new CellRect(botLeft.x, botLeft.z, 17, 17);
				calculatedRect.ClipInsideMap(map);
			}
			return calculatedRect;
		}
	}

	public Section(IntVec3 sectCoords, Map map)
	{
		botLeft = sectCoords * 17;
		this.map = map;
		bool flag = map.info.disableSunShadows || map.Biome.disableShadows;
		bool disableShadows = map.Biome.disableShadows;
		foreach (Type item2 in typeof(SectionLayer).AllSubclassesNonAbstract())
		{
			if ((!flag || !typeof(SectionLayer_SunShadows).IsAssignableFrom(item2)) && (!disableShadows || !typeof(SectionLayer_EdgeShadows).IsAssignableFrom(item2)) && (!(item2 == typeof(SectionLayer_PollutionCloud)) || ModsConfig.BiotechActive))
			{
				SectionLayer sectionLayer = (SectionLayer)Activator.CreateInstance(item2, this);
				layers.Add(sectionLayer);
				if (sectionLayer is SectionLayer_Dynamic item)
				{
					dynamic.Add(item);
				}
			}
		}
	}

	public void DrawSection()
	{
		if (anyLayerDirty)
		{
			RegenerateDirtyLayers();
		}
		for (int i = 0; i < layers.Count; i++)
		{
			layers[i].DrawLayer();
		}
		if (DebugViewSettings.drawSectionEdges)
		{
			Vector3 vector = botLeft.ToVector3();
			GenDraw.DrawLineBetween(vector, vector + new Vector3(0f, 0f, 17f));
			GenDraw.DrawLineBetween(vector, vector + new Vector3(17f, 0f, 0f));
			if (CellRect.Contains(UI.MouseCell()))
			{
				Vector3 vector2 = bounds.Min.ToVector3();
				Vector3 vector3 = bounds.Max.ToVector3() + new Vector3(1f, 0f, 1f);
				GenDraw.DrawLineBetween(vector2, vector2 + new Vector3(bounds.Width, 0f, 0f), SimpleColor.Magenta);
				GenDraw.DrawLineBetween(vector2, vector2 + new Vector3(0f, 0f, bounds.Height), SimpleColor.Magenta);
				GenDraw.DrawLineBetween(vector3, vector3 - new Vector3(bounds.Width, 0f, 0f), SimpleColor.Magenta);
				GenDraw.DrawLineBetween(vector3, vector3 - new Vector3(0f, 0f, bounds.Height), SimpleColor.Magenta);
			}
		}
	}

	public void DrawDynamicSections(CellRect view)
	{
		for (int i = 0; i < dynamic.Count; i++)
		{
			if (dynamic[i].ShouldDrawDynamic(view))
			{
				dynamic[i].DrawLayer();
			}
		}
	}

	public void RegenerateSingleLayer(SectionLayer layer)
	{
		if (!layer.Visible)
		{
			return;
		}
		try
		{
			layer.Regenerate();
			if (layer.Isnt<SectionLayer_Dynamic>())
			{
				bounds = bounds.Encapsulate(layer.GetBoundaryRect());
			}
		}
		catch (Exception arg)
		{
			Log.Error($"Could not regenerate layer {layer.ToStringSafe()}: {arg}");
		}
		layer.RefreshSubMeshBounds();
	}

	public void RegenerateAllLayers()
	{
		bounds = CellRect;
		for (int i = 0; i < layers.Count; i++)
		{
			if (!layers[i].Visible)
			{
				continue;
			}
			try
			{
				layers[i].Regenerate();
				if (layers[i].Isnt<SectionLayer_Dynamic>())
				{
					bounds = bounds.Encapsulate(layers[i].GetBoundaryRect());
				}
			}
			catch (Exception arg)
			{
				Log.Error($"Could not regenerate layer {layers[i].ToStringSafe()}: {arg}");
			}
		}
		for (int j = 0; j < layers.Count; j++)
		{
			layers[j].RefreshSubMeshBounds();
		}
	}

	private void RegenerateDirtyLayers()
	{
		bounds = CellRect;
		anyLayerDirty = false;
		for (int i = 0; i < layers.Count; i++)
		{
			if (!layers[i].Visible)
			{
				continue;
			}
			if (layers[i].Dirty)
			{
				try
				{
					layers[i].Regenerate();
					if (layers[i].Isnt<SectionLayer_Dynamic>())
					{
						bounds = bounds.Encapsulate(layers[i].GetBoundaryRect());
					}
				}
				catch (Exception arg)
				{
					Log.Error($"Could not regenerate layer {layers[i].ToStringSafe()}: {arg}");
				}
			}
			else if (layers[i].Isnt<SectionLayer_Dynamic>())
			{
				bounds = bounds.Encapsulate(layers[i].GetBoundaryRect());
			}
		}
		for (int j = 0; j < layers.Count; j++)
		{
			layers[j].RefreshSubMeshBounds();
		}
	}

	public bool TryUpdate(CellRect view)
	{
		if (dirtyFlags == 0L)
		{
			return false;
		}
		bounds = CellRect;
		bool flag = false;
		bool flag2 = bounds.Overlaps(view);
		bool result = false;
		for (int i = 0; i < layers.Count; i++)
		{
			SectionLayer sectionLayer = layers[i];
			sectionLayer.Dirty = sectionLayer.Dirty || (dirtyFlags & sectionLayer.relevantChangeTypes) != 0;
			if (!sectionLayer.Dirty)
			{
				continue;
			}
			if (!flag2)
			{
				flag = flag || sectionLayer.Dirty;
				continue;
			}
			try
			{
				sectionLayer.Regenerate();
			}
			catch (Exception arg)
			{
				Log.Error($"Could not regenerate layer {sectionLayer.ToStringSafe()}: {arg}");
			}
			finally
			{
			}
			result = true;
			sectionLayer.Dirty = false;
		}
		for (int j = 0; j < layers.Count; j++)
		{
			if (layers[j].Isnt<SectionLayer_Dynamic>())
			{
				bounds = bounds.Encapsulate(layers[j].GetBoundaryRect());
			}
		}
		for (int k = 0; k < layers.Count; k++)
		{
			layers[k].RefreshSubMeshBounds();
		}
		anyLayerDirty = flag;
		dirtyFlags = 0uL;
		return result;
	}

	public SectionLayer GetLayer(Type type)
	{
		return layers.FirstOrDefault((SectionLayer sect) => sect.GetType() == type);
	}

	public void Dispose()
	{
		for (int i = 0; i < layers.Count; i++)
		{
			layers[i].Dispose();
		}
		for (int j = 0; j < dynamic.Count; j++)
		{
			dynamic[j].Dispose();
		}
		layers.Clear();
		dynamic.Clear();
	}
}
