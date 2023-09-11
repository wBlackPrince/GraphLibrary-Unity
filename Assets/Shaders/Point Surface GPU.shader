Shader "Graph/Point Surface GPU"{
    Properties{
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }

    SubShader{

        CGPROGRAM

        // указание сгенерировать поверхностный шейдер
        // со стандартным шейдером, полной поддержкой теней
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        // избежание фиктивного шейдера(отключение ассинхрннной компиляции шейдеров)
        #pragma editor_sync_compilation
        // качество шейдера
        #pragma target 4.5

        sampler2D _MainTex;


        // входная структура нашей конфигурации
        struct Input{
            float3 worldPos;
        };

        float _Smoothness;

        #include "PointGPU.hlsl"

        // второй параметр - данные конфигурации поверхности
        void ConfigureSurface (Input input,inout  SurfaceOutputStandard surface){
            // альбедо - показатель, того сколько света диффузно отражается от поверхности
            // если альбедо не полностью белое, часть световой энергии поглощается, а не отражается
            // saturate -  фиксация цветов в диапозоне 0-1
            surface.Albedo = saturate(input.worldPos * 0.5 + 0.5);
            surface.Smoothness = _Smoothness;
        }

        ENDCG

    }
    FallBack "Diffuse"
}
