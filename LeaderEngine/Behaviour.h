#pragma once

namespace LeaderEngine {
	class Behaviour {
	public:
		virtual void onStart() {} //called when Behaviour is initialized
		virtual void onUpdate() {} //called before rendering every frame

		Behaviour();
		~Behaviour();
	};
}
