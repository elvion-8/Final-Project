// Made with Amplify Shader Editor v1.9.7.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Vefects/SH_Vefects_BIRP_Galaxy"
{
	Properties
	{
		[Space(33)][Header(Galaxy Background)][Space(13)]_GalaxyTexture("Galaxy Texture", CUBE) = "white" {}
		_GalaxyEmissionIntensity("Galaxy Emission Intensity", Float) = 1
		_GalaxyBaseColorIntensity("Galaxy Base Color Intensity", Range( 0 , 1)) = 0
		_GalaxyTint("Galaxy Tint", Color) = (1,1,1,0)
		_GalaxyHueShift("Galaxy Hue Shift", Float) = 0
		_GalaxySaturationShift("Galaxy Saturation Shift", Float) = 0
		[Space(33)][Header(Stars)][Space(13)]_StarsTexture("Stars Texture", 2D) = "white" {}
		_StarsTile("Stars Tile", Vector) = (1,1,0,0)
		_StarsTileOverall("Stars Tile Overall", Float) = 1
		_StarsSpeed("Stars Speed", Vector) = (-0.05,-0.1,0,0)
		_StarsColor01("Stars Color 01", Color) = (0,0.9407032,1,0)
		_StarsColor02("Stars Color 02", Color) = (0.7019608,0,1,0)
		_StarsHueShift("Stars Hue Shift", Float) = 0
		_StarsEmissionIntensity("Stars Emission Intensity", Float) = 333
		_StarsDistortion("Stars Distortion", Float) = 0
		[Space(33)][Header(Stars Noise)][Space(13)]_StarsNoiseTexture("Stars Noise Texture", 2D) = "black" {}
		_StarsNoiseTile("Stars Noise Tile", Vector) = (1,1,0,0)
		_StarsNoiseTileOverall("Stars Noise Tile Overall", Float) = 1
		_StarsNoiseSpeed("Stars Noise Speed", Vector) = (0.1,-0.05,0,0)
		[Space(33)][Header(Fresnel)][Space(13)]_FresnelColor("Fresnel Color", Color) = (0,0.6859784,1,0)
		_FresnelHueShift("Fresnel Hue Shift", Float) = 0
		_FresnelEmissionIntensity("Fresnel Emission Intensity", Float) = 13
		_FresnelBias("Fresnel Bias", Float) = 0
		_FresnelScale("Fresnel Scale", Float) = 1
		_FresnelPower("Fresnel Power", Float) = 5
		[Toggle(_FRESNELINVERT_ON)] _FresnelInvert("Fresnel Invert", Float) = 0
		[Space(33)][Header(Other)][Space(13)]_Metallic("Metallic", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		[Space(33)][Header(Reflection)][Space(13)][Toggle(_USEREFLECTIONVECTOR_ON)] _UseReflectionVector("Use Reflection Vector", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _USEREFLECTIONVECTOR_ON
		#pragma shader_feature_local _FRESNELINVERT_ON
		#define ASE_VERSION 19701
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldPos;
			float3 worldRefl;
			INTERNAL_DATA
			float3 worldNormal;
			float2 uv_texcoord;
		};

		uniform samplerCUBE _GalaxyTexture;
		uniform float _GalaxyHueShift;
		uniform float _GalaxySaturationShift;
		uniform float4 _GalaxyTint;
		uniform float _GalaxyEmissionIntensity;
		uniform float _GalaxyBaseColorIntensity;
		uniform float4 _FresnelColor;
		uniform float _FresnelBias;
		uniform float _FresnelScale;
		uniform float _FresnelPower;
		uniform float _FresnelHueShift;
		uniform float _FresnelEmissionIntensity;
		uniform float4 _StarsColor01;
		uniform float4 _StarsColor02;
		uniform sampler2D _StarsTexture;
		uniform float2 _StarsSpeed;
		uniform float4 _StarsTexture_ST;
		uniform float2 _StarsTile;
		uniform float _StarsTileOverall;
		uniform float _StarsDistortion;
		uniform sampler2D _StarsNoiseTexture;
		uniform float2 _StarsNoiseSpeed;
		uniform float4 _StarsNoiseTexture_ST;
		uniform float2 _StarsNoiseTile;
		uniform float _StarsNoiseTileOverall;
		uniform float _StarsHueShift;
		uniform float _StarsEmissionIntensity;
		uniform float _Metallic;
		uniform float _Smoothness;


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

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_worldPos );
			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
			float3 ase_worldReflection = i.worldRefl;
			#ifdef _USEREFLECTIONVECTOR_ON
				float3 staticSwitch87 = ase_worldReflection;
			#else
				float3 staticSwitch87 = ase_viewDirWS;
			#endif
			float3 hsvTorgb4_g1 = RGBToHSV( float4( texCUBE( _GalaxyTexture, staticSwitch87 ).rgb , 0.0 ).rgb );
			float3 hsvTorgb8_g1 = HSVToRGB( float3(( hsvTorgb4_g1.x + _GalaxyHueShift ),( hsvTorgb4_g1.y + _GalaxySaturationShift ),( hsvTorgb4_g1.z + 0.0 )) );
			float3 temp_output_4_0 = ( ( saturate( hsvTorgb8_g1 ) * _GalaxyTint.rgb ) * _GalaxyEmissionIntensity );
			o.Albedo = ( temp_output_4_0 * _GalaxyBaseColorIntensity );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV8 = dot( ase_worldNormal, ase_viewDirWS );
			float fresnelNode8 = ( _FresnelBias + _FresnelScale * pow( max( 1.0 - fresnelNdotV8 , 0.0001 ), _FresnelPower ) );
			float temp_output_64_0 = saturate( fresnelNode8 );
			#ifdef _FRESNELINVERT_ON
				float staticSwitch62 = ( 1.0 - temp_output_64_0 );
			#else
				float staticSwitch62 = temp_output_64_0;
			#endif
			float3 hsvTorgb4_g2 = RGBToHSV( ( _FresnelColor * saturate( staticSwitch62 ) ).rgb );
			float3 hsvTorgb8_g2 = HSVToRGB( float3(( hsvTorgb4_g2.x + _FresnelHueShift ),( hsvTorgb4_g2.y + 0.0 ),( hsvTorgb4_g2.z + 0.0 )) );
			float2 uv_StarsTexture = i.uv_texcoord * _StarsTexture_ST.xy + _StarsTexture_ST.zw;
			float2 panner65 = ( 1.0 * _Time.y * _StarsSpeed + ( ( uv_StarsTexture * _StarsTile ) * _StarsTileOverall ));
			float2 uv_StarsNoiseTexture = i.uv_texcoord * _StarsNoiseTexture_ST.xy + _StarsNoiseTexture_ST.zw;
			float2 panner68 = ( 1.0 * _Time.y * _StarsNoiseSpeed + ( ( uv_StarsNoiseTexture * _StarsNoiseTile ) * _StarsNoiseTileOverall ));
			float4 tex2DNode32 = tex2D( _StarsNoiseTexture, panner68 );
			float4 tex2DNode28 = tex2D( _StarsTexture, ( panner65 + ( _StarsDistortion * tex2DNode32.g ) ) );
			float4 lerpResult26 = lerp( _StarsColor01 , _StarsColor02 , saturate( ( ( tex2DNode28.g * tex2DNode32.g ) * 30.0 ) ));
			float4 lerpResult37 = lerp( float4( 0,0,0,0 ) , lerpResult26 , tex2DNode28.g);
			float3 hsvTorgb4_g3 = RGBToHSV( lerpResult37.rgb );
			float3 hsvTorgb8_g3 = HSVToRGB( float3(( hsvTorgb4_g3.x + _StarsHueShift ),( hsvTorgb4_g3.y + 0.0 ),( hsvTorgb4_g3.z + 0.0 )) );
			o.Emission = ( ( saturate( hsvTorgb8_g2 ) * _FresnelEmissionIntensity ) + ( temp_output_4_0 + ( saturate( hsvTorgb8_g3 ) * _StarsEmissionIntensity ) ) );
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
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
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
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
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.worldRefl = -worldViewDir;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
Node;AmplifyShaderEditor.TextureCoordinatesNode;34;-3968,1408;Inherit;False;0;32;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;70;-3584,1536;Inherit;False;Property;_StarsNoiseTile;Stars Noise Tile;16;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;-3584,1408;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-3328,1536;Inherit;False;Property;_StarsNoiseTileOverall;Stars Noise Tile Overall;17;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;67;-2944,1536;Inherit;False;Property;_StarsNoiseSpeed;Stars Noise Speed;18;0;Create;True;0;0;0;False;0;False;0.1,-0.05;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-3328,1408;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-3968,768;Inherit;False;0;28;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;75;-3584,896;Inherit;False;Property;_StarsTile;Stars Tile;7;0;Create;True;0;0;0;False;0;False;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.PannerNode;68;-2944,1408;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-3584,768;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;76;-3328,896;Inherit;False;Property;_StarsTileOverall;Stars Tile Overall;8;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-2432,1024;Float;False;Property;_StarsDistortion;Stars Distortion;14;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;66;-2432,896;Inherit;False;Property;_StarsSpeed;Stars Speed;9;0;Create;True;0;0;0;False;0;False;-0.05,-0.1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-3328,768;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;32;-2432,1408;Inherit;True;Property;_StarsNoiseTexture;Stars Noise Texture;15;0;Create;True;0;0;0;False;3;Space(33);Header(Stars Noise);Space(13);False;-1;c0e12b60cf1bfeb4a9e85b9f780abaa2;c0e12b60cf1bfeb4a9e85b9f780abaa2;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-2176,1024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;65;-2176,768;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;41;-1792,768;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-2560,-256;Float;False;Property;_FresnelBias;Fresnel Bias;22;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-2560,-192;Float;False;Property;_FresnelScale;Fresnel Scale;23;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-2560,-128;Float;False;Property;_FresnelPower;Fresnel Power;24;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;28;-1536,768;Inherit;True;Property;_StarsTexture;Stars Texture;6;0;Create;True;0;0;0;False;3;Space(33);Header(Stars);Space(13);False;-1;644f27aae8365bc4eac6e7c2899c78fb;644f27aae8365bc4eac6e7c2899c78fb;True;0;False;white;LockedToTexture2D;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.FresnelNode;8;-2304,-256;Inherit;True;Standard;WorldNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1280,1408;Float;False;Constant;_Gain;Gain;14;0;Create;True;0;0;0;False;0;False;30;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;31;-1408,1280;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;64;-1920,-256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-1280,1280;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;7;-2560,128;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldReflectionVector;86;-2560,384;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.OneMinusNode;63;-1664,-128;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;40;-1152,1280;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;30;-1792,1184;Float;False;Property;_StarsColor02;Stars Color 02;11;0;Create;True;0;0;0;False;0;False;0.7019608,0,1,0;0.7015796,0,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;27;-1792,1024;Float;False;Property;_StarsColor01;Stars Color 01;10;0;Create;True;0;0;0;False;0;False;0,0.9407032,1,0;0,0.9407032,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.StaticSwitch;87;-2304,128;Inherit;False;Property;_UseReflectionVector;Use Reflection Vector;28;0;Create;True;0;0;0;False;3;Space(33);Header(Reflection);Space(13);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-1536,512;Inherit;False;Property;_GalaxySaturationShift;Galaxy Saturation Shift;5;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-1536,384;Inherit;False;Property;_GalaxyHueShift;Galaxy Hue Shift;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;26;-1280,1024;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-1920,128;Inherit;True;Property;_GalaxyTexture;Galaxy Texture;0;0;Create;True;0;0;0;False;3;Space(33);Header(Galaxy Background);Space(13);False;-1;b9f9da1e400de224b8fcd0c5ad4a46db;b9f9da1e400de224b8fcd0c5ad4a46db;True;0;False;white;LockedToCube;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.StaticSwitch;62;-1664,-256;Inherit;False;Property;_FresnelInvert;Fresnel Invert;25;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;80;-1152,384;Inherit;False;Property;_GalaxyTint;Galaxy Tint;3;0;Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;17;-1408,-256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;-1664,-512;Float;False;Property;_FresnelColor;Fresnel Color;19;0;Create;True;0;0;0;False;3;Space(33);Header(Fresnel);Space(13);False;0,0.6859784,1,0;0,0.6859784,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.FunctionNode;77;-1536,128;Inherit;True;HueShift;-1;;1;9f07e9ddd8ab81c47b3582f22189b65b;0;4;14;COLOR;0,0,0,0;False;15;FLOAT;0;False;16;FLOAT;0;False;17;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;37;-1152,768;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-1024,1024;Inherit;False;Property;_StarsHueShift;Stars Hue Shift;12;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-768,256;Float;False;Property;_GalaxyEmissionIntensity;Galaxy Emission Intensity;1;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;81;-1152,128;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-1280,-256;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;82;-1024,0;Inherit;False;Property;_FresnelHueShift;Fresnel Hue Shift;20;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;-640,896;Float;False;Property;_StarsEmissionIntensity;Stars Emission Intensity;13;0;Create;True;0;0;0;False;0;False;333;333;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;85;-1024,768;Inherit;True;HueShift;-1;;3;9f07e9ddd8ab81c47b3582f22189b65b;0;4;14;COLOR;0,0,0,0;False;15;FLOAT;0;False;16;FLOAT;0;False;17;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-768,128;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-640,-128;Float;False;Property;_FresnelEmissionIntensity;Fresnel Emission Intensity;21;0;Create;True;0;0;0;False;0;False;13;13;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;83;-1024,-256;Inherit;True;HueShift;-1;;2;9f07e9ddd8ab81c47b3582f22189b65b;0;4;14;COLOR;0,0,0,0;False;15;FLOAT;0;False;16;FLOAT;0;False;17;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-640,768;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-640,-256;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-512,256;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-256,-128;Inherit;False;Property;_GalaxyBaseColorIntensity;Galaxy Base Color Intensity;2;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-384,0;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;45;-256,208;Float;False;Property;_Smoothness;Smoothness;27;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-256,-256;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;44;-256,128;Float;False;Property;_Metallic;Metallic;26;0;Create;True;0;0;0;False;3;Space(33);Header(Other);Space(13);False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;89;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Vefects/SH_Vefects_BIRP_Galaxy;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;46;384,-256;Inherit;False;100;100;;0;Enjoy the shader and have fun luv u all <3;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;47;384,-512;Inherit;False;510.7999;131.2;Ge Lush was here! :D;0;Ge Lush was here! :D;0,0,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;48;384,0;Inherit;False;100;100;;0;Reach out to info@vefects.com via email;0,0,0,1;0;0
WireConnection;69;0;34;0
WireConnection;69;1;70;0
WireConnection;71;0;69;0
WireConnection;71;1;72;0
WireConnection;68;0;71;0
WireConnection;68;2;67;0
WireConnection;73;0;29;0
WireConnection;73;1;75;0
WireConnection;74;0;73;0
WireConnection;74;1;76;0
WireConnection;32;1;68;0
WireConnection;43;0;42;0
WireConnection;43;1;32;2
WireConnection;65;0;74;0
WireConnection;65;2;66;0
WireConnection;41;0;65;0
WireConnection;41;1;43;0
WireConnection;28;1;41;0
WireConnection;8;1;12;0
WireConnection;8;2;13;0
WireConnection;8;3;14;0
WireConnection;31;0;28;2
WireConnection;31;1;32;2
WireConnection;64;0;8;0
WireConnection;38;0;31;0
WireConnection;38;1;39;0
WireConnection;63;0;64;0
WireConnection;40;0;38;0
WireConnection;87;1;7;0
WireConnection;87;0;86;0
WireConnection;26;0;27;0
WireConnection;26;1;30;0
WireConnection;26;2;40;0
WireConnection;2;1;87;0
WireConnection;62;1;64;0
WireConnection;62;0;63;0
WireConnection;17;0;62;0
WireConnection;77;14;2;5
WireConnection;77;15;78;0
WireConnection;77;16;79;0
WireConnection;37;1;26;0
WireConnection;37;2;28;2
WireConnection;81;0;77;0
WireConnection;81;1;80;5
WireConnection;16;0;15;0
WireConnection;16;1;17;0
WireConnection;85;14;37;0
WireConnection;85;15;84;0
WireConnection;4;0;81;0
WireConnection;4;1;5;0
WireConnection;83;14;16;0
WireConnection;83;15;82;0
WireConnection;24;0;85;0
WireConnection;24;1;25;0
WireConnection;10;0;83;0
WireConnection;10;1;11;0
WireConnection;23;0;4;0
WireConnection;23;1;24;0
WireConnection;9;0;10;0
WireConnection;9;1;23;0
WireConnection;49;0;4;0
WireConnection;49;1;50;0
WireConnection;89;0;49;0
WireConnection;89;2;9;0
WireConnection;89;3;44;0
WireConnection;89;4;45;0
ASEEND*/
//CHKSM=56299FE83EDA9549EF764932CA74391A006F4033