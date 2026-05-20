using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Ship_Pregen : SymbolResolver
{
	private struct SpawnDescriptor
	{
		public IntVec3 offset;

		public ThingDef def;

		public Rot4 rot;
	}

	public override void Resolve(ResolveParams rp)
	{
		SpawnDescriptor[] obj = new SpawnDescriptor[30]
		{
			new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, 0),
				def = ThingDefOf.Ship_Reactor,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, 12),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, 6),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-4, 0, -2),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(4, 0, -2),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, -7),
				def = ThingDefOf.Ship_Beam,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(0, 0, 16),
				def = ThingDefOf.Ship_SensorCluster,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, -7),
				def = ThingDefOf.Ship_ComputerCore,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 15),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 13),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 11),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 9),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 7),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, 5),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-4, 0, 2),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-5, 0, 1),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 15),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 13),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 11),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 9),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 7),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(2, 0, 5),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(5, 0, 2),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(6, 0, 1),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.East
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-6, 0, -3),
				def = ThingDefOf.Ship_Engine,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(7, 0, -3),
				def = ThingDefOf.Ship_Engine,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(3, 0, -11),
				def = ThingDefOf.Ship_Engine,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-2, 0, -11),
				def = ThingDefOf.Ship_Engine,
				rot = Rot4.North
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, -8),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			},
			new SpawnDescriptor
			{
				offset = new IntVec3(-1, 0, -6),
				def = ThingDefOf.Ship_CryptosleepCasket,
				rot = Rot4.West
			}
		};
		IntVec3 centerCell = rp.rect.CenterCell;
		IntVec3 intVec = new IntVec3(-1, 0, -3);
		SpawnDescriptor[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			SpawnDescriptor spawnDescriptor = array[i];
			Thing thing = ThingMaker.MakeThing(spawnDescriptor.def);
			thing.SetFaction(rp.faction);
			if (rp.hpPercentRange.HasValue)
			{
				thing.HitPoints = Mathf.Clamp(Mathf.RoundToInt((float)thing.MaxHitPoints * rp.hpPercentRange.Value.RandomInRange), 1, thing.MaxHitPoints);
				GenLeaving.DropFilthDueToDamage(thing, thing.MaxHitPoints - thing.HitPoints);
			}
			CompHibernatable compHibernatable = thing.TryGetComp<CompHibernatable>();
			if (compHibernatable != null)
			{
				compHibernatable.State = HibernatableStateDefOf.Hibernating;
			}
			GenSpawn.Spawn(thing, centerCell + intVec + spawnDescriptor.offset, BaseGen.globalSettings.map, spawnDescriptor.rot);
		}
	}
}
