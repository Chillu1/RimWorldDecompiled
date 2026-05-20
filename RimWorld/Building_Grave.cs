using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Building_Grave : Building_CorpseCasket, INotifyHauledTo
{
	private Graphic cachedGraphicFull;

	public Pawn AssignedPawn
	{
		get
		{
			if (CompAssignableToPawn == null || !CompAssignableToPawn.AssignedPawnsForReading.Any())
			{
				return null;
			}
			return CompAssignableToPawn.AssignedPawnsForReading[0];
		}
	}

	public CompAssignableToPawn_Grave CompAssignableToPawn => GetComp<CompAssignableToPawn_Grave>();

	public override Graphic Graphic
	{
		get
		{
			if (base.HasCorpse)
			{
				if (def.building.fullGraveGraphicData == null)
				{
					return base.Graphic;
				}
				if (cachedGraphicFull == null)
				{
					cachedGraphicFull = def.building.fullGraveGraphicData.GraphicColoredFor(this);
				}
				return cachedGraphicFull;
			}
			return base.Graphic;
		}
	}

	public override bool StorageTabVisible
	{
		get
		{
			if (base.StorageTabVisible)
			{
				return AssignedPawn == null;
			}
			return false;
		}
	}

	public override void EjectContents()
	{
		base.EjectContents();
		if (base.Spawned)
		{
			base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things);
		}
	}

	public virtual void Notify_HauledTo(Pawn hauler, Thing thing, int count)
	{
		CompArt comp = GetComp<CompArt>();
		if (comp != null && !comp.Active && hauler.RaceProps.Humanlike)
		{
			comp.JustCreatedBy(hauler);
			comp.InitializeArt(base.Corpse.InnerPawn);
		}
		base.Map.mapDrawer.MapMeshDirty(base.Position, (ulong)MapMeshFlagDefOf.Buildings | (ulong)MapMeshFlagDefOf.Things);
		hauler.records.Increment(RecordDefOf.CorpsesBuried);
		TaleRecorder.RecordTale(TaleDefOf.BuriedCorpse, hauler, base.Corpse?.InnerPawn);
	}

	public override bool Accepts(Thing thing)
	{
		if (!base.Accepts(thing))
		{
			return false;
		}
		if (base.HasCorpse)
		{
			return false;
		}
		if (AssignedPawn != null)
		{
			if (!(thing is Corpse corpse))
			{
				return false;
			}
			if (corpse.InnerPawn != AssignedPawn)
			{
				return false;
			}
		}
		else if (!storageSettings.AllowedToAccept(thing))
		{
			return false;
		}
		return true;
	}

	public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
	{
		if (base.TryAcceptThing(thing, allowSpecialEffects))
		{
			if (thing is Corpse corpse && corpse.InnerPawn.ownership != null && corpse.InnerPawn.ownership.AssignedGrave != this)
			{
				corpse.InnerPawn.ownership.UnclaimGrave();
			}
			if (base.Spawned)
			{
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlagDefOf.Things);
			}
			return true;
		}
		return false;
	}

	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!StorageTabVisible)
		{
			yield break;
		}
		foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(storageSettings))
		{
			yield return item;
		}
	}

	public override void Notify_ColorChanged()
	{
		base.Notify_ColorChanged();
		cachedGraphicFull = null;
	}

	public override string GetInspectString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.GetInspectString());
		if (base.HasCorpse)
		{
			if (base.Tile.Valid)
			{
				string text = GenDate.DateFullStringAt(GenDate.TickGameToAbs(base.Corpse.timeOfDeath), Find.WorldGrid.LongLatOf(base.Tile));
				stringBuilder.AppendLine();
				stringBuilder.Append("DiedOn".Translate(text));
			}
		}
		else if (AssignedPawn != null)
		{
			stringBuilder.AppendLine();
			stringBuilder.Append("AssignedColonist".Translate());
			stringBuilder.Append(": ");
			stringBuilder.Append(AssignedPawn.LabelCap);
		}
		return stringBuilder.ToString();
	}
}
