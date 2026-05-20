using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Verse;

public class DrawBatch
{
	private class BatchData
	{
		public Matrix4x4[] matrices;

		public int ptr;

		public Vector4[] colors;

		public bool hasAnyColors;

		private static readonly Vector4 WhiteColor = Color.white;

		public BatchData()
		{
			matrices = new Matrix4x4[1023];
			colors = new Vector4[1023];
			ptr = 0;
		}

		public void Clear()
		{
			ptr = 0;
			hasAnyColors = false;
		}

		public void Add(Matrix4x4 matrix)
		{
			matrices[ptr] = matrix;
			colors[ptr] = WhiteColor;
			ptr++;
		}

		public void Add(Matrix4x4 matrix, Color? color)
		{
			matrices[ptr] = matrix;
			colors[ptr] = color ?? ((Color)WhiteColor);
			ptr++;
			hasAnyColors = true;
		}
	}

	private struct BatchKey : IEquatable<BatchKey>
	{
		public readonly Mesh mesh;

		public readonly Material material;

		public readonly int layer;

		public readonly bool renderInstanced;

		public readonly DrawBatchPropertyBlock propertyBlock;

		private int hash;

		public BatchKey(Mesh mesh, Material material, int layer, bool renderInstanced, DrawBatchPropertyBlock propertyBlock)
		{
			this.mesh = mesh;
			this.material = material;
			this.layer = layer;
			this.renderInstanced = renderInstanced && SystemInfo.supportsInstancing;
			this.propertyBlock = propertyBlock;
			hash = mesh.GetHashCode();
			hash = Gen.HashCombineInt(hash, material.GetHashCode());
			hash = Gen.HashCombineInt(hash, layer | ((renderInstanced ? 1 : 0) << 8));
			hash = ((propertyBlock == null) ? hash : Gen.HashCombineInt(hash, propertyBlock.GetHashCode()));
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is BatchKey other))
			{
				return false;
			}
			return Equals(other);
		}

		public bool Equals(BatchKey other)
		{
			if ((object)mesh == other.mesh && (object)material == other.material && layer == other.layer && renderInstanced == other.renderInstanced)
			{
				return propertyBlock == other.propertyBlock;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return hash;
		}
	}

	private Dictionary<BatchKey, List<BatchData>> batches = new Dictionary<BatchKey, List<BatchData>>();

	private List<BatchData> batchDataListCache = new List<BatchData>();

	private List<List<BatchData>> batchListCache = new List<List<BatchData>>();

	private HashSet<DrawBatchPropertyBlock> myPropertyBlocks = new HashSet<DrawBatchPropertyBlock>();

	private List<DrawBatchPropertyBlock> propertyBlockCache = new List<DrawBatchPropertyBlock>();

	private MaterialPropertyBlock tmpPropertyBlock;

	private HashSet<DrawBatchPropertyBlock> tmpPropertyBlocks = new HashSet<DrawBatchPropertyBlock>();

	public const int MaxCountPerBatch = 1023;

	private static bool PropertyBlockLeakDebug;

	private BatchKey lastBatchKey;

	private List<BatchData> lastBatchList;

	public DrawBatchPropertyBlock GetPropertyBlock()
	{
		DrawBatchPropertyBlock drawBatchPropertyBlock = null;
		if (propertyBlockCache.Count == 0)
		{
			drawBatchPropertyBlock = new DrawBatchPropertyBlock();
			myPropertyBlocks.Add(drawBatchPropertyBlock);
		}
		else
		{
			drawBatchPropertyBlock = propertyBlockCache.Pop();
		}
		if (PropertyBlockLeakDebug)
		{
			drawBatchPropertyBlock.leakDebugString = "Allocated from:\n\n---------------\n\n" + StackTraceUtility.ExtractStackTrace();
		}
		return drawBatchPropertyBlock;
	}

	public void ReturnPropertyBlock(DrawBatchPropertyBlock propertyBlock)
	{
		if (myPropertyBlocks.Contains(propertyBlock))
		{
			propertyBlockCache.Add(propertyBlock);
		}
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int layer, Color? color = null, bool renderInstanced = false, DrawBatchPropertyBlock propertyBlock = null)
	{
		GetBatchDataForInsertion(new BatchKey(mesh, material, layer, renderInstanced, propertyBlock)).Add(matrix, color);
	}

	public void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, int layer, bool renderInstanced = false)
	{
		GetBatchDataForInsertion(new BatchKey(mesh, material, layer, renderInstanced, null)).Add(matrix);
	}

	public void Flush(bool draw = true)
	{
		if (tmpPropertyBlock == null)
		{
			tmpPropertyBlock = new MaterialPropertyBlock();
		}
		tmpPropertyBlocks.Clear();
		tmpPropertyBlocks.AddRange(propertyBlockCache);
		try
		{
			foreach (KeyValuePair<BatchKey, List<BatchData>> batch in batches)
			{
				BatchKey key = batch.Key;
				try
				{
					foreach (BatchData item in batch.Value)
					{
						BatchData batchData = item;
						if (draw)
						{
							tmpPropertyBlock.Clear();
							if (key.propertyBlock != null)
							{
								key.propertyBlock.Write(tmpPropertyBlock);
							}
							if (key.renderInstanced)
							{
								key.material.enableInstancing = true;
								if (batchData.hasAnyColors)
								{
									tmpPropertyBlock.SetVectorArray("_Color", batchData.colors);
								}
								Graphics.DrawMeshInstanced(key.mesh, 0, key.material, item.matrices, item.ptr, tmpPropertyBlock, ShadowCastingMode.On, receiveShadows: true, key.layer);
							}
							else
							{
								for (int i = 0; i < batchData.ptr; i++)
								{
									Matrix4x4 matrix = batchData.matrices[i];
									Vector4 vector = batchData.colors[i];
									if (batchData.hasAnyColors)
									{
										tmpPropertyBlock.SetColor("_Color", vector);
									}
									Graphics.DrawMesh(key.mesh, matrix, key.material, key.layer, null, 0, tmpPropertyBlock);
								}
							}
						}
						batchData.Clear();
						batchDataListCache.Add(batchData);
					}
				}
				finally
				{
					if (key.propertyBlock != null && myPropertyBlocks.Contains(key.propertyBlock))
					{
						tmpPropertyBlocks.Add(key.propertyBlock);
						key.propertyBlock.Clear();
						propertyBlockCache.Add(key.propertyBlock);
					}
					batchListCache.Add(batch.Value);
					batch.Value.Clear();
				}
			}
		}
		finally
		{
			foreach (DrawBatchPropertyBlock myPropertyBlock in myPropertyBlocks)
			{
				if (!tmpPropertyBlocks.Contains(myPropertyBlock))
				{
					Log.Warning("Property block from FleckDrawBatch leaked!" + ((myPropertyBlock.leakDebugString == null) ? null : ("Leak debug information: \n" + myPropertyBlock.leakDebugString)));
				}
			}
			HashSet<DrawBatchPropertyBlock> hashSet = myPropertyBlocks;
			myPropertyBlocks = tmpPropertyBlocks;
			tmpPropertyBlocks = hashSet;
			batches.Clear();
			lastBatchKey = default(BatchKey);
			lastBatchList = null;
		}
	}

	private BatchData GetBatchDataForInsertion(BatchKey key)
	{
		List<BatchData> value;
		if (lastBatchList != null && key.GetHashCode() == lastBatchKey.GetHashCode() && key.Equals(lastBatchKey))
		{
			value = lastBatchList;
		}
		else
		{
			if (!batches.TryGetValue(key, out value))
			{
				value = ((batchListCache.Count == 0) ? new List<BatchData>() : batchListCache.Pop());
				batches.Add(key, value);
				value.Add((batchDataListCache.Count == 0) ? new BatchData() : batchDataListCache.Pop());
			}
			lastBatchList = value;
			lastBatchKey = key;
		}
		int index = value.Count - 1;
		if (value[index].ptr < 1023)
		{
			return value[index];
		}
		BatchData batchData = ((batchDataListCache.Count == 0) ? new BatchData() : batchDataListCache.Pop());
		value.Add(batchData);
		return batchData;
	}

	public void MergeWith(DrawBatch other)
	{
		foreach (KeyValuePair<BatchKey, List<BatchData>> batch in other.batches)
		{
			foreach (BatchData item in batch.Value)
			{
				while (item.ptr > 0)
				{
					BatchData batchDataForInsertion = GetBatchDataForInsertion(batch.Key);
					int num = Mathf.Min(item.ptr, 1023 - batchDataForInsertion.ptr);
					Array.Copy(item.matrices, 0, batchDataForInsertion.matrices, batchDataForInsertion.ptr, num);
					Array.Copy(item.colors, 0, batchDataForInsertion.colors, batchDataForInsertion.ptr, num);
					batchDataForInsertion.ptr += num;
					item.ptr -= num;
				}
			}
		}
	}
}
