using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Dialog_WorldSearch : Dialog_Search<WorldSearchElement>
{
	private readonly List<WorldSearchElement> searchSet;

	private readonly HashSet<WorldObject> listedWorldObjects = new HashSet<WorldObject>();

	private readonly HashSet<PlanetTile> listedTiles = new HashSet<PlanetTile>();

	protected override bool ShouldClose => WorldRendererUtility.DrawingMap;

	protected override List<WorldSearchElement> SearchSet => searchSet;

	protected override TaggedString SearchLabel => "SearchTheWorld".Translate();

	public bool IsListed(WorldObject wo)
	{
		return listedWorldObjects.Contains(wo);
	}

	public bool IsListed(PlanetTile tile)
	{
		return listedTiles.Contains(tile);
	}

	public Dialog_WorldSearch()
	{
		searchSet = new List<WorldSearchElement>();
		InitializeSearchSet();
	}

	private void InitializeSearchSet()
	{
		searchSet.Clear();
		foreach (WorldObject allWorldObject in Find.World.worldObjects.AllWorldObjects)
		{
			WorldSearchElement worldSearchElement = new WorldSearchElement
			{
				tile = allWorldObject.Tile,
				worldObject = allWorldObject
			};
			if (ModsConfig.OdysseyActive)
			{
				worldSearchElement.landmark = Find.World.landmarks.landmarks.TryGetValue(allWorldObject.Tile);
			}
			worldSearchElement.mutators = Find.World.grid[allWorldObject.Tile].mutatorsNullable;
			searchSet.Add(worldSearchElement);
		}
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		foreach (KeyValuePair<PlanetTile, Landmark> landmark in Find.World.landmarks.landmarks)
		{
			if (!searchSet.Any((WorldSearchElement o) => o.tile == landmark.Key))
			{
				WorldSearchElement item = new WorldSearchElement
				{
					tile = landmark.Key,
					landmark = landmark.Value,
					mutators = Find.World.grid[landmark.Key].mutatorsNullable
				};
				searchSet.Add(item);
			}
		}
	}

	protected override void TryAddElement(WorldSearchElement element)
	{
		if (searchResultsSet.Add(element) && ElementMatch(element))
		{
			searchResults.Add(element.DisplayLabel.ToLower(), element);
			if (element.worldObject != null)
			{
				listedWorldObjects.Add(element.worldObject);
			}
			if (element.tile.Valid)
			{
				listedTiles.Add(element.tile);
			}
			SetInitialSizeAndPosition();
		}
	}

	public override void Notify_CommonSearchChanged()
	{
		base.Notify_CommonSearchChanged();
		listedWorldObjects.Clear();
		listedTiles.Clear();
	}

	private bool ElementMatch(WorldSearchElement element)
	{
		if (element.worldObject != null && TextMatch(element.worldObject.Label))
		{
			return true;
		}
		if (element.landmark != null)
		{
			if (TextMatch(element.landmark.name))
			{
				return true;
			}
			if (TextMatch(element.landmark.def.label))
			{
				return true;
			}
		}
		if (!element.mutators.NullOrEmpty())
		{
			foreach (TileMutatorDef mutator in element.mutators)
			{
				if (TextMatch(mutator.label))
				{
					return true;
				}
				if (TextMatch(mutator.Label(element.tile)))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void TryRemoveElement(WorldSearchElement element)
	{
		throw new NotImplementedException();
	}

	protected override void DoIcon(WorldSearchElement element, Rect iconRect)
	{
		if (element.worldObject != null)
		{
			GUI.color = element.worldObject.ExpandingIconColor;
			GUI.DrawTexture(iconRect, element.worldObject.ExpandingIcon);
			GUI.color = Color.white;
		}
	}

	protected override void DoLabel(WorldSearchElement element, Rect labelRect)
	{
		Widgets.Label(labelRect, element.DisplayLabel);
	}

	protected override void ClikedOnElement(WorldSearchElement element)
	{
		Find.WorldSelector.ClearSelection();
		if (element.worldObject != null)
		{
			Find.WorldSelector.Select(element.worldObject);
		}
		else
		{
			Find.WorldSelector.SelectedTile = element.tile;
		}
		Find.WorldCameraDriver.JumpTo(element.tile);
	}

	protected override bool ShouldSkipElement(WorldSearchElement element)
	{
		return element.DisplayLabel.NullOrEmpty();
	}

	protected override void OnHighlightUpdate(WorldSearchElement element)
	{
	}
}
