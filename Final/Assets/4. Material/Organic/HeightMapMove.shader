Shader "Custom/AnimatedHeightNormalHeartbeatColor"
{
    Properties
    {
        _Color ("Main Color (Tint)", Color) = (1, 1, 1, 1) // 색감 변경을 위한 Color 속성 추가
        _MainTex ("Main Texture", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "gray" {}
        _NoiseMap ("Noise Map (Scrolling)", 2D) = "gray" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}

        _HeightAmount ("Height Amount", Range(0, 5)) = 1.0
        _NoiseSpeed ("Noise Speed (X, Y)", Vector) = (0.1, 0.1, 0, 0)
        _NoiseStrength ("Noise Strength", Range(0, 2)) = 0.5
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0

        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 0.5
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD3;
                float3 worldTangent : TEXCOORD4;
                float3 worldBinormal : TEXCOORD5;
            };

            fixed4 _Color; // 선언된 Color 변수
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _HeightMap;
            sampler2D _NoiseMap;
            sampler2D _NormalMap;

            float _HeightAmount;
            float2 _NoiseSpeed;
            float _NoiseStrength;
            float _NormalStrength;

            float _PulseSpeed;
            float _PulseAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // 1. 심장 박동 맥동 계산 (Sin 파동)
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float currentPulseAmount = 1.0 + (pulse * _PulseAmount);

                // 2. 시간에 따른 노이즈 UV 좌표 이동 계산
                float2 noiseUV = o.uv + (_Time.y * _NoiseSpeed);

                // 3. Vertex Shader에서 텍스처 샘플링 (tex2Dlod)
                float height = tex2Dlod(_HeightMap, float4(o.uv, 0, 0)).r;
                float noise = tex2Dlod(_NoiseMap, float4(noiseUV, 0, 0)).r;

                // 4. 노이즈 결합
                float animatedHeight = height + ((noise - 0.5) * _NoiseStrength);

                // 5. 심장 박동 펄스 적용
                animatedHeight *= currentPulseAmount;

                // 6. 버텍스 위치 이동 (Normal 방향)
                v.vertex.xyz += v.normal * (animatedHeight * _HeightAmount);

                // 7. TBN 행렬 계산용 벡터 준비
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBinormal = cross(o.worldNormal, o.worldTangent) * v.tangent.w;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 노말맵 샘플링 및 탄젠트 공간 노말 추출
                float3 tangentNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
                tangentNormal.xy *= _NormalStrength;
                tangentNormal = normalize(tangentNormal);

                // 2. TBN 행렬 구성
                float3x3 TBN = float3x3(i.worldTangent, i.worldBinormal, i.worldNormal);

                // 3. 월드 공간 노말 계산
                float3 worldNormal = normalize(mul(tangentNormal, TBN));

                // 4. 기본 조명 계산 (Lambert)
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float ndotl = max(0, dot(worldNormal, lightDir));
                fixed3 diff = ndotl * _LightColor0.rgb;

                // 5. 메인 텍스처 샘플링 및 _Color(Tint) 곱셈 연산
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // 6. 라이팅 연산 적용
                col.rgb *= diff;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}