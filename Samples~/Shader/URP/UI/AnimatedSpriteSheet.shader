Shader "VoyageForge/URP/UI/AnimatedSpriteSheet"
{
    Properties
    {
        [PerRendererData] _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Cols("Columns", Float) = 4
        _Rows("Rows", Float) = 4
        _FrameCount("Frame Count", Float) = 16
        _FPS("Frames per Second", Float) = 12
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask("Color Mask", Float) = 15
        [HideInInspector] _ClipRect("Clip Rect", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "LightMode"="Universal2D" }
        LOD 100
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_UI_ALPHACLIP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionH : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _Cols;
            float _Rows;
            float _FrameCount;
            float _FPS;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionH = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.color = IN.color * _Color;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                int frameIndex = int(fmod(_Time.y * _FPS, _FrameCount));
                int col = frameIndex % int(_Cols);
                int row = frameIndex / int(_Cols);

                float2 uvPerFrame = float2(1.0/_Cols, 1.0/_Rows);
                float2 uvOffset = float2(col * uvPerFrame.x, 1.0 - uvPerFrame.y - row * uvPerFrame.y);
                float2 uv = IN.uv * uvPerFrame + uvOffset;

                half4 colSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * IN.color;

                #if UNITY_UI_ALPHACLIP
                clip(colSample.a - _AlphaClipThreshold);
                #endif

                return colSample;
            }
            ENDHLSL
        }
    }
}