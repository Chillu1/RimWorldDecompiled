using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldGrid : IExposable, IDisposable
{
	private Dictionary<int, PlanetLayer> planetLayers = new Dictionary<int, PlanetLayer>();

	private Vector3 surfaceViewCenter;

	private float surfaceViewAngle;

	private int nextLayerId;

	private SurfaceLayer surface;

	private PlanetLayer orbit;

	private readonly List<WorldDrawLayerBase> globalLayers = new List<WorldDrawLayerBase>();

	private readonly List<PlanetTile> tmpNeighbors = new List<PlanetTile>();

	public const int SubdivisionsCount = 10;

	public const float PlanetRadius = 100f;

	private int cachedTraversalDistance = -1;

	private PlanetTile cachedTraversalDistanceForStart = PlanetTile.Invalid;

	private PlanetTile cachedTraversalDistanceForEnd = PlanetTile.Invalid;

	private PlanetLayer cachedLayer;

	private static readonly List<int> tmpLayerIds = new List<int>();

	public SurfaceLayer Surface => surface;

	public PlanetLayer Orbit => orbit;

	public IReadOnlyList<WorldDrawLayerBase> GlobalLayers => globalLayers;

	public float SurfaceViewAngle => surfaceViewAngle;

	public Vector3 SurfaceViewCenter => surfaceViewCenter;

	public int TilesCount => surface.TilesCount;

	public IEnumerable<SurfaceTile> Tiles => surface.Tiles.Cast<SurfaceTile>();

	public IReadOnlyDictionary<int, PlanetLayer> PlanetLayers => planetLayers;

	public NativeArray<Vector3> UnsafeVerts => surface.UnsafeVerts;

	public NativeArray<int> UnsafeTileIDToVerts_offsets => surface.UnsafeTileIDToVerts_offsets;

	public NativeArray<int> UnsafeTileIDToNeighbors_offsets => surface.UnsafeTileIDToNeighbors_offsets;

	public NativeArray<PlanetTile> UnsafeTileIDToNeighbors_values => surface.UnsafeTileIDToNeighbors_values;

	public Vector3 NorthPolePos => surface.NorthPolePos;

	public SurfaceTile this[int tileID] => surface[tileID];

	public Tile this[PlanetTile tile] => tile.Layer.Tiles[tile.tileId];

	public bool HasWorldData => surface.HasWorldData;

	public float AverageTileSize => surface.AverageTileSize;

	public event Action<PlanetLayer> OnPlanetLayerAdded;

	public event Action<PlanetLayer> OnPlanetLayerRemoved;

	public void GenerateFreshWorld()
	{
		CalculateViewCenterAndAngle();
		CreateRequiredLayers();
	}

	private void InitializeGlobalLayers()
	{
		globalLayers.Clear();
		foreach (GlobalWorldDrawLayerDef allDef in DefDatabase<GlobalWorldDrawLayerDef>.AllDefs)
		{
			globalLayers.Add((WorldDrawLayerBase)Activator.CreateInstance(allDef.worldDrawLayer));
		}
	}

	private void CreateRequiredLayers()
	{
		InitializeGlobalLayers();
		Dictionary<PlanetLayer, List<LayerConnection>> dictionary = new Dictionary<PlanetLayer, List<LayerConnection>>();
		foreach (ScenPart allPart in Find.Scenario.AllParts)
		{
			if (!(allPart is ScenPart_PlanetLayer scenPart_PlanetLayer))
			{
				continue;
			}
			bool flag = false;
			foreach (PlanetLayer value in planetLayers.Values)
			{
				if (!(value.ScenarioTag != scenPart_PlanetLayer.tag))
				{
					dictionary[value] = scenPart_PlanetLayer.connections;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				PlanetLayer planetLayer;
				if (scenPart_PlanetLayer == Find.Scenario.surfaceLayer)
				{
					PlanetLayerDef layer = scenPart_PlanetLayer.layer;
					PlanetLayerSettings settings = scenPart_PlanetLayer.Settings;
					Vector3? overrideViewCenter = surfaceViewCenter;
					float? overrideViewAngle = surfaceViewAngle;
					planetLayer = (surface = (SurfaceLayer)RegisterPlanetLayer(layer, settings, null, null, overrideViewCenter, overrideViewAngle));
					surface.MarkRootSurface();
				}
				else
				{
					planetLayer = RegisterPlanetLayer(scenPart_PlanetLayer.layer, scenPart_PlanetLayer.Settings);
				}
				if (ModsConfig.OdysseyActive && scenPart_PlanetLayer.layer == PlanetLayerDefOf.Orbit)
				{
					orbit = planetLayer;
				}
				planetLayer.ScenarioTag = scenPart_PlanetLayer.tag;
				dictionary[planetLayer] = scenPart_PlanetLayer.connections;
			}
		}
		foreach (var (planetLayer3, list2) in dictionary)
		{
			foreach (LayerConnection item in list2)
			{
				if (TryGetLayerByTag(item.tag, out var layer2) && !planetLayer3.HasConnectionFromTo(layer2))
				{
					planetLayer3.AddConnection(layer2, item.fuelCost);
					if (item.zoomMode == LayerConnection.ZoomMode.ZoomIn)
					{
						planetLayer3.zoomInToLayer = layer2;
					}
					else if (item.zoomMode == LayerConnection.ZoomMode.ZoomOut)
					{
						planetLayer3.zoomOutToLayer = layer2;
					}
				}
			}
		}
	}

	public bool TryGetLayerByTag(string tag, out PlanetLayer layer)
	{
		for (int i = 0; i < planetLayers.Count; i++)
		{
			if (planetLayers[i].ScenarioTag == tag)
			{
				layer = planetLayers[i];
				return true;
			}
		}
		layer = null;
		return false;
	}

	public Vector2 LongLatOf(PlanetTile tile)
	{
		if (!tile.Valid)
		{
			if (Find.AnyPlayerHomeMap == null)
			{
				return Vector2.zero;
			}
			tile = Find.AnyPlayerHomeMap.Tile;
		}
		return tile.Layer.LongLatOf(tile);
	}

	public PlanetLayer RegisterPlanetLayer(PlanetLayerDef layer, PlanetLayerSettings settings, float? overrideRadius = null, Vector3? overrideOrigin = null, Vector3? overrideViewCenter = null, float? overrideViewAngle = null)
	{
		float radius = overrideRadius ?? settings.radius;
		Vector3 value = overrideOrigin ?? settings.origin;
		Vector3 value2 = overrideViewCenter ?? surfaceViewCenter;
		float viewAngle = overrideViewAngle ?? (settings.useSurfaceViewAngle ? surfaceViewAngle : settings.viewAngle);
		return RegisterPlanetLayer(layer, value, radius, viewAngle, settings.extraCameraAltitude, settings.subdivisions, settings.backgroundWorldCameraOffset, settings.backgroundWorldCameraParallaxDistancePer100Cells, value2);
	}

	public PlanetLayer RegisterPlanetLayer(PlanetLayerDef def, Vector3? origin = null, float radius = 100f, float viewAngle = 180f, float extraCameraAltitude = 0f, int subdivisions = 10, float backgroundWorldCameraOffset = 0f, float backgroundWorldCameraParallaxDistancePer100Cells = 0f, Vector3? overrideViewCenter = null)
	{
		int num = nextLayerId++;
		Vector3 vector = origin ?? Vector3.zero;
		Vector3 vector2 = overrideViewCenter ?? surfaceViewCenter;
		PlanetLayer planetLayer = (planetLayers[num] = (PlanetLayer)Activator.CreateInstance(def.layerType, num, def, radius, vector, vector2, viewAngle, subdivisions, extraCameraAltitude, backgroundWorldCameraOffset, backgroundWorldCameraParallaxDistancePer100Cells));
		PlanetLayer planetLayer3 = planetLayer;
		planetLayer3.InitializeLayer();
		this.OnPlanetLayerAdded?.Invoke(planetLayer3);
		return planetLayer3;
	}

	public void RemovePlanetLayer(PlanetLayer layer)
	{
		if (!planetLayers.Remove(layer.LayerID))
		{
			return;
		}
		this.OnPlanetLayerRemoved?.Invoke(layer);
		foreach (var (_, planetLayer2) in PlanetLayers)
		{
			if (planetLayer2 != layer)
			{
				planetLayer2.RemoveConnection(layer);
			}
		}
		foreach (WorldObject item in Find.WorldObjects.AllWorldObjectsOnLayer(layer))
		{
			item.Destroy();
		}
		layer.Dispose();
	}

	public bool TryGetFirstLayerOfDef(PlanetLayerDef def, out PlanetLayer layer)
	{
		layer = FirstLayerOfDef(def);
		return layer != null;
	}

	public bool TryGetFirstAdjacentLayerOfDef(PlanetTile origin, PlanetLayerDef def, out PlanetLayer layer)
	{
		if (origin.LayerDef == def)
		{
			layer = origin.Layer;
			return true;
		}
		for (int i = 0; i < planetLayers.Count; i++)
		{
			if (planetLayers[i].Def == def && planetLayers[i].DirectConnectionTo(origin.Layer))
			{
				layer = planetLayers[i];
				return true;
			}
		}
		layer = null;
		return false;
	}

	public PlanetLayer FirstLayerOfDef(PlanetLayerDef def)
	{
		if (def == null)
		{
			return surface;
		}
		for (int i = 0; i < planetLayers.Count; i++)
		{
			if (planetLayers[i].Def == def)
			{
				return planetLayers[i];
			}
		}
		return null;
	}

	public Vector3 GetTileCenter(PlanetTile tile)
	{
		return tile.Layer.GetTileCenter(tile);
	}

	public bool InBounds(PlanetTile tile)
	{
		return tile.Layer.InBounds(tile);
	}

	public float GetHeadingFromTo(Vector3 from, Vector3 to)
	{
		return GetHeadingFromTo(surface, from, to);
	}

	public float GetHeadingFromTo(PlanetLayer layer, Vector3 from, Vector3 to)
	{
		return layer.GetHeadingFromTo(from, to);
	}

	public float GetHeadingFromTo(PlanetTile fromTile, PlanetTile toTile)
	{
		return fromTile.Layer.GetHeadingFromTo(fromTile, toTile);
	}

	public Direction8Way GetDirection8WayFromTo(PlanetTile fromTile, PlanetTile toTile)
	{
		return fromTile.Layer.GetDirection8WayFromTo(fromTile, toTile);
	}

	public Rot4 GetRotFromTo(PlanetTile fromTile, PlanetTile toTile)
	{
		return fromTile.Layer.GetRotFromTo(fromTile, toTile);
	}

	public void GetTileVertices(PlanetTile tile, List<Vector3> outVerts)
	{
		tile.Layer.GetTileVertices(tile, outVerts);
	}

	public void GetTileVerticesIndices(PlanetTile tile, List<int> outVertsIndices)
	{
		tile.Layer.GetTileVerticesIndices(tile, outVertsIndices);
	}

	public void GetTileNeighbors(PlanetTile tile, List<PlanetTile> outNeighbors)
	{
		tile.Layer.GetTileNeighbors(tile, outNeighbors);
	}

	public int GetTileNeighborCount(PlanetTile tile)
	{
		return tile.Layer.GetTileNeighborCount(tile);
	}

	public int GetMaxTileNeighborCountEver(PlanetTile tile)
	{
		return tile.Layer.GetMaxTileNeighborCountEver(tile);
	}

	public bool IsNeighbor(PlanetTile tileA, PlanetTile tileB)
	{
		return tileA.Layer.IsNeighbor(tileA, tileB);
	}

	public bool IsNeighborOrSame(PlanetTile tileA, PlanetTile tileB)
	{
		return tileA.Layer.IsNeighborOrSame(tileA, tileB);
	}

	public int GetNeighborId(PlanetTile tileA, PlanetTile tileB)
	{
		return tileA.Layer.GetNeighborId(tileA, tileB);
	}

	public PlanetTile GetTileNeighbor(PlanetTile tile, int adjacentId)
	{
		return tile.Layer.GetTileNeighbor(tile, adjacentId);
	}

	public float TileRadiusToAngle(float radius)
	{
		return TileRadiusToAngle(surface, radius);
	}

	public float TileRadiusToAngle(PlanetLayer layer, float radius)
	{
		return layer.TileRadiusToAngle(radius);
	}

	public float DistOnSurfaceToAngle(float dist)
	{
		return DistOnSurfaceToAngle(surface, dist);
	}

	public float DistOnSurfaceToAngle(PlanetLayer layer, float dist)
	{
		return layer.DistOnSurfaceToAngle(dist);
	}

	public float DistanceFromEquatorNormalized(PlanetTile tile)
	{
		return tile.Layer.DistanceFromEquatorNormalized(tile);
	}

	public float ApproxDistanceInTiles(float sphericalDistance)
	{
		return ApproxDistanceInTiles(surface, sphericalDistance);
	}

	public float ApproxDistanceInTiles(PlanetLayer layer, float sphericalDistance)
	{
		return layer.ApproxDistanceInTiles(sphericalDistance);
	}

	public float ApproxDistanceInTiles(PlanetTile tileA, PlanetTile tileB)
	{
		return tileA.Layer.ApproxDistanceInTiles(tileA, tileB);
	}

	public void OverlayRoad(PlanetTile fromTile, PlanetTile toTile, RoadDef roadDef)
	{
		if (roadDef == null)
		{
			Log.ErrorOnce("Attempted to remove road with overlayRoad; not supported", 90292249);
			return;
		}
		RoadDef roadDef2 = GetRoadDef(fromTile, toTile, visibleOnly: false);
		if (roadDef2 == roadDef)
		{
			return;
		}
		Tile obj = this[fromTile];
		Tile obj2 = this[toTile];
		if (obj.Isnt<SurfaceTile>(out var casted) || obj2.Isnt<SurfaceTile>(out var casted2))
		{
			return;
		}
		if (roadDef2 != null)
		{
			if (roadDef2.priority >= roadDef.priority)
			{
				return;
			}
			casted.potentialRoads.RemoveAll((SurfaceTile.RoadLink rl) => rl.neighbor == toTile);
			casted2.potentialRoads.RemoveAll((SurfaceTile.RoadLink rl) => rl.neighbor == fromTile);
		}
		if (casted.potentialRoads == null)
		{
			casted.potentialRoads = new List<SurfaceTile.RoadLink>();
		}
		if (casted2.potentialRoads == null)
		{
			casted2.potentialRoads = new List<SurfaceTile.RoadLink>();
		}
		casted.potentialRoads.Add(new SurfaceTile.RoadLink
		{
			neighbor = toTile,
			road = roadDef
		});
		casted2.potentialRoads.Add(new SurfaceTile.RoadLink
		{
			neighbor = fromTile,
			road = roadDef
		});
	}

	public RoadDef GetRoadDef(PlanetTile fromTile, PlanetTile toTile, bool visibleOnly = true)
	{
		if (!IsNeighbor(fromTile, toTile))
		{
			Log.ErrorOnce("Tried to find road information between non-neighboring tiles", 12390444);
			return null;
		}
		if (this[fromTile].Isnt<SurfaceTile>(out var casted))
		{
			return null;
		}
		List<SurfaceTile.RoadLink> list = (visibleOnly ? casted.Roads : casted.potentialRoads);
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].neighbor == toTile)
			{
				return list[i].road;
			}
		}
		return null;
	}

	public void OverlayRiver(PlanetTile fromTile, PlanetTile toTile, RiverDef riverDef)
	{
		if (riverDef == null)
		{
			Log.ErrorOnce("Attempted to remove river with overlayRiver; not supported", 90292250);
			return;
		}
		RiverDef riverDef2 = GetRiverDef(fromTile, toTile, visibleOnly: false);
		if (riverDef2 == riverDef)
		{
			return;
		}
		Tile obj = this[fromTile];
		Tile obj2 = this[toTile];
		if (obj.Isnt<SurfaceTile>(out var casted) || obj2.Isnt<SurfaceTile>(out var casted2))
		{
			return;
		}
		if (riverDef2 != null)
		{
			if (riverDef2.degradeThreshold >= riverDef.degradeThreshold)
			{
				return;
			}
			casted.potentialRivers.RemoveAll((SurfaceTile.RiverLink rl) => rl.neighbor == toTile);
			casted2.potentialRivers.RemoveAll((SurfaceTile.RiverLink rl) => rl.neighbor == fromTile);
		}
		SurfaceTile surfaceTile = casted;
		if (surfaceTile.potentialRivers == null)
		{
			surfaceTile.potentialRivers = new List<SurfaceTile.RiverLink>();
		}
		surfaceTile = casted2;
		if (surfaceTile.potentialRivers == null)
		{
			surfaceTile.potentialRivers = new List<SurfaceTile.RiverLink>();
		}
		casted.potentialRivers.Add(new SurfaceTile.RiverLink
		{
			neighbor = toTile,
			river = riverDef
		});
		casted2.potentialRivers.Add(new SurfaceTile.RiverLink
		{
			neighbor = fromTile,
			river = riverDef
		});
		casted2.riverDist = Mathf.Max(casted2.riverDist, casted.riverDist + 1);
	}

	public RiverDef GetRiverDef(PlanetTile fromTile, PlanetTile toTile, bool visibleOnly = true)
	{
		if (!IsNeighbor(fromTile, toTile))
		{
			Log.ErrorOnce("Tried to find river information between non-neighboring tiles", 12390444);
			return null;
		}
		SurfaceTile surfaceTile = (SurfaceTile)this[fromTile];
		List<SurfaceTile.RiverLink> list = (visibleOnly ? surfaceTile.Rivers : surfaceTile.potentialRivers);
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].neighbor == toTile)
			{
				return list[i].river;
			}
		}
		return null;
	}

	public float GetRoadMovementDifficultyMultiplier(PlanetTile fromTile, PlanetTile toTile, StringBuilder explanation = null)
	{
		List<SurfaceTile.RoadLink> roads = ((SurfaceTile)this[fromTile]).Roads;
		if (roads == null)
		{
			return 1f;
		}
		if (!toTile.Valid)
		{
			toTile = FindMostReasonableAdjacentTileForDisplayedPathCost(fromTile);
		}
		for (int i = 0; i < roads.Count; i++)
		{
			if (!(roads[i].neighbor == toTile))
			{
				continue;
			}
			float movementCostMultiplier = roads[i].road.movementCostMultiplier;
			if (explanation != null)
			{
				if (explanation.Length > 0)
				{
					explanation.AppendLine();
				}
				explanation.Append($"{roads[i].road.LabelCap}: {movementCostMultiplier.ToStringPercent()}");
			}
			return movementCostMultiplier;
		}
		return 1f;
	}

	public PlanetTile FindMostReasonableAdjacentTileForDisplayedPathCost(PlanetTile fromTile)
	{
		SurfaceTile obj = (SurfaceTile)this[fromTile];
		float num = 1f;
		PlanetTile planetTile = PlanetTile.Invalid;
		List<SurfaceTile.RoadLink> roads = obj.Roads;
		if (roads != null)
		{
			for (int i = 0; i < roads.Count; i++)
			{
				float movementCostMultiplier = roads[i].road.movementCostMultiplier;
				if (movementCostMultiplier < num && !Find.World.Impassable(roads[i].neighbor))
				{
					num = movementCostMultiplier;
					planetTile = roads[i].neighbor;
				}
			}
		}
		if (planetTile != PlanetTile.Invalid)
		{
			return planetTile;
		}
		tmpNeighbors.Clear();
		GetTileNeighbors(fromTile, tmpNeighbors);
		for (int j = 0; j < tmpNeighbors.Count; j++)
		{
			if (!Find.World.Impassable(tmpNeighbors[j]))
			{
				return tmpNeighbors[j];
			}
		}
		return fromTile;
	}

	public int TraversalDistanceBetween(PlanetTile start, PlanetTile end, bool passImpassable = true, int maxDist = int.MaxValue, bool canTraverseLayers = false)
	{
		if (start == end)
		{
			return 0;
		}
		if (!start.Valid || !end.Valid)
		{
			return int.MaxValue;
		}
		PlanetTile planetTile = start;
		if (cachedTraversalDistanceForStart == start && cachedTraversalDistanceForEnd == end && cachedLayer == end.Layer && passImpassable && maxDist == int.MaxValue)
		{
			return cachedTraversalDistance;
		}
		if (start.Layer != end.Layer)
		{
			if (!canTraverseLayers)
			{
				return int.MaxValue;
			}
			cachedLayer = end.Layer;
			start = end.Layer.GetClosestTile_NewTemp(start);
		}
		if (!passImpassable && !Find.WorldReachability.CanReach(start, end))
		{
			return int.MaxValue;
		}
		int finalDist = int.MaxValue;
		int maxTilesToProcess = ((maxDist == int.MaxValue) ? int.MaxValue : TilesNumWithinTraversalDistance(maxDist + 1));
		start.Layer.Filler.FloodFill(start, (PlanetTile x) => passImpassable || !Find.World.Impassable(x), delegate(PlanetTile tile, int dist)
		{
			if (tile == end)
			{
				finalDist = dist;
				return true;
			}
			return false;
		}, maxTilesToProcess);
		if (passImpassable && maxDist == int.MaxValue)
		{
			cachedTraversalDistance = finalDist;
			cachedTraversalDistanceForStart = planetTile;
			cachedTraversalDistanceForEnd = end;
		}
		return finalDist;
	}

	public int TilesNumWithinTraversalDistance(int traversalDist)
	{
		if (traversalDist < 0)
		{
			return 0;
		}
		return 3 * traversalDist * (traversalDist + 1) + 1;
	}

	public bool IsOnEdge(PlanetTile tileID)
	{
		if (InBounds(tileID))
		{
			return GetTileNeighborCount(tileID) < GetMaxTileNeighborCountEver(tileID);
		}
		return false;
	}

	private void CalculateViewCenterAndAngle()
	{
		surfaceViewAngle = Find.World.PlanetCoverage * 180f;
		surfaceViewCenter = Vector3.back;
		float angle = 45f;
		if (surfaceViewAngle > 45f)
		{
			angle = Mathf.Max(90f - surfaceViewAngle, 0f);
		}
		surfaceViewCenter = Quaternion.AngleAxis(angle, Vector3.right) * surfaceViewCenter;
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref nextLayerId, "nextLayerId", 0);
		Scribe_Collections.Look(ref planetLayers, "layers", LookMode.Value, LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			EnsureNextLayerIDValid();
			CalculateViewCenterAndAngle();
			if (planetLayers.NullOrEmpty())
			{
				planetLayers = new Dictionary<int, PlanetLayer>();
				CreateRequiredLayers();
				surface.ExposeBody();
				StandardizeTileData();
			}
			else
			{
				int key;
				PlanetLayer value;
				foreach (KeyValuePair<int, PlanetLayer> planetLayer3 in planetLayers)
				{
					planetLayer3.Deconstruct(out key, out value);
					int item = key;
					PlanetLayer planetLayer = value;
					if (planetLayer.Def == null)
					{
						tmpLayerIds.Add(item);
						continue;
					}
					if (planetLayer.IsRootSurface)
					{
						surface = (SurfaceLayer)planetLayer;
					}
					if (ModsConfig.OdysseyActive && planetLayer.Def == PlanetLayerDefOf.Orbit)
					{
						orbit = planetLayer;
					}
				}
				if (surface == null)
				{
					foreach (KeyValuePair<int, PlanetLayer> planetLayer4 in planetLayers)
					{
						planetLayer4.Deconstruct(out key, out value);
						PlanetLayer planetLayer2 = value;
						if (planetLayer2.Def == PlanetLayerDefOf.Surface)
						{
							surface = (SurfaceLayer)planetLayer2;
							surface.MarkRootSurface();
							break;
						}
					}
				}
				foreach (int tmpLayerId in tmpLayerIds)
				{
					planetLayers.Remove(tmpLayerId);
				}
				tmpLayerIds.Clear();
			}
		}
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			CreateRequiredLayers();
			StandardizeTileData();
		}
	}

	private void EnsureNextLayerIDValid()
	{
		if (planetLayers.NullOrEmpty())
		{
			return;
		}
		foreach (var (num2, _) in planetLayers)
		{
			if (num2 >= nextLayerId)
			{
				nextLayerId = num2 + 1;
			}
		}
	}

	public void StandardizeTileData()
	{
		foreach (var (_, planetLayer2) in planetLayers)
		{
			if (planetLayer2.Tiles.Empty())
			{
				planetLayer2.Standardize();
			}
		}
	}

	public IEnumerable<Gizmo> GetGizmos()
	{
		if (Current.ProgramState == ProgramState.Entry)
		{
			yield break;
		}
		foreach (KeyValuePair<int, PlanetLayer> planetLayer2 in planetLayers)
		{
			var (_, layer) = (KeyValuePair<int, PlanetLayer>)(ref planetLayer2);
			if (!layer.IsSelected && (!layer.Def.viewGizmoOnlyVisibleWithDirectConnection || (Find.WorldSelector.SelectedLayer != null && Find.WorldSelector.SelectedLayer.HasConnectionFromTo(layer))))
			{
				AcceptanceReport acceptanceReport = layer.CanSelectLayer();
				Command_Action command_Action = new Command_Action
				{
					defaultLabel = "WorldSelectLayer".Translate(layer.Def.Named("LAYER")),
					hotKey = KeyBindingDefOf.Misc1,
					defaultDesc = layer.Def.viewGizmoTooltip,
					icon = layer.Def.ViewGizmoTexture,
					action = delegate
					{
						PlanetLayer.Selected = layer;
					}
				};
				if (!acceptanceReport.Accepted)
				{
					command_Action.Disable(acceptanceReport.Reason);
				}
				yield return command_Action;
			}
		}
		if (!Find.WindowStack.IsOpen<WorldInspectPane>() && WorldGizmoUtility.TryGetCaravanGizmo(out var gizmo))
		{
			yield return gizmo;
		}
		yield return WorldGizmoUtility.GetJumpToGizmo();
	}

	public void Dispose()
	{
		foreach (KeyValuePair<int, PlanetLayer> planetLayer in planetLayers)
		{
			planetLayer.Deconstruct(out var _, out var value);
			value.Dispose();
		}
	}
}
