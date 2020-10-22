using System.Collections.Generic;
using RimWorld;

namespace Verse
{
	public abstract class ThingComp
	{
		public ThingWithComps parent;

		public CompProperties props;

		public IThingHolder ParentHolder => parent.ParentHolder;

		public virtual void Initialize(CompProperties props)
		{
			this.props = props;
		}

		public virtual void ReceiveCompSignal(string signal)
		{
		}

		public virtual void PostExposeData()
		{
		}

		public virtual void PostSpawnSetup(bool respawningAfterLoad)
		{
		}

		public virtual void PostDeSpawn(Map map)
		{
		}

		public virtual void PostDestroy(DestroyMode mode, Map previousMap)
		{
		}

		public virtual void PostPostMake()
		{
		}

		public virtual void CompTick()
		{
		}

		public virtual void CompTickRare()
		{
		}

		public virtual void CompTickLong()
		{
		}

		public virtual void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			absorbed = false;
		}

		public virtual void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
		}

		public virtual void PostDraw()
		{
		}

		public virtual void PostDrawExtraSelectionOverlays()
		{
		}

		public virtual void PostPrintOnto(SectionLayer layer)
		{
		}

		public virtual void CompPrintForPowerGrid(SectionLayer layer)
		{
		}

		public virtual void PreAbsorbStack(Thing otherStack, int count)
		{
		}

		public virtual void PostSplitOff(Thing piece)
		{
		}

		public virtual string TransformLabel(string label)
		{
			return label;
		}

		public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			yield break;
		}

		public virtual IEnumerable<Gizmo> CompGetWornGizmosExtra()
		{
			yield break;
		}

		public virtual bool AllowStackWith(Thing other)
		{
			return true;
		}

		public virtual string CompInspectStringExtra()
		{
			return null;
		}

		public virtual string GetDescriptionPart()
		{
			return null;
		}

		public virtual IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			yield break;
		}

		public virtual IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(List<Pawn> selPawns)
		{
			yield break;
		}

		public virtual void PrePreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
		}

		public virtual void PrePostIngested(Pawn ingester)
		{
		}

		public virtual void PostIngested(Pawn ingester)
		{
		}

		public virtual void PostPostGeneratedForTrader(TraderKindDef trader, int forTile, Faction forFaction)
		{
		}

		public virtual void Notify_SignalReceived(Signal signal)
		{
		}

		public virtual void Notify_LordDestroyed()
		{
		}

		public virtual void DrawGUIOverlay()
		{
		}

		public virtual IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			return null;
		}

		public virtual void Notify_Equipped(Pawn pawn)
		{
		}

		public virtual void Notify_UsedWeapon(Pawn pawn)
		{
		}

		public virtual void Notify_KilledPawn(Pawn pawn)
		{
		}

		public override string ToString()
		{
			return string.Concat(GetType().Name, "(parent=", parent, " at=", (parent != null) ? parent.Position : IntVec3.Invalid, ")");
		}
	}
}
