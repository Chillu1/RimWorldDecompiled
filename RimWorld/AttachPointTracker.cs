using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class AttachPointTracker
{
	private ThingWithComps parent;

	private Dictionary<AttachPointType, AttachPoint> cache;

	private bool didReadOverrides;

	public string ThingId => parent.ThingID;

	public AttachPointTracker(List<AttachPoint> points, ThingWithComps parent)
	{
		this.parent = parent;
		cache = new Dictionary<AttachPointType, AttachPoint>();
		foreach (AttachPoint point in points)
		{
			cache[point.type] = point;
		}
		cache[AttachPointType.RootNone] = new AttachPoint
		{
			offset = Vector3.zero,
			type = AttachPointType.RootNone
		};
	}

	public void Add(AttachPointTracker other, bool overwrite = true)
	{
		foreach (KeyValuePair<AttachPointType, AttachPoint> item in other.cache)
		{
			if (overwrite || !cache.ContainsKey(item.Key))
			{
				cache[item.Key] = item.Value;
			}
		}
	}

	private void TryCacheKindDefOverrides()
	{
		if (didReadOverrides || !(parent is Pawn pawn))
		{
			return;
		}
		if (pawn.TryGetAlternate(out var ag, out var _) && ag.attachPoints != null)
		{
			foreach (AttachPoint attachPoint in ag.attachPoints)
			{
				cache[attachPoint.type] = attachPoint;
			}
		}
		GraphicData graphicData = pawn.Drawer.renderer.BodyGraphic?.data;
		if (graphicData == null)
		{
			return;
		}
		if (graphicData.attachPoints != null && graphicData.attachPoints.Count > 0)
		{
			foreach (AttachPoint attachPoint2 in graphicData.attachPoints)
			{
				cache[attachPoint2.type] = attachPoint2;
			}
		}
		didReadOverrides = true;
	}

	public Vector3 GetRotatedOffset(AttachPointType type)
	{
		return GetRotatedOffset(type, parent.Rotation);
	}

	public Vector3 GetRotatedOffset(AttachPointType type, Rot4 rot)
	{
		Vector3 offset = GetPointOrDefault(type).offset;
		if (parent is Pawn pawn)
		{
			offset *= pawn.ageTracker.CurLifeStage.attachPointScaleFactor;
		}
		return offset.RotatedBy(rot);
	}

	public Vector3 GetWorldPos(AttachPointType type)
	{
		return parent.DrawPos + GetRotatedOffset(type);
	}

	public IEnumerable<AttachPointType> PointTypes(int min = 0, int max = int.MaxValue)
	{
		TryCacheKindDefOverrides();
		foreach (KeyValuePair<AttachPointType, AttachPoint> item in cache)
		{
			int key = (int)item.Key;
			if (key >= min && key <= max)
			{
				yield return item.Key;
			}
		}
	}

	public IEnumerable<AttachPoint> Points(int min = 0, int max = int.MaxValue)
	{
		foreach (KeyValuePair<AttachPointType, AttachPoint> item in cache)
		{
			int key = (int)item.Key;
			if (key >= min && key <= max)
			{
				yield return item.Value;
			}
		}
	}

	public AttachPoint GetPointOrDefault(AttachPointType type)
	{
		if (TryGetPoint(type, out var pt))
		{
			return pt;
		}
		return cache[AttachPointType.RootNone];
	}

	public bool TryGetPoint(AttachPointType type, out AttachPoint pt)
	{
		return cache.TryGetValue(type, out pt);
	}
}
