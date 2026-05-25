Shader "URP/PerlinPortal"
{
    Properties
    {
        _CellSize   ("Cell Size",  Range(0, 1)) = 1
        _ScrollSpeed("Scroll Speed", Range(0, 1)) = 1
        _Color("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Cull Back
        ZWrite On
        ZTest LEqual

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex   vert
            #pragma fragment frag

            // URP Core (matrizes, transforms, _Time, etc.)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _CellSize;
            float _ScrollSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos   : TEXCOORD0;
                float2 uv         : TEXCOORD1;
            };            

            float4 _Color;

            // ---------- Utilidades (substitui Random.cginc) ----------
            // Hash 3D -> 3D determinístico, bom para gradientes/ruído
            float3 rand3dTo3d(float3 p)
            {
                // Variante do hash de Dave Hoskins
                p = frac(p * 0.1031);
                p += dot(p, p.yxz + 33.33);
                return frac((p.xxy + p.yxx) * p.zyx);
            }

            // Easing iguais aos do shader original
            float easeIn(float t)         { return t * t; }
            float easeOut(float t)        { return 1.0 - easeIn(1.0 - t); }
            float easeInOut(float t)
            {
                float a = easeIn(t);
                float b = easeOut(t);
                return lerp(a, b, t);
            }

            // Perlin-like 3D (gradiente em cada canto da célula)
            float perlinNoise(float3 value)
            {
                float3 f = frac(value);

                float ix = easeInOut(f.x);
                float iy = easeInOut(f.y);
                float iz = easeInOut(f.z);

                float cellNoiseZ[2];

                [unroll] for (int z = 0; z <= 1; z++)
                {
                    float cellNoiseY[2];

                    [unroll] for (int y = 0; y <= 1; y++)
                    {
                        float cellNoiseX[2];

                        [unroll] for (int x = 0; x <= 1; x++)
                        {
                            float3 cell = floor(value) + float3(x, y, z);
                            float3 g = rand3dTo3d(cell) * 2.0 - 1.0;   // direção do gradiente
                            float3 d = f - float3(x, y, z);            // vetor até o canto
                            cellNoiseX[x] = dot(g, d);
                        }

                        cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], ix);
                    }

                    cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], iy);
                }

                return lerp(cellNoiseZ[0], cellNoiseZ[1], iz);
            }

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.worldPos   = worldPos;
                o.positionCS = TransformWorldToHClip(worldPos);
                o.uv         = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                // Evita div/0 quando _CellSize ~ 0
                float cell = max(_CellSize, 1e-4);

                float3 value = i.worldPos / cell;
                value.y += _Time.y * _ScrollSpeed;

                // ~0..1
                float noise = perlinNoise(value) + 0.5;

                // Linhas animadas
                noise = frac(noise * 6.0);

                float px = fwidth(noise);
                float heightLine = smoothstep(1.0 - px, 1.0, noise);
                heightLine += smoothstep(px, 0.0, noise);

                return half4(heightLine, heightLine, heightLine, 1.0) * _Color;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
