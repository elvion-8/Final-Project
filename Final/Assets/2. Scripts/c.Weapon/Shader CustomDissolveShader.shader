Shader "Custom/DissolveShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        [Header(Dissolve)]
        _DissolveTex ("Dissolve Texture (Noise)", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0.0
        _EdgeColor ("Edge Color (Glow)", Color) = (0,1,1,1)
        _EdgeWidth ("Edge Width", Range(0,0.5)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        sampler2D _MainTex;
        sampler2D _DissolveTex;
        struct Input
        {
            float2 uv_MainTex;
            float2 uv_DissolveTex;
        };
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _DissolveAmount;
        fixed4 _EdgeColor;
        float _EdgeWidth;
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float dissolveVal = tex2D(_DissolveTex, IN.uv_DissolveTex).r;
            clip(dissolveVal - _DissolveAmount);
            if (dissolveVal - _DissolveAmount < _EdgeWidth && _DissolveAmount > 0.0)
            {
                o.Emission = _EdgeColor.rgb * 3.0; // Emission 강도를 높여 발광 연출
            }
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}