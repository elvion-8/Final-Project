Shader "Custom/Particles/AdditiveAlways" {
    Properties {
        _MainTex ("Particle Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
    }
    SubShader {
        Tags { 
            "Queue"="Transparent+1000" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane" 
        }
        Blend SrcAlpha One
        Cull Off 
        Lighting Off 
        ZWrite Off
        ZTest LEqual // 기본 깊이 테스트 수행
        Offset -5, -5 // 렌더링 판정을 카메라 쪽으로 당겨 몹보다 앞으로 띄웁니다.
        
        BindChannels {
            Bind "Color", color
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
        }
        Pass {
            SetTexture [_MainTex] {
                combine texture * primary
            }
            SetTexture [_MainTex] {
                constantColor [_TintColor]
                combine previous * constant double, previous * constant
            }
        }
    }
}
