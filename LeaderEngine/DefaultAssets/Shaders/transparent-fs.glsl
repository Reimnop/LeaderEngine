#version 330 core

layout (location = 1) out vec4 accumulation;
layout (location = 2) out float revealage;

uniform bool useTexture;

uniform vec4 color;
uniform vec3 lightDir;

uniform mat4 projection;

uniform sampler2D texture0;
uniform sampler2D shadowMap;

in vec3 VertCol;
in vec3 Normal;
in vec2 TexCoord;
in vec4 FragPosLightSpace;

in vec3 FragPos;

in float Depth;

uniform vec3 ambient = vec3(0.94);
uniform vec3 lightColor = vec3(1.0);

uniform float intensity = 1.2;

float ShadowCalculation(vec4 fragPosLightSpace)
{
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w * 0.5 + 0.5;

	if(projCoords.z > 1.0)
        return 1.0;

	vec3 norm = normalize(Normal);

	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
			float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r; 
			shadow += projCoords.z - 0.0005 > pcfDepth ? 0.0 : 1.0;        
		}    
	}
	shadow /= 9.0;

    return shadow;
}

void writePixel(vec3 color, float alpha, float wsZ) {
    float w = 
		max(min(1.0, max(max(color.r, color.g), color.b) * alpha), alpha) *
		clamp(0.03 / (1e-5 + pow(wsZ / 512.0, 4.0)), 1e-2, 3e3);

    accumulation = vec4(color * alpha * w, alpha);
    revealage = alpha * w;
}

void main() 
{
	vec4 objectColor = vec4(VertCol, 1.0) * color;

	if (useTexture)
		objectColor *= texture(texture0, TexCoord);

	vec3 norm = normalize(Normal);

	float shadow = ShadowCalculation(FragPosLightSpace);

	vec3 outColor = (ambient + shadow * max(dot(norm, lightDir), 0.0) * lightColor * intensity) * objectColor.rgb;
	
	writePixel(outColor, objectColor.a, Depth);
}
