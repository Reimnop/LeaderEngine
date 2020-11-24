#pragma once

#include <vector>

namespace LeaderEngine {
	class VertexArray {
	public:
		std::vector<float> vertices;
		std::vector<unsigned int> indices;

		VertexArray(std::vector<float> vertices, std::vector<unsigned int> indices);
		~VertexArray();

		void use();
	private:
		unsigned int vbo, ebo, vao;

		void init();
	};
}