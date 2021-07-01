#version 450 core

layout (location = 0) out vec4 fragColor;

in vec2 TexCoord;

uniform sampler2D sourceTexture;
uniform sampler2D gPosition;

uniform float threshold = 0.8;
uniform float radius = 0.004;

const int SAMPLES = 8;

vec2 vogelDiskSample(int sampleIndex, int samplesCount, float phi)
{
	float r = sqrt(sampleIndex + 0.5) / sqrt(samplesCount);
	float theta = sampleIndex * 2.4 + phi;

	float sine = sin(theta);
	float cosine = cos(theta);
  
	return vec2(r * cosine, r * sine);
}

void main() {
	vec3 currentPos = texture(gPosition, TexCoord).xyz;

	float delta = 0.0;
	for (int i = 0; i < SAMPLES; i++) {
		vec3 vDelta = abs(texture(gPosition, vogelDiskSample(i, SAMPLES, 0.4) * radius + TexCoord).xyz - currentPos);
		float cDelta = vDelta.x + vDelta.y + vDelta.z;

		delta = max(delta, cDelta);
	}

	if (delta > threshold)
		fragColor = vec4(0.0, 0.0, 0.0, 1.0);
	else
		fragColor = texture(sourceTexture, TexCoord);
}