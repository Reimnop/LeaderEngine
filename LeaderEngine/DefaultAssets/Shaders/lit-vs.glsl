#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aVertCol;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec2 aTexCoord;

out vec3 VertCol;
out vec3 Normal;
out vec2 TexCoord;

out vec3 FragPos;

uniform mat4 mvp;
uniform mat4 model;

void main() 
{
	VertCol = aVertCol;
	Normal = aNormal;
	TexCoord = aTexCoord;

	FragPos = vec3(vec4(aPos, 1.0) * model);

	gl_Position = vec4(aPos, 1.0) * mvp;
}