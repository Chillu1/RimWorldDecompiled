using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class FloatMenuContext
{
	public List<Pawn> allSelectedPawns;

	public Vector3 clickPosition;

	public Map map;

	private IntVec3 cachedClickedCell;

	private List<Thing> cachedClickedThings;

	private List<Pawn> cachedClickedPawns;

	private Room cachedClickedRoom;

	private Zone cachedClickedZone;

	public IntVec3 ClickedCell => cachedClickedCell;

	public Room ClickedRoom => cachedClickedRoom;

	public Zone ClickedZone => cachedClickedZone;

	public List<Thing> ClickedThings => cachedClickedThings;

	public List<Pawn> ClickedPawns => cachedClickedPawns;

	public bool IsMultiselect => allSelectedPawns.Count > 1;

	public Pawn FirstSelectedPawn
	{
		get
		{
			foreach (Pawn allSelectedPawn in allSelectedPawns)
			{
				if (FloatMenuMakerMap.currentProvider == null || FloatMenuMakerMap.currentProvider.SelectedPawnValid(allSelectedPawn, this))
				{
					return allSelectedPawn;
				}
			}
			return null;
		}
	}

	public IEnumerable<Pawn> ValidSelectedPawns
	{
		get
		{
			foreach (Pawn allSelectedPawn in allSelectedPawns)
			{
				if (FloatMenuMakerMap.currentProvider == null || FloatMenuMakerMap.currentProvider.SelectedPawnValid(allSelectedPawn, this))
				{
					yield return allSelectedPawn;
				}
			}
		}
	}

	public FloatMenuContext(List<Pawn> selectedPawns, Vector3 clickPosition, Map map)
	{
		allSelectedPawns = selectedPawns;
		this.clickPosition = clickPosition;
		this.map = map;
		cachedClickedCell = IntVec3.FromVector3(clickPosition);
		cachedClickedRoom = cachedClickedCell.GetRoom(map);
		cachedClickedZone = cachedClickedCell.GetZone(map);
		cachedClickedThings = GenUI.ThingsUnderMouse(clickPosition, 0.8f, TargetingParameters.ForThing());
		cachedClickedPawns = GenUI.ThingsUnderMouse(clickPosition, 0.8f, TargetingParameters.ForPawns()).OfType<Pawn>().ToList();
		selectedPawns.RemoveAll((Pawn pawn) => !pawn.CanTakeOrder);
	}
}
