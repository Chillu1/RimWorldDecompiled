using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class MinifiedThing : ThingWithComps, IThingHolder
	{
		private const float MaxMinifiedGraphicSize = 1.1f;

		private const float CrateToGraphicScale = 1.16f;

		private ThingOwner innerContainer;

		private Graphic cachedGraphic;

		private Graphic crateFrontGraphic;

		public Thing InnerThing
		{
			get
			{
				if (innerContainer.Count == 0)
				{
					return null;
				}
				return innerContainer[0];
			}
			set
			{
				if (value == InnerThing)
				{
					return;
				}
				if (value == null)
				{
					innerContainer.Clear();
					return;
				}
				if (innerContainer.Count != 0)
				{
					Log.Warning("Assigned 2 things to the same MinifiedThing " + this.ToStringSafe() + " (first=" + innerContainer[0].ToStringSafe() + " second=" + value.ToStringSafe() + ")");
					innerContainer.ClearAndDestroyContents();
				}
				innerContainer.TryAdd(value);
			}
		}

		public override Graphic Graphic
		{
			get
			{
				if (cachedGraphic == null)
				{
					cachedGraphic = InnerThing.Graphic.ExtractInnerGraphicFor(InnerThing);
					if ((float)InnerThing.def.size.x > 1.1f || (float)InnerThing.def.size.z > 1.1f)
					{
						Vector2 minifiedDrawSize = GetMinifiedDrawSize(InnerThing.def.size.ToVector2(), 1.1f);
						Vector2 newDrawSize = new Vector2(minifiedDrawSize.x / (float)InnerThing.def.size.x * cachedGraphic.drawSize.x, minifiedDrawSize.y / (float)InnerThing.def.size.z * cachedGraphic.drawSize.y);
						cachedGraphic = cachedGraphic.GetCopy(newDrawSize);
					}
				}
				return cachedGraphic;
			}
		}

		public override string LabelNoCount => InnerThing.LabelNoCount;

		public override string DescriptionDetailed => InnerThing.DescriptionDetailed;

		public override string DescriptionFlavor => InnerThing.DescriptionFlavor;

		public MinifiedThing()
		{
			innerContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
		}

		public override void Tick()
		{
			if (InnerThing == null)
			{
				Log.Error("MinifiedThing with null InnerThing. Destroying.");
				Destroy();
				return;
			}
			base.Tick();
			if (InnerThing is Building_Battery)
			{
				innerContainer.ThingOwnerTick();
			}
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override Thing SplitOff(int count)
		{
			MinifiedThing minifiedThing = (MinifiedThing)base.SplitOff(count);
			if (minifiedThing != this)
			{
				minifiedThing.InnerThing = ThingMaker.MakeThing(InnerThing.def, InnerThing.Stuff);
				ThingWithComps thingWithComps = InnerThing as ThingWithComps;
				if (thingWithComps != null)
				{
					for (int i = 0; i < thingWithComps.AllComps.Count; i++)
					{
						thingWithComps.AllComps[i].PostSplitOff(minifiedThing.InnerThing);
					}
				}
			}
			return minifiedThing;
		}

		public override bool CanStackWith(Thing other)
		{
			MinifiedThing minifiedThing = other as MinifiedThing;
			if (minifiedThing == null)
			{
				return false;
			}
			if (base.CanStackWith(other))
			{
				return InnerThing.CanStackWith(minifiedThing.InnerThing);
			}
			return false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
		}

		public override void DrawExtraSelectionOverlays()
		{
			base.DrawExtraSelectionOverlays();
			Blueprint_Install blueprint_Install = InstallBlueprintUtility.ExistingBlueprintFor(this);
			if (blueprint_Install != null)
			{
				GenDraw.DrawLineBetween(this.TrueCenter(), blueprint_Install.TrueCenter());
			}
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			if (crateFrontGraphic == null)
			{
				crateFrontGraphic = GraphicDatabase.Get<Graphic_Single>("Things/Item/Minified/CrateFront", ShaderDatabase.Cutout, GetMinifiedDrawSize(InnerThing.def.size.ToVector2(), 1.1f) * 1.16f, Color.white);
			}
			crateFrontGraphic.DrawFromDef(drawLoc + Altitudes.AltIncVect * 0.1f, Rot4.North, null);
			if (Graphic is Graphic_Single)
			{
				Graphic.Draw(drawLoc, Rot4.North, this);
			}
			else
			{
				Graphic.Draw(drawLoc, Rot4.South, this);
			}
		}

		public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
		{
			bool spawned = base.Spawned;
			Map map = base.Map;
			base.Destroy(mode);
			if (InnerThing == null)
			{
				return;
			}
			InstallBlueprintUtility.CancelBlueprintsFor(this);
			if (spawned)
			{
				switch (mode)
				{
				case DestroyMode.Deconstruct:
					SoundDefOf.Building_Deconstructed.PlayOneShot(new TargetInfo(base.Position, map));
					GenLeaving.DoLeavingsFor(InnerThing, map, mode, this.OccupiedRect());
					break;
				case DestroyMode.KillFinalize:
					GenLeaving.DoLeavingsFor(InnerThing, map, mode, this.OccupiedRect());
					break;
				}
			}
			if (InnerThing is MonumentMarker)
			{
				InnerThing.Destroy();
			}
		}

		public override void PreTraded(TradeAction action, Pawn playerNegotiator, ITrader trader)
		{
			base.PreTraded(action, playerNegotiator, trader);
			InstallBlueprintUtility.CancelBlueprintsFor(this);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			yield return InstallationDesignatorDatabase.DesignatorFor(def);
		}

		public override string GetInspectString()
		{
			string text = "NotInstalled".Translate();
			string inspectString = InnerThing.GetInspectString();
			if (!inspectString.NullOrEmpty())
			{
				text += "\n";
				text += inspectString;
			}
			return text;
		}

		private Vector2 GetMinifiedDrawSize(Vector2 drawSize, float maxSideLength)
		{
			float num = maxSideLength / Mathf.Max(drawSize.x, drawSize.y);
			if (num >= 1f)
			{
				return drawSize;
			}
			return drawSize * num;
		}
	}
}
