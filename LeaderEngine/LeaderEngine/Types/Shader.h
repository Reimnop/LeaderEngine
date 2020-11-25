#pragma once

#include <unordered_map>

namespace LeaderEngine {
	class Shader {
	public:
		Shader(const char* vertSource, const char* fragSource);
		~Shader();

		void use();

		int getAttribLocation(const char* attribName);
	private:
		unsigned int handle;

		std::unordered_map<char*, int> uniformLocations;
	};
}
