using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse;

public class Graphic
{
	private struct AtlasReplacementInfoCacheKey : IEquatable<AtlasReplacementInfoCacheKey>
	{
		public readonly Material mat;

		public readonly TextureAtlasGroup group;

		public readonly bool flipUv;

		public readonly bool vertexColors;

		private readonly int hash;

		public AtlasReplacementInfoCacheKey(Material mat, TextureAtlasGroup group, bool flipUv, bool vertexColors)
		{
			this.mat = mat;
			this.group = group;
			this.flipUv = flipUv;
			this.vertexColors = vertexColors;
			hash = Gen.HashCombine(mat.GetHashCode(), group.GetHashCode());
			if (flipUv)
			{
				hash = ~hash;
			}
			if (vertexColors)
			{
				hash ^= 123893723;
			}
		}

		public bool Equals(AtlasReplacementInfoCacheKey other)
		{
			if ((object)mat == other.mat && group == other.group && flipUv == other.flipUv)
			{
				return vertexColors == other.vertexColors;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return hash;
		}
	}

	private struct CachedAtlasReplacementInfo
	{
		public Material material;

		public Vector2[] uvs;

		public Color32 vertexColor;
	}

	public GraphicData data;

	public string path;

	public string maskPath;

	public Color color = Color.white;

	public Color colorTwo = Color.white;

	public Vector2 drawSize = Vector2.one;

	private Graphic_Shadow cachedShadowGraphicInt;

	private Graphic cachedShadowlessGraphicInt;

	private static Dictionary<AtlasReplacementInfoCacheKey, CachedAtlasReplacementInfo> replacementInfoCache = new Dictionary<AtlasReplacementInfoCacheKey, CachedAtlasReplacementInfo>();

	public Shader Shader
	{
		get
		{
			Material matSingle = MatSingle;
			if (matSingle != null)
			{
				return matSingle.shader;
			}
			return ShaderDatabase.Cutout;
		}
	}

	public Graphic_Shadow ShadowGraphic
	{
		get
		{
			if (cachedShadowGraphicInt == null && data != null && data.shadowData != null)
			{
				cachedShadowGraphicInt = new Graphic_Shadow(data.shadowData);
			}
			return cachedShadowGraphicInt;
		}
		set
		{
			cachedShadowGraphicInt = value;
		}
	}

	public Color Color => color;

	public Color ColorTwo => colorTwo;

	public virtual Material MatSingle => BaseContent.BadMat;

	public virtual Material MatWest => MatSingle;

	public virtual Material MatSouth => MatSingle;

	public virtual Material MatEast => MatSingle;

	public virtual Material MatNorth => MatSingle;

	public virtual bool WestFlipped
	{
		get
		{
			if (DataAllowsFlip)
			{
				return !ShouldDrawRotated;
			}
			return false;
		}
	}

	public virtual bool EastFlipped => false;

	public virtual bool ShouldDrawRotated => false;

	public virtual float DrawRotatedExtraAngleOffset => 0f;

	public virtual bool UseSameGraphicForGhost => false;

	protected bool DataAllowsFlip
	{
		get
		{
			if (data != null)
			{
				return data.allowFlip;
			}
			return true;
		}
	}

	public static bool TryGetTextureAtlasReplacementInfo(Material mat, TextureAtlasGroup group, bool flipUv, bool vertexColors, out Material material, out Vector2[] uvs, out Color32 vertexColor)
	{
		material = mat;
		uvs = null;
		vertexColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		AtlasReplacementInfoCacheKey key = new AtlasReplacementInfoCacheKey(mat, group, flipUv, vertexColors);
		if (replacementInfoCache.TryGetValue(key, out var value))
		{
			material = value.material;
			uvs = value.uvs;
			if (vertexColors)
			{
				vertexColor = value.vertexColor;
			}
			return true;
		}
		if (GlobalTextureAtlasManager.TryGetStaticTile(group, (Texture2D)mat.mainTexture, out var tile))
		{
			if (!MaterialPool.TryGetRequestForMat(mat, out var request))
			{
				Log.Error("Tried getting texture atlas replacement info for a material that was not created by MaterialPool!");
				return false;
			}
			uvs = new Vector2[4];
			Printer_Plane.GetUVs(tile.uvRect, out uvs[0], out uvs[1], out uvs[2], out uvs[3], flipUv);
			request.mainTex = tile.atlas.ColorTexture;
			if (vertexColors)
			{
				vertexColor = request.color;
				request.color = Color.white;
			}
			if (request.maskTex != null)
			{
				request.maskTex = tile.atlas.MaskTexture;
			}
			material = MaterialPool.MatFrom(request);
			replacementInfoCache.Add(key, new CachedAtlasReplacementInfo
			{
				material = material,
				uvs = uvs,
				vertexColor = vertexColor
			});
			return true;
		}
		return false;
	}

	public virtual void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
	{
	}

	public virtual void Init(GraphicRequest req)
	{
		Log.ErrorOnce($"Cannot init Graphic of class {GetType()}", 658928);
	}

	public virtual Material NodeGetMat(PawnDrawParms parms)
	{
		return MatAt(parms.facing, parms.pawn);
	}

	public virtual Material MatAt(Rot4 rot, Thing thing = null)
	{
		return rot.AsInt switch
		{
			0 => MatNorth, 
			1 => MatEast, 
			2 => MatSouth, 
			3 => MatWest, 
			_ => BaseContent.BadMat, 
		};
	}

	public virtual Mesh MeshAt(Rot4 rot)
	{
		Vector2 vector = drawSize;
		if (rot.IsHorizontal && !ShouldDrawRotated)
		{
			vector = vector.Rotated();
		}
		if ((rot == Rot4.West && WestFlipped) || (rot == Rot4.East && EastFlipped))
		{
			return MeshPool.GridPlaneFlip(vector);
		}
		return MeshPool.GridPlane(vector);
	}

	public virtual Material MatSingleFor(Thing thing)
	{
		return MatSingle;
	}

	public Vector3 DrawOffset(Rot4 rot)
	{
		if (data == null)
		{
			return Vector3.zero;
		}
		return data.DrawOffsetForRot(rot);
	}

	public void Draw(Vector3 loc, Rot4 rot, Thing thing, float extraRotation = 0f)
	{
		DrawWorker(loc, rot, thing.def, thing, extraRotation);
	}

	public void DrawFromDef(Vector3 loc, Rot4 rot, ThingDef thingDef, float extraRotation = 0f)
	{
		DrawWorker(loc, rot, thingDef, null, extraRotation);
	}

	public virtual void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
	{
		Mesh mesh = MeshAt(rot);
		Quaternion quat = QuatFromRot(rot);
		if (extraRotation != 0f)
		{
			quat *= Quaternion.Euler(Vector3.up * extraRotation);
		}
		if (data != null && data.addTopAltitudeBias)
		{
			quat *= Quaternion.Euler(Vector3.left * 2f);
		}
		loc += DrawOffset(rot);
		Material mat = MatAt(rot, thing);
		DrawMeshInt(mesh, loc, quat, mat);
		if (ShadowGraphic != null)
		{
			ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
		}
	}

	protected virtual void DrawMeshInt(Mesh mesh, Vector3 loc, Quaternion quat, Material mat)
	{
		Graphics.DrawMesh(mesh, loc, quat, mat, 0);
	}

	public virtual void Print(SectionLayer layer, Thing thing, float extraRotation)
	{
		Vector2 size;
		bool flag;
		if (ShouldDrawRotated)
		{
			size = drawSize;
			flag = false;
		}
		else
		{
			size = (thing.Rotation.IsHorizontal ? drawSize.Rotated() : drawSize);
			flag = (thing.Rotation == Rot4.West && WestFlipped) || (thing.Rotation == Rot4.East && EastFlipped);
		}
		if (thing.MultipleItemsPerCellDrawn())
		{
			size *= 0.8f;
		}
		float num = AngleFromRot(thing.Rotation) + extraRotation;
		if (flag && data != null)
		{
			num += data.flipExtraRotation;
		}
		Vector3 center = thing.TrueCenter() + DrawOffset(thing.Rotation);
		Material material = MatAt(thing.Rotation, thing);
		TryGetTextureAtlasReplacementInfo(material, thing.def.category.ToAtlasGroup(), flag, vertexColors: true, out material, out var uvs, out var vertexColor);
		Printer_Plane.PrintPlane(layer, center, size, material, num, flag, uvs, new Color32[4] { vertexColor, vertexColor, vertexColor, vertexColor });
		ShadowGraphic?.Print(layer, thing, 0f);
	}

	public virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
	{
		Log.ErrorOnce("CloneColored not implemented on this subclass of Graphic: " + GetType().ToString(), 66300);
		return BaseContent.BadGraphic;
	}

	[Obsolete("Will be removed in a future release")]
	public virtual Graphic GetCopy(Vector2 newDrawSize)
	{
		return GetCopy(newDrawSize, null);
	}

	public virtual Graphic GetCopy(Vector2 newDrawSize, Shader overrideShader)
	{
		return GraphicDatabase.Get(GetType(), path, overrideShader ?? Shader, newDrawSize, color, colorTwo);
	}

	public virtual Graphic GetShadowlessGraphic()
	{
		if (data == null || data.shadowData == null)
		{
			return this;
		}
		if (cachedShadowlessGraphicInt == null)
		{
			GraphicData graphicData = new GraphicData();
			graphicData.CopyFrom(data);
			graphicData.shadowData = null;
			cachedShadowlessGraphicInt = graphicData.Graphic;
		}
		return cachedShadowlessGraphicInt;
	}

	protected float AngleFromRot(Rot4 rot)
	{
		if (ShouldDrawRotated)
		{
			float asAngle = rot.AsAngle;
			asAngle += DrawRotatedExtraAngleOffset;
			if ((rot == Rot4.West && WestFlipped) || (rot == Rot4.East && EastFlipped))
			{
				asAngle += 180f;
			}
			return asAngle;
		}
		return 0f;
	}

	public Quaternion QuatFromRot(Rot4 rot)
	{
		float num = AngleFromRot(rot);
		if (num == 0f)
		{
			return Quaternion.identity;
		}
		return Quaternion.AngleAxis(num, Vector3.up);
	}
}
