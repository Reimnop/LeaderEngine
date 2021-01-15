#version 400 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D blurredSSAO;
uniform sampler2D gAlbedoSpec;
uniform sampler2D gPosition;
uniform sampler2D gNormal;
uniform sampler2D alpha;
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

    vec3 resultLight = (ambientColor * texture(blurredSSAO, TexCoord).r + shadow * max(dot(Normal, -lightDir), 0.0) * lightColor * intensity) * Albedo;

    //alpha
    vec4 alphaTex = texture(alpha, TexCoord);

    vec4 outColor = vec4(resultLight, alphaTex.r);
    float depth = texture(depthTexture, TexCoord).r;

    float weight = 
        max(min(1.0, max(max(outColor.r, outColor.g), outColor.b) * outColor.a), outColor.a) *
        clamp(0.03 / (1e-5 + pow(depth / 200.0, 4.0)), 1e-2, 3e3);

	vec4 accum = vec4(outColor.rgb * outColor.a, outColor.a) * weight;

	float revealage;
    if (alphaTex.b >= 0.5)
        revealage = alphaTex.a * weight;
    else
        revealage = 1.0;

    vec4 result = vec4(accum.rgb / max(accum.a, 1e-5), revealage);

	fragColor = result;

    gl_FragDepth = texture(depthTexture, TexCoord).r;
}