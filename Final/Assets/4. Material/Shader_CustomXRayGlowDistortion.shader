Shader "Custom/XRayGlowDistortion_FrontOnly"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Color ("Glow Color", Color) = (0, 1, 0, 0.3)
        _DistortSpeed ("Distortion Speed", Float) = 3.0
        _DistortScale ("Distortion Scale", Float) = 8.0
        _DistortStrength ("Distortion Strength", Float) = 0.04
        _RimPower ("Rim Power (외곽선 두께)", Range(0.5, 8.0)) = 3.0
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Cull Back
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float3 viewDir : TEXTCOORD2;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _DistortSpeed;
            float _DistortScale;
            float _DistortStrength;
            float _RimPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _DistortSpeed;

                float distortX = sin(i.uv.y * _DistortScale + time) * _DistortStrength;
                float distortY = cos(i.uv.x * _DistortScale + time) * _DistortStrength;
                
                // 1. 텍스처 UV 왜곡
                float2 distortedUV = i.uv + float2(distortX, distortY);
                fixed4 texCol = tex2D(_MainTex, distortedUV);
                
                // 2. 외곽선(Rim)도 일렁이게 만들기 위해 노멀 벡터에 왜곡값 적용
                float3 normal = normalize(i.normal);
                normal.x += distortX * 2.0;
                normal.y += distortY * 2.0;
                normal = normalize(normal);

                float3 viewDir = normalize(i.viewDir);
                
                // 3. 일렁이는 노멀 기반으로 외곽선 계산
                float rim = 1.0 - saturate(dot(viewDir, normal));
                rim = pow(rim, _RimPower);

                // 4. 최종 색상 및 투명도 결합
                fixed4 finalColor = texCol * _Color;
                finalColor.rgb += rim * _Color.rgb; 
                finalColor.a = saturate(_Color.a + rim * 0.5);

                return finalColor;
            }
            ENDCG
        }
    }
}