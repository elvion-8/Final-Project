// Made with Amplify Shader Editor v1.9.7.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Vefects/SH_Vefects_VFX_BIRP_Goo_Splash_01"
{
	Properties
	{
		_Color01("Color 01", Color) = (0.3960784,0.4352941,0.09019608,1)
		_Color02("Color 02", Color) = (0.3098039,0.372549,0.1294118,1)
		_Color03("Color 03", Color) = (0.9882354,0.9843138,0.2705882,1)
		_ColorFresnel("Color Fresnel", Color) = (0.08235294,0.8901961,0.8313726,1)
		_HueShift("Hue Shift", Float) = 0
		_Emission("Emission", Float) = 0.1
		_Specular("Specular", Float) = 0.01
		_SmoothnessMin("Smoothness Min", Float) = 0.5
		_SmoothnessMax("Smoothness Max", Float) = 0.96
		[Space(33)][Header(Noise)][Space(13)]_NoiseTexture("Noise Texture", 2D) = "white" {}
		[Space(33)][Header(Normal)][Space(13)]_NoiseTextureNormal("Noise Texture Normal", 2D) = "white" {}
		_NormalIntensity("Normal Intensity", Float) = 1
		[Space(33)][Header(Distortion)][Space(13)]_NoiseTextureDistortion("Noise Texture Distortion", 2D) = "white" {}
		_NoiseTextureDistortionScale("Noise Texture Distortion Scale", Vector) = (0.5,0.5,0,0)
		_NoiseTextureDistortionPan("Noise Texture Distortion Pan", Vector) = (-0.05,-0.1,0,0)
		_NoiseDistortionLerp("Noise Distortion Lerp", Float) = 0.03
		[Space(33)][Header(Fresnel)][Space(13)]_FresnelScale("Fresnel Scale", Float) = 1
		_FresnelPower("Fresnel Power", Float) = 3
		_FresnelBias("Fresnel Bias", Float) = 0
		_ErosionSmoothness("Erosion Smoothness", Float) = 0.1
		[Space(33)][Header(AR)][Space(13)]_Cull("Cull", Float) = 2
		_Src("Src", Float) = 5
		_Dst("Dst", Float) = 10
		_ZWrite("ZWrite", Float) = 0
		_ZTest("ZTest", Float) = 2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull [_Cull]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		Blend [_Src] [_Dst]
		
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#define ASE_VERSION 19701
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 uv_texcoord;
			float3 worldPos;
			INTERNAL_DATA
			float3 worldNormal;
		};

		uniform float _Src;
		uniform float _Dst;
		uniform float _ZWrite;
		uniform float _ZTest;
		uniform float _Cull;
		uniform sampler2D _NoiseTextureNormal;
		uniform sampler2D _NoiseTextureDistortion;
		uniform float2 _NoiseTextureDistortionPan;
		uniform float2 _NoiseTextureDistortionScale;
		uniform float _NoiseDistortionLerp;
		uniform float _NormalIntensity;
		uniform float4 _Color02;
		uniform float4 _Color01;
		uniform sampler2D _NoiseTexture;
		uniform float4 _Color03;
		uniform float _HueShift;
		uniform float4 _ColorFresnel;
		uniform float _FresnelBias;
		uniform float _FresnelScale;
		uniform float _FresnelPower;
		uniform float _Emission;
		uniform float _Specular;
		uniform float _SmoothnessMin;
		uniform float _SmoothnessMax;
		uniform float _ErosionSmoothness;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 panner72 = ( 1.0 * _Time.y * _NoiseTextureDistortionPan + ( i.uv_texcoord.xy * _NoiseTextureDistortionScale ));
			float lerpResult61 = lerp( -1.0 , 1.0 , tex2D( _NoiseTextureDistortion, panner72 ).g);
			float2 temp_cast_0 = (lerpResult61).xx;
			float2 lerpResult63 = lerp( float2( 0,0 ) , temp_cast_0 , _NoiseDistortionLerp);
			float2 temp_output_14_0 = ( i.uv_texcoord.xy + lerpResult63 );
			float3 lerpResult15 = lerp( float3(0,0,1) , tex2D( _NoiseTextureNormal, temp_output_14_0 ).rgb , _NormalIntensity);
			float3 normal18 = lerpResult15;
			o.Normal = normal18;
			float4 tex2DNode11 = tex2D( _NoiseTexture, temp_output_14_0 );
			float3 lerpResult34 = lerp( _Color02.rgb , _Color01.rgb , tex2DNode11.g);
			float3 lerpResult30 = lerp( lerpResult34 , _Color03.rgb , saturate( ( ( tex2DNode11.g - 0.9 ) / 2.0 ) ));
			float temp_output_40_0 = saturate( ( ( pow( tex2DNode11.g , 3.0 ) - 0.5 ) * 2.0 ) );
			float3 lerpResult36 = lerp( ( lerpResult30 * 0.7 ) , lerpResult30 , temp_output_40_0);
			float3 hsvTorgb44 = RGBToHSV( lerpResult36 );
			float3 hsvTorgb43 = HSVToRGB( float3(( hsvTorgb44.x + _HueShift ),hsvTorgb44.y,hsvTorgb44.z) );
			float3 color20 = hsvTorgb43;
			o.Albedo = color20;
			float3 temp_cast_1 = (0.0).xxx;
			float3 ase_worldPos = i.worldPos;
			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float fresnelNdotV53 = dot( mul(ase_tangentToWorldFast,normal18), ase_viewDirWS );
			float fresnelNode53 = ( _FresnelBias + _FresnelScale * pow( max( 1.0 - fresnelNdotV53 , 0.0001 ), _FresnelPower ) );
			float3 lerpResult54 = lerp( temp_cast_1 , _ColorFresnel.rgb , saturate( fresnelNode53 ));
			float3 emission50 = ( lerpResult54 * _Emission );
			o.Emission = emission50;
			float3 temp_cast_2 = (_Specular).xxx;
			o.Specular = temp_cast_2;
			float lerpResult48 = lerp( _SmoothnessMin , _SmoothnessMax , temp_output_40_0);
			o.Smoothness = lerpResult48;
			float clampResult9_g2 = clamp( ( ( length( (float2( -1,-1 ) + (i.uv_texcoord.xy - float2( 0,0 )) * (float2( 1,1 ) - float2( -1,-1 )) / (float2( 1,1 ) - float2( 0,0 ))) ) + -0.0 ) * 0.3 ) , 0.0 , 1.0 );
			float temp_output_174_0 = ( i.uv_texcoord.z + saturate( clampResult9_g2 ) );
			float smoothstepResult171 = smoothstep( temp_output_174_0 , ( temp_output_174_0 + _ErosionSmoothness ) , tex2DNode11.g);
			float opacity175 = smoothstepResult171;
			o.Alpha = opacity175;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular keepalpha fullforwardshadows noforwardadd 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xyzw = customInputData.uv_texcoord;
				o.customPack1.xyzw = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xyzw;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19701
Node;AmplifyShaderEditor.TextureCoordinatesNode;70;-7424,768;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;73;-7040,896;Inherit;False;Property;_NoiseTextureDistortionScale;Noise Texture Distortion Scale;14;0;Create;True;0;0;0;False;0;False;0.5,0.5;0.5,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-7040,768;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;74;-6400,896;Inherit;False;Property;_NoiseTextureDistortionPan;Noise Texture Distortion Pan;15;0;Create;True;0;0;0;False;0;False;-0.05,-0.1;-0.05,-0.1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;72;-6400,768;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;13;-6016,768;Inherit;True;Property;_NoiseTextureDistortion;Noise Texture Distortion;13;0;Create;True;0;0;0;False;3;Space(33);Header(Distortion);Space(13);False;-1;1f0e4d7c9fccb3545b652c6042307b03;1f0e4d7c9fccb3545b652c6042307b03;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;61;-5504,768;Inherit;False;3;0;FLOAT;-1;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-4864,384;Inherit;False;Property;_NoiseDistortionLerp;Noise Distortion Lerp;16;0;Create;True;0;0;0;False;0;False;0.03;0.03;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;65;-5120,256;Inherit;False;Constant;_Vector1;Vector 1;17;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-6784,128;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;63;-4864,256;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;-4608,128;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;11;-4224,128;Inherit;True;Property;_NoiseTexture;Noise Texture;10;0;Create;True;0;0;0;False;3;Space(33);Header(Noise);Space(13);False;-1;03f50bc3e5dddf8469291c735da41a6d;03f50bc3e5dddf8469291c735da41a6d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleSubtractOpNode;27;-3712,0;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.9;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;16;-1792,640;Inherit;False;Constant;_Vector0;Vector 0;3;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;17;-1536,640;Inherit;False;Property;_NormalIntensity;Normal Intensity;12;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;32;-3712,-768;Inherit;False;Property;_Color02;Color 02;2;0;Create;True;0;0;0;False;0;False;0.3098039,0.372549,0.1294118,1;0.1058823,0.1058823,0.1058823,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleDivideOpNode;28;-3584,0;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;37;-3712,256;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;31;-3712,-1024;Inherit;False;Property;_Color01;Color 01;1;0;Create;True;0;0;0;False;0;False;0.3960784,0.4352941,0.09019608,1;0.2352941,0.2352941,0.2352941,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;12;-4224,512;Inherit;True;Property;_NoiseTextureNormal;Noise Texture Normal;11;0;Create;True;0;0;0;False;3;Space(33);Header(Normal);Space(13);False;-1;d97044aa1c840254997a0963e222ba4d;d97044aa1c840254997a0963e222ba4d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;15;-1536,512;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;33;-3712,-512;Inherit;False;Property;_Color03;Color 03;3;0;Create;True;0;0;0;False;0;False;0.9882354,0.9843138,0.2705882,1;0.2705882,0,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;34;-3200,-640;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;38;-3584,256;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;29;-3456,0;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;165;-4224,1664;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;166;-4224,1792;Inherit;False;Constant;_Float2;Float 2;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;167;-4224,1920;Inherit;False;Constant;_Float3;Float 3;19;0;Create;True;0;0;0;False;0;False;0.3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;30;-3200,-128;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;-3456,256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2944,-256;Inherit;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;0.7;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;18;-1280,512;Inherit;True;normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;163;-3968,1664;Inherit;False;RadialGradient;-1;;2;ec972f7745a8353409da2eb8d000a2e3;0;3;1;FLOAT2;0,0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;40;-3328,256;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;35;-2944,-384;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0.7;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-2688,-1024;Inherit;False;Property;_FresnelPower;Fresnel Power;18;0;Create;True;0;0;0;False;0;False;3;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-2688,-896;Inherit;False;Property;_FresnelBias;Fresnel Bias;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;-2304,-1152;Inherit;False;18;normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-2688,-1152;Inherit;False;Property;_FresnelScale;Fresnel Scale;17;0;Create;True;0;0;0;False;3;Space(33);Header(Fresnel);Space(13);False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;164;-3584,1664;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;170;-4224,1152;Inherit;False;0;4;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;36;-2688,-128;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FresnelNode;53;-2048,-1152;Inherit;False;Standard;TangentNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;174;-3328,1280;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;173;-2944,1536;Inherit;False;Property;_ErosionSmoothness;Erosion Smoothness;20;0;Create;True;0;0;0;False;0;False;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;44;-2432,-128;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;46;-2176,-256;Inherit;False;Property;_HueShift;Hue Shift;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;49;-2688,-1408;Inherit;False;Property;_ColorFresnel;Color Fresnel;4;0;Create;True;0;0;0;False;0;False;0.08235294,0.8901961,0.8313726,1;0.6274511,0.427451,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;55;-2048,-1536;Inherit;False;Constant;_Float1;Float 1;13;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;56;-1536,-1152;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;172;-2944,1408;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;45;-2176,-128;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;54;-2048,-1408;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-1536,-1536;Inherit;False;Property;_Emission;Emission;6;0;Create;True;0;0;0;False;0;False;0.1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;171;-2944,1152;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;43;-2048,-128;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1536,-1408;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;182;718,-50;Inherit;False;1252;162.95;Ge Lush was here! <3;5;178;179;180;181;177;Ge Lush was here! <3;0,0,0,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;175;-1280,1152;Inherit;False;opacity;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;50;-1280,-1408;Inherit;False;emission;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-640,512;Inherit;False;Property;_SmoothnessMin;Smoothness Min;8;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;-640,640;Inherit;False;Property;_SmoothnessMax;Smoothness Max;9;0;Create;True;0;0;0;False;0;False;0.96;0.96;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;20;-1280,-128;Inherit;True;color;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;21;-640,0;Inherit;False;20;color;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;48;-384,512;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;19;-640,256;Inherit;False;18;normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;51;-640,128;Inherit;False;50;emission;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-640,384;Inherit;False;Property;_Specular;Specular;7;0;Create;True;0;0;0;False;0;False;0.01;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;176;-640,768;Inherit;False;175;opacity;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;178;1024,0;Inherit;False;Property;_Src;Src;22;0;Create;True;0;0;0;True;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;179;1280,0;Inherit;False;Property;_Dst;Dst;23;0;Create;True;0;0;0;True;0;False;10;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;180;1536,0;Inherit;False;Property;_ZWrite;ZWrite;24;0;Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;181;1792,0;Inherit;False;Property;_ZTest;ZTest;25;0;Create;True;0;0;0;True;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;177;768,0;Inherit;False;Property;_Cull;Cull;21;0;Create;True;0;0;0;True;3;Space(33);Header(AR);Space(13);False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;184;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;StandardSpecular;Vefects/SH_Vefects_VFX_BIRP_Goo_Splash_01;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;False;Back;0;True;_ZWrite;0;True;_ZTest;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;1;5;True;_Src;10;True;_Dst;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;True;_Cull;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;71;0;70;0
WireConnection;71;1;73;0
WireConnection;72;0;71;0
WireConnection;72;2;74;0
WireConnection;13;1;72;0
WireConnection;61;2;13;2
WireConnection;63;0;65;0
WireConnection;63;1;61;0
WireConnection;63;2;64;0
WireConnection;14;0;10;0
WireConnection;14;1;63;0
WireConnection;11;1;14;0
WireConnection;27;0;11;2
WireConnection;28;0;27;0
WireConnection;37;0;11;2
WireConnection;12;1;14;0
WireConnection;15;0;16;0
WireConnection;15;1;12;5
WireConnection;15;2;17;0
WireConnection;34;0;32;5
WireConnection;34;1;31;5
WireConnection;34;2;11;2
WireConnection;38;0;37;0
WireConnection;29;0;28;0
WireConnection;30;0;34;0
WireConnection;30;1;33;5
WireConnection;30;2;29;0
WireConnection;39;0;38;0
WireConnection;18;0;15;0
WireConnection;163;1;165;0
WireConnection;163;6;166;0
WireConnection;163;7;167;0
WireConnection;40;0;39;0
WireConnection;35;0;30;0
WireConnection;35;1;41;0
WireConnection;164;0;163;0
WireConnection;36;0;35;0
WireConnection;36;1;30;0
WireConnection;36;2;40;0
WireConnection;53;0;60;0
WireConnection;53;1;59;0
WireConnection;53;2;57;0
WireConnection;53;3;58;0
WireConnection;174;0;170;3
WireConnection;174;1;164;0
WireConnection;44;0;36;0
WireConnection;56;0;53;0
WireConnection;172;0;174;0
WireConnection;172;1;173;0
WireConnection;45;0;44;1
WireConnection;45;1;46;0
WireConnection;54;0;55;0
WireConnection;54;1;49;5
WireConnection;54;2;56;0
WireConnection;171;0;11;2
WireConnection;171;1;174;0
WireConnection;171;2;172;0
WireConnection;43;0;45;0
WireConnection;43;1;44;2
WireConnection;43;2;44;3
WireConnection;22;0;54;0
WireConnection;22;1;23;0
WireConnection;175;0;171;0
WireConnection;50;0;22;0
WireConnection;20;0;43;0
WireConnection;48;0;26;0
WireConnection;48;1;47;0
WireConnection;48;2;40;0
WireConnection;184;0;21;0
WireConnection;184;1;19;0
WireConnection;184;2;51;0
WireConnection;184;3;25;0
WireConnection;184;4;48;0
WireConnection;184;9;176;0
ASEEND*/
//CHKSM=50A2C9D593B7EF1F877D0451DFF51C65F0D4947F