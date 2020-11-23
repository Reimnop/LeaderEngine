#pragma once

#include <list>
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

		/*template <typename T>
		T* getComponent()
		{
			for (auto c : components)
				if (dynamic_cast<T&>(*c))
					return (T*)c;
			return nullptr;
		}*/

		void removeComponent(Component* component);
	private:
		std::list<Component*> components;
	};
}