Shader "Custom/VenomSymbioteShader"
{
    Properties
    {
        _MainTex ("Base Character Texture", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _NoiseTex ("Symbiote Crawl Noise", 2D) = "gray" {}
        _SymbioteColor ("Symbiote Goo Color", Color) = (0.02, 0.02, 0.03, 1.0)
        [HDR] _WetGlowColor ("Wet Specular Gloss Color", Color) = (0.7, 0.85, 1.0, 1.0)
        _CrawlSpeed ("Symbiote Crawl Speed", Range(0, 10)) = 3.0
        _VeinScale ("Vein Tendril Density", Range(1, 20)) = 8.0
        _DripPhase ("Continuous Phase (C#에서 공급)", Float) = 0.0
        _DripAmount ("3D Gravity Drip Extrude (아래로 길게 늘어남)", Range(0, 0.5)) = 0.2
        _DripDensity ("Drip Density", Range(1, 15)) = 5.0
        _Glossiness ("Wet Glossiness", Range(1, 32)) = 16.0
        _RimPower ("Rim Wet Highlight", Range(0.5, 8)) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _NoiseTex;
        fixed4 _SymbioteColor;
        fixed4 _WetGlowColor;
        float _CrawlSpeed;
        float _VeinScale;
        float _DripPhase;
        float _DripAmount;
        float _DripDensity;
        float _Glossiness;
        float _RimPower;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
        };

        void vert(inout appdata_full v)
        {
            float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

            // 1. 위에서 아래로 끊김 없이 연속 흘러내리는 월드 노이즈 위상 (_DripPhase)
            float dripFlow = worldPos.y * _DripDensity - _DripPhase;
            float noise1 = sin(worldPos.x * _DripDensity + dripFlow);
            float noise2 = cos(worldPos.z * _DripDensity + dripFlow * 0.7);
            float dripNoise = noise1 * noise2;

            // 2. 동글동글함 대신 아래로 길게 처지는 물방울 스트레치 (Sagging Teardrop Shape)
            float sagTaper = pow(saturate(dripNoise * 0.6 + 0.4), 2.5);

            // 3. 월드 실제 중력 방향(-Y)을 오브젝트 로컬 공간으로 변환
            float3 worldGravity = float3(0.0, -1.0, 0.0);
            float3 localGravity = mul((float3x3)unity_WorldToObject, worldGravity);

            // 4. 단순 외곽 부풀림(v.normal)을 최소화하고 아래쪽(-Y) 중력 늘어남을 극대화
            float3 sagDir = normalize(v.normal * 0.2 + localGravity * 1.5);

            // 5. 정점(Vertex)을 월드 중력 아래 방향으로 길게 늘어트림
            v.vertex.xyz += sagDir * (sagTaper * _DripAmount);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 1. 캐릭터 메인 텍스처
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

            // 2. 아래로 끊임없이 흘러내리는 심비오트 UV 스크롤
            float2 noiseUV = float2(IN.worldPos.x * 0.5, IN.worldPos.y * (_DripDensity * 0.1) - _DripPhase * 0.05);
            fixed4 noise = tex2D(_NoiseTex, noiseUV);

            // 3. 아래로 길게 늘어지는 심비오트 핏줄/점액 덩굴 패턴
            float veinPattern = sin(IN.worldPos.y * _VeinScale - _DripPhase * 1.2 + noise.r * 5.0);
            veinPattern = smoothstep(0.15, 0.85, abs(veinPattern));

            // 4. 베놈 칠흑 유체 베이스 + 캐릭터 메인 컬러 혼합
            fixed3 symbioteBody = lerp(_SymbioteColor.rgb, c.rgb * _SymbioteColor.rgb, 0.2);
            o.Albedo = lerp(symbioteBody, c.rgb, veinPattern * 0.3);

            // 5. 법선 맵 (노멀 맵)
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

            // 6. 베놈 특유의 젖은 고광택 (Wet High-Gloss Metallic & Smoothness)
            float wetMetallic = lerp(0.85, 0.2, veinPattern);
            float wetSmoothness = lerp(0.95, 0.6, veinPattern);
            o.Metallic = wetMetallic;
            o.Smoothness = wetSmoothness;

            // 7. 림 라이트 (외곽 젖은 반사 광택)
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            fixed3 rimColor = pow(rim, _RimPower) * _WetGlowColor.rgb * 1.5;
            o.Emission = rimColor + (1.0 - veinPattern) * _WetGlowColor.rgb * 0.2;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
