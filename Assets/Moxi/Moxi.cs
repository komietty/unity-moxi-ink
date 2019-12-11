using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kmty.gist;

namespace moxi {
    public class Moxi : System.IDisposable {

        public RenderTexture Surf => surf.Read;
        public RenderTexture Flow => flow.Read;
        public RenderTexture Fixt => fixt.Read;
        public Vector4[] BrushHeads => brushHeads;

        protected PingPongRenderTexture surf;
        protected PingPongRenderTexture flow;
        protected PingPongRenderTexture fixt;
        protected ComputeShader moxi;
        protected ComputeShader trsf;
        protected int w, h;
        protected LBE lbe;
        protected Vector4[] brushHeads;

        public Moxi(int w, int h, Texture2D grain, Texture2D pinning) {
            this.w = w;
            this.h = h;
            InitBuffer();
            lbe = new LBE(w, h, grain, pinning);
            moxi = (ComputeShader)Resources.Load("Moxi");
            trsf = (ComputeShader)Resources.Load("Trsf");
            moxi.SetInt("_W", w);
            moxi.SetInt("_H", h);
        }

        void InitBuffer() {
            var d = new RenderTextureDescriptor(w, h, RenderTextureFormat.ARGBFloat, 0, 0);
            surf = new PingPongRenderTexture(d);
            flow = new PingPongRenderTexture(d);
            fixt = new PingPongRenderTexture(d);
        }

        public void Deposition(Vector2 mousePos, float brushWidth, float wtrSupply, float inkSupply) {
            var k = moxi.FindKernel("Deposition");
            moxi.SetVectorArray("_BrushHeads", CalcBrushHeads(mousePos, brushWidth));
            moxi.SetFloat("_WtrSupply", wtrSupply);
            moxi.SetFloat("_InkSupply", inkSupply);
            moxi.SetTexture(k, "Surf_R", surf.Read);
            moxi.SetTexture(k, "Flow_R", flow.Read);
            moxi.SetTexture(k, "Surf_W", surf.Write);
            moxi.SetTexture(k, "Flow_W", flow.Write);
            ComputeShaderUtil.Dispatch2D(moxi, k, w, h);
            surf.Swap();
            flow.Swap();
        }

        public void Percolation(float omega) {
            var k1 = trsf.FindKernel("InterfaceF2L");
            var k2 = trsf.FindKernel("InterfaceL2F");
            trsf.SetTexture(k1, "Flow_R", flow.Read);
            trsf.SetTexture(k1, "VelPhi_R", lbe.VelPhi_R);
            trsf.SetTexture(k1, "VelPhi_W", lbe.VelPhi_W);
            ComputeShaderUtil.Dispatch2D(trsf, k1, w, h);
            lbe.SwapVelphi();
            lbe.Collision(omega);
            lbe.Streaming();
            lbe.Simulation();
            trsf.SetTexture(k2, "VelPhi_R", lbe.VelPhi_R);
            trsf.SetTexture(k2, "Flow_R", flow.Read);
            trsf.SetTexture(k2, "Flow_W", flow.Write);
            ComputeShaderUtil.Dispatch2D(trsf, k2, w, h);
            flow.Swap();
        }

        public void PigmentSupply() {
            var k = moxi.FindKernel("Supply");
            moxi.SetTexture(k, "Surf_R", surf.Read);
            moxi.SetTexture(k, "Surf_W", surf.Write);
            moxi.SetTexture(k, "Flow_R", flow.Read);
            moxi.SetTexture(k, "Flow_W", flow.Write);
            ComputeShaderUtil.Dispatch2D(moxi, k, w, h);
            flow.Swap();
            surf.Swap();
        }

        public void PigmentAdvection() {
            var k = moxi.FindKernel("Advection");
            moxi.SetTexture(k, "ForceV_R", lbe.ForceV);
            moxi.SetTexture(k, "ForceD_R", lbe.ForceD);
            moxi.SetTexture(k, "VelPhi_R", lbe.VelPhi_R);
            moxi.SetTexture(k, "Flow_R", flow.Read);
            moxi.SetTexture(k, "Flow_W", flow.Write);
            ComputeShaderUtil.Dispatch2D(moxi, k, w, h);
            flow.Swap();
        }

        public void PigmentFixture() {
            var k = moxi.FindKernel("Fixture");
            moxi.SetTexture(k, "Flow_R", flow.Read);
            moxi.SetTexture(k, "Flow_W", flow.Write);
            moxi.SetTexture(k, "Fixt_R", fixt.Read);
            moxi.SetTexture(k, "Fixt_W", fixt.Write);
            ComputeShaderUtil.Dispatch2D(moxi, k, w, h);
            flow.Swap();
            fixt.Swap();
        }

        protected Vector2 mousePosCache;
        public Vector4[] CalcBrushHeads(Vector2 mousePos, float width) {
            var m = mousePos;
            var dir = (m - mousePosCache).normalized;
            var bnr = (Vector2)Vector3.Cross(dir, Vector3.forward);
            brushHeads = new Vector4[] {
                new Vector4(m.x, m.y, width, 1),
                new Vector4(m.x + bnr.x, m.y + bnr.y, 0, 1)
            };
            mousePosCache = mousePos;
            return brushHeads;
        }

        public void Reset() {
            Dispose();
            InitBuffer();
            lbe.Reset();
        }

        public void Dispose() {
            surf.Dispose();
            flow.Dispose();
            fixt.Dispose();
            lbe.Dispose();
        }
    }
}
