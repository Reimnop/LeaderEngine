#version 330 core

layout (location = 0) out vec4 fragColor;

uniform sampler2D texture0;

in vec2 TexCoord;

void main() 
{
	vec4 color = texture(texture0, TexCoord);
	float alpha = (color.x + color.y + color.z) / 3.0;
	fragColor = vec4(vec3(color), alpha);
}