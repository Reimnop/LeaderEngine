#version 400 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D blurredSSAO;
uniform sampler2D gAlbedoSpec;
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D depthTexture;

uniform sampler2D shadowMap;

uniform vec3 lightDir;

uniform mat4 lightSpaceMatrix;

uniform float intensity = 1.0;
uniform vec3 lightColor = vec3(1.0);

uniform vec3 ambientColor = vec3(0.5);

in vec2 TexCoord;

float ShadowCalculation(vec4 fragPosLightSpace, vec3 normal)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w * 0.5 + 0.5;

	if(projCoords.z > 1.0)
        return 1.0;

    float bias = max(0.0005 * (1.0 - dot(normal, lightDir)), 0.0005);
    
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
            shadow += projCoords.z - bias > pcfDepth ? 0.0 : 1.0;        
        }    
    }
    shadow /= 9.0;

    return shadow;
}  

void main() 
{
    vec3 Albedo = texture(gAlbedoSpec, TexCoord).rgb;
    vec3 FragPos = texture(gPosition, TexCoord).rgb;
    vec3 Normal = texture(gNormal, TexCoord).rgb;

    vec4 FragPosLightSpace = vec4(FragPos, 1.0) * lightSpaceMatrix;

    float shadow = ShadowCalculation(FragPosLightSpace, Normal);

    vec3 result = (ambientColor * texture(blurredSSAO, TexCoord).r + shadow * max(dot(Normal, -lightDir), 0.0) * lightColor * intensity) * Albedo;

	fragColor = vec4(result, 1.0);

    gl_FragDepth = texture(depthTexture, TexCoord).r;
}