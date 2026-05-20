using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class FleckDef : Def
{
	public Type fleckSystemClass;

	public AltitudeLayer altitudeLayer;

	public float altitudeLayerIncOffset;

	public bool drawGUIOverlay;

	public GraphicData graphicData;

	public List<GraphicData> randomGraphics;

	public bool realTime;

	public bool attachedToHead;

	public bool useAttachLink;

	public float fadeInTime;

	public float solidTime = 1f;

	public float fadeOutTime;

	public Vector3 acceleration = Vector3.zero;

	public FloatRange speedPerTime = FloatRange.Zero;

	public float growthRate;

	public List<CurvedScaler> scalers;

	public bool collide;

	public SoundDef landSound;

	public Vector3 unattachedDrawOffset;

	public Vector3 attachedDrawOffset;

	public bool rotateTowardsMoveDirection;

	public float rotateTowardsMoveDirectionExtraAngle;

	public bool drawOffscreen;

	public FloatRange archHeight = FloatRange.Zero;

	public FloatRange archDuration;

	public SimpleCurve archCurve;

	public Vector3 scalingAnchor = Vector3.one * 0.5f;

	public float Lifetime => fadeInTime + solidTime + fadeOutTime;

	public override IEnumerable<string> ConfigErrors()
	{
		if (fleckSystemClass == null)
		{
			yield return "FleckDef without system class type set!";
		}
		else if (!typeof(FleckSystem).IsAssignableFrom(fleckSystemClass))
		{
			yield return "FleckDef has system class type assigned which is not assignable to FleckSystemBase!";
		}
		if (graphicData == null && randomGraphics.NullOrEmpty())
		{
			yield return "Fleck graphic data and random graphics are null!";
		}
		else if (graphicData != null && !typeof(Graphic_Fleck).IsAssignableFrom(graphicData.graphicClass))
		{
			yield return "Fleck graphic class is not derived from Graphic_Fleck!";
		}
		else if (!randomGraphics.NullOrEmpty() && randomGraphics.Any((GraphicData g) => !typeof(Graphic_Fleck).IsAssignableFrom(g.graphicClass)))
		{
			yield return "random fleck graphic class is not derived from Graphic_Fleck!";
		}
	}

	public GraphicData GetGraphicData(int id)
	{
		if (graphicData != null)
		{
			return graphicData;
		}
		Rand.PushState(id);
		try
		{
			return randomGraphics.RandomElement();
		}
		finally
		{
			Rand.PopState();
		}
	}
}
