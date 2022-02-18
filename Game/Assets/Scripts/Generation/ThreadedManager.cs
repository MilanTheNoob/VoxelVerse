using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

/// <summary>
/// Deals with all the pain known as threading
/// </summary>
public class ThreadedManager : MonoBehaviour
{
	private static readonly List<Action> executeOnMainThread = new List<Action>();
	private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
	private static bool actionToExecuteOnMainThread = false;

	static ThreadedManager instance;
	Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

	void Awake() { instance = FindObjectOfType<ThreadedManager>(); }

	public static void RequestData(Func<object> generateData, Action<object> callback)
	{
		ThreadStart threadStart = delegate { instance.DataThread(generateData, callback); };
		new Thread(threadStart).Start();
	}

	void DataThread(Func<object> generateData, Action<object> callback)
	{
		object data = generateData();
		lock (dataQueue) { dataQueue.Enqueue(new ThreadInfo(callback, data)); }
	}

	/// <summary>
	/// Executes an action on the main thread
	/// </summary>
	/// <param name="_action">The action to perform</param>
	public static void ExecuteOnMainThread(Action _action)
	{
		if (_action == null) { Debug.Log("No action to execute on main thread!"); return; }
		lock (executeOnMainThread) { executeOnMainThread.Add(_action); actionToExecuteOnMainThread = true; }
	}

	void Update()
	{
		if (dataQueue.Count > 0)
		{
			for (int i = 0; i < dataQueue.Count; i++)
			{
				ThreadInfo threadInfo = dataQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}

		if (actionToExecuteOnMainThread)
		{
			executeCopiedOnMainThread.Clear();
			lock (executeOnMainThread)
			{
				executeCopiedOnMainThread.AddRange(executeOnMainThread);
				executeOnMainThread.Clear();
				actionToExecuteOnMainThread = false;
			}

			for (int i = 0; i < executeCopiedOnMainThread.Count; i++) { executeCopiedOnMainThread[i](); }
		}
	}

	struct ThreadInfo
	{
		public readonly Action<object> callback;
		public readonly object parameter;

		public ThreadInfo(Action<object> callback, object parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}
}