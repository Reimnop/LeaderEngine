#version 450 core

layout (location = 0) out float fragColor;

in vec2 TexCoord;

uniform sampler2D gPosition;
uniform sampler2D gNormal;

const int KERNEL_SIZE = 64;
uniform vec3 kernel[KERNEL_SIZE];
uniform float radius = 0.5;

const int NOISE_SIZE = 64;
uniform vec3 noise[NOISE_SIZE];

uniform mat4 view;
uniform mat4 projection;

uniform float bias = 0.025;

float rand(vec2 co){
	return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

void main() {
	int noiseIndex = int(rand(TexCoord) * NOISE_SIZE);

	vec3 fragPos   = vec3(vec4(texture(gPosition, TexCoord).xyz, 1.0) * view);
	vec3 normal    = texture(gNormal, TexCoord).rgb;
	vec3 randomVec = noise[noiseIndex];

	vec3 tangent   = normalize(randomVec - normal * dot(randomVec, normal));
	vec3 bitangent = cross(normal, tangent);
	mat3 TBN       = mat3(tangent, bitangent, normal);

	float occlusion = 0.0;
	for (int i = 0; i < KERNEL_SIZE; i++) {
		vec3 samplePos = TBN * kernel[i];
		samplePos = fragPos + samplePos * radius;

		vec4 offset = vec4(samplePos, 1.0);
		offset      = offset * projection;
		offset.xyz /= offset.w;
		offset.xyz  = offset.xyz * 0.5 + 0.5;

		vec4 viewSpaceSample = vec4(texture(gPosition, offset.xy).xyz, 1.0) * view;
		float sampleDepth = viewSpaceSample.z;
		float rangeCheck = smoothstep(0.0, 1.0, radius / abs(fragPos.z - sampleDepth));
		occlusion += (sampleDepth >= samplePos.z + bias ? 1.0 : 0.0) * rangeCheck;
	}

	fragColor = 1.0 - occlusion / KERNEL_SIZE;
}