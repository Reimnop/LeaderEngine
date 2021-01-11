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

out vec3 NormalWorldSpace;
out vec3 FragPosWorldSpace;

out vec3 NormalViewSpace;
out vec3 FragPosViewSpace;

uniform mat4 mvp;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

uniform mat4 modelLS;

uniform mat4 lightSpaceMatrix;

void main() 
{
	VertCol = aVertCol;

	Normal = aNormal * mat3(transpose(inverse(modelLS)));
	TexCoord = aTexCoord;
	FragPos = vec3(vec4(aPos, 1.0) * modelLS);
	FragPosLightSpace = vec4(FragPos, 1.0) * lightSpaceMatrix;

	NormalWorldSpace = vec3(vec4(aNormal, 1.0) * model);
	FragPosWorldSpace = vec3(vec4(aPos, 1.0) * model);

	NormalViewSpace = vec3(vec4(aNormal, 1.0) * model * view);
	FragPosViewSpace = vec3(vec4(aPos, 1.0) * model * view);

	gl_Position = vec4(aPos, 1.0) * mvp;
}