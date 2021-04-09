#version 430 core

layout (location = 0) out vec4 fragColor;

in vec3 Color;
in vec2 TexCoord;

in vec3 Normal;
in vec3 FragCoord;

uniform bool hasDiffuse;
uniform sampler2D diffuse;

uniform vec3 color;

uniform vec3 camPos;

vec3 gammaCorrect(float gamma, vec3 col) {
	return pow(col, vec3(1 / gamma));
}

void main() {
	vec3 outColor = color;
	if (hasDiffuse)
		outColor *= texture(diffuse, TexCoord).rgb;

	float shade = max(dot(normalize(Normal), normalize(camPos - FragCoord)), 0.2);

	fragColor = vec4(gammaCorrect(2.2, outColor * shade), 1.0);
}