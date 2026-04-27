# Technical Interview Task — Screen-Space Outline Effect

## Overview

Implement a **screen-space outline render effect** as a URP `ScriptableRendererFeature`.

When complete, opaque objects in the scene should receive a configurable outline driven by depth and/or surface normal discontinuities between neighbouring pixels — no per-object shaders or mesh modifications required.

**Time expectation: ~2 hours.** Do not over-engineer; a working solution with clean code beats a half-finished ambitious one.

---

## Requirements

### Must-have
- [ ] A `ScriptableRendererFeature` (`OutlineFeature`) that can be added via the URP Renderer asset Inspector
- [ ] A `ScriptableRenderPass` (`OutlinePass`) that performs a full-screen edge detection blit
- [ ] A shader that detects edges by comparing depth values of neighbouring pixels
- [ ] Configurable parameters exposed in the Inspector:
  - Outline color
  - Outline thickness (pixels)
  - Depth sensitivity threshold

### Nice-to-have (bonus, discussed in interview)
- Normal-based edge detection alongside depth
- Effect remains correct across different screen resolutions and aspect ratios
- Mobile-conscious shader (minimal samples, appropriate precision qualifiers)
- Brief comments on non-obvious math or trade-offs in the shader

---

## Getting Started

1. Open `Assets/Scenes/SampleScene.unity`
2. In this scene you will find a collection of primitives at varying depths, so edge detection will be easy to verify.
3. Open `Assets/Settings/PC_Renderer.asset` in the Inspector and add your `OutlineFeature` once it compiles.
4. Fill in all `// TODO` sections in:
   - `Assets/InterviewTask/Scripts/OutlineFeature.cs`
   - `Assets/InterviewTask/Scripts/OutlinePass.cs`
   - `Assets/InterviewTask/Shaders/Outline.shader`

---

## Hints

- Call `ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal)` in the pass constructor — this tells URP to prepare `_CameraDepthTexture` and `_CameraNormalsTexture` for you.
- URP's `Blit.hlsl` provides `Vert`, `Varyings`, and `_BlitTexture` so you do not need to write a vertex shader.
- A **Roberts cross** operator on depth is a good starting point: sample the four diagonal neighbours and compare.
- In C#, look into `Blitter.BlitCameraTexture` for the actual blit call in `Execute`.
- The depth texture stores non-linear values — keep that in mind when choosing your comparison threshold.
