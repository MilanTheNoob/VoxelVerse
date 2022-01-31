using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class ThreadedManager : MonoBehaviour
{
	private static readonly List<Action> executeOnMainThread = new List<Action>();
	private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
	private static bool actionToExecuteOnMainThread = false;

	static ThreadedManager instance;
	Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

	void Awake()
	{
		instance = FindObjectOfType<ThreadedManager>();
	}

	public static void RequestData(Func<object> generateData, Action<object> callback)
	{
		ThreadStart threadStart = delegate {
			instance.DataThread(generateData, callback);
		};

		new Thread(threadStart).Start();
	}

	void DataThread(Func<object> generateData, Action<object> callback)
	{
		object data = generateData();
		lock (dataQueue)
		{
			dataQueue.Enqueue(new ThreadInfo(callback, data));
		}
	}

	public static void ExecuteOnMainThread(Action _action)
	{
		if (_action == null)
		{
			Debug.Log("No action to execute on main thread!");
			return;
		}

		lock (executeOnMainThread)
		{
			executeOnMainThread.Add(_action);
			actionToExecuteOnMainThread = true;
		}
	}

	/// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
	public static void UpdateMain()
	{
		if (actionToExecuteOnMainThread)
		{
			executeCopiedOnMainThread.Clear();
			lock (executeOnMainThread)
			{
				executeCopiedOnMainThread.AddRange(executeOnMainThread);
				executeOnMainThread.Clear();
				actionToExecuteOnMainThread = false;
			}

			for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
			{
				executeCopiedOnMainThread[i]();
			}
		}
	}


	void Update()
	{
		UpdateMain();

		if (dataQueue.Count > 0)
		{
			for (int i = 0; i < dataQueue.Count; i++)
			{
				ThreadInfo threadInfo = dataQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
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