#pragma once

#include <unordered_map>

namespace LeaderEngine {
	class Shader {
	public:
		Shader(const char* vertSource, const char* fragSource);
		~Shader();

		void use();
	private:
		unsigned int handle;

		std::unordered_map<char*, int> uniformLocations;
	};
}
