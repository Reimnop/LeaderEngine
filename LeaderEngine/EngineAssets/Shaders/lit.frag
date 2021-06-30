#version 450 core

#extension GL_ARB_bindless_texture : enable

layout (location = 0) out vec4 fragColor;
layout (location = 1) out vec3 gAlbedo;
layout (location = 2) out vec3 gPosition;
layout (location = 3) out vec3 gNormal;

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

//light uniforms
uniform vec3 lightDir;
uniform float lightIntensity = 1.0;
uniform float ambient = 0.05;

//shadow mapping
const int MAX_CASCADE = 4;
const int SAMPLES = 16;

uniform float bBias = 0.0002;
uniform int cascadeCount;
uniform float cascadeDepths[MAX_CASCADE + 1];
uniform mat4 cascadeViewProjs[MAX_CASCADE];
uniform sampler2D cascadeShadowMaps[MAX_CASCADE];

float interleavedGradientNoise(vec2 pos)
{
	vec3 magic = vec3(0.06711056, 0.00583715, 52.9829189);
	return fract(magic.z * fract(dot(pos, magic.xy)));
}

vec2 vogelDiskSample(int sampleIndex, int samplesCount, float phi)
{
	float r = sqrt(sampleIndex + 0.5) / sqrt(samplesCount);
	float theta = sampleIndex * 2.4 + phi;

	float sine = sin(theta);
	float cosine = cos(theta);
  
	return vec2(r * cosine, r * sine);
}

float calculateShadow(vec4 fragPosLightSpace, sampler2D cascadeMap) {
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w * 0.5 + 0.5;

	if(projCoords.z > 1.0)
        return 1.0;

	vec3 norm = normalize(Normal);

	float bias = max(bBias * (1.0 - dot(norm, lightDir)), bBias);

	float shadow = 0.0;
	vec2 texelSize = 1.0 / textureSize(cascadeMap, 0);
	for(int i = 0; i < SAMPLES; i++)
	{
		vec2 coords = vogelDiskSample(i, SAMPLES, interleavedGradientNoise(projCoords.xy));
		vec2 shadowUV = projCoords.xy + coords * texelSize;

		float pcfDepth = texture(cascadeMap, shadowUV).r; 
		shadow += projCoords.z - bias > pcfDepth ? 0.0 : 1.0;        
	}
	shadow /= SAMPLES;

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

	vec3 outColor = (ambient + diffuseIntensity * shadow) * obColor;

	fragColor = vec4(outColor, 1.0);

	gAlbedo = obColor;
	gPosition = FragPos;
	gNormal = norm;
}