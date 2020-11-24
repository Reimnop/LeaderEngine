#include <GL/glew.h>
#include <GLFW/glfw3.h>
#include "VertexArray.h"

using namespace LeaderEngine;

VertexArray::VertexArray(std::vector<float> vertices, std::vector<unsigned int> indices) {
	this->vertices = vertices.data();
	this->indices = indices.data();
}

VertexArray::~VertexArray() {
	delete this->vertices;
	delete this->indices;
}

void VertexArray::init() {
	glGenBuffers(1, &vbo);
}