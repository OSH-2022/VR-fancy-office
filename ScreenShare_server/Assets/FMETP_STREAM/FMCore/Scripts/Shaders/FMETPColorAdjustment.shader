Shader "Hidden/FMETPColorAdjustment"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _Brightness ("Brightness", float) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID //for single pass vr
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO //for single pass vr
            };

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex); //for single pass vr
            float _Brightness;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v); //for single pass vr
                UNITY_INITIALIZE_OUTPUT(v2f, o); //for single pass vr
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //for single pass vr

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //for single pass vr

                float4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
                col.rgb *= _Brightness;
                return col;
            }
            ENDCG
        }
    }
}
