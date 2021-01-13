#version 400 core

layout (location = 0) out float fragColor;

uniform sampler2D gAlbedoSpec;
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D depthMap;

in vec2 TexCoord;

//SSAO
uniform sampler2D texNoise;

const int kernelSize = 64;

uniform vec3 samples[kernelSize];
uniform mat4 projection;

uniform float power = 5.0;
uniform float radius = 0.5;
uniform float bias = 0.0005;

uniform vec2 vSize;
uniform vec2 nSize;

float calcOcclusion() {
    vec3 Albedo = texture(gAlbedoSpec, TexCoord).rgb;
    vec3 FragPos = texture(gPosition, TexCoord).rgb;
    vec3 Normal = texture(gNormal, TexCoord).rgb;
    
    vec2 noiseScale = vec2(vSize.x / nSize.x, vSize.y / nSize.y);

    vec3 randomVec = texture(texNoise, TexCoord * noiseScale).xyz;

    vec3 tangent   = normalize(randomVec - Normal * dot(randomVec, Normal));
    vec3 bitangent = cross(Normal, tangent);
    mat3 TBN       = mat3(tangent, bitangent, Normal);

    float occlusion = 0.0;
    for(int i = 0; i < kernelSize; i++)
    {
        vec3 samplePos = TBN * samples[i];
        samplePos = FragPos + samplePos * radius;
        
        vec4 offset = vec4(samplePos, 1.0);
        offset      = offset * projection;
        offset.xyz /= offset.w;
        offset.xyz  = offset.xyz * 0.5 + 0.5;

        float sampleDepth = texture(gPosition, offset.xy).z;
        float rangeCheck = smoothstep(0.0, 1.0, radius / abs(FragPos.z - sampleDepth));
        occlusion += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
    }

    occlusion = 1.0 - (occlusion / kernelSize);

    return occlusion;
}

void main() 
{
    float result;

    if (power > 0)
        result = pow(calcOcclusion(), power);
    else
        result = 1.0;

	fragColor = result;
}