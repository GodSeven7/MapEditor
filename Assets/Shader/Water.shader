Shader "Custom/Water" {
	Properties{  
        _MainTint("Diffuse Tint", Color) = (1,1,1,1)  
        _MainTex("Base (RGB)", 2D) = "white" {}  
        _ScrollXspeed("xspeed",Range(0,10)) = 2  
        _ScrollYspeed("yspeed",Range(0,10)) = 2  
    }  
        SubShader{  
            Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
            LOD 200  
  
            CGPROGRAM  
            // Physically based Standard lighting model, and enable shadows on all light types  
            #pragma surface surf Standard fullforwardshadows alpha  
  
            // Use shader model 3.0 target, to get nicer looking lighting  
            #pragma target 3.0  
  
            sampler2D _MainTex;  
  
            struct Input {  
                float2 uv_MainTex;  
            };  
            fixed4 _MainTint;  
            fixed _ScrollXspeed;  
            fixed _ScrollYspeed;  
  
            void surf(Input IN, inout SurfaceOutputStandard o) {  
                fixed2 scrollUV = IN.uv_MainTex;  
                fixed xSrollValue = _ScrollXspeed*_Time;  
                fixed yScollValue = _ScrollYspeed*_Time;  
                scrollUV += fixed2(xSrollValue, yScollValue);  
  
                half4 c = tex2D(_MainTex, scrollUV);  
  
                half _Glossiness;  
                half _Metallic;  
                o.Albedo = c.rgb* _MainTint;  
				o.Alpha = _MainTint.a;
            }  
            ENDCG  
        }  
            FallBack "Diffuse"  
}