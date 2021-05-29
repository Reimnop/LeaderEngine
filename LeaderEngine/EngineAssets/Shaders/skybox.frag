#version 430 core

layout (location = 0) out vec4 fragColor;

in vec3 TexCoord;

uniform samplerCube skybox;

void main() {
	fragColor = texture(skybox, TexCoord);
}