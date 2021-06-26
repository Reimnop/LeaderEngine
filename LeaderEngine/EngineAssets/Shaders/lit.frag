#version 450 core

#extension GL_ARB_bindless_texture : enable

layout (location = 0) out vec4 fragColor;

in vec3 Color;
in vec2 TexCoord;

in vec3 Normal;
in vec3 FragPos;

layout (std140, binding = 0) uniform Material 
{
	vec3 color;
	bool hasDiffuse;
	layout (bindless_sampler) sampler2D diffuse;
};

uniform vec3 camPos;

//light uniforms
uniform vec3 lightDir;
uniform float lightIntensity = 1.0;

//shadow mapping
const int MAX_CASCADE = 4;
uniform float bBias = 0.0002;
uniform int cascadeCount;
uniform float cascadeDepths[MAX_CASCADE + 1];
uniform mat4 cascadeViewProjs[MAX_CASCADE];
uniform sampler2D cascadeShadowMaps[MAX_CASCADE];

uniform float ambient = 0.05;

float calculateShadow(vec4 fragPosLightSpace, sampler2D cascadeMap) {
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w * 0.5 + 0.5;

	if(projCoords.z > 1.0)
        return 1.0;

	vec3 norm = normalize(Normal);

	float bias = max(bBias * (1.0 - dot(norm, lightDir)), bBias);

	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(cascadeMap, 0);
	for(int x = -1; x <= 1; ++x)
	{
		for(int y = -1; y <= 1; ++y)
		{
			float pcfDepth = texture(cascadeMap, projCoords.xy + vec2(x, y) * texelSize).r; 
			shadow += projCoords.z - bias > pcfDepth ? 0.0 : 1.0;        
		}    
	}
	shadow /= 9.0;

    return shadow;
}

vec4 calculateFragPosLightSpace(vec3 fragPos, mat4 lightViewProj) {
	return vec4(fragPos, 1) * lightViewProj;
}

void main() {
	vec3 obColor = color;
	if (hasDiffuse)
		obColor *= texture(diffuse, TexCoord).rgb;

	float depth = gl_FragCoord.z;

	depth = cascadeDepths[0] + (1 - depth) * (cascadeDepths[cascadeCount] - cascadeDepths[0]);

	int cascadeIndex = 0;
	for (int i = 0; i < cascadeCount; i++) {
		if (depth > cascadeDepths[i] && depth <= cascadeDepths[i + 1]) {
			cascadeIndex = cascadeCount - i - 1;
			break;
		}
	}

	vec3 norm = normalize(Normal);

	float diffuseIntensity = max(dot(norm, lightDir), 0.0) * lightIntensity;

	vec4 fragPosLightSpace = calculateFragPosLightSpace(FragPos, cascadeViewProjs[cascadeIndex]);
	float shadow = calculateShadow(fragPosLightSpace, cascadeShadowMaps[cascadeIndex]);

	float calculatedAmbient = max(dot(norm, normalize(camPos - FragPos)), 0.25) * ambient;

	vec3 outColor = (calculatedAmbient + diffuseIntensity * shadow) * obColor;

	fragColor = vec4(outColor, 1.0);
}