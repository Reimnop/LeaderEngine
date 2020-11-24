#include <GL/glew.h>
#include <GLFW/glfw3.h>
#include "VertexArray.h"

using namespace LeaderEngine;

VertexArray::VertexArray(std::vector<float> vertices, std::vector<unsigned int> indices) {
	this->vertices = vertices.data();
	this->indices = indices.data();
	init();
}

VertexArray::~VertexArray() {
	glDeleteBuffers(1, &vbo);
	glDeleteBuffers(1, &ebo);
	glDeleteVertexArrays(1, &vao);

	delete vertices;
	delete indices;
}

void VertexArray::init() {
	glGenBuffers(1, &vbo);
	glBindBuffer(GL_ARRAY_BUFFER, vbo);
	glBufferData(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);

	glGenBuffers(1, &ebo);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
	glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indices), indices, GL_STATIC_DRAW);

	glGenVertexArrays(1, &vao);
	glBindVertexArray(vao);

	glEnableVertexAttribArray(0);
	glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeof(float) * 3, 0);

	glBindBuffer(GL_ARRAY_BUFFER, vbo);
	glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
}

void VertexArray::use() {
	glBindVertexArray(vao);
}