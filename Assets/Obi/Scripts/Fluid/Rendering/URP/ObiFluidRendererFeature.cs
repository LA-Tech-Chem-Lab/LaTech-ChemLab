﻿#if UNITY_2019_2_OR_NEWER && SRP_UNIVERSAL
using System;

            // request camera  depth and color buffers.
            m_VolumePass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color);
            // Copy settings;
            this.passes = passes;

                // render each pass (there's only one mesh per pass) onto temp buffer to calculate its color and thickness.
                for (int i = 0; i < passes.Length; ++i)

                            // fluid mesh renders absorption color and thickness onto temp buffer:
                            var renderSystem = fluidMesher.actor.solver.GetRenderSystem<ObiFluidSurfaceMesher>() as IFluidRenderSystem;

                            // calculate transmission from thickness & absorption and accumulate onto transmission buffer.
                            cmd.SetGlobalFloat("_Thickness", passes[i].thickness);

                // get temporary buffer with depth support, render fluid surface depth:
                cmd.SetRenderTarget(m_SurfHandle, m_DepthHandle, 0, CubemapFace.Unknown, -1);
                            // fluid mesh renders surface onto surface buffer
                            var renderSystem = fluidMesher.actor.solver.GetRenderSystem<ObiFluidSurfaceMesher>() as IFluidRenderSystem;

                // render foam, using distance to surface depth to modulate alpha:
                cmd.SetRenderTarget(m_FoamHandle, 0, CubemapFace.Unknown, -1);
                            {
                                var rend = solver.GetRenderSystem<ObiFoamGenerator>() as ObiFoamRenderSystem;

                                if (rend != null)
                                {
                                    rend.renderBatch.material.SetFloat("_FadeDepth", foamFadeDepth);
                                    rend.renderBatch.material.SetFloat("_VelocityStretching", solver.maxFoamVelocityStretch);
                                    rend.renderBatch.material.SetFloat("_FadeIn", solver.foamFade.x);
                                    rend.renderBatch.material.SetFloat("_FadeOut", solver.foamFade.y);
                                    cmd.DrawMesh(rend.renderBatch.mesh, solver.transform.localToWorldMatrix, rend.renderBatch.material);
                                }
#endif