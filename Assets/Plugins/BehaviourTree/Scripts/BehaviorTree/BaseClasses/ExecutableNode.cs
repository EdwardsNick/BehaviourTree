using System.Collections;

namespace BehaviorTree {

	/*
	This interface is a simple representation of a node that can be executed as an co-routine
	This interface probably isn't necessary but it exists for now
	*/
	public interface ExecutableNode {

		IEnumerator Execute();
	}
}
