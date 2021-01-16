#version 330 core

layout (location = 0) out vec4 fragColor;
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
	mat4 proj = transpose(projection);

    float ndcZ = 2.0 * wsZ - 1.0;
    // linearize depth for proper depth weighting
    //See: https://stackoverflow.com/questions/7777913/how-to-render-depth-linearly-in-modern-opengl-with-gl-fragcoord-z-in-fragment-sh
    //or: https://stackoverflow.com/questions/11277501/how-to-recover-view-space-position-given-view-space-depth-value-and-ndc-xy
    float linearZ = (proj[2][2] + 1.0) * wsZ / (proj[2][2] + ndcZ);
    float tmp = (linearZ * 0.99) * alpha * 10.0;
    float w = clamp(pow(tmp, 10.0), 0.2, 512.0);
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

	vec3 outColor = (ambient + shadow * max(dot(norm, -lightDir), 0.0) * lightColor * intensity) * objectColor.rgb;
	
	writePixel(outColor, objectColor.a, Depth);
}
