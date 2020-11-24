#pragma once

#include <glm/glm.hpp>
#include "Component.h";

using namespace glm;

namespace LeaderEngine {
	class Transform : public Component {
	public:
		vec3 position;
		vec4 rotation;
		vec3 scale;
	};
}