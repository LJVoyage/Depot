Shader "VoyageForge/Builtin/UI/AnimatedSpriteSheet"
{
    Properties
    {
        [PerRendererData] _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Cols("Columns", Float) = 4
        _Rows("Rows", Float) = 4
        _FrameCount("Frame Count", Float) = 16
        _FPS("Frames per Second", Float) = 12
        [HideInInspector] _ZTestMode("ZTest Mode", Float) = 4
        [HideInInspector] _AlphaClipThreshold("Alpha Clip Threshold", Float) = 0.001
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
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [_ZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_UI_ALPHACLIP
            #include "UnityUI.cginc"
            #include "UnityCG.cginc"

            struct a2v
            {
                float4 vertex:POSITION;
                float4 color:COLOR;
                float2 texcoord:TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID };

            struct v2f
            {
                float4 vertex:SV_POSITION;
                fixed4 color:COLOR;
                float2 uv:TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Cols;
            float _Rows;
            float _FrameCount;
            float _FPS;
            float _AlphaClipThreshold;

            v2f vert(a2v IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.color = IN.color * _Color;
                OUT.uv = IN.texcoord;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float frameF = fmod(_Time.y * _FPS, _FrameCount);
                uint frameIndex = (uint)frameF;
                uint col = frameIndex % (uint)_Cols;
                uint row = frameIndex / (uint)_Cols;


                float2 uvPerFrame = float2(1.0 / _Cols, 1.0 / _Rows);
                float2 uvOffset = float2(col * uvPerFrame.x, 1.0 - uvPerFrame.y - row * uvPerFrame.y);
                float2 uv = IN.uv * uvPerFrame + uvOffset;

                fixed4 colSample = tex2D(_MainTex, uv) * IN.color;

                #if UNITY_UI_ALPHACLIP
                clip((float)colSample.a - _AlphaClipThreshold);
                #endif

                return colSample;
            }
            ENDCG
        }
    }
}