using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

[StaticConstructorOnStartup]
public abstract class PawnOverlayDrawer
{
	public enum OverlayLayer
	{
		Body,
		Head
	}

	protected struct DrawCall
	{
		public Mesh overlayMesh;

		public Matrix4x4 matrix;

		public Material overlayMat;

		public bool displayOverApparel;

		public Color? colorOverride;

		public Vector4? maskTexScale;

		public Vector4? maskTexOffset;

		public Vector4? mainTexScale;

		public Vector4? mainTexOffset;

		public int rotation;
	}

	protected struct CacheKey : IEquatable<CacheKey>
	{
		public Mesh bodyMesh;

		public Rot4 pawnRot;

		public OverlayLayer layer;

		private int hash;

		public CacheKey(Mesh bodyMesh, Rot4 pawnRot, OverlayLayer layer)
		{
			this.bodyMesh = bodyMesh;
			this.pawnRot = pawnRot;
			this.layer = layer;
			hash = ((!(bodyMesh == null)) ? bodyMesh.GetHashCode() : 0);
			hash = Gen.HashCombineInt(pawnRot.GetHashCode(), hash);
			hash = Gen.HashCombineInt(layer.GetHashCode(), hash);
		}

		public override int GetHashCode()
		{
			return hash;
		}

		public bool Equals(CacheKey other)
		{
			if (other.bodyMesh == bodyMesh && other.pawnRot == pawnRot)
			{
				return other.layer == layer;
			}
			return false;
		}

		public override bool Equals(object other)
		{
			if (other is CacheKey other2)
			{
				return Equals(other2);
			}
			return false;
		}
	}

	protected Pawn pawn;

	private static MaterialPropertyBlock propBlock = new MaterialPropertyBlock();

	protected Dictionary<CacheKey, List<DrawCall>> drawCallCache = new Dictionary<CacheKey, List<DrawCall>>();

	protected static List<List<DrawCall>> drawCallListPool = new List<List<DrawCall>>();

	public PawnOverlayDrawer(Pawn pawn)
	{
		this.pawn = pawn;
	}

	private static List<DrawCall> GetDrawCallList()
	{
		if (drawCallListPool.Count == 0)
		{
			return new List<DrawCall>();
		}
		List<DrawCall> result = drawCallListPool[drawCallListPool.Count - 1];
		drawCallListPool.RemoveAt(drawCallListPool.Count - 1);
		return result;
	}

	private static void ReturnDrawCallList(List<DrawCall> lst)
	{
		lst.Clear();
		drawCallListPool.Add(lst);
	}

	public void ClearCache()
	{
		foreach (List<DrawCall> value in drawCallCache.Values)
		{
			ReturnDrawCallList(value);
		}
		drawCallCache.Clear();
	}

	protected abstract void WriteCache(CacheKey key, PawnDrawParms parms, List<DrawCall> writeTarget);

	public void RenderPawnOverlay(Matrix4x4 matrix, Mesh bodyMesh, OverlayLayer layer, PawnDrawParms parms, bool? overApparel = null)
	{
		CacheKey key = new CacheKey(bodyMesh, parms.facing, layer);
		if (!drawCallCache.TryGetValue(key, out var value))
		{
			value = GetDrawCallList();
			WriteCache(key, parms, value);
			drawCallCache.Add(key, value);
		}
		foreach (DrawCall item in value)
		{
			if (!overApparel.HasValue || overApparel == item.displayOverApparel)
			{
				DoDrawCall(item, matrix, parms.DrawNow);
			}
		}
	}

	private void DoDrawCall(DrawCall call, Matrix4x4 matrix, bool drawNow)
	{
		if (drawNow)
		{
			if (call.maskTexOffset.HasValue)
			{
				call.overlayMat.SetVector(ShaderPropertyIDs.MaskTextureOffset, call.maskTexOffset.Value);
				call.overlayMat.SetVector(ShaderPropertyIDs.MaskTextureScale, call.maskTexScale.Value);
			}
			if (call.mainTexOffset.HasValue)
			{
				call.overlayMat.SetVector(ShaderPropertyIDs.MainTextureOffset, call.mainTexOffset.Value);
			}
			if (call.mainTexScale.HasValue)
			{
				call.overlayMat.SetVector(ShaderPropertyIDs.MainTextureScale, call.mainTexScale.Value);
			}
			if (call.colorOverride.HasValue)
			{
				call.overlayMat.SetColor(ShaderPropertyIDs.Color, call.colorOverride.Value);
			}
			call.overlayMat.SetInt(ShaderPropertyIDs.Rotation, call.rotation);
			call.overlayMat.SetPass(0);
			Graphics.DrawMeshNow(call.overlayMesh, matrix * call.matrix);
			call.overlayMat.SetVector(ShaderPropertyIDs.MaskTextureOffset, Vector4.zero);
			call.overlayMat.SetVector(ShaderPropertyIDs.MaskTextureScale, Vector4.one);
			call.overlayMat.SetVector(ShaderPropertyIDs.MainTextureOffset, Vector4.zero);
			call.overlayMat.SetVector(ShaderPropertyIDs.MainTextureScale, Vector4.one);
			if (call.colorOverride.HasValue)
			{
				call.overlayMat.SetColor(ShaderPropertyIDs.Color, Color.white);
			}
		}
		else
		{
			propBlock.Clear();
			if (call.maskTexOffset.HasValue)
			{
				propBlock.SetVector(ShaderPropertyIDs.MaskTextureOffset, call.maskTexOffset.Value);
				propBlock.SetVector(ShaderPropertyIDs.MaskTextureScale, call.maskTexScale.Value);
			}
			if (call.mainTexOffset.HasValue)
			{
				propBlock.SetVector(ShaderPropertyIDs.MainTextureOffset, call.mainTexOffset.Value);
			}
			if (call.mainTexScale.HasValue)
			{
				propBlock.SetVector(ShaderPropertyIDs.MainTextureScale, call.mainTexScale.Value);
			}
			if (call.colorOverride.HasValue)
			{
				propBlock.SetColor(ShaderPropertyIDs.Color, call.colorOverride.Value);
			}
			propBlock.SetInt(ShaderPropertyIDs.Rotation, call.rotation);
			Graphics.DrawMesh(call.overlayMesh, matrix * call.matrix, call.overlayMat, 0, null, 0, propBlock);
		}
	}
}
