using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class VerbTracker : IExposable
	{
		public IVerbOwner directOwner;

		private List<Verb> verbs;

		public List<Verb> AllVerbs
		{
			get
			{
				if (verbs == null)
				{
					InitVerbsFromZero();
				}
				return verbs;
			}
		}

		public Verb PrimaryVerb
		{
			get
			{
				if (verbs == null)
				{
					InitVerbsFromZero();
				}
				for (int i = 0; i < verbs.Count; i++)
				{
					if (verbs[i].verbProps.isPrimary)
					{
						return verbs[i];
					}
				}
				return null;
			}
		}

		public VerbTracker(IVerbOwner directOwner)
		{
			this.directOwner = directOwner;
		}

		public void VerbsTick()
		{
			if (verbs != null)
			{
				for (int i = 0; i < verbs.Count; i++)
				{
					verbs[i].VerbTick();
				}
			}
		}

		public IEnumerable<Command> GetVerbsCommands(KeyCode hotKey = KeyCode.None)
		{
			CompEquippable ce = directOwner as CompEquippable;
			if (ce == null)
			{
				yield break;
			}
			Thing ownerThing = ce.parent;
			List<Verb> verbs = AllVerbs;
			for (int i = 0; i < verbs.Count; i++)
			{
				Verb verb = verbs[i];
				if (verb.verbProps.hasStandardCommand)
				{
					yield return CreateVerbTargetCommand(ownerThing, verb);
				}
			}
			if (!directOwner.Tools.NullOrEmpty() && ce != null && ce.parent.def.IsMeleeWeapon)
			{
				yield return CreateVerbTargetCommand(ownerThing, verbs.Where((Verb v) => v.verbProps.IsMeleeAttack).FirstOrDefault());
			}
		}

		private Command_VerbTarget CreateVerbTargetCommand(Thing ownerThing, Verb verb)
		{
			Command_VerbTarget command_VerbTarget = new Command_VerbTarget();
			command_VerbTarget.defaultDesc = ownerThing.LabelCap + ": " + ownerThing.def.description.CapitalizeFirst();
			command_VerbTarget.icon = ownerThing.def.uiIcon;
			command_VerbTarget.iconAngle = ownerThing.def.uiIconAngle;
			command_VerbTarget.iconOffset = ownerThing.def.uiIconOffset;
			command_VerbTarget.tutorTag = "VerbTarget";
			command_VerbTarget.verb = verb;
			if (verb.caster.Faction != Faction.OfPlayer)
			{
				command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
			}
			else if (verb.CasterIsPawn)
			{
				if (verb.CasterPawn.WorkTagIsDisabled(WorkTags.Violent))
				{
					command_VerbTarget.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
				}
				else if (!verb.CasterPawn.drafter.Drafted)
				{
					command_VerbTarget.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
				}
			}
			return command_VerbTarget;
		}

		public Verb GetVerb(VerbCategory category)
		{
			List<Verb> allVerbs = AllVerbs;
			if (allVerbs != null)
			{
				for (int i = 0; i < allVerbs.Count; i++)
				{
					if (allVerbs[i].verbProps.category == category)
					{
						return allVerbs[i];
					}
				}
			}
			return null;
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref verbs, "verbs", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && verbs != null)
			{
				if (verbs.RemoveAll((Verb x) => x == null) != 0)
				{
					Log.Error("Some verbs were null after loading. directOwner=" + directOwner.ToStringSafe());
				}
				List<Verb> sources = verbs;
				verbs = new List<Verb>();
				InitVerbs(delegate(Type type, string id)
				{
					Verb verb = sources.FirstOrDefault((Verb v) => v.loadID == id && v.GetType() == type);
					if (verb == null)
					{
						Log.Warning($"Replaced verb {type}/{id}; may have been changed through a version update or a mod change");
						verb = (Verb)Activator.CreateInstance(type);
					}
					verbs.Add(verb);
					return verb;
				});
			}
		}

		private void InitVerbsFromZero()
		{
			verbs = new List<Verb>();
			InitVerbs(delegate(Type type, string id)
			{
				Verb verb = (Verb)Activator.CreateInstance(type);
				verbs.Add(verb);
				return verb;
			});
		}

		private void InitVerbs(Func<Type, string, Verb> creator)
		{
			List<VerbProperties> verbProperties = directOwner.VerbProperties;
			if (verbProperties != null)
			{
				for (int i = 0; i < verbProperties.Count; i++)
				{
					try
					{
						VerbProperties verbProperties2 = verbProperties[i];
						string text = Verb.CalculateUniqueLoadID(directOwner, i);
						InitVerb(creator(verbProperties2.verbClass, text), verbProperties2, null, null, text);
					}
					catch (Exception ex)
					{
						Log.Error("Could not instantiate Verb (directOwner=" + directOwner.ToStringSafe() + "): " + ex);
					}
				}
			}
			List<Tool> tools = directOwner.Tools;
			if (tools != null)
			{
				for (int j = 0; j < tools.Count; j++)
				{
					Tool tool = tools[j];
					foreach (ManeuverDef maneuver in tool.Maneuvers)
					{
						try
						{
							VerbProperties verb = maneuver.verb;
							string text2 = Verb.CalculateUniqueLoadID(directOwner, tool, maneuver);
							InitVerb(creator(verb.verbClass, text2), verb, tool, maneuver, text2);
						}
						catch (Exception ex2)
						{
							Log.Error("Could not instantiate Verb (directOwner=" + directOwner.ToStringSafe() + "): " + ex2);
						}
					}
				}
			}
		}

		private void InitVerb(Verb verb, VerbProperties properties, Tool tool, ManeuverDef maneuver, string id)
		{
			verb.loadID = id;
			verb.verbProps = properties;
			verb.verbTracker = this;
			verb.tool = tool;
			verb.maneuver = maneuver;
			verb.caster = directOwner.ConstantCaster;
		}
	}
}
