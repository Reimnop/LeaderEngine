#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D blurredSSAO;
uniform sampler2D gAlbedoSpec;
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D depthMap;

uniform sampler2D shadowMap;

uniform vec3 lightDir;

uniform mat4 modelLS;
uniform mat4 lightSpaceMatrix;

in vec2 TexCoord;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w * 0.5 + 0.5;

	if(projCoords.z > 1.0)
        return 1.0;

    return projCoords.z - 0.0005 > texture(shadowMap, projCoords.xy).r ? 0.0 : 1.0;
}  

void main() 
{
    vec3 Albedo = texture(gAlbedoSpec, TexCoord).rgb;
    vec3 FragPos = texture(gPosition, TexCoord).rgb;
    vec3 Normal = texture(gNormal, TexCoord).rgb;

    vec4 FragPosLightSpace = vec4(FragPos, 1.0) * lightSpaceMatrix;

    float shadow = ShadowCalculation(FragPosLightSpace);

    vec3 result = (0.5 + shadow * max(dot(Normal, lightDir), 0.0)) * Albedo;

	fragColor = vec4(result, 1.0);
}