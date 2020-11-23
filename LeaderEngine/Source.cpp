#include "Behaviour.h"
#include <iostream>

class derived : public LeaderEngine::Behaviour {
public:
	int a = 0;

	void onUpdate() {
		std::cout << "a" << std::endl;
	}
};