#pragma once

namespace LeaderEngine {
	class Component {
	public:
		virtual void start() {}
		virtual void update() {}
		virtual void lateUpdate() {}
		virtual void onClosing() {}
	};
}