using UnityEngine;
using kmty.gist;

namespace moxi {
    public class LBE : System.IDisposable {
        public RenderTexture VelPhi_R => velphi.Read;
        public RenderTexture VelPhi_W => velphi.Write;
        public RenderTexture ForceV => forces[0];
        public RenderTexture ForceD => forces[1];
        public RenderTexture ForceR => forces[2];
        protected PingPongRenderTexture[] forces;
        protected PingPongRenderTexture   velphi;
        protected ComputeShader cs;
        protected Texture2D grain, pinning;
        protected int w, h;

        public LBE(int w, int h, Texture2D grain, Texture2D pinning) {
            this.w = w;
            this.h = h;
            this.grain = grain; 
            this.pinning = pinning; 
            cs = (ComputeShader)Resources.Load("LBE");
            cs.SetInt("_W", w);
            cs.SetInt("_H", h);
            InitBuffer();
        }

        void InitBuffer() {
            var d = new RenderTextureDescriptor(w, h, RenderTextureFormat.ARGBFloat, 0, 0);
            velphi = new PingPongRenderTexture(d);
            forces = new PingPongRenderTexture[] {
                new PingPongRenderTexture(d),
                new PingPongRenderTexture(d),
                new PingPongRenderTexture(d)
            };
        }

        public void Collision(float omega) {
            var k = cs.FindKernel("Collision");
            cs.SetFloat("_Omega", omega);
            cs.SetTexture(k, "VelPhi_R", velphi.Read);
            cs.SetTexture(k, "ForceV_R", forces[0].Read);
            cs.SetTexture(k, "ForceD_R", forces[1].Read);
            cs.SetTexture(k, "ForceR_R", forces[2].Read);
            cs.SetTexture(k, "ForceV_W", forces[0].Write);
            cs.SetTexture(k, "ForceD_W", forces[1].Write);
            cs.SetTexture(k, "ForceR_W", forces[2].Write);
            ComputeShaderUtil.Dispatch2D(cs, k, w, h);
            foreach (var f in forces) f.Swap();
        }

        public void Streaming() {
            var k = cs.FindKernel("Streaming");
            cs.SetTexture(k, "Grain", grain);
            cs.SetTexture(k, "Pinning", pinning);
            cs.SetTexture(k, "VelPhi_R", velphi.Read);
            cs.SetTexture(k, "ForceV_R", forces[0].Read);
            cs.SetTexture(k, "ForceD_R", forces[1].Read);
            cs.SetTexture(k, "ForceR_R", forces[2].Read);
            cs.SetTexture(k, "ForceV_W", forces[0].Write);
            cs.SetTexture(k, "ForceD_W", forces[1].Write);
            cs.SetTexture(k, "ForceR_W", forces[2].Write);
            ComputeShaderUtil.Dispatch2D(cs, k, w, h);
            foreach (var f in forces) f.Swap();
        }

        public void Simulation() {
            var k = cs.FindKernel("Simulation");
            cs.SetTexture(k, "Pinning", pinning);
            cs.SetTexture(k, "ForceV_R", forces[0].Read);
            cs.SetTexture(k, "ForceD_R", forces[1].Read);
            cs.SetTexture(k, "ForceR_R", forces[2].Read);
            cs.SetTexture(k, "VelPhi_R", velphi.Read);
            cs.SetTexture(k, "VelPhi_W", velphi.Write);
            ComputeShaderUtil.Dispatch2D(cs, k, w, h);
            velphi.Swap();
        }

        public void Dispose() {
            foreach (var f in forces) f.Dispose();
            velphi.Dispose();
        }

        public void Reset() => InitBuffer();
        public void SwapVelphi() => velphi.Swap();
    }
}
