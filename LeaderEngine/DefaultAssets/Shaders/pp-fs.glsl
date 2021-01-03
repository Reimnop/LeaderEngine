#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D texture0;
uniform sampler2D depthMap;

in vec2 TexCoord;

float gamma = 2.2;

vec3 gammaCorrect(vec3 col) {
	return pow(col, vec3(gamma));
}

void main() 
{
	fragColor = vec4(texture(texture0, TexCoord).rgb, 1.0);
}