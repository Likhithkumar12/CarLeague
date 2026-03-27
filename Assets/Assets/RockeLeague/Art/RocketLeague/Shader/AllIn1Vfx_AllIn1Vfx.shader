Shader "AllIn1Vfx/AllIn1Vfx" {
	Properties {
		_RenderingMode ("Rendering Mode", Float) = 0
		_SrcMode ("SrcMode", Float) = 5
		_DstMode ("DstMode", Float) = 10
		_CullingOption ("Culling Option", Float) = 0
		_ZWrite ("Depth Write", Float) = 0
		_ZTestMode ("Z Test Mode", Float) = 4
		_ColorMask ("Color Write Mask", Float) = 15
		_Alpha ("Global Alpha", Range(0, 1)) = 1
		_Color ("Global Color", Vector) = (1,1,1,1)
		_TimingSeed ("Random Seed", Float) = 0
		_EditorDrawers ("Editor Drawers", Float) = 60
		_MainTex ("Shape1 Texture", 2D) = "white" {}
		[HDR] _ShapeColor ("Shape1 Color", Vector) = (1,1,1,1)
		_ShapeXSpeed ("Shape1 X Speed", Float) = 0
		_ShapeYSpeed ("Shape1 Y Speed", Float) = 0
		_ShapeContrast ("Shape1 Contrast", Range(0, 10)) = 1
		_ShapeBrightness ("Shape1 Brightness", Range(-1, 1)) = 0
		_ShapeDistortTex ("Distortion Texture", 2D) = "black" {}
		_ShapeDistortAmount ("Distortion Amount", Range(0, 10)) = 0.5
		_ShapeDistortXSpeed ("Scroll speed X", Float) = 0.1
		_ShapeDistortYSpeed ("Scroll speed Y", Float) = 0.1
		_ShapeColorWeight ("Shape1 RGB Weight", Range(0, 5)) = 1
		_ShapeAlphaWeight ("Shape1 A Weight", Range(0, 5)) = 1
		_Shape2Tex ("Shape2 Texture", 2D) = "white" {}
		[HDR] _Shape2Color ("Shape2 Color", Vector) = (1,1,1,1)
		_Shape2XSpeed ("Shape2 X Speed", Float) = 0
		_Shape2YSpeed ("Shape2 Y Speed", Float) = 0
		_Shape2Contrast ("Shape2 Contrast", Range(0, 10)) = 1
		_Shape2Brightness ("Shape2 Brightness", Range(-1, 1)) = 0
		_Shape2DistortTex ("Distortion Texture", 2D) = "black" {}
		_Shape2DistortAmount ("Distortion Amount", Range(0, 10)) = 0.5
		_Shape2DistortXSpeed ("Scroll Speed X", Float) = 0.1
		_Shape2DistortYSpeed ("Scroll Speed Y", Float) = 0.1
		_Shape2ColorWeight ("Shape2 RGB Weight", Range(0, 5)) = 2
		_Shape2AlphaWeight ("Shape2 A Weight", Range(0, 5)) = 2
		_Shape3Tex ("Shape3 Texture", 2D) = "white" {}
		[HDR] _Shape3Color ("Shape3 Color", Vector) = (1,1,1,1)
		_Shape3XSpeed ("Shape3 X Speed", Float) = 0
		_Shape3YSpeed ("Shape3 Y Speed", Float) = 0
		_Shape3Contrast ("Shape3 Contrast", Range(0, 10)) = 1
		_Shape3Brightness ("Shape3 Brightness", Range(-1, 1)) = 0
		_Shape3DistortTex ("Distortion Texture", 2D) = "black" {}
		_Shape3DistortAmount ("Distortion Amount", Range(0, 10)) = 0.5
		_Shape3DistortXSpeed ("Scroll Speed X", Float) = 0.1
		_Shape3DistortYSpeed ("Scroll Speed Y", Float) = 0.1
		_Shape3ColorWeight ("Shape3 RGB Weight", Range(0, 5)) = 2
		_Shape3AlphaWeight ("Shape3 A Weight", Range(0, 5)) = 2
		_SoftFactor ("Soft Particles Factor", Range(0.01, 3)) = 0.5
		[NoScaleOffset] _ColorRampTex ("Color Ramp Texture", 2D) = "white" {}
		_ColorRampLuminosity ("Color Ramp luminosity", Range(-1, 1)) = 0
		[AllIn1VfxGradient] _ColorRampTexGradient ("Color Ramp Gradient", 2D) = "white" {}
		_ColorRampBlend ("Color Ramp Blend", Range(0, 1)) = 1
		_AlphaCutoffValue ("Alpha cutoff value", Range(0, 1)) = 0.25
		_AlphaStepMin ("Smoothstep Min", Range(0, 1)) = 0
		_AlphaStepMax ("Smoothstep Max", Range(0, 1)) = 0.075
		_AlphaFadeAmount ("Fade Amount", Range(-0.1, 1)) = -0.1
		_AlphaFadeSmooth ("Fade Transition", Range(0, 1.5)) = 0.075
		_AlphaFadePow ("Fade Power", Range(0.001, 10)) = 1
		_TrailWidthPower ("Trail Width Power", Range(0.1, 5)) = 1
		[AllIn1VfxGradient] _TrailWidthGradient ("Trail Width Gradient", 2D) = "white" {}
		_GlowColor ("Glow Color", Vector) = (1,1,1,1)
		_Glow ("Glow Color Intensity", Float) = 0
		_GlowGlobal ("Global Glow Intensity", Float) = 1
		_GlowTex ("Glow Mask Texture", 2D) = "white" {}
		_DepthGlowDist ("Depth Distance", Range(0.01, 10)) = 0.5
		_DepthGlowPow ("Depth Power", Range(0.01, 10)) = 1
		_DepthGlowColor ("Glow Color", Vector) = (1,1,1,1)
		_DepthGlow ("Glow Color Intensity", Float) = 1
		_DepthGlowGlobal ("Global Glow Intensity", Float) = 1
		_MaskTex ("Mask Texture", 2D) = "white" {}
		_MaskPow ("Mask Power", Range(0.001, 10)) = 1
		_HsvShift ("Hue Shift", Range(0, 360)) = 180
		_HsvSaturation ("Saturation", Range(0, 2)) = 1
		_HsvBright ("Brightness", Range(0, 2)) = 1
		_RandomSh1Mult ("Shape 1 Mult", Range(0, 1)) = 1
		_RandomSh2Mult ("Shape 2 Mult", Range(0, 1)) = 1
		_RandomSh3Mult ("Shape 3 Mult", Range(0, 1)) = 1
		_PixelateSize ("Pixelate size", Range(4, 512)) = 32
		_DistortTex ("Distortion Texture", 2D) = "black" {}
		_DistortAmount ("Distortion Amount", Range(0, 10)) = 0.5
		_DistortTexXSpeed ("Scroll Speed X", Range(-50, 50)) = 5
		_DistortTexYSpeed ("Scroll Speed Y", Range(-50, 50)) = 5
		[HDR] _BackFaceTint ("Backface Tint", Vector) = (0.5,0.5,0.5,1)
		[HDR] _FrontFaceTint ("Frontface Tint", Vector) = (1,1,1,1)
		_ShakeUvSpeed ("Shake Speed", Range(0, 50)) = 20
		_ShakeUvX ("X Multiplier", Range(-15, 15)) = 5
		_ShakeUvY ("Y Multiplier", Range(-15, 15)) = 4
		_WaveAmount ("Wave Amount", Range(0, 25)) = 7
		_WaveSpeed ("Wave Speed", Range(0, 25)) = 10
		_WaveStrength ("Wave Strength", Range(0, 25)) = 7.5
		_WaveX ("Wave X Axis", Range(0, 1)) = 0
		_WaveY ("Wave Y Axis", Range(0, 1)) = 0.5
		_RoundWaveStrength ("Wave Strength", Range(0, 1)) = 0.7
		_RoundWaveSpeed ("Wave Speed", Range(0, 5)) = 2
		_TwistUvAmount ("Twist Amount", Range(0, 3.1416)) = 1
		_TwistUvPosX ("Twist Pos X Axis", Range(0, 1)) = 0.5
		_TwistUvPosY ("Twist Pos Y Axis", Range(0, 1)) = 0.5
		_TwistUvRadius ("Twist Radius", Range(0, 3)) = 0.75
		_HandDrawnAmount ("Hand Drawn Amount", Range(0, 40)) = 10
		_HandDrawnSpeed ("Hand Drawn Speed", Range(1, 30)) = 5
		_OffsetSh1 ("Shape 1 Offset Mult", Range(-5, 5)) = 1
		_OffsetSh2 ("Shape 2 Offset Mult", Range(-5, 5)) = 1
		_OffsetSh3 ("Shape 3 Offset Mult", Range(-5, 5)) = 1
		_DistNormalMap ("Normal Map", 2D) = "bump" {}
		_DistortionPower ("Distortion Power", Float) = 10
		_DistortionBlend ("Distortion Blend", Range(0, 1)) = 1
		_DistortionScrollXSpeed ("Scroll speed X Axis", Float) = 0
		_DistortionScrollYSpeed ("Scroll speed Y Axis", Float) = 0
		_TextureScrollXSpeed ("Speed X Axis", Float) = 1
		_TextureScrollYSpeed ("Speed Y Axis", Float) = 0
		_VertOffsetTex ("Offset Noise Texture", 2D) = "white" {}
		_VertOffsetAmount ("Offset Amount", Range(0, 2)) = 0.5
		_VertOffsetPower ("Offset Power", Range(0.01, 10)) = 1
		_VertOffsetTexXSpeed ("Scroll Speed X", Range(-2, 2)) = 0.1
		_VertOffsetTexYSpeed ("Scroll Speed Y", Range(-2, 2)) = 0.1
		_FadeTex ("Fade Texture", 2D) = "white" {}
		_FadeAmount ("Fade Amount", Range(-0.1, 1)) = -0.1
		_FadeTransition ("Fade Transition", Range(0.01, 0.75)) = 0.075
		_FadePower ("Fade Power", Range(0.001, 10)) = 1
		_FadeScrollXSpeed ("Speed X Axis", Float) = 0
		_FadeScrollYSpeed ("Speed Y Axis", Float) = 0
		_FadeBurnTex ("Fade Burn Texture", 2D) = "white" {}
		[HDR] _FadeBurnColor ("Fade Burn Color", Vector) = (1,1,0,1)
		_FadeBurnWidth ("Fade Burn Width", Range(0, 0.2)) = 0.01
		_FadeBurnGlow ("Fade Burn Glow", Range(1, 250)) = 5
		[HDR] _ColorGradingLight ("Light Color Tint", Vector) = (1,1,1,1)
		[HDR] _ColorGradingMiddle ("Mid Tone Color Tint", Vector) = (1,1,1,1)
		[HDR] _ColorGradingDark ("Dark/Shadow Color Tint", Vector) = (1,1,1,1)
		_ColorGradingMidPoint ("Mid Point", Range(0.01, 0.99)) = 0.5
		_CamDistFadeStepMin ("Far Fade Start Point", Range(0, 1000)) = 0
		_CamDistFadeStepMax ("Far Fade End Point", Range(0, 1000)) = 100
		_CamDistProximityFade ("Close Fade Start Point", Range(0, 250)) = 0
		_ScreenUvShDistScale ("Scale With Dist Amount", Range(0, 1)) = 1
		_ScreenUvSh2DistScale ("Scale With Dist Amount", Range(0, 1)) = 1
		_ScreenUvSh3DistScale ("Scale With Dist Amount", Range(0, 1)) = 1
		[HDR] _RimColor ("Rim Color", Vector) = (1,1,1,1)
		_RimBias ("Rim Bias", Range(0, 1)) = 0
		_RimScale ("Rim Scale", Range(0, 25)) = 1
		_RimPower ("Rim Power", Range(0.1, 20)) = 5
		_RimIntensity ("Rim Intensity", Range(0, 50)) = 1
		_RimAddAmount ("Add Amount (0 is mult)", Range(0, 1)) = 1
		_RimErodesAlpha ("Erode Transparency", Range(0, 2)) = 0
		_Shape1MaskTex ("Shape 1 Mask Texture", 2D) = "white" {}
		_Shape1MaskPow ("Shape 1 Mask Power", Range(0.001, 10)) = 1
		_LightAmount ("Light Amount", Range(0, 1)) = 0
		[HDR] _LightColor ("Light Color", Vector) = (1,1,1,1)
		_ShadowAmount ("Shadow Amount", Range(0, 1)) = 0.4
		_ShadowStepMin ("Shadow Min", Range(0, 1)) = 0
		_ShadowStepMax ("Shadow Max", Range(0, 1)) = 1
		_PosterizeNumColors ("Number of Colors", Range(1, 30)) = 5
		_ShapeRotationOffset ("Rotation Offset", Range(0, 6.2831855)) = 0
		_ShapeRotationSpeed ("Rotation Speed", Float) = 0
		_Shape2RotationOffset ("Rotation Offset", Range(0, 6.2831855)) = 0
		_Shape2RotationSpeed ("Rotation Speed", Float) = 0
		_Shape3RotationOffset ("Rotation Offset", Range(0, 6.2831855)) = 0
		_Shape3RotationSpeed ("Rotation Speed", Float) = 0
		_Sh1BlendOffset ("Shape 1 Blend Offset", Range(-5, 5)) = 0
		_Sh2BlendOffset ("Shape 2 Blend Offset", Range(-5, 5)) = 0
		_Sh3BlendOffset ("Shape 3 Blend Offset", Range(-5, 5)) = 0
		_DebugShape ("Shape Debug Number", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;
			float4 _Color;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy) * _Color;
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/InternalErrorShader"
	//CustomEditor "AllIn1VfxCustomMaterialEditor"
}