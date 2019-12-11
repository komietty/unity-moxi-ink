using System;
using System.IO;
using UnityEngine;

namespace kmty.gist {
    public static class RenderTextureUtil {
        static public void Build(ref RenderTexture tgt, int w, int h, int d = 0, bool randomWrite = true, RenderTextureFormat f = RenderTextureFormat.ARGBFloat) {
            if (tgt != null) {
                UnityEngine.Object.Destroy(tgt);
                tgt = null;
            }
            tgt = new RenderTexture(w, h, d, f);
            tgt.enableRandomWrite = randomWrite;
            tgt.Create();
        }
        static public void Rebuild(RenderTexture src, ref RenderTexture tgt) {
            if (src != null && (tgt == null || tgt.width != src.width || tgt.height != src.height))
                Build(ref tgt, src.width, src.height, src.depth, src.enableRandomWrite, src.format);
        }
        static public void Clear(RenderTexture tgt) {
            if (tgt == null) return;
            var store = RenderTexture.active;
            RenderTexture.active = tgt;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = store;
        }
        static public void Destroy(RenderTexture tgt) {
            if (tgt != null) {
                UnityEngine.Object.Destroy(tgt);
                tgt = null;
            }
        }
        static public void savePng(RenderTexture rt, string path) {
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.Destroy(tex);
            File.WriteAllBytes(Application.dataPath + $"{path}.png", bytes);
        }
    }

    public class PingPongRenderTexture : IDisposable {
        protected RenderTexture rt0, rt1;
        public RenderTexture Read => rt0;
        public RenderTexture Write => rt1;

        public PingPongRenderTexture(int w, int h, int depth, RenderTextureFormat format, FilterMode filter = FilterMode.Point) {

            rt0 = new RenderTexture(w, h, depth, format) {
                filterMode = filter,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
                enableRandomWrite = true
            };
            rt1 = new RenderTexture(w, h, depth, format) {
                filterMode = filter,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
                enableRandomWrite = true
            };
            rt0.Create();
            rt1.Create();
        }
        

        public PingPongRenderTexture(RenderTextureDescriptor disc, FilterMode filter = FilterMode.Point) {
            rt0 = new RenderTexture(disc) {
                filterMode = filter,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
                enableRandomWrite = true
            };
; 
            rt1 = new RenderTexture(disc) {
                filterMode = filter,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
                enableRandomWrite = true
            };
;
            rt0.Create();
            rt1.Create();
        }

        public PingPongRenderTexture(RenderTexture rt) {
            rt0 = new RenderTexture(rt); 
            rt1 = new RenderTexture(rt);
            rt0.Create();
            rt1.Create();
        }
        
        public void Copy() {
            Graphics.Blit(rt0, rt1); 
        }

        public void Swap() {
            var tmp = rt0;
            rt0 = rt1;
            rt1 = tmp;
        }

        public void Dispose() {
            RenderTextureUtil.Destroy(rt0); 
            RenderTextureUtil.Destroy(rt1); 
        }

        public static implicit operator RenderTexture(PingPongRenderTexture prt) => prt.Read;
    }
}