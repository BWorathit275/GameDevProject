# Basement VFX — how to integrate

playable in
**`Assets/Scenes/Sandbox/VFX_Test.unity`**; this guide is for moving it into real game

---

## 1. Try it first

1. Open `Assets/Scenes/Sandbox/VFX_Test.unity` and press **Play**.
2. Hearts show top-left. Let the ghostfind you, or press **H** to take a hit, **R** to reset.
3. Walk/sprint the hallway

`HeartTest` (in `Assets/Script/VFXTest/`) is a **sandbox-only ** for heart system
Don't put it in the real game — see step 4

---

## 2. What's included

| Effect | Prefab | Script(s) |
|---|---|---|
| Room dust (lit specks, soft blobs) | `AmbientDust/AmbientDust.prefab` | `EmitterLead`, `DustGust` |
| Dust in the flashlight beam | `AmbientDust/FlashlightDust.prefab` | `DustGust` |
| Ankle-height ground fog | `GroundFog/GroundFog.prefab` | `EmitterLead`, `DustGust`, `FogLightScatter` |
| Pipe water drips (splash + ripple) | `PipeDrips/PipeDrip.prefab` | — |
| Screen blood on hit (flash, splats, lens streaks) | `Blood/BloodScreen.prefab` | `BloodScreenHit` |
| Body blood on hit (spray, bleeding, floor spots) | `Blood/BloodBody.prefab` | — |
| Injury look by hearts (vignette, drained colour, heartbeat, warp) | `Injury/InjuryEffect.prefab` | — (Animator) |

Scripts live in `Assets/Script/`: `EmitterLead.cs`, `DustGust.cs`, `FogLightScatter.cs`, `BloodScreenHit.cs`.

---

## 3. Add the prefabs to the player and the scene

### On the Player prefab
Drag each prefab in as a child at these **local positions** (Player's origin is at the feet):

| Prefab | Parent | Local position |
|---|---|---|
| `AmbientDust` | `Player` | (0, 1.6, 0) |
| `GroundFog` | `Player` | (0, 0.45, 0) |
| `BloodBody` | `Player` | (0, 1.25, 0.2) |
| `FlashlightDust` | `Player/Main Camera/Spot Light` (the flashlight) | (0, 0, 0) |
| `BloodScreen` | `Player/Main Camera` | (0, 0, 0.3) |

Leave rotations at zero. Don't change the prefabs' own child positions

### In the Basement scene
- **`InjuryEffect`**: drop one anywhere in the scene
- **`PipeDrip`**: place one under a pipe wherever you want drips — just below the pipe's underside
  The two in `VFX_Test` are at (50.10, 1.48, 52.09) and (50.05, 1.48, 49.95)
---

## 4. Hook up the heart system (code)

Give the script that handles the player's hearts these references and calls:

```csharp
[Header("Hit VFX")]
public BloodScreenHit bloodScreen;
public ParticleSystem bloodBody;
public Animator injuryAnimator;

// When the ghost hits the player:
void OnHitByGhost(Transform ghost) //just mockup
{;
    bloodScreen.Play(); 
    bloodBody.Play();
    UpdateInjury();
}

// Call after any change to hearts (hit, heal, respawn):
void UpdateInjury()
{
    // 0 = full hearts, 1 = last heart
    injuryAnimator.SetFloat("Injury", (maxHearts - hearts) / (float)(maxHearts - 1));
}
```


### Optional: stir the dust and fog on events
```csharp
DustGust.Trigger(doorTransform.position);        // e.g. a door slams
DustGust.Trigger(ghost.position, 0.6f);          // weaker stir, e.g. ghost runs past
```
Dust/fog within 12 m of the point swirls for a moment, then settles.

## 6. Tuning cheat sheet

| Want to change… | Where |
|---|---|
| How much dust / fog | prefab → *Emission → Rate over Time* (keep Rate over Distance 0) |
| Fog height | `GroundFog` → *Shape → Position Y / Scale Y* |
| Fog brightness in the beam / in the dark | `FogLightScatter` → *Scatter* / *Ambient* / *Max Brightness* |
| Strength of a gust | `DustGust` → *Push*, *Turbulence Boost*, *Settle Time* |
| Screen blood amount | `BloodScreen/SideLeft·SideRight·SideTop` → *Emission → Bursts* |
| Red flash | `BloodScreen/HitFlash` (colour in `Blood/BloodFlash.mat`) |
| Injury look per level | `Injury/InjuryProfile.asset` (base), `InjuryWarpProfile.asset` (warp), clips `InjuryLight` / `InjuryCritical` |

---
