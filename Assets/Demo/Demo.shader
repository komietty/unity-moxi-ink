Shader "Hidden/Demo"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Cull Off ZWrite Off ZTest Always
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
			int _ShowR;
			int _ShowG;
			int _ShowB;

            fixed4 frag (v2f_img i) : SV_Target {
                fixed4 c = tex2D(_MainTex, i.uv);
				c.r *= _ShowR;
				c.g *= _ShowG;
				c.b *= _ShowB;
                return c;
            }
            ENDCG
        }
        Pass {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;

            fixed4 frag (v2f_img i) : SV_Target {
                fixed4 c = tex2D(_MainTex, i.uv);
                return fixed4(1 - c.yyy, 1);
            }
            ENDCG
        }
    }
}
