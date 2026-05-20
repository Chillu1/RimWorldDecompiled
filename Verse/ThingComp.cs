using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

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

	public virtual void PostMapInit()
	{
	}

	public virtual void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
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

	public virtual void CompTickInterval(int delta)
	{
	}

	public virtual void CompTickRare()
	{
	}

	public virtual void CompTickLong()
	{
	}

	public virtual void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
	{
		absorbed = false;
	}

	public virtual void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
	{
	}

	public virtual void DrawAt(Vector3 drawLoc, bool flip = false)
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
		return Enumerable.Empty<Gizmo>();
	}

	public virtual IEnumerable<Gizmo> CompGetWornGizmosExtra()
	{
		return Enumerable.Empty<Gizmo>();
	}

	public virtual bool AllowStackWith(Thing other)
	{
		return true;
	}

	public virtual string CompInspectStringExtra()
	{
		return null;
	}

	public virtual string CompTipStringExtra()
	{
		return null;
	}

	public virtual string GetDescriptionPart()
	{
		return null;
	}

	public virtual IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
	{
		return Enumerable.Empty<FloatMenuOption>();
	}

	public virtual IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns)
	{
		return Enumerable.Empty<FloatMenuOption>();
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

	public virtual void PostPostGeneratedForTrader(TraderKindDef trader, PlanetTile forTile, Faction forFaction)
	{
	}

	public virtual void Notify_SignalReceived(Signal signal)
	{
	}

	public virtual void Notify_LordDestroyed()
	{
	}

	public virtual void Notify_MapRemoved()
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

	public virtual void Notify_Unequipped(Pawn pawn)
	{
	}

	public virtual void Notify_UsedVerb(Pawn pawn, Verb verb)
	{
	}

	public virtual void Notify_UsedWeapon(Pawn pawn)
	{
	}

	public virtual void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
	{
	}

	public virtual void Notify_KilledPawn(Pawn pawn)
	{
	}

	public virtual void Notify_PassedToWorld()
	{
	}

	public virtual void Notify_WearerDied()
	{
	}

	public virtual void Notify_Downed()
	{
	}

	public virtual void Notify_Released()
	{
	}

	public virtual void Notify_DefsHotReloaded()
	{
	}

	public virtual void Notify_AddBedThoughts(Pawn pawn)
	{
	}

	public virtual void Notify_AbandonedAtTile(PlanetTile tile)
	{
	}

	public virtual void Notify_KilledLeavingsLeft(List<Thing> leavings)
	{
	}

	public virtual void Notify_Arrested(bool succeeded)
	{
	}

	public virtual void Notify_PrisonBreakout()
	{
	}

	public virtual void Notify_Hacked(Pawn hacker = null)
	{
	}

	public virtual void Notify_ColorChanged()
	{
	}

	public virtual IEnumerable<ThingDefCountClass> GetAdditionalLeavings(Map map, DestroyMode mode)
	{
		return Enumerable.Empty<ThingDefCountClass>();
	}

	public virtual IEnumerable<ThingDefCountClass> GetAdditionalHarvestYield()
	{
		return Enumerable.Empty<ThingDefCountClass>();
	}

	public virtual void CompDrawWornExtras()
	{
	}

	public virtual bool CompAllowVerbCast(Verb verb)
	{
		return true;
	}

	public virtual bool CompPreventClaimingBy(Faction faction)
	{
		return false;
	}

	public virtual bool CompForceDeconstructable()
	{
		return false;
	}

	public virtual float CompGetSpecialApparelScoreOffset()
	{
		return 0f;
	}

	public virtual void Notify_RecipeProduced(Pawn pawn)
	{
	}

	public virtual void Notify_DuplicatedFrom(Pawn source)
	{
	}

	public virtual void Notify_BecameVisible()
	{
	}

	public virtual void Notify_BecameInvisible()
	{
	}

	public virtual void Notify_ForcedVisible()
	{
	}

	public virtual float GetStatFactor(StatDef stat)
	{
		return 1f;
	}

	public virtual float GetStatOffset(StatDef stat)
	{
		return 0f;
	}

	public virtual void GetStatsExplanation(StatDef stat, StringBuilder sb, string whitespace = "")
	{
	}

	public virtual void PreSwapMap()
	{
	}

	public virtual void PostSwapMap()
	{
	}

	public virtual Color? ForceColor()
	{
		return null;
	}

	public virtual bool DontDrawParent()
	{
		return false;
	}

	public virtual bool WantHoldWeapon(Pawn pawn)
	{
		return false;
	}

	public virtual AcceptanceReport CanEnterPortal()
	{
		return true;
	}

	public virtual List<PawnRenderNode> CompRenderNodes()
	{
		return null;
	}

	public override string ToString()
	{
		return GetType().Name + "(parent=" + parent?.ToString() + " at=" + (parent?.Position ?? IntVec3.Invalid).ToString() + ")";
	}
}
