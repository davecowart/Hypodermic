using System;
using Hypodermic;

namespace TestHarness {
	class Program {
		static void Main(string[] args) {
			var profiler = Profiler.Instance();

			var thing = new Thing();
			thing.Do();

			profiler.WriteToConsole();
			Console.ReadLine();
		}
	}

	class Thing {
		[Profile]
		public void Do() {
			DoInnerOne();
			DoInnerTwo();
		}

		[Profile]
		public void DoInnerOne() {
			Console.WriteLine("DoInnerOne");
		}

		[Profile]
		public void DoInnerTwo() {
			Console.WriteLine("DoInnerTwo");
			WayInner();
		}

		[Profile]
		private void WayInner() {
			Console.WriteLine("WayInner");
		}
	}
}
