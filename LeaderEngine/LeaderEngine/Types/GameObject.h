#pragma once

#include <unordered_map>
#include <typeindex>
#include <vector>
#include "Component.h"
#include "Transform.h"

namespace LeaderEngine {
	class GameObject {
	public:
		char* name;

		bool active = true;

		Transform* transform;

		//constructor and destructor
		GameObject(const char* name);
		~GameObject();

		//methods
		void start();
		void update();
		void render();

		void setActive(bool active);

		void addComponent(Component* component);
		void addComponents(std::vector<Component*> components);

		template <typename T>
		T* getComponent()
		{
			return (T*)components[typeid(T)];
		}

		template<typename T>
		void removeComponent();
	private:
		std::unordered_map<std::type_index, Component*> components;
	};
}