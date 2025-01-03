Shader "Depth/Custom Liquid Occlude"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainColor] _EndColor("End Color", Color) = (1, 1, 1, 0)
        _Fill("Fill", Range(0.0, 1.0)) = 0.5
        _Min("Minimum", Float) = 0.0
        _Max("Maximum", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" }

        // 0. It's important to have One OneMinusSrcAlpha so it blends properly against transparent background (passthrough)
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            // 1. Keywords are used to enable different occlusions
            #pragma multi_compile _ HARD_OCCLUSION SOFT_OCCLUSION

            // if your shaders are in a BiRP project, you would add this include instead:
            #include "Packages/com.meta.xr.sdk.core/Shaders/EnvironmentDepth/BiRP/EnvironmentOcclusionBiRP.cginc"

            struct Attributes
            {
                float4 vertex : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 positionOS : TEXCOORD1;

                // 3. This macro adds 'posWorld' to the varyings struct.
                //    The subsequent macros require this field to be named as such.
                //    The number has to be filled with the recent TEXCOORD number + 1
                //    Or 0 as in this case, if there are no other TEXCOORD fields
                META_DEPTH_VERTEX_OUTPUT(0)

                UNITY_VERTEX_INPUT_INSTANCE_ID
                // 4. The fragment shader needs to understand to which eye it's currently
                //    rendering, in order to get depth from the correct texture.
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EndColor;
                half _Fill;
                half _Min;
                half _Max;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = UnityObjectToClipPos(input.vertex.xyz); // Note: input.vertex.xyz is object space position
                output.positionOS = input.vertex;

                // 5. World position is required to calculate the occlusions.
                //    This macro will calculate and set world position value in the output Varyings structure.
                META_DEPTH_INITIALIZE_VERTEX_OUTPUT(output, input.vertex);

                // 6. Passes stereo information to frag shader
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                // 7. Initializes global stereo constant for the frag shader
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 finalColor = _BaseColor;
                float3 objectPos = input.positionOS.xyz;
                float mappedFill = _Min + _Fill * (_Max - _Min);

                float alpha = step(objectPos.z, mappedFill);
                finalColor = lerp(_BaseColor, _EndColor, alpha);
                // finalColor.a = alpha * _BaseColor.a;

                // 8. A third macro required to enable occlusions.
                //    It requires previous macros to be there as well as the naming behind the macro is strict.
                //    It will enable soft or hard occlusions depending on the current keyword set.
                //    finalColor value will be multiplied by the occlusion visibility value.
                //    Occlusion visibility value is 0 if virtual object is completely covered by environment and vice versa.
                //    Fully occluded pixels will be discarded
                META_DEPTH_OCCLUDE_OUTPUT_PREMULTIPLY(input, finalColor, 0);

                return finalColor;
            }
            ENDHLSL
        }
    }
}
