Shader "Unlit/ChameleonShader"{
    Properties{
        _Color("Color", Color) = (1, 1, 1, 0)
    }


    SubShader{
        Tags { "RenderType"="Opaque" }

        Pass{
            CGPROGRAM
            #pragma vertex vert  // вершинному шейдеру соостветсвует функция vert
            #pragma fragment frag   // фрагментному шейдеру соответсвует  фрагментный

            #include "UnityCG.cginc"

            float4 _Color;


            struct MeshData{ // automatically filled by Unity
                float4 vertex : POSITION;  // vertex position
                //float3 normals: Normal;
                //float3 color: Color;
                float2 uv0 : TEXCOORD0;   // diffuse, normal map, textures
                //float2 uv1 : TEXCOORD1;  // uv-coordinates, lightmap-coordinates
            };

            struct Interpolators{   // vertex shader -> fragment shader
                float2 uv0 : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };



            Interpolators vert (MeshData v){  // вершинный шейдер
                Interpolators o;
                o.uv0 = v.uv0;
                o.vertex = UnityObjectToClipPos( v.vertex );
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target{  // фрагментный шейдер
                //return float4(sin(i.uv0.x + _Time.x), cos(i.uv0.y), 1, 1);
                return float4(sin(_Color.x + _Time.x * 4.0), cos(_Color.y + _Time.y), sin(_Color.x + _Color.y + _Time.z), 1);
            }
            ENDCG
        }
    }
}
