#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aVertCol;
layout (location = 2) in vec3 aNormal;
layout (location = 3) in vec2 aTexCoord;

out vec3 VertCol;

out vec3 Normal;
out vec2 TexCoord;
out vec3 FragPos;
out vec4 FragPosLightSpace;

out float Depth;

uniform mat4 mvp;
uniform mat4 model;

uniform mat4 lightSpaceMatrix;

void main() 
{
	VertCol = aVertCol;

	Normal = aNormal * mat3(transpose(inverse(model)));
	TexCoord = aTexCoord;
	FragPos = vec3(vec4(aPos, 1.0) * model);
	FragPosLightSpace = vec4(FragPos, 1.0) * lightSpaceMatrix;

	vec4 pos = vec4(aPos, 1.0) * mvp;

	Depth = ((pos.xyz / pos.w) * 0.5 + 0.5).z;

	gl_Position = pos;
}