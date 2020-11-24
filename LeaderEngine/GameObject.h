#pragma once

#include <unordered_map>
#include <typeindex>
#include "Component.h"

namespace LeaderEngine {
	class GameObject {
	public:
		char* name;

		//constructor and destructor
		GameObject(const char* name);
		~GameObject();

		//methods
		void start();
		void update();
		void render();

		void addComponent(Component* component);

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