using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using LudeonTK;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

[BurstCompile]
public class PlanetLayer : IExposable, ILoadReferenceable, IDisposable
{
	public delegate float CalculateAverageTileSize_000153C0_0024PostfixBurstDelegate(in NativeArray<int> tileIDToNeighbors_offsets, in NativeArray<PlanetTile> tileIDToNeighbors_values, in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts);

	internal static class CalculateAverageTileSize_000153C0_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(CalculateAverageTileSize_000153C0_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static CalculateAverageTileSize_000153C0_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static float Invoke(in NativeArray<int> tileIDToNeighbors_offsets, in NativeArray<PlanetTile> tileIDToNeighbors_values, in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeArray<PlanetTile>, ref NativeArray<int>, ref NativeArray<Vector3>, float>)functionPointer)(ref tileIDToNeighbors_offsets, ref tileIDToNeighbors_values, ref tileIDToVerts_offsets, ref verts);
				}
			}
			return CalculateAverageTileSize_0024BurstManaged(in tileIDToNeighbors_offsets, in tileIDToNeighbors_values, in tileIDToVerts_offsets, in verts);
		}
	}

	public delegate float IntGetTileSize_000153C2_0024PostfixBurstDelegate(in NativeArray<int> tileIDToNeighbors_offsets, in NativeArray<PlanetTile> tileIDToNeighbors_values, in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts, in PlanetTile tile);

	internal static class IntGetTileSize_000153C2_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(IntGetTileSize_000153C2_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static IntGetTileSize_000153C2_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static float Invoke(in NativeArray<int> tileIDToNeighbors_offsets, in NativeArray<PlanetTile> tileIDToNeighbors_values, in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts, in PlanetTile tile)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeArray<PlanetTile>, ref NativeArray<int>, ref NativeArray<Vector3>, ref PlanetTile, float>)functionPointer)(ref tileIDToNeighbors_offsets, ref tileIDToNeighbors_values, ref tileIDToVerts_offsets, ref verts, ref tile);
				}
			}
			return IntGetTileSize_0024BurstManaged(in tileIDToNeighbors_offsets, in tileIDToNeighbors_values, in tileIDToVerts_offsets, in verts, in tile);
		}
	}

	public delegate void IntGetTileCenter_000153C5_0024PostfixBurstDelegate(in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts, int tileID, out Vector3 center);

	internal static class IntGetTileCenter_000153C5_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		private static IntPtr DeferredCompilation;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = (nint)BurstCompiler.GetILPPMethodFunctionPointer2(DeferredCompilation, (RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/, typeof(IntGetTileCenter_000153C5_0024PostfixBurstDelegate).TypeHandle);
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public static void Constructor()
		{
			DeferredCompilation = BurstCompiler.CompileILPPMethod2((RuntimeMethodHandle)/*OpCode not supported: LdMemberToken*/);
		}

		public static void Initialize()
		{
		}

		static IntGetTileCenter_000153C5_0024BurstDirectCall()
		{
			Constructor();
		}

		public unsafe static void Invoke(in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts, int tileID, out Vector3 center)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeArray<Vector3>, int, ref Vector3, void>)functionPointer)(ref tileIDToVerts_offsets, ref verts, tileID, ref center);
					return;
				}
			}
			IntGetTileCenter_0024BurstManaged(in tileIDToVerts_offsets, in verts, tileID, out center);
		}
	}

	private PlanetLayerDef def;

	private string scenarioTag;

	private bool isRootSurface;

	private int layerId = -1;

	private int subdivisions;

	private float backgroundWorldCameraOffset;

	private float backgroundWorldCameraParallaxDistancePer100Cells;

	private float radius;

	private float viewAngle;

	private float extraCameraAltitude;

	private Vector3 viewCenter;

	private Vector3 origin;

	public PlanetLayer zoomOutToLayer;

	public PlanetLayer zoomInToLayer;

	private Dictionary<PlanetLayer, PlanetLayerConnection> connections = new Dictionary<PlanetLayer, PlanetLayerConnection>();

	protected readonly List<Tile> tiles = new List<Tile>();

	private FastTileFinder finder;

	private WorldPathing pather;

	private WorldFloodFiller filler;

	private Func<PlanetTile, Tile> tileCreatorFunc;

	private byte[] tileBiome;

	private NativeArray<Vector3> verts;

	private NativeArray<int> tileIDToVerts_offsets;

	private NativeArray<int> tileIDToNeighbors_offsets;

	private NativeArray<PlanetTile> tileIDToNeighbors_values;

	private float averageTileSize;

	private static List<TriangleIndices> tmp_tris = new List<TriangleIndices>();

	private static List<Vector3> tmp_verts = new List<Vector3>();

	private static List<Vector3> tmp_generatedVerts = new List<Vector3>();

	private static List<int> tmp_generatedVertOffsets = new List<int>();

	private static List<TriangleIndices> tmp_generatedTris = new List<TriangleIndices>();

	private static List<int> tmp_generatedTileVerts = new List<int>();

	private static List<int> tmp_adjacentTris = new List<int>();

	private static List<int> tmp_tileIDs = new List<int>();

	private static List<int> tmp_neighbourVerts = new List<int>();

	private static List<PlanetTile> tmp_neighborsToAdd = new List<PlanetTile>();

	private static List<int> tmp_vertToTris_offsets = new List<int>();

	private static List<int> tmp_vertToTris_values = new List<int>();

	private static List<int> tmp_vertToTileIDs_offsets = new List<int>();

	private static List<int> tmp_vertToTileIDs_values = new List<int>();

	private static List<int> tmp_tileIDToVerts_offsets = new List<int>();

	private static List<int> tmp_tileIDToVerts_values = new List<int>();

	private const int MaxTileVertices = 6;

	private List<PlanetLayer> tmpLayers;

	private List<PlanetLayerConnection> tmpConnections;

	private static readonly HashSet<PlanetLayer> TmpVisited = new HashSet<PlanetLayer>();

	private static readonly PriorityQueue<PlanetLayer, int> openSet = new PriorityQueue<PlanetLayer, int>();

	private static readonly Dictionary<PlanetLayer, PlanetLayer> cameFrom = new Dictionary<PlanetLayer, PlanetLayer>();

	private static readonly Dictionary<PlanetLayer, int> gScore = new Dictionary<PlanetLayer, int>();

	private static readonly List<PlanetLayer> toEnqueue = new List<PlanetLayer>();

	public IReadOnlyDictionary<PlanetLayer, PlanetLayerConnection> Connections => connections;

	public List<WorldDrawLayer> WorldDrawLayers { get; } = new List<WorldDrawLayer>();

	public FastTileFinder FastTileFinder => finder;

	public WorldPathing Pather => pather;

	public WorldFloodFiller Filler => filler;

	public virtual bool Visible => true;

	public bool IsSelected => Selected == this;

	public int LayerID => layerId;

	public PlanetLayerDef Def => def;

	public List<Tile> Tiles => tiles;

	public int TilesCount => tileIDToNeighbors_offsets.Length;

	public bool IsRootSurface => isRootSurface;

	public NativeArray<Vector3> UnsafeVerts => verts;

	public NativeArray<int> UnsafeTileIDToVerts_offsets => tileIDToVerts_offsets;

	public NativeArray<int> UnsafeTileIDToNeighbors_offsets => tileIDToNeighbors_offsets;

	public NativeArray<PlanetTile> UnsafeTileIDToNeighbors_values => tileIDToNeighbors_values;

	public virtual Vector3 Origin => origin;

	public Vector3 NorthPolePos => new Vector3(0f, Radius, 0f);

	public float AverageTileSize => averageTileSize;

	public float Radius => radius;

	public float ExtraCameraAltitude => extraCameraAltitude;

	public float BackgroundWorldCameraOffset => backgroundWorldCameraOffset;

	public float BackgroundWorldCameraParallaxDistancePer100Cells => backgroundWorldCameraParallaxDistancePer100Cells;

	public Vector3 ViewCenter => viewCenter;

	public float ViewAngle => viewAngle;

	public Tile this[int tileID]
	{
		get
		{
			if ((uint)tileID >= TilesCount)
			{
				return null;
			}
			return tiles[tileID];
		}
	}

	public Tile this[PlanetTile tile] => this[tile.tileId];

	public virtual bool Raycastable
	{
		get
		{
			if (!def.alwaysRaycastable)
			{
				return Selected == this;
			}
			return true;
		}
	}

	public virtual bool CanReachLayer => true;

	public string ScenarioTag
	{
		get
		{
			return scenarioTag;
		}
		set
		{
			scenarioTag = value;
		}
	}

	public static PlanetLayer Selected
	{
		get
		{
			return Find.WorldSelector.SelectedLayer;
		}
		set
		{
			Find.WorldSelector.SelectedLayer = value;
		}
	}

	public string GetUniqueLoadID()
	{
		return $"PlanetLayer_{layerId}";
	}

	public PlanetTile PlanetTileForID(int tileID)
	{
		return new PlanetTile(tileID, layerId);
	}

	public PlanetLayer()
	{
	}

	public PlanetLayer(int layerId, PlanetLayerDef def, float radius, Vector3 origin, Vector3 viewCenter, float viewAngle, int subdivisions, float extraCameraAltitude, float backgroundWorldCameraOffset, float backgroundWorldCameraParallaxDistancePer100Cells)
	{
		this.layerId = layerId;
		this.def = def;
		this.radius = radius;
		this.origin = origin;
		this.viewCenter = viewCenter;
		this.viewAngle = viewAngle;
		this.subdivisions = subdivisions;
		this.extraCameraAltitude = extraCameraAltitude;
		this.backgroundWorldCameraOffset = backgroundWorldCameraOffset;
		this.backgroundWorldCameraParallaxDistancePer100Cells = backgroundWorldCameraParallaxDistancePer100Cells;
	}

	public void MarkRootSurface()
	{
		isRootSurface = true;
	}

	public void AddConnection(PlanetLayer target, float cost)
	{
		if (connections.ContainsKey(target))
		{
			Log.Error($"Attempted to add a layer connection ({target}) which already exists.");
			return;
		}
		connections[target] = new PlanetLayerConnection
		{
			origin = this,
			target = target,
			fuelCost = cost
		};
	}

	public void RemoveConnection(PlanetLayer target)
	{
		connections.Remove(target);
	}

	public bool HasConnectionFromTo(PlanetLayer layer)
	{
		return connections.ContainsKey(layer);
	}

	public PlanetLayerConnection GetConnectionFromTo(PlanetLayer layer)
	{
		return connections[layer];
	}

	public bool TryGetConnectionFromTo(PlanetLayer layer, out PlanetLayerConnection connection)
	{
		return connections.TryGetValue(layer, out connection);
	}

	public virtual AcceptanceReport CanSelectLayer()
	{
		return true;
	}

	public void RunWorldGeneration()
	{
		RunWorldGeneration(Find.World.info.seedString, Rand.Int);
	}

	public void RunWorldGeneration(int seed)
	{
		RunWorldGeneration(Find.World.info.seedString, seed);
	}

	public void RunWorldGeneration(string seedString, int seed)
	{
		WorldGenerator.GeneratePlanetLayer(this, seedString, seed);
	}

	public void InitializeLayer()
	{
		tmp_tris.Clear();
		tmp_verts.Clear();
		Dispose();
		IcosahedronGenerator.GenerateIcosahedron(tmp_verts, tmp_tris, Radius, viewCenter, viewAngle);
		for (int i = 0; i < subdivisions + 1; i++)
		{
			bool lastPass = i == subdivisions;
			Subdivide(lastPass);
		}
		verts = NativeArrayUtility.GetNativeArrayCopy(tmp_generatedVerts, Allocator.Persistent);
		tileIDToVerts_offsets = NativeArrayUtility.GetNativeArrayCopy(tmp_generatedVertOffsets, Allocator.Persistent);
		CalculateTileNeighbors();
		ClearAndDeallocateWorkingLists();
		averageTileSize = CalculateAverageTileSize(in tileIDToNeighbors_offsets, in tileIDToNeighbors_values, in tileIDToVerts_offsets, in verts);
		WorldDrawLayers.Clear();
		foreach (Type worldDrawLayer2 in def.worldDrawLayers)
		{
			WorldDrawLayer worldDrawLayer = (WorldDrawLayer)Activator.CreateInstance(worldDrawLayer2);
			worldDrawLayer.Initialize(this);
			WorldDrawLayers.Add(worldDrawLayer);
		}
		finder = new FastTileFinder(this);
		pather = new WorldPathing(this);
		filler = new WorldFloodFiller(this);
	}

	private void Subdivide(bool lastPass)
	{
		PackedListOfLists.GenerateVertToTrisPackedList(tmp_verts, tmp_tris, tmp_vertToTris_offsets, tmp_vertToTris_values);
		int count = tmp_verts.Count;
		int i = 0;
		for (int count2 = tmp_tris.Count; i < count2; i++)
		{
			TriangleIndices triangleIndices = tmp_tris[i];
			Vector3 vector = (tmp_verts[triangleIndices.v1] + tmp_verts[triangleIndices.v2] + tmp_verts[triangleIndices.v3]) / 3f;
			tmp_verts.Add(vector.normalized * Radius);
		}
		tmp_generatedTris.Clear();
		if (lastPass)
		{
			tmp_vertToTileIDs_offsets.Clear();
			tmp_vertToTileIDs_values.Clear();
			tmp_tileIDToVerts_offsets.Clear();
			tmp_tileIDToVerts_values.Clear();
			int j = 0;
			for (int count3 = tmp_verts.Count; j < count3; j++)
			{
				tmp_vertToTileIDs_offsets.Add(tmp_vertToTileIDs_values.Count);
				if (j >= count)
				{
					for (int k = 0; k < 6; k++)
					{
						tmp_vertToTileIDs_values.Add(-1);
					}
				}
			}
		}
		for (int l = 0; l < count; l++)
		{
			PackedListOfLists.GetList(tmp_vertToTris_offsets, tmp_vertToTris_values, l, tmp_adjacentTris);
			int count4 = tmp_adjacentTris.Count;
			if (!lastPass)
			{
				for (int m = 0; m < count4; m++)
				{
					int num = tmp_adjacentTris[m];
					int v = count + num;
					int nextOrderedVertex = tmp_tris[num].GetNextOrderedVertex(l);
					int num2 = -1;
					for (int n = 0; n < count4; n++)
					{
						if (m != n)
						{
							TriangleIndices triangleIndices2 = tmp_tris[tmp_adjacentTris[n]];
							if (triangleIndices2.v1 == nextOrderedVertex || triangleIndices2.v2 == nextOrderedVertex || triangleIndices2.v3 == nextOrderedVertex)
							{
								num2 = tmp_adjacentTris[n];
								break;
							}
						}
					}
					if (num2 >= 0)
					{
						int v2 = count + num2;
						tmp_generatedTris.Add(new TriangleIndices(l, v2, v));
					}
				}
			}
			else if (count4 == 5 || count4 == 6)
			{
				int num3 = 0;
				int nextOrderedVertex2 = tmp_tris[tmp_adjacentTris[num3]].GetNextOrderedVertex(l);
				int num4 = num3;
				int currentTriangleVertex = nextOrderedVertex2;
				tmp_generatedTileVerts.Clear();
				for (int num5 = 0; num5 < count4; num5++)
				{
					int item = count + tmp_adjacentTris[num4];
					tmp_generatedTileVerts.Add(item);
					int nextAdjacentTriangle = GetNextAdjacentTriangle(num4, currentTriangleVertex);
					int nextOrderedVertex3 = tmp_tris[tmp_adjacentTris[nextAdjacentTriangle]].GetNextOrderedVertex(l);
					num4 = nextAdjacentTriangle;
					currentTriangleVertex = nextOrderedVertex3;
				}
				FinalizeGeneratedTile(tmp_generatedTileVerts);
			}
		}
		tmp_tris.Clear();
		tmp_tris.AddRange(tmp_generatedTris);
	}

	private void FinalizeGeneratedTile(List<int> generatedTileVertsList)
	{
		if ((generatedTileVertsList.Count != 5 && generatedTileVertsList.Count != 6) || generatedTileVertsList.Count > 6)
		{
			Log.Error($"Planet shape generation internal error: generated a tile with {generatedTileVertsList.Count} vertices. Only 5 and 6 are allowed.");
		}
		else if (!ShouldDiscardGeneratedTile(generatedTileVertsList))
		{
			int count = tmp_generatedVertOffsets.Count;
			tmp_generatedVertOffsets.Add(tmp_generatedVerts.Count);
			int i = 0;
			for (int count2 = generatedTileVertsList.Count; i < count2; i++)
			{
				int index = generatedTileVertsList[i];
				tmp_generatedVerts.Add(tmp_verts[index]);
				tmp_vertToTileIDs_values[tmp_vertToTileIDs_values.IndexOf(-1, tmp_vertToTileIDs_offsets[index])] = count;
			}
			PackedListOfLists.AddList(tmp_tileIDToVerts_offsets, tmp_tileIDToVerts_values, generatedTileVertsList);
		}
	}

	private bool ShouldDiscardGeneratedTile(List<int> generatedTileVertsList)
	{
		Vector3 zero = Vector3.zero;
		int i = 0;
		for (int count = generatedTileVertsList.Count; i < count; i++)
		{
			zero += tmp_verts[generatedTileVertsList[i]];
		}
		return !MeshUtility.VisibleForWorldgen(zero / generatedTileVertsList.Count, Radius, viewCenter, viewAngle);
	}

	private void CalculateTileNeighbors()
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<PlanetTile> list3 = new List<PlanetTile>();
		int i = 0;
		for (int count = tmp_tileIDToVerts_offsets.Count; i < count; i++)
		{
			tmp_neighborsToAdd.Clear();
			PackedListOfLists.GetList(tmp_tileIDToVerts_offsets, tmp_tileIDToVerts_values, i, tmp_neighbourVerts);
			int j = 0;
			for (int count2 = tmp_neighbourVerts.Count; j < count2; j++)
			{
				PackedListOfLists.GetList(tmp_vertToTileIDs_offsets, tmp_vertToTileIDs_values, tmp_neighbourVerts[j], tmp_tileIDs);
				PackedListOfLists.GetList(tmp_vertToTileIDs_offsets, tmp_vertToTileIDs_values, tmp_neighbourVerts[(j + 1) % tmp_neighbourVerts.Count], list);
				int k = 0;
				for (int count3 = tmp_tileIDs.Count; k < count3; k++)
				{
					int num = tmp_tileIDs[k];
					if (num != i && num != -1 && list.Contains(num))
					{
						tmp_neighborsToAdd.Add(new PlanetTile(num, layerId));
					}
				}
			}
			PackedListOfLists.AddList(list2, list3, tmp_neighborsToAdd);
		}
		tileIDToNeighbors_offsets = NativeArrayUtility.GetNativeArrayCopy(list2, Allocator.Persistent);
		tileIDToNeighbors_values = NativeArrayUtility.GetNativeArrayCopy(list3, Allocator.Persistent);
	}

	private int GetNextAdjacentTriangle(int currentAdjTriangleIndex, int currentTriangleVertex)
	{
		int i = 0;
		for (int count = tmp_adjacentTris.Count; i < count; i++)
		{
			if (currentAdjTriangleIndex != i)
			{
				TriangleIndices triangleIndices = tmp_tris[tmp_adjacentTris[i]];
				if (triangleIndices.v1 == currentTriangleVertex || triangleIndices.v2 == currentTriangleVertex || triangleIndices.v3 == currentTriangleVertex)
				{
					return i;
				}
			}
		}
		Log.Error("Planet shape generation internal error: could not find next adjacent triangle.");
		return -1;
	}

	[BurstCompile(CompileSynchronously = true)]
	private static float CalculateAverageTileSize(in NativeArray<int> tileIDToNeighbors_offsets, in NativeArray<PlanetTile> tileIDToNeighbors_values, in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts)
	{
		return CalculateAverageTileSize_000153C0_0024BurstDirectCall.Invoke(in tileIDToNeighbors_offsets, in tileIDToNeighbors_values, in tileIDToVerts_offsets, in verts);
	}

	public float GetTileSize(PlanetTile tile)
	{
		return IntGetTileSize(in tileIDToNeighbors_offsets, in tileIDToNeighbors_values, in tileIDToVerts_offsets, in verts, in tile);
	}

	[BurstCompile]
	private static float IntGetTileSize(in NativeArray<int> tileIDToNeighbors_offsets, in NativeArray<PlanetTile> tileIDToNeighbors_values, in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts, in PlanetTile tile)
	{
		return IntGetTileSize_000153C2_0024BurstDirectCall.Invoke(in tileIDToNeighbors_offsets, in tileIDToNeighbors_values, in tileIDToVerts_offsets, in verts, in tile);
	}

	public Vector3 GetTileCenter(PlanetTile tile)
	{
		if (tile.Layer != this)
		{
			Log.Error($"Attempted to get the center of a tile that is not the same planet layer ({def.label}, ID: {layerId}): {tile} (layer: {tile.LayerDef.label})");
			return Vector3.zero;
		}
		return GetTileCenter(tile.tileId);
	}

	public Vector3 GetTileCenter(int tileID)
	{
		if (tileID < 0 || tileID >= TilesCount)
		{
			Log.Error($"Attempted to access a tile with ID {tileID}, but it is out of range (count: {TilesCount})");
			return Vector3.zero;
		}
		IntGetTileCenter(in tileIDToVerts_offsets, in verts, tileID, out var center);
		return center;
	}

	[BurstCompile]
	private static void IntGetTileCenter(in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts, int tileID, out Vector3 center)
	{
		IntGetTileCenter_000153C5_0024BurstDirectCall.Invoke(in tileIDToVerts_offsets, in verts, tileID, out center);
	}

	public float GetHeadingFromTo(PlanetTile fromTile, PlanetTile toTile)
	{
		return GetHeadingFromTo(fromTile.tileId, toTile.tileId);
	}

	public float GetHeadingFromTo(int fromTileID, int toTileID)
	{
		if (fromTileID == toTileID)
		{
			return 0f;
		}
		Vector3 tileCenter = GetTileCenter(fromTileID);
		Vector3 tileCenter2 = GetTileCenter(toTileID);
		return GetHeadingFromTo(tileCenter, tileCenter2);
	}

	public float GetHeadingFromTo(Vector3 from, Vector3 to)
	{
		if (from == to)
		{
			return 0f;
		}
		Vector3 northPolePos = NorthPolePos;
		WorldRendererUtility.GetTangentialVectorFacing(from, northPolePos, out var forward, out var right);
		WorldRendererUtility.GetTangentialVectorFacing(from, to, out var forward2, out var _);
		float num = Vector3.Angle(forward, forward2);
		if (Vector3.Dot(forward2, right) < 0f)
		{
			num = 360f - num;
		}
		return num;
	}

	public bool InBounds(PlanetTile tile)
	{
		return InBounds(tile.tileId);
	}

	public bool InBounds(int tileID)
	{
		return (uint)tileID < TilesCount;
	}

	public Vector2 LongLatOf(PlanetTile tile)
	{
		return LongLatOf(tile.tileId);
	}

	public Vector2 LongLatOf(int tileID)
	{
		Vector3 tileCenter = GetTileCenter(tileID);
		float x = Mathf.Atan2(tileCenter.x, 0f - tileCenter.z) * 57.29578f;
		float y = Mathf.Asin(tileCenter.y / Radius) * 57.29578f;
		return new Vector2(x, y);
	}

	public Direction8Way GetDirection8WayFromTo(PlanetTile fromTile, PlanetTile toTile)
	{
		return GetDirection8WayFromTo(fromTile.tileId, toTile.tileId);
	}

	public Direction8Way GetDirection8WayFromTo(int fromTileID, int toTileID)
	{
		float headingFromTo = GetHeadingFromTo(fromTileID, toTileID);
		if (headingFromTo >= 337.5f || headingFromTo < 22.5f)
		{
			return Direction8Way.North;
		}
		if (headingFromTo < 67.5f)
		{
			return Direction8Way.NorthEast;
		}
		if (headingFromTo < 112.5f)
		{
			return Direction8Way.East;
		}
		if (headingFromTo < 157.5f)
		{
			return Direction8Way.SouthEast;
		}
		if (headingFromTo < 202.5f)
		{
			return Direction8Way.South;
		}
		if (headingFromTo < 247.5f)
		{
			return Direction8Way.SouthWest;
		}
		if (headingFromTo < 292.5f)
		{
			return Direction8Way.West;
		}
		return Direction8Way.NorthWest;
	}

	public Rot4 GetRotFromTo(PlanetTile fromTile, PlanetTile toTile)
	{
		return GetRotFromTo(fromTile.tileId, toTile.tileId);
	}

	public Rot4 GetRotFromTo(int fromTileID, int toTileID)
	{
		float headingFromTo = GetHeadingFromTo(fromTileID, toTileID);
		if (headingFromTo >= 315f || headingFromTo < 45f)
		{
			return Rot4.North;
		}
		if (headingFromTo < 135f)
		{
			return Rot4.East;
		}
		if (headingFromTo < 225f)
		{
			return Rot4.South;
		}
		return Rot4.West;
	}

	public void GetTileVertices(PlanetTile tile, List<Vector3> outVerts)
	{
		GetTileVertices(tile.tileId, outVerts);
	}

	public void GetTileVertices(int tileID, List<Vector3> outVerts)
	{
		PackedListOfLists.GetList(tileIDToVerts_offsets, verts, tileID, outVerts);
	}

	public void GetTileVerticesIndices(PlanetTile tile, List<int> outVertsIndices)
	{
		GetTileVerticesIndices(tile.tileId, outVertsIndices);
	}

	public void GetTileVerticesIndices(int tileID, List<int> outVertsIndices)
	{
		PackedListOfLists.GetListValuesIndices(tileIDToVerts_offsets, verts, tileID, outVertsIndices);
	}

	public void GetTileNeighbors(PlanetTile tile, List<PlanetTile> outNeighbors)
	{
		GetTileNeighbors(tile.tileId, outNeighbors);
	}

	public void GetTileNeighbors(int tileID, List<PlanetTile> outNeighbors)
	{
		PackedListOfLists.GetList(tileIDToNeighbors_offsets, tileIDToNeighbors_values, tileID, outNeighbors);
	}

	public int GetTileNeighborCount(PlanetTile tile)
	{
		return GetTileNeighborCount(tile.tileId);
	}

	public int GetTileNeighborCount(int tileID)
	{
		return PackedListOfLists.GetListCount(tileIDToNeighbors_offsets, tileIDToNeighbors_values, tileID);
	}

	public int GetMaxTileNeighborCountEver(PlanetTile tile)
	{
		return GetMaxTileNeighborCountEver(tile.tileId);
	}

	public int GetMaxTileNeighborCountEver(int tileID)
	{
		return PackedListOfLists.GetListCount(tileIDToVerts_offsets, verts, tileID);
	}

	public bool IsNeighbor(PlanetTile tileA, PlanetTile tileB)
	{
		return IsNeighbor(tileA.tileId, tileB.tileId);
	}

	public bool IsNeighbor(int tileA, int tileB)
	{
		(int start, int end) listIndexes = PackedListOfLists.GetListIndexes(tileIDToNeighbors_offsets, tileIDToNeighbors_values, tileA);
		int item = listIndexes.start;
		int item2 = listIndexes.end;
		for (int i = item; i < item2; i++)
		{
			if (tileIDToNeighbors_values[i].tileId == tileB)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNeighborId(PlanetTile tileA, PlanetTile tileB)
	{
		return GetNeighborId(tileA.tileId, tileB.tileId);
	}

	public int GetNeighborId(int tileA, int tileB)
	{
		(int start, int end) listIndexes = PackedListOfLists.GetListIndexes(tileIDToNeighbors_offsets, tileIDToNeighbors_values, tileA);
		int item = listIndexes.start;
		int item2 = listIndexes.end;
		int num = item;
		int num2 = 0;
		while (num < item2)
		{
			if (tileIDToNeighbors_values[num].tileId == tileB)
			{
				return num2;
			}
			num++;
			num2++;
		}
		return -1;
	}

	public PlanetTile GetTileNeighbor(PlanetTile tile, int adjacentId)
	{
		return GetTileNeighbor(tile.tileId, adjacentId);
	}

	public PlanetTile GetTileNeighbor(int tileID, int adjacentId)
	{
		var (num, num2) = PackedListOfLists.GetListIndexes(tileIDToNeighbors_offsets, tileIDToNeighbors_values, tileID);
		if (num + adjacentId >= num2)
		{
			Log.Error($"Attempted to get tile neighbour {tileID} with invalid adjacent ID: {adjacentId}, adjacencies: {num2 - num}");
		}
		return tileIDToNeighbors_values[num + adjacentId];
	}

	public bool IsNeighborOrSame(PlanetTile tileA, PlanetTile tileB)
	{
		return IsNeighborOrSame(tileA.tileId, tileB.tileId);
	}

	public bool IsNeighborOrSame(int tileA, int tileB)
	{
		if (tileA != tileB)
		{
			return IsNeighbor(tileA, tileB);
		}
		return true;
	}

	public float TileRadiusToAngle(float radii)
	{
		return DistOnSurfaceToAngle(radii * averageTileSize);
	}

	public float DistOnSurfaceToAngle(float dist)
	{
		return dist / (MathF.PI * 2f * Radius) * 360f;
	}

	public float ApproxDistanceInTiles(float sphericalDistance)
	{
		return sphericalDistance * Radius / averageTileSize;
	}

	public float DistanceFromEquatorNormalized(PlanetTile tile)
	{
		return DistanceFromEquatorNormalized(tile.tileId);
	}

	public float DistanceFromEquatorNormalized(int tile)
	{
		return Mathf.Abs(GetTileCenter(tile).y / Radius);
	}

	public float ApproxDistanceInTiles(PlanetTile tileA, PlanetTile tileB)
	{
		return ApproxDistanceInTiles(tileA.tileId, tileB.tileId);
	}

	public float ApproxDistanceInTiles(int tileA, int tileB)
	{
		Vector3 tileCenter = GetTileCenter(tileA);
		Vector3 tileCenter2 = GetTileCenter(tileB);
		return ApproxDistanceInTiles(GenMath.SphericalDistance(tileCenter.normalized, tileCenter2.normalized));
	}

	private static void ClearAndDeallocateWorkingLists()
	{
		ClearAndDeallocate(ref tmp_tris);
		ClearAndDeallocate(ref tmp_verts);
		ClearAndDeallocate(ref tmp_generatedVerts);
		ClearAndDeallocate(ref tmp_generatedVertOffsets);
		ClearAndDeallocate(ref tmp_generatedTris);
		ClearAndDeallocate(ref tmp_generatedTileVerts);
		ClearAndDeallocate(ref tmp_adjacentTris);
		ClearAndDeallocate(ref tmp_tileIDs);
		ClearAndDeallocate(ref tmp_neighbourVerts);
		ClearAndDeallocate(ref tmp_neighborsToAdd);
		ClearAndDeallocate(ref tmp_vertToTris_offsets);
		ClearAndDeallocate(ref tmp_vertToTris_values);
		ClearAndDeallocate(ref tmp_vertToTileIDs_offsets);
		ClearAndDeallocate(ref tmp_vertToTileIDs_values);
		ClearAndDeallocate(ref tmp_tileIDToVerts_offsets);
		ClearAndDeallocate(ref tmp_tileIDToVerts_values);
	}

	private static void ClearAndDeallocate<T>(ref List<T> list)
	{
		list.Clear();
		list.TrimExcess();
		list = new List<T>();
	}

	public void ExposeData()
	{
		Scribe_Defs.Look(ref def, "def");
		Scribe_Values.Look(ref layerId, "layerId", -1);
		Scribe_Values.Look(ref subdivisions, "subdivisions", 0);
		Scribe_Values.Look(ref scenarioTag, "scenarioTag");
		Scribe_Values.Look(ref isRootSurface, "isRootSurface", defaultValue: false);
		Scribe_Values.Look(ref backgroundWorldCameraOffset, "backgroundWorldCameraOffset", 0f);
		Scribe_Values.Look(ref backgroundWorldCameraParallaxDistancePer100Cells, "backgroundWorldCameraParallaxDistance", 0f);
		Scribe_Values.Look(ref radius, "radius", 0f);
		Scribe_Values.Look(ref viewAngle, "viewAngle", 0f);
		Scribe_Values.Look(ref extraCameraAltitude, "extraCameraAltitude", 0f);
		Scribe_Values.Look(ref viewCenter, "viewCenter");
		Scribe_Values.Look(ref origin, "origin");
		Scribe_References.Look(ref zoomInToLayer, "zoomInToLayer");
		Scribe_References.Look(ref zoomOutToLayer, "zoomOutToLayer");
		Scribe_Collections.Look(ref connections, "connections", LookMode.Reference, LookMode.Deep, ref tmpLayers, ref tmpConnections);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			InitializeLayer();
		}
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			TilesToRawData();
		}
		ExposeBody();
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			RawDataToTiles();
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			finder.RegenerateCache();
		}
	}

	public virtual void Standardize()
	{
		if (tiles.Any())
		{
			TilesToRawData();
		}
		RawDataToTiles();
		finder.RegenerateCache();
	}

	internal virtual void ExposeBody()
	{
		DataExposeUtility.LookByteArray(ref tileBiome, "tileBiome");
	}

	protected virtual void TilesToRawData()
	{
		tileBiome = DataSerializeUtility.SerializeUshort(TilesCount, (int i) => tiles[i].PrimaryBiome.shortHash);
	}

	protected virtual void RawDataToTiles()
	{
		if (tiles.Count != TilesCount)
		{
			Func<PlanetTile, Tile> orCreateTileFactoryMethod = GetOrCreateTileFactoryMethod();
			tiles.Clear();
			for (int i = 0; i < TilesCount; i++)
			{
				Tile tile = orCreateTileFactoryMethod(new PlanetTile(i, this));
				tiles.Add(tile);
				tile.PrimaryBiome = def.DefaultBiome;
				tile.elevation = Radius;
			}
		}
		DataSerializeUtility.LoadUshort(tileBiome, TilesCount, delegate(int index, ushort data)
		{
			tiles[index].PrimaryBiome = DefDatabase<BiomeDef>.GetByShortHash(data) ?? def.DefaultBiome;
		});
	}

	public void SetAllLayersDirty()
	{
		foreach (WorldDrawLayer worldDrawLayer in WorldDrawLayers)
		{
			worldDrawLayer.SetDirty();
		}
	}

	public void SetDirty<T>() where T : WorldDrawLayer
	{
		foreach (WorldDrawLayer worldDrawLayer in WorldDrawLayers)
		{
			if (worldDrawLayer is T)
			{
				worldDrawLayer.SetDirty();
			}
		}
	}

	public T GetLayer<T>() where T : WorldDrawLayer
	{
		foreach (WorldDrawLayer worldDrawLayer in WorldDrawLayers)
		{
			if (worldDrawLayer is T result)
			{
				return result;
			}
		}
		return null;
	}

	public void RegenerateAllLayersNow()
	{
		foreach (WorldDrawLayer worldDrawLayer in WorldDrawLayers)
		{
			if (worldDrawLayer.Visible)
			{
				worldDrawLayer.RegenerateNow();
			}
		}
	}

	public bool LineIntersects(Vector3 A, Vector3 B, float radiusFactor = 1f)
	{
		Vector3 vector = Origin;
		Vector3 lhs = vector - A;
		Vector3 vector2 = B - A;
		float sqrMagnitude = vector2.sqrMagnitude;
		float num = Vector3.Dot(lhs, vector2) / sqrMagnitude;
		if (num < 0f)
		{
			return false;
		}
		if (num > 1f)
		{
			return false;
		}
		Vector3 b = A + vector2 * num;
		return Vector3.Distance(vector, b) < Radius * radiusFactor;
	}

	[Obsolete("Use GetClosestTile_NewTemp")]
	public PlanetTile GetClosestTile(PlanetTile other)
	{
		return GetClosestTile_NewTemp(other);
	}

	public PlanetTile GetClosestTile_NewTemp(PlanetTile other, bool validSettlement = false)
	{
		if (other.Layer == this)
		{
			return other;
		}
		FastTileFinder.TileQueryParams query = new FastTileFinder.TileQueryParams(other, 0f, float.MaxValue, FastTileFinder.LandmarkMode.Any, reachable: true, Hilliness.Undefined, Hilliness.Undefined, checkBiome: true, validSettlement);
		if (!FastTileFinder.Closest(query, out var tile))
		{
			return PlanetTile.Invalid;
		}
		return tile;
	}

	public bool DirectConnectionTo(PlanetLayer other)
	{
		return connections.ContainsKey(other);
	}

	public bool HasConnectionPathTo(PlanetLayer other)
	{
		bool result = IsConnectedToRecursive(other);
		TmpVisited.Clear();
		return result;
	}

	private bool IsConnectedToRecursive(PlanetLayer other)
	{
		TmpVisited.Add(this);
		if (connections.ContainsKey(other))
		{
			return true;
		}
		foreach (var (planetLayer2, _) in connections)
		{
			if (!TmpVisited.Contains(planetLayer2) && planetLayer2.IsConnectedToRecursive(other))
			{
				return true;
			}
		}
		return false;
	}

	private static void ResetPathVars()
	{
		openSet.Clear();
		cameFrom.Clear();
		gScore.Clear();
		toEnqueue.Clear();
	}

	private Func<PlanetTile, Tile> GetOrCreateTileFactoryMethod()
	{
		if (tileCreatorFunc != null)
		{
			return tileCreatorFunc;
		}
		ConstructorInfo constructor = def.tileType.GetConstructor(new Type[1] { typeof(PlanetTile) });
		if (constructor == null)
		{
			throw new Exception($"Unable to deserialize tiles of type {def.tileType} because it does not have a constructor that takes a single PlanetTile argument.");
		}
		DynamicMethod dynamicMethod = new DynamicMethod("DynamicMethod_CreateTile_" + def.tileType.Name, typeof(Tile), new Type[1] { typeof(PlanetTile) }, typeof(PlanetLayer), skipVisibility: true);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		iLGenerator.Emit(OpCodes.Ldarg_0);
		iLGenerator.Emit(OpCodes.Newobj, constructor);
		iLGenerator.Emit(OpCodes.Ret);
		tileCreatorFunc = (Func<PlanetTile, Tile>)dynamicMethod.CreateDelegate(typeof(Func<PlanetTile, Tile>));
		return tileCreatorFunc;
	}

	public bool TryGetPath(PlanetLayer goal, List<PlanetLayerConnection> path, out float cost, int max = int.MaxValue)
	{
		return TryGetPath(this, goal, path, out cost, max);
	}

	public static bool TryGetPath(PlanetLayer start, PlanetLayer goal, List<PlanetLayerConnection> path, out float cost, int max = int.MaxValue)
	{
		ResetPathVars();
		gScore.Add(start, 0);
		openSet.Enqueue(start, gScore[start]);
		while (openSet.Count != 0)
		{
			PlanetLayer planetLayer = openSet.Dequeue();
			if (planetLayer == goal)
			{
				ReconstructPath(path, cameFrom, planetLayer);
				cost = path.Sum((PlanetLayerConnection x) => x.fuelCost);
				ResetPathVars();
				return true;
			}
			toEnqueue.Clear();
			foreach (var (planetLayer3, planetLayerConnection2) in planetLayer.connections)
			{
				if (!planetLayer3.CanReachLayer)
				{
					continue;
				}
				if (planetLayer3 == goal)
				{
					cameFrom[planetLayer3] = planetLayer;
					ReconstructPath(path, cameFrom, planetLayer3);
					cost = path.Sum((PlanetLayerConnection x) => x.fuelCost);
					ResetPathVars();
					return true;
				}
				int num = gScore[planetLayer] + (int)planetLayerConnection2.fuelCost;
				if (num > max)
				{
					break;
				}
				if (!gScore.ContainsKey(planetLayer3) || num < gScore[planetLayer3])
				{
					cameFrom[planetLayer3] = planetLayer;
					gScore[planetLayer3] = num;
					openSet.Enqueue(planetLayer3, gScore[planetLayer3]);
					toEnqueue.Add(planetLayer3);
				}
			}
			foreach (PlanetLayer item in toEnqueue)
			{
				openSet.Enqueue(item, gScore[item]);
			}
		}
		cost = 0f;
		ResetPathVars();
		return false;
	}

	private static void ReconstructPath(List<PlanetLayerConnection> path, Dictionary<PlanetLayer, PlanetLayer> from, PlanetLayer current)
	{
		path.Clear();
		path.Add(from[current].GetConnectionFromTo(current));
		while (from.ContainsKey(from[current]))
		{
			current = from[current];
			path.Add(from[current].GetConnectionFromTo(current));
		}
		path.Reverse();
	}

	public void Dispose()
	{
		foreach (WorldDrawLayer worldDrawLayer in WorldDrawLayers)
		{
			worldDrawLayer.Dispose();
		}
		finder?.Dispose();
		pather?.Dispose();
		NativeArrayUtility.EnsureDisposed(ref verts);
		NativeArrayUtility.EnsureDisposed(ref tileIDToVerts_offsets);
		NativeArrayUtility.EnsureDisposed(ref tileIDToNeighbors_offsets);
		NativeArrayUtility.EnsureDisposed(ref tileIDToNeighbors_values);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	public static float CalculateAverageTileSize_0024BurstManaged(in NativeArray<int> tileIDToNeighbors_offsets, in NativeArray<PlanetTile> tileIDToNeighbors_values, in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts)
	{
		if (tileIDToNeighbors_offsets.Length == 0)
		{
			return 1f;
		}
		int length = tileIDToNeighbors_offsets.Length;
		double num = 0.0;
		int num2 = 0;
		for (int i = 0; i < length; i++)
		{
			IntGetTileCenter(in tileIDToVerts_offsets, in verts, i, out var center);
			(int start, int end) listIndexes = PackedListOfLists.GetListIndexes(tileIDToNeighbors_offsets, tileIDToNeighbors_values, i);
			int item = listIndexes.start;
			int item2 = listIndexes.end;
			for (int j = item; j < item2; j++)
			{
				int tileId = tileIDToNeighbors_values[j].tileId;
				IntGetTileCenter(in tileIDToVerts_offsets, in verts, tileId, out var center2);
				num += (double)Vector3.Distance(center, center2);
				num2++;
			}
		}
		return (float)(num / (double)num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	public static float IntGetTileSize_0024BurstManaged(in NativeArray<int> tileIDToNeighbors_offsets, in NativeArray<PlanetTile> tileIDToNeighbors_values, in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts, in PlanetTile tile)
	{
		double num = 0.0;
		int num2 = 0;
		IntGetTileCenter(in tileIDToVerts_offsets, in verts, tile, out var center);
		int num3 = (((int)tile + 1 < tileIDToNeighbors_offsets.Length) ? tileIDToNeighbors_offsets[(int)tile + 1] : tileIDToNeighbors_values.Length);
		for (int i = tileIDToNeighbors_offsets[tile]; i < num3; i++)
		{
			int tileId = tileIDToNeighbors_values[i].tileId;
			IntGetTileCenter(in tileIDToVerts_offsets, in verts, tileId, out var center2);
			num += (double)Vector3.Distance(center, center2);
			num2++;
		}
		return (float)(num / (double)num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	public static void IntGetTileCenter_0024BurstManaged(in NativeArray<int> tileIDToVerts_offsets, in NativeArray<Vector3> verts, int tileID, out Vector3 center)
	{
		if (tileID < 0 || tileID >= tileIDToVerts_offsets.Length)
		{
			center = Vector3.zero;
			return;
		}
		if (!tileIDToVerts_offsets.IsCreated)
		{
			center = Vector3.zero;
			return;
		}
		(int start, int end) listIndexes = PackedListOfLists.GetListIndexes(tileIDToVerts_offsets, verts, tileID);
		int item = listIndexes.start;
		int item2 = listIndexes.end;
		Vector3 zero = Vector3.zero;
		int num = 0;
		for (int i = item; i < item2; i++)
		{
			zero += verts[i];
			num++;
		}
		center = zero / num;
	}
}
