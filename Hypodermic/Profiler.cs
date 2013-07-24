using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hypodermic {
	[Serializable]
	public class Profile : PostSharp.Aspects.MethodInterceptionAspect {
		public override void OnInvoke(PostSharp.Aspects.MethodInterceptionArgs args) {
			var name = args.Instance.GetType().Name + args.Method.Name;
			var node = Profiler.Instance().StartProfile(name, args.Method.GetParameters());
			var s = Stopwatch.StartNew();
			base.OnInvoke(args);
			s.Stop();
			Profiler.Instance().EndProfile(node, s.ElapsedMilliseconds);
		}
	}


	public class ProfileNode {
		public ProfileNode(string methodName, ParameterInfo[] parameters = null) {
			Identifier = Guid.NewGuid();
			MethodName = methodName;
			Parameters = parameters;

			Children = new List<ProfileNode>();
		}

		public Guid Identifier { get; set; }
		public string MethodName { get; set; }
		public long Milliseconds { get; set; }
		public ParameterInfo[] Parameters { get; set; }
		public List<ProfileNode> Children { get; set; }
		public ProfileNode Parent { get; set; }
		public int Depth {
			get {
				var node = this;
				var count = 0;
				while (node.Parent != null) {
					count++;
					node = node.Parent;
				}
				return count;
			}
		}

		public ProfileNode AddChild(ProfileNode node) {
			node.Parent = this;
			Children.Add(node);
			return node;
		}

		public ProfileNode AddChild(string methodName, ParameterInfo[] parameters) {
			return AddChild(new ProfileNode(methodName, parameters));
		}

		public override string ToString() {
			return string.Format("{0} completed in {1}ms", MethodName, Milliseconds);
		}

		public string ToStringWithDepth() {
			var sb = new StringBuilder();
			for (var i = 0; i < Depth * 2; i++) {
				sb.Append(" ");
			}
			sb.Append(ToString());
			return sb.ToString();
		}
	}

	public class Profiler {
		public ProfileNode ApplicationNode = new ProfileNode("Application");
		public Stack<Guid> NodePath = new Stack<Guid>();
		public Guid CurrentNodeGuid { get { return NodePath.Peek(); } }
		public ProfileNode CurrentNode {
			get {
				ProfileNode node = null;
				foreach (var guid in NodePath.ToArray().Reverse()) {
					if (node == null) {
						node = ApplicationNode;
					} else {
						node = node.Children.Single(n => n.Identifier == guid);
					}
				}
				return node;
			}
		}

		private Profiler() {
			NodePath.Push(ApplicationNode.Identifier);
		}

		private static Profiler _instance;

		public static Profiler Instance() {
			return _instance ?? (_instance = new Profiler());
		}

		public ProfileNode StartProfile(string methodName, ParameterInfo[] parameters) {
			var node = CurrentNode.AddChild(methodName, parameters);
			NodePath.Push(node.Identifier);
			return node;
		}

		public void EndProfile(ProfileNode node, long milliseconds) {
			node.Milliseconds = milliseconds;
			NodePath.Pop();
		}

		public void WriteToConsole() {
			RenderNode(ApplicationNode);
		}

		private static void RenderNode(ProfileNode node) {
			Console.WriteLine(node.ToStringWithDepth());
			foreach (var childNode in node.Children) {
				RenderNode(childNode);
			}
		}
	}
}

