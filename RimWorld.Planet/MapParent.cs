using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public class MapParent : WorldObject, IThingHolder
	{
		public bool forceRemoveWorldObjectWhenMapRemoved;

		private HashSet<IncidentTargetTagDef> hibernatableIncidentTargets;

		private static readonly Texture2D ShowMapCommand = ContentFinder<Texture2D>.Get("UI/Commands/ShowMap");

		public bool HasMap => Map != null;

		protected virtual bool UseGenericEnterMapFloatMenuOption => true;

		public Map Map => Current.Game.FindMap(this);

		public virtual MapGeneratorDef MapGeneratorDef
		{
			get
			{
				if (def.mapGenerator == null)
				{
					return MapGeneratorDefOf.Encounter;
				}
				return def.mapGenerator;
			}
		}

		public virtual IEnumerable<GenStepWithParams> ExtraGenStepDefs
		{
			get
			{
				yield break;
			}
		}

		public override bool ExpandMore
		{
			get
			{
				if (!base.ExpandMore)
				{
					return HasMap;
				}
				return true;
			}
		}

		public virtual bool HandlesConditionCausers => false;

		public virtual void PostMapGenerate()
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostMapGenerate();
			}
			QuestUtility.SendQuestTargetSignals(questTags, "MapGenerated", this.Named("SUBJECT"));
		}

		public virtual void Notify_MyMapAboutToBeRemoved()
		{
		}

		public virtual void Notify_MyMapRemoved(Map map)
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostMyMapRemoved();
			}
			QuestUtility.SendQuestTargetSignals(questTags, "MapRemoved", this.Named("SUBJECT"));
		}

		public virtual void Notify_CaravanFormed(Caravan caravan)
		{
			List<WorldObjectComp> allComps = base.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				allComps[i].PostCaravanFormed(caravan);
			}
		}

		public virtual void Notify_HibernatableChanged()
		{
			RecalculateHibernatableIncidentTargets();
		}

		public virtual void FinalizeLoading()
		{
			RecalculateHibernatableIncidentTargets();
		}

		public virtual bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
		{
			alsoRemoveWorldObject = false;
			return false;
		}

		public override void PostRemove()
		{
			base.PostRemove();
			if (HasMap)
			{
				Current.Game.DeinitAndRemoveMap(Map);
			}
		}

		public override void Tick()
		{
			base.Tick();
			CheckRemoveMapNow();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref forceRemoveWorldObjectWhenMapRemoved, "forceRemoveWorldObjectWhenMapRemoved", defaultValue: false);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (!HasMap)
			{
				yield break;
			}
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandShowMap".Translate();
			command_Action.defaultDesc = "CommandShowMapDesc".Translate();
			command_Action.icon = ShowMapCommand;
			command_Action.hotKey = KeyBindingDefOf.Misc1;
			command_Action.action = delegate
			{
				Current.Game.CurrentMap = Map;
				if (!CameraJumper.TryHideWorld())
				{
					SoundDefOf.TabClose.PlayOneShotOnCamera();
				}
			};
			yield return command_Action;
		}

		public override IEnumerable<IncidentTargetTagDef> IncidentTargetTags()
		{
			foreach (IncidentTargetTagDef item in base.IncidentTargetTags())
			{
				yield return item;
			}
			if (hibernatableIncidentTargets == null || hibernatableIncidentTargets.Count <= 0)
			{
				yield break;
			}
			foreach (IncidentTargetTagDef hibernatableIncidentTarget in hibernatableIncidentTargets)
			{
				yield return hibernatableIncidentTarget;
			}
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(caravan))
			{
				yield return floatMenuOption;
			}
			if (!UseGenericEnterMapFloatMenuOption)
			{
				yield break;
			}
			foreach (FloatMenuOption floatMenuOption2 in CaravanArrivalAction_Enter.GetFloatMenuOptions(caravan, this))
			{
				yield return floatMenuOption2;
			}
		}

		public override IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative)
		{
			CompLaunchable representative2 = representative;
			MapParent mapParent = this;
			foreach (FloatMenuOption transportPodsFloatMenuOption in base.GetTransportPodsFloatMenuOptions(pods, representative2))
			{
				yield return transportPodsFloatMenuOption;
			}
			if (!TransportPodsArrivalAction_LandInSpecificCell.CanLandInSpecificCell(pods, this))
			{
				yield break;
			}
			yield return new FloatMenuOption("LandInExistingMap".Translate(Label), delegate
			{
				Map myMap = representative2.parent.Map;
				Map map = mapParent.Map;
				Current.Game.CurrentMap = map;
				CameraJumper.TryHideWorld();
				Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate(LocalTargetInfo x)
				{
					representative2.TryLaunch(mapParent.Tile, new TransportPodsArrivalAction_LandInSpecificCell(mapParent, x.Cell));
				}, null, delegate
				{
					if (Find.Maps.Contains(myMap))
					{
						Current.Game.CurrentMap = myMap;
					}
				}, CompLaunchable.TargeterMouseAttachment);
			});
		}

		public void CheckRemoveMapNow()
		{
			if (HasMap && ShouldRemoveMapNow(out bool alsoRemoveWorldObject))
			{
				Map map = Map;
				Current.Game.DeinitAndRemoveMap(map);
				if (alsoRemoveWorldObject || forceRemoveWorldObjectWhenMapRemoved)
				{
					Destroy();
				}
			}
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (this.EnterCooldownBlocksEntering())
			{
				if (!text.NullOrEmpty())
				{
					text += "\n";
				}
				text += "EnterCooldown".Translate(this.EnterCooldownDaysLeft().ToString("0.#"));
			}
			if (!HandlesConditionCausers && HasMap)
			{
				List<Thing> list = Map.listerThings.ThingsInGroup(ThingRequestGroup.ConditionCauser);
				for (int i = 0; i < list.Count; i++)
				{
					text += "\n" + list[i].LabelShortCap + " (" + "ConditionCauserRadius".Translate(list[i].TryGetComp<CompCauseGameCondition>().Props.worldRange) + ")";
				}
			}
			return text;
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			if (!HandlesConditionCausers && HasMap)
			{
				int num = 0;
				List<Thing> list = Map.listerThings.ThingsInGroup(ThingRequestGroup.ConditionCauser);
				for (int i = 0; i < list.Count; i++)
				{
					num = Mathf.Max(num, list[i].TryGetComp<CompCauseGameCondition>().Props.worldRange);
				}
				if (num > 0)
				{
					GenDraw.DrawWorldRadiusRing(base.Tile, num);
				}
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return null;
		}

		public virtual void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
			if (HasMap)
			{
				outChildren.Add(Map);
			}
		}

		private void RecalculateHibernatableIncidentTargets()
		{
			hibernatableIncidentTargets = null;
			foreach (ThingWithComps item in Map.listerThings.ThingsOfDef(ThingDefOf.Ship_Reactor).OfType<ThingWithComps>())
			{
				CompHibernatable compHibernatable = item.TryGetComp<CompHibernatable>();
				if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Starting && compHibernatable.Props.incidentTargetWhileStarting != null)
				{
					if (hibernatableIncidentTargets == null)
					{
						hibernatableIncidentTargets = new HashSet<IncidentTargetTagDef>();
					}
					hibernatableIncidentTargets.Add(compHibernatable.Props.incidentTargetWhileStarting);
				}
			}
		}
	}
}
