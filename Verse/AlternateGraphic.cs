using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class AlternateGraphic
{
	private float weight = 0.5f;

	private string texPath;

	private string rottingTexPath;

	private string dessicatedTexPath;

	private string swimmingTexPath;

	private string stationaryTexPath;

	private Color? color;

	private Color? colorTwo;

	public GraphicData graphicData;

	public GraphicData dessicatedGraphicData;

	public GraphicData swimmingGraphicData;

	public GraphicData rottingGraphicData;

	public GraphicData stationaryGraphicData;

	public List<AttachPoint> attachPoints;

	public float Weight => weight;

	public Graphic GetGraphic(Graphic other)
	{
		if (graphicData == null)
		{
			graphicData = new GraphicData();
		}
		graphicData.CopyFrom(other.data);
		if (!texPath.NullOrEmpty())
		{
			graphicData.texPath = texPath;
		}
		graphicData.color = color ?? other.color;
		graphicData.colorTwo = colorTwo ?? other.colorTwo;
		return graphicData.Graphic;
	}

	public Graphic GetSwimmingGraphic(Graphic other)
	{
		if (swimmingGraphicData == null)
		{
			swimmingGraphicData = new GraphicData();
		}
		swimmingGraphicData.CopyFrom(other.data);
		if (!swimmingTexPath.NullOrEmpty())
		{
			swimmingGraphicData.texPath = swimmingTexPath;
		}
		return swimmingGraphicData.Graphic;
	}

	public Graphic GetDessicatedGraphic(Graphic other)
	{
		if (dessicatedGraphicData == null)
		{
			dessicatedGraphicData = new GraphicData();
		}
		dessicatedGraphicData.CopyFrom(other.data);
		if (!dessicatedTexPath.NullOrEmpty())
		{
			dessicatedGraphicData.texPath = dessicatedTexPath;
		}
		return dessicatedGraphicData.Graphic;
	}

	public Graphic GetRottingGraphic(Graphic other)
	{
		if (rottingGraphicData == null)
		{
			rottingGraphicData = new GraphicData();
		}
		rottingGraphicData.CopyFrom(other.data);
		if (!rottingTexPath.NullOrEmpty())
		{
			rottingGraphicData.texPath = rottingTexPath;
		}
		return rottingGraphicData.Graphic;
	}

	public Graphic GetStationaryGraphic(Graphic other)
	{
		if (stationaryGraphicData == null)
		{
			stationaryGraphicData = new GraphicData();
		}
		stationaryGraphicData.CopyFrom(other.data);
		if (!stationaryTexPath.NullOrEmpty())
		{
			stationaryGraphicData.texPath = stationaryTexPath;
		}
		return stationaryGraphicData.Graphic;
	}
}
