#version 330 core

layout (location = 0) out vec4 fragColor;

uniform int useTexture;
uniform sampler2D texture0;

in vec3 VertCol;
in vec2 TexCoord;

void main() 
{
	if (useTexture == 1)
		fragColor = texture(texture0, TexCoord) * vec4(VertCol, 1.0);
	else 
		fragColor = vec4(VertCol, 1.0);
}
