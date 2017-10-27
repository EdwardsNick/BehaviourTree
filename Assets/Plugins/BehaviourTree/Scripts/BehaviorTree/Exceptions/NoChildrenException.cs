using System;

namespace BehaviorTree {

	//DEPRECIATED
	//For now

	public class NoChildrenException : Exception {

		public NoChildrenException() {
		}

		public NoChildrenException(string message)
			: base(message) {
		}

		public NoChildrenException(string message, Exception inner)
			: base(message, inner) {
		}
	}

}
