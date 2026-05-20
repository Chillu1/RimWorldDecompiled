using System.Collections.Generic;
using System.Xml;
using RimWorld;
using UnityEngine;

namespace Verse;

public class DrawData
{
	public struct RotationalData
	{
		public Rot4? rotation;

		public Vector2? pivot;

		public Vector3? offset;

		public float? rotationOffset;

		public bool? flip;

		public float? layer;

		public RotationalData(Rot4? rotation, float layer)
		{
			this = default(RotationalData);
			this.rotation = rotation;
			this.layer = layer;
			pivot = null;
			offset = null;
			rotationOffset = null;
			flip = null;
		}
	}

	private class BodyTypeDefWithScale
	{
		public BodyTypeDef bodyType;

		public float scale = 1f;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "bodyType", xmlRoot.Name);
			scale = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}
	}

	public static readonly Vector2 PivotCenter = new Vector2(0.5f, 0.5f);

	private RotationalData defaultData;

	private RotationalData? dataNorth;

	private RotationalData? dataEast;

	private RotationalData? dataSouth;

	private RotationalData? dataWest;

	public bool scaleOffsetByBodySize;

	public bool useBodyPartAnchor;

	public float scale = 1f;

	public float childScale = 1f;

	private List<BodyTypeDefWithScale> bodyTypeScales;

	public float ScaleFor(Pawn pawn)
	{
		float num = scale;
		if (pawn.RaceProps.Humanlike)
		{
			if (pawn.DevelopmentalStage.Child())
			{
				num *= childScale;
			}
			if (bodyTypeScales != null)
			{
				foreach (BodyTypeDefWithScale bodyTypeScale in bodyTypeScales)
				{
					if (pawn.story.bodyType == bodyTypeScale.bodyType)
					{
						num *= bodyTypeScale.scale;
						break;
					}
				}
			}
		}
		return num;
	}

	public Vector2 PivotForRot(Rot4 rot)
	{
		return rot.AsInt switch
		{
			0 => dataNorth?.pivot ?? defaultData.pivot ?? PivotCenter, 
			1 => dataEast?.pivot ?? defaultData.pivot ?? PivotCenter, 
			2 => dataSouth?.pivot ?? defaultData.pivot ?? PivotCenter, 
			3 => dataWest?.pivot ?? defaultData.pivot ?? PivotCenter, 
			_ => defaultData.pivot ?? PivotCenter, 
		};
	}

	public Vector3 OffsetForRot(Rot4 rot)
	{
		switch (rot.AsInt)
		{
		case 0:
			return dataNorth?.offset ?? defaultData.offset ?? Vector3.zero;
		case 1:
		{
			Vector3? vector = dataEast?.offset;
			if (vector.HasValue)
			{
				return vector.Value;
			}
			Vector3? vector2 = dataWest?.offset;
			if (vector2.HasValue)
			{
				Vector3 value = vector2.Value;
				value.x *= -1f;
				return value;
			}
			Vector3? offset = defaultData.offset;
			if (offset.HasValue)
			{
				return offset.Value;
			}
			return Vector3.zero;
		}
		case 2:
			return dataSouth?.offset ?? defaultData.offset ?? Vector3.zero;
		case 3:
		{
			Vector3? vector3 = dataWest?.offset;
			if (vector3.HasValue)
			{
				return vector3.Value;
			}
			Vector3? vector4 = dataEast?.offset;
			if (vector4.HasValue)
			{
				Vector3 value2 = vector4.Value;
				value2.x *= -1f;
				return value2;
			}
			Vector3? offset2 = defaultData.offset;
			if (offset2.HasValue)
			{
				return offset2.Value;
			}
			return dataWest?.offset ?? defaultData.offset ?? Vector3.zero;
		}
		default:
			return defaultData.offset ?? Vector3.zero;
		}
	}

	public float RotationOffsetForRot(Rot4 rot)
	{
		return rot.AsInt switch
		{
			0 => dataNorth?.rotationOffset ?? defaultData.rotationOffset.GetValueOrDefault(), 
			1 => dataEast?.rotationOffset ?? defaultData.rotationOffset.GetValueOrDefault(), 
			2 => dataSouth?.rotationOffset ?? defaultData.rotationOffset.GetValueOrDefault(), 
			3 => dataWest?.rotationOffset ?? defaultData.rotationOffset.GetValueOrDefault(), 
			_ => defaultData.rotationOffset.GetValueOrDefault(), 
		};
	}

	public bool FlipForRot(Rot4 rot)
	{
		return rot.AsInt switch
		{
			0 => dataNorth?.flip ?? (defaultData.flip == true), 
			1 => dataEast?.flip ?? (defaultData.flip == true), 
			2 => dataSouth?.flip ?? (defaultData.flip == true), 
			3 => dataWest?.flip ?? (defaultData.flip == true), 
			_ => defaultData.flip == true, 
		};
	}

	public float LayerForRot(Rot4 rot, float defaultLayer)
	{
		return rot.AsInt switch
		{
			0 => dataNorth?.layer ?? defaultData.layer ?? defaultLayer, 
			1 => dataEast?.layer ?? defaultData.layer ?? defaultLayer, 
			2 => dataSouth?.layer ?? defaultData.layer ?? defaultLayer, 
			3 => dataWest?.layer ?? defaultData.layer ?? defaultLayer, 
			_ => defaultData.layer ?? defaultLayer, 
		};
	}

	public static DrawData NewWithData(params RotationalData[] data)
	{
		DrawData drawData = new DrawData();
		for (int i = 0; i < data.Length; i++)
		{
			RotationalData value = data[i];
			if (!value.rotation.HasValue)
			{
				drawData.defaultData = value;
				continue;
			}
			switch (value.rotation.Value.AsInt)
			{
			case 0:
				drawData.dataNorth = value;
				break;
			case 1:
				drawData.dataEast = value;
				break;
			case 2:
				drawData.dataSouth = value;
				break;
			case 3:
				drawData.dataWest = value;
				break;
			}
		}
		return drawData;
	}
}
