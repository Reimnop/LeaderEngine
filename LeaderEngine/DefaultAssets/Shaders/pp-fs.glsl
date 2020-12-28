#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D texture0;

in vec2 TexCoord;

float gamma = 2.2;

void main() 
{
	fragColor = vec4(pow(texture(texture0, TexCoord).rgb, vec3(1.0 / gamma)), 1.0);
}