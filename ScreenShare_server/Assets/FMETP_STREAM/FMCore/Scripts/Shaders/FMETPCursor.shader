Shader "Hidden/FMETPCursor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CursorTex ("Cursor", 2D) = "white" {}

        _ScreenWidth ("CursorWidth", float) = 1
        _ScreenHeight ("CursorHeight", float) = 1

        _CursorWidth ("CursorWidth", float) = 1
        _CursorHeight ("CursorHeight", float) = 1

        
        _CursorScaling ("CursorScaling", float) = 1
        
        _CursorPointX ("CursorPointX", float) = 1
        _CursorPointY ("CursorPointY", float) = 1

        
        _CursorHotSpotX ("CursorHotSpotX", float) = 1
        _CursorHotSpotY ("CursorHotSpotY", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _CursorTex;
            
            float _ScreenWidth;
            float _ScreenHeight;

            float _CursorWidth;
            float _CursorHeight;

            float _CursorScaling;

            float _CursorPointX;
            float _CursorPointY;
            float _CursorHotSpotX;
            float _CursorHotSpotY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float screenWidth = _ScreenWidth;
                float screenHeight = _ScreenHeight;
                
                float cursorScaling = _CursorScaling;
                float cursorWidth = _CursorWidth;
                float cursorHeight = _CursorHeight;

                float cursorPointX = _CursorPointX;
                float cursorPointY = _CursorPointY;

                float cursorHotSpotX = _CursorHotSpotX;
                float cursorHotSpotY = _CursorHotSpotY;

                //flip vertically
                float2 rawUV = float2(i.uv.x, 1.0 - i.uv.y);
                fixed4 screenColor = tex2D(_MainTex, rawUV);

                float2 uvCursor = rawUV;
                uvCursor.x *= cursorScaling * screenWidth / cursorWidth;
                uvCursor.y *= cursorScaling * screenHeight / cursorHeight;

                float2 pointUV = float2(cursorPointX / screenWidth, cursorPointY / screenHeight);
                float2 cursorScreenRatio = float2(cursorWidth / screenWidth, cursorHeight / screenHeight);

                float boundleft = pointUV.x;
                float boundright = pointUV.x + (cursorScreenRatio.x / cursorScaling);
                
                float boundbottom = pointUV.y;
                float boundtop = pointUV.y + (cursorScreenRatio.y / cursorScaling);

                if(rawUV.x > boundleft && rawUV.x < boundright && rawUV.y > boundbottom && rawUV.y < boundtop)
                {
                    float uvCursorOffX = (cursorPointX / cursorWidth) * cursorScaling;
                    float uvCursorOffY = (cursorPointY / cursorHeight) * cursorScaling;

                    uvCursor.x -= uvCursorOffX;
                    uvCursor.y -= uvCursorOffY;

                    fixed4 cursorColor = tex2D(_CursorTex, uvCursor);
                    if(cursorColor.a > 0)
                    {
                        screenColor.rgb = cursorColor.rgb;
                    }
                    else
                    {
                        //screenColor.rgb += float4(0.25,0,0,1);
                    }
                }
                return screenColor;
            }
            ENDCG
        }
    }
}
