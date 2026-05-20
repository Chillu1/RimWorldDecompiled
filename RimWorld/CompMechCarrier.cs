using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class CompMechCarrier : ThingComp, IThingHolder
	{
		private const int LowIngredientCountThreshold = 250;

		private int cooldownTicksRemaining;

		private ThingOwner innerContainer;

		private List<Pawn> spawnedPawns = new List<Pawn>();

		private MechCarrierGizmo gizmo;

		public int maxToFill;

		private List<Thing> tmpResources = new List<Thing>();

		public CompProperties_MechCarrier Props => (CompProperties_MechCarrier)props;

		public AcceptanceReport CanSpawn
		{
			get
			{
				if (parent is Pawn pawn)
				{
					if (pawn.IsSelfShutdown())
					{
						return "SelfShutdown".Translate();
					}
					if (pawn.Faction == Faction.OfPlayer && !pawn.IsColonyMechPlayerControlled)
					{
						return false;
					}
					if (!pawn.Awake() || pawn.Downed || pawn.Dead || !pawn.Spawned)
					{
						return false;
					}
				}
				if (MaxCanSpawn <= 0)
				{
					return "MechCarrierNotEnoughResources".Translate();
				}
				if (cooldownTicksRemaining > 0)
				{
					return "CooldownTime".Translate() + " " + cooldownTicksRemaining.ToStringSecondsFromTicks();
				}
				return true;
			}
		}

		public int IngredientCount => innerContainer.TotalStackCountOfDef(Props.fixedIngredient);

		public int AmountToAutofill => Mathf.Max(0, maxToFill - IngredientCount);

		public int MaxCanSpawn => Mathf.Min(Mathf.FloorToInt(IngredientCount / Props.costPerPawn), Props.maxPawnsToSpawn);

		public bool LowIngredientCount => IngredientCount < 250;

		public float PercentageFull => (float)IngredientCount / (float)Props.maxIngredientCount;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			if (!ModLister.CheckBiotech("Mech carrier"))
			{
				parent.Destroy();
			}
		}

		public override void PostPostMake()
		{
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
			if (Props.startingIngredientCount > 0)
			{
				Thing thing = ThingMaker.MakeThing(Props.fixedIngredient);
				thing.stackCount = Props.startingIngredientCount;
				innerContainer.TryAdd(thing, Props.startingIngredientCount);
			}
			maxToFill = Props.startingIngredientCount;
		}

		public void TrySpawnPawns()
		{
			int maxCanSpawn = MaxCanSpawn;
			if (maxCanSpawn <= 0)
			{
				return;
			}
			PawnGenerationRequest request = new PawnGenerationRequest(Props.spawnPawnKind, parent.Faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
			tmpResources.Clear();
			tmpResources.AddRange(innerContainer);
			Lord lord = ((parent is Pawn p) ? p.GetLord() : null);
			for (int i = 0; i < maxCanSpawn; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(request);
				GenSpawn.Spawn(pawn, parent.Position, parent.Map);
				spawnedPawns.Add(pawn);
				lord?.AddPawn(pawn);
				int num = Props.costPerPawn;
				for (int j = 0; j < tmpResources.Count; j++)
				{
					Thing thing = innerContainer.Take(tmpResources[j], Mathf.Min(tmpResources[j].stackCount, Props.costPerPawn));
					num -= thing.stackCount;
					thing.Destroy();
					if (num <= 0)
					{
						break;
					}
				}
				if (Props.spawnedMechEffecter != null)
				{
					Effecter effecter = new Effecter(Props.spawnedMechEffecter);
					effecter.Trigger(Props.attachSpawnedMechEffecter ? ((TargetInfo)pawn) : new TargetInfo(pawn.Position, pawn.Map), TargetInfo.Invalid);
					effecter.Cleanup();
				}
			}
			tmpResources.Clear();
			cooldownTicksRemaining = Props.cooldownTicks;
			if (Props.spawnEffecter != null)
			{
				Effecter effecter2 = new Effecter(Props.spawnEffecter);
				effecter2.Trigger(Props.attachSpawnedEffecter ? ((TargetInfo)parent) : new TargetInfo(parent.Position, parent.Map), TargetInfo.Invalid);
				effecter2.Cleanup();
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!(parent is Pawn { IsColonyMech: not false } pawn) || pawn.GetOverseer() == null)
			{
				yield break;
			}
			foreach (Gizmo item in base.CompGetGizmosExtra())
			{
				yield return item;
			}
			if (Find.Selector.SingleSelectedThing == parent)
			{
				if (gizmo == null)
				{
					gizmo = new MechCarrierGizmo(this);
				}
				yield return gizmo;
			}
			AcceptanceReport canSpawn = CanSpawn;
			Command_ActionWithCooldown act = new Command_ActionWithCooldown
			{
				cooldownPercentGetter = () => Mathf.InverseLerp(Props.cooldownTicks, 0f, cooldownTicksRemaining),
				action = delegate
				{
					TrySpawnPawns();
				},
				hotKey = KeyBindingDefOf.Misc2,
				Disabled = !canSpawn.Accepted,
				icon = ContentFinder<Texture2D>.Get("UI/Gizmos/ReleaseWarUrchins"),
				defaultLabel = "MechCarrierRelease".Translate(Props.spawnPawnKind.labelPlural),
				defaultDesc = "MechCarrierDesc".Translate(Props.maxPawnsToSpawn, Props.spawnPawnKind.labelPlural, Props.spawnPawnKind.label, Props.costPerPawn, Props.fixedIngredient.label)
			};
			if (!canSpawn.Reason.NullOrEmpty())
			{
				act.Disable(canSpawn.Reason);
			}
			if (DebugSettings.ShowDevGizmos)
			{
				if (cooldownTicksRemaining > 0)
				{
					Command_Action command_Action = new Command_Action();
					command_Action.defaultLabel = "DEV: Reset cooldown";
					command_Action.action = delegate
					{
						cooldownTicksRemaining = 0;
					};
					yield return command_Action;
				}
				Command_Action command_Action2 = new Command_Action();
				command_Action2.defaultLabel = "DEV: Fill with " + Props.fixedIngredient.label;
				command_Action2.action = delegate
				{
					while (IngredientCount < Props.maxIngredientCount)
					{
						int stackCount = Mathf.Min(Props.maxIngredientCount - IngredientCount, Props.fixedIngredient.stackLimit);
						Thing thing = ThingMaker.MakeThing(Props.fixedIngredient);
						thing.stackCount = stackCount;
						innerContainer.TryAdd(thing, thing.stackCount);
					}
				};
				yield return command_Action2;
				Command_Action command_Action3 = new Command_Action();
				command_Action3.defaultLabel = "DEV: Empty " + Props.fixedIngredient.label;
				command_Action3.action = delegate
				{
					innerContainer.ClearAndDestroyContents();
				};
				yield return command_Action3;
			}
			yield return act;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public override string CompInspectStringExtra()
		{
			string text = base.CompInspectStringExtra();
			if (!text.NullOrEmpty())
			{
				text += "\n";
			}
			return text + ("CasketContains".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst());
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			innerContainer?.ClearAndDestroyContents();
			for (int i = 0; i < spawnedPawns.Count; i++)
			{
				if (!spawnedPawns[i].Dead)
				{
					spawnedPawns[i].Kill(null, null);
				}
			}
		}

		public override void PostDrawExtraSelectionOverlays()
		{
			if (!Find.Selector.IsSelected(parent))
			{
				return;
			}
			for (int i = 0; i < spawnedPawns.Count; i++)
			{
				if (!spawnedPawns[i].Dead)
				{
					GenDraw.DrawLineBetween(parent.TrueCenter(), spawnedPawns[i].TrueCenter());
				}
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Values.Look(ref cooldownTicksRemaining, "cooldownTicksRemaining", 0);
			Scribe_Values.Look(ref maxToFill, "maxToFill", 0);
			Scribe_Collections.Look(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				spawnedPawns.RemoveAll((Pawn x) => x == null);
			}
		}

		public override void CompTick()
		{
			if (cooldownTicksRemaining > 0)
			{
				cooldownTicksRemaining--;
			}
		}
	}
}
