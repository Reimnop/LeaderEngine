#include <GL/glew.h>
#include <GLFW/glfw3.h>
#include "Shader.h"

using namespace LeaderEngine;

Shader::Shader(const char* vertSource, const char* fragSource) {
	unsigned int vertShader = glCreateShader(GL_VERTEX_SHADER);
	glShaderSource(vertShader, 1, &vertSource, NULL);
	glCompileShader(vertShader);

	unsigned int fragShader = glCreateShader(GL_FRAGMENT_SHADER);
	glShaderSource(fragShader, 1, &fragSource, NULL);
	glCompileShader(fragShader);

	handle = glCreateProgram();
	glAttachShader(handle, vertShader);
	glAttachShader(handle, fragShader);
	glLinkProgram(handle);

	glDetachShader(handle, vertShader);
	glDetachShader(handle, fragShader);
	glDeleteShader(vertShader);
	glDeleteShader(fragShader);

	int numberOfUniforms;

	glGetProgramiv(handle, GL_ACTIVE_UNIFORMS, &numberOfUniforms);

	for (int i = 0; i < numberOfUniforms; i++) {
		char* name = NULL;
		glGetActiveUniform(handle, i, 255, NULL, NULL, NULL, name);

		int location = glGetUniformLocation(handle, name);

		uniformLocations[name] = location;

		delete[] name;
	}
}

Shader::~Shader() {
	for (auto& loc : uniformLocations) {
		delete[] loc.first;
	}

	glDeleteShader(handle);
}

void Shader::use() {
	glUseProgram(handle);
}