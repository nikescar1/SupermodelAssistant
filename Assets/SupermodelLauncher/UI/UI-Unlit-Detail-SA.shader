// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "SA/UI-Unlit-Detail"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)

        _DetailTex ("Detail (RGB)", 2D) = "white" {}
        _Strength ("Detail Strength", Range(0, 1.0)) = 0.2
		_Scale("Scale", Float) = 1

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        LOD 100

        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType"="Plane"
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
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                fixed4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _DetailTex;
            float4 _MainTex_ST;
            float4 _DetailTex_ST;
            float4 _DetailTex_TexelSize;
            fixed4 _Color;
            fixed _Strength;
			fixed _Scale;

            fixed4 _TextureSampleAdd;

            bool _UseClipRect;
            float4 _ClipRect;

            bool _UseAlphaClip;

            v2f vert (appdata_t IN)
            {
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				//OUT.texcoord2 = TRANSFORM_TEX(IN.texcoord2, _DetailTex);
#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw - 1.0)*float2(-1, 1);
#endif
				OUT.color = IN.color;
				OUT.screenPos = ComputeScreenPos(OUT.vertex);
				return OUT;
            }

            fixed4 frag (v2f IN) : COLOR
            {
				half4 color = tex2D(_MainTex, IN.texcoord);
				//IN.texcoord.x += _Time.x * _Speed;
				half2 screenUV = IN.screenPos.xy / IN.screenPos.w;
				//screenUV.y += _Time.x * _SpeedY;
				//screenUV.x += _Time.x * _SpeedX;
				//color.rgb = lerp(color.rgb, color.rgb * detail.rgb, detail.a * _Strength);
				half4 detailColor = tex2D(_DetailTex, screenUV * _Scale);// *_Color;
				color.rgb = lerp(color.rgb * IN.color, detailColor.rgb, _Strength);
				//color = half4(detailColor.rgb * IN.color,color.a);
				clip(color.a - 0.01);
				return color;
            }
            ENDCG

        }
    }
}
