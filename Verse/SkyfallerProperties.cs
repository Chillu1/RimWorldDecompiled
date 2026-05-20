using UnityEngine;

namespace Verse;

public class SkyfallerProperties
{
	public bool hitRoof = true;

	public IntRange ticksToImpactRange = new IntRange(120, 200);

	public IntRange ticksToDiscardInReverse = IntRange.Zero;

	public bool reversed;

	public bool flightFlippedHorizontally;

	public float explosionRadius = 3f;

	public DamageDef explosionDamage;

	public bool damageSpawnedThings;

	public float explosionDamageFactor = 1f;

	public IntRange metalShrapnelCountRange = IntRange.Zero;

	public IntRange rubbleShrapnelCountRange = IntRange.Zero;

	public float shrapnelDistanceFactor = 1f;

	public ThingDef spawnThing;

	public bool minimalRoofDestruction;

	public SkyfallerMovementType movementType;

	public float speed = 1f;

	public string shadow = "Things/Skyfaller/SkyfallerShadowCircle";

	public Vector2 shadowSize = Vector2.one;

	public float cameraShake;

	public SoundDef impactSound;

	public bool rotateGraphicTowardsDirection;

	public SoundDef anticipationSound;

	public SoundDef floatingSound;

	public int anticipationSoundTicks = 100;

	public int motesPerCell = 3;

	public float moteSpawnTime = float.MinValue;

	public SimpleCurve xPositionCurve;

	public SimpleCurve zPositionCurve;

	public SimpleCurve angleCurve;

	public SimpleCurve rotationCurve;

	public SimpleCurve speedCurve;

	public int fadeInTicks;

	public int fadeOutTicks;

	public bool MakesShrapnel
	{
		get
		{
			if (metalShrapnelCountRange.max <= 0)
			{
				return rubbleShrapnelCountRange.max > 0;
			}
			return true;
		}
	}

	public bool CausesExplosion
	{
		get
		{
			if (explosionDamage != null)
			{
				return explosionRadius > 0f;
			}
			return false;
		}
	}
}
