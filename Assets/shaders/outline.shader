Shader "Custom/URP_Outline_Normalized"
{
    Properties
    {
        [Header(Base Texture)]
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.5)) = 0.05
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        // --- پاس اول: رسم اوت‌لاین (پشت مدل) ---
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="UniversalForward" }
            
            // برای اینکه جلوی مدل رندر نشود و فقط لبه‌های بیرونی دیده شوند
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
            };

            float4 _OutlineColor;
            float _OutlineWidth;

            Varyings vert(Attributes input)
            {
                Varyings output;

                // تبدیل موقعیت و نرمال به فضای جهان (World Space)
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                // محاسبه مقیاس شیء (Object Scale) از روی ماتریس World برای خنثی کردن تأثیر Scale
                // این بخش باعث می‌شود اندازه اوت‌لاین در مدل‌های با ابعاد و اسکیل متفاوت یکسان بماند
                float3 scale;
                scale.x = length(float3(GetObjectToWorldMatrix()[0].xyz));
                scale.y = length(float3(GetObjectToWorldMatrix()[1].xyz));
                scale.z = length(float3(GetObjectToWorldMatrix()[2].xyz));
                float averageScale = (scale.x + scale.y + scale.z) / 3.0;

                // نرمالایز کردن نرمال در فضای جهان
                float3 worldNormal = normalize(normalInput.normalWS);

                // اصلاح ضخامت بر اساس مقیاس شیء تا در آبجکت‌های بزرگتر یا کوچکتر، اوت‌لاین دفرمه نشود
                float adjustedWidth = _OutlineWidth / (averageScale + 0.0001);

                // انتقال راس‌ها در راستای نرمال‌ها (به صورت محلی یا جهان)
                float3 displacedPosition = input.positionOS.xyz + (input.normalOS * adjustedWidth);
                
                // تبدیل موقعیت جدید به فضای کلیپ (Clip Space)
                output.positionCS = TransformObjectToHClip(displacedPosition);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // --- پاس دوم: رندر عادی مدل ---
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            float4 _BaseColor;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = _MainTex.Sample(sampler_MainTex, input.uv);
                return texColor * _BaseColor;
            }
            ENDHLSL
        }
    }
}
