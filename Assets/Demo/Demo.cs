using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using kmty.gist;

namespace moxi {
    public class Demo : MonoBehaviour {

        public enum Mode { Surf, Flow, Fixt, Finl }
        [SerializeField, Range(10, 40)] protected float brushWidth;
        [SerializeField, Range(0, 1)] protected float omega;
        [SerializeField] protected float waterSupply, inkSupply;
        [SerializeField] protected Texture2D grain;
        [SerializeField] protected Texture2D pinning;

        [Header("PostProcess")]
        [SerializeField] protected Mode mode = 0;
        [SerializeField] protected bool r, g, b;

        protected int w, h;
        protected Moxi moxi;
        protected Camera cam => Camera.main;
        protected RenderTexture debugTex;
        protected Material debugMat;
        protected bool miniDebug;

        void Start() {
            w = Screen.width;
            h = Screen.height;
            moxi = new Moxi(w, h, grain, pinning);
            RenderTextureUtil.Build(ref debugTex, w, h);
            debugMat = new Material(Shader.Find("Hidden/Demo"));
            cam.aspect = 1;
            cam.transform.position = Vector3.zero;
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.R)) moxi.Reset();
            if (Input.GetKeyDown(KeyCode.D)) miniDebug = !miniDebug;
            if (Input.GetKeyDown(KeyCode.Space)) mode = (Mode)(((int)mode + 1) % 4);

            var m = (Vector2)cam.ScreenToWorldPoint(Input.mousePosition);
            var p = (m / (cam.orthographicSize * 2) + Vector2.one * 0.5f) * w;

            if (Input.GetMouseButton(0)) moxi.Deposition(p, brushWidth, waterSupply, inkSupply);
            else                         moxi.Deposition(p, brushWidth, 0, 0);

            moxi.Percolation(omega);
            moxi.PigmentSupply();
            moxi.PigmentAdvection();
            moxi.PigmentFixture();

            switch (mode) {
                case Mode.Surf: debugTex = PostProcess(moxi.Surf, 0); break;
                case Mode.Flow: debugTex = PostProcess(moxi.Flow, 0); break;
                case Mode.Fixt: debugTex = PostProcess(moxi.Fixt, 0); break;
                case Mode.Finl: debugTex = PostProcess(moxi.Fixt, 1); break;
            }
        }

        void OnDestroy() {
            moxi.Dispose();
            Destroy(debugMat);
        }

        void OnGUI() {
            if (moxi == null) return;
            var s = new Vector2(Screen.width, Screen.height);
            var m = s / 3f;
            GUI.DrawTexture(new Rect(0, 0, s.x, s.y), debugTex);
            if (miniDebug) {
                GUI.DrawTexture(new Rect(0, 0,       m.x, m.y), moxi.Surf);
                GUI.DrawTexture(new Rect(0, m.y,     m.x, m.y), moxi.Flow);
                GUI.DrawTexture(new Rect(0, m.y * 2, m.x, m.y), moxi.Fixt);
            }
        }

        RenderTexture PostProcess(RenderTexture t, int pass) {
            debugMat.SetInt("_ShowR", r ? 1 : 0);
            debugMat.SetInt("_ShowG", g ? 1 : 0);
            debugMat.SetInt("_ShowB", b ? 1 : 0);
            Graphics.Blit(t, debugTex, debugMat, pass);
            return debugTex;
        }
    }
}
