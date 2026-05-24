# AGENTS.md — KAELIS Crystal Breakthrough Mode

## 0. Main Decision

The project is now in **Crystal Breakthrough Mode**.

The previous cautious approach did not produce the requested visual result. Codex is now allowed to work more boldly on Premium3D crystal optics, crystal scaling, crystal shape transfer, and menu bindings.

The goal is not to preserve weak existing Premium3D behavior. The goal is to make KAELIS move forward.

Main user criticism:

- Premium3D forms still look like before.
- Classic2D has the best crystal forms and the most beautiful form transitions.
- Mouse wheel scaling works only in Premium3D, but should also affect Classic2D visual scale.
- Premium3D crystals remain too transparent.
- The user asked for gemstone-like refraction, not see-through glass.
- Menu controls still do not produce enough visible difference.

Therefore:


Protect only what is truly successful.
Free everything else for improvement.


---

## 1. Absolute Protected Zone

Do NOT modify or degrade:


Classic2D mirror system
Classic2D kaleidoscope shader behavior
Classic2D existing crystal/form templates
Classic2D existing form switching / morphing behavior


This is the project’s current strongest visual asset.

Classic2D forms and transitions are the reference and must remain intact.

---

## 2. Active Development Zone

Codex may freely inspect, copy concepts from, refactor, extend, adapt, or rebuild:


Premium3D crystal forms
Premium3D volumetric mesh generation
Premium3D material/optics system
Premium3D shader/material parameters
Premium3D reflection/refraction/transparency behavior
Premium3D input binding
Premium3D mouse wheel scaling
Classic2D-inspired scaling behavior
Menu optics bindings
Menu modes/presets/settings bindings
Crystal settings bridge
Command routing
Diagnostics
Preset payloads


Codex may change files under:


Assets/_Project/Kaleidoscope2/Menu/**
Assets/_Project/Kaleidoscope2/DiamondFocus/**
Assets/_Project/Kaleidoscope2/Input/**
Assets/_Project/Kaleidoscope2/Core/**
Assets/_Project/Kaleidoscope2/Diagnostics/**


Codex may inspect Classic2D implementation and copy ideas, formulas, shape definitions, parameters, and transition logic into Premium3D equivalents, but must not damage the original Classic2D behavior.

---

## 3. Forbidden / High-Risk Areas

Do not casually modify:


Source/**
OutputPreview internals
RuntimeMenuController internals
camera/render pipeline logic
global render settings
scene-wide camera setup
Classic2D shader internals


Exception:
If the planned fix absolutely requires a change outside the allowed zone, Codex must first report:

- why it is required;
- what file must change;
- what behavior is protected;
- expected risk;
- validation plan.

---

## 4. Block Architecture Requirement

Even with expanded freedom, the architecture must remain block-based.

Required separation:


Classic2D reference system
Premium3D crystal system
Menu control system
Command bridge
Input system
Preset system
Diagnostics


Preferred flow:


Menu / Input
    -> Command / Action Router
        -> Crystal Settings / Mode Settings
            -> Premium3D Renderer / Mesh / Material
                -> Diagnostics


No random cross-wiring.
No hidden direct hacks from UI into shader values without a named bridge or settings object.
No new giant god-class.

---

## 5. Classic2D As Source Of Inspiration

Classic2D is not to be broken.
Classic2D is to be studied.

Codex must analyze:


How Classic2D shapes are generated
How Classic2D forms transition/morph
Why Classic2D looks more beautiful
Which parameters create its “wow” effect
How scale/zoom/center/mirror geometry contribute to the result


Then Codex must transfer the experience to Premium3D.

Meaning:


Classic2D remains 2D.
Premium3D gets corresponding 3D volumetric crystal forms inspired by Classic2D.


Do not simply rename existing Premium3D shapes.

Premium3D shapes must become a real volumetric continuation of Classic2D visual language.

---

## 6. Premium3D Crystal Shape Requirements

Premium3D crystals must:


have real volume
have real side faces
have front/back depth
preserve recognizable silhouette
support all optics materials
support mouse wheel scale
support smooth form transitions where feasible
look more interesting than current weak shapes


Required shape direction:


Classic2D-derived radial shard
Classic2D-derived mandala crystal
Classic2D-derived diamond/star form
Classic2D-derived polygon crystal
Classic2D-derived rhombic/marquise form
Classic2D-derived round/disco multifacet form
Classic2D-derived triangular/trilliant form
Classic2D-derived oval/ring-like form


If existing Premium3D forms are weak, Codex may replace them.

---

## 7. Mouse Wheel Scaling Requirement

Mouse wheel scaling must work consistently.

Required:


Classic2D visible result responds to mouse wheel where appropriate.
Premium3D crystal scale responds to mouse wheel.
All Premium3D shapes respond.
Range: 20% – 300%.
Default: 100%.
No pulsing.
No background counter-scaling.
No mode-specific failure.


If Classic2D already uses wheel for zoom, Codex must analyze current behavior and reconcile it with user expectation:


Wheel should visibly change crystal/visual scale in Classic2D too.


Do not break existing Classic2D beauty.

---

## 8. Transparency Rule: No See-Through Crystal

The user explicitly rejects “transparent bubble” crystals.

Premium3D crystals must not look like empty glass.

Required:


No direct see-through window through the center.
No soap-bubble lens look.
No fully transparent crystal body.
No background visible directly through the whole crystal.


Correct behavior:


The crystal may transmit light through facets.
The crystal may refract the background.
The crystal may show color and depth through optical paths.
But it must never behave like fully transparent flat glass.


A real cut diamond is optically clear as material, but visually it is not a simple transparent window. It bends, reflects, splits, blocks, and redirects light.

Therefore Premium3D default must favor:


low direct transmission
strong facet refraction
strong internal reflection
strong Fresnel/edge reflection
controlled opacity
hidden reflection environment
spectral dispersion


Direct Transparency must become a real control, but the default should be gemstone-like, not window-like.

---

## 9. Absolute Mirror Requirement

Absolute Mirror must be a real mode, not a label.

Required:


facets behave like polished mirror surfaces
direct transmission is near zero
reflection dominates
hidden/backdrop reflection is visible in facets
material looks luxurious and reflective
not white, not flat, not opaque plastic


---

## 10. Facet Refraction Requirement

Facet refraction must be real and obvious.

Required:


different facets distort the image differently
distortion depends on facet normals
high values produce dramatic but stable bending
prism/dispersion is visible on edges and facets
internal reflections create depth


Wrong result:


one smooth bubble lens
flat transparent pane
weak distortion invisible to user


At high slider values, the result must be surprising and expressive.

---

## 11. Menu Control Truth Rule

No fake controls.

Every menu control must be one of:


REAL_BINDING
PARTIAL_BINDING
RESERVED
BROKEN


Target for this phase:


Premium3D optics controls should become REAL_BINDING.
Mouse wheel scaling controls should become REAL_BINDING.
Preset Apply should become REAL_BINDING.
Classic/Premium shape transfer controls should become REAL_BINDING where feasible.


---

## 12. Preset Requirement

Factory presets must become meaningful.

Required:


Diamond Palace
Blue Ice
Golden Prism
Ruby Night
Emerald Depth
Opal Dream
Cosmic Glass
Dark Luxury
Absolute Mirror


Each preset should apply:


mode
shape
material
direct transparency
reflection
refraction
dispersion
internal reflections
brightness/contrast
bloom/glow
scale if appropriate


Do not fake preset application.

---

## 13. Freedom With Responsibility

Codex may change more than before.

Codex may:


replace weak Premium3D mesh generation
add new volumetric mesh factories
add new crystal shape enum values
add new settings objects
add new command routes
add new diagnostics
add new menu bridges
adjust shader/material properties
expand ranges
remove obsolete Premium3D dead code


But Codex must not:


break Classic2D forms
break Classic2D transitions
destroy block architecture
hide failures behind vague reports
claim visual success without validation


---

## 14. Required Reporting

Every planning/implementation pass must report:


What Classic2D behavior was inspected
What was copied/adapted into Premium3D
What files changed
What controls are real
What controls are still reserved
Whether Classic2D remained untouched
Whether wheel scaling works in both modes
Whether transparency is fully controlled
Whether absolute mirror is real
Whether facet refraction is visibly stronger
Whether shape transfer produced new 3D volumetric shapes


---

## 15. Final Principle

The project can be rolled back with Git.

Do not waste time preserving weak Premium3D code.

Preserve the beautiful Classic2D behavior.
Use it as inspiration.
Make Premium3D worthy of it.
