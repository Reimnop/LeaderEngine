#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D depthMap;
uniform sampler2D albedoSpec;
uniform sampler2D position;
uniform sampler2D normal;

in vec2 TexCoord;

float gamma = 2.2;

vec3 gammaCorrect(vec3 col) {
	return pow(col, vec3(gamma));
}

void main() 
{
	fragColor = vec4(texture(albedoSpec, TexCoord).rgb, 1.0);
}