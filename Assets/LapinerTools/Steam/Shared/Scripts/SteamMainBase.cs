using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using LapinerTools.Steam.Data;
using LapinerTools.Steam.Data.Internal;

using Steamworks;

namespace LapinerTools.Steam
{
	/// <summary>
	/// SteamMainBase is derived by all Steam APIs of the Easy Steamworks Integration Unity plugin.
	/// Derive from this class if you want to create a Steam API based on the Easy Steamworks Integration structure.
	/// Use this class to handle Steam errors, execute Steam API calls, handle call results and keep track of pending API calls.
	/// </summary>
	public class SteamMainBase<SteamMainT> : MonoBehaviour where SteamMainT : SteamMainBase<SteamMainT>
	{
		protected static SteamMainT s_instance;
		/// <summary>
		/// You can use the static Instance property to access the Steam API from wherever you need it in your code. See also SteamMainBase.IsInstanceSet.
		/// </summary>
		public static SteamMainT Instance
		{
			get
			{
				// first try to find an existing instance
				if (s_instance == null)
				{
					s_instance = FindObjectOfType<SteamMainT>();
				}
				// if no instance exists yet, then create a new one
				if (s_instance == null)
				{
					s_instance = new GameObject(typeof(SteamMainT).Name).AddComponent<SteamMainT>();
				}
				return s_instance;
			}
		}
		/// <summary>
		/// The static IsInstanceSet property can be used before accessing the Instance property to prevent a new instance from being created in the teardown phase.
		/// For example, if you want to unregister from an event, then first check that IsInstanceSet is true. If it is false, then your event registration is not valid anymore.
		/// </summary>
		public static bool IsInstanceSet { get{ return s_instance != null; } }


#region Members

		// Pending data requests
		protected SteamRequestList m_pendingRequests = new SteamRequestList();
		// Event handlers that will be removed after being fired once
		private Dictionary<string, List<object>> m_singleShotEventHandlers = new Dictionary<string, List<object>>();
		// This lock object is used to make multithreading safe
		protected object m_lock = new object();
		
#endregion


#region Events

		/// <summary>
		/// Invoked when a Steam error has occured.
		/// </summary>
		public event System.Action<ErrorEventArgs> OnError;
		
#endregion


#region API

		[SerializeField, Tooltip("Set this property to true if you want to see a detailed log in the console. Disabled by default.")]
		protected bool m_isDebugLogEnabled = false;
		/// <summary>
		/// Set this property to <c>true</c> if you want to see a detailed log in the console. Disabled by default.
		/// </summary>
		public bool IsDebugLogEnabled
		{
			get{ return m_isDebugLogEnabled; }
			set{ m_isDebugLogEnabled = value; }
		}

		/// <summary>
		/// The Execute method will handle Steam CallResult creation and storage.
		/// Simply pass the configured SteamAPICall and the callback that you want to be invoked when the work is done.
		/// Pending results can be viewed in the console with SteamMainBase.IsDebugLogEnabled set to <c>true</c>.
		/// </summary>
		/// <param name="p_steamCall">the configured SteamAPICall.</param>
		/// <param name="p_onCompleted">invoked when the work is done.</param>
		/// <typeparam name="T">The CallResult type.</typeparam>
		public void Execute<T>(SteamAPICall_t p_steamCall, CallResult<T>.APIDispatchDelegate p_onCompleted)
		{
			CallResult<T> callResult = CallResult<T>.Create(p_onCompleted);
			callResult.Set(p_steamCall, null);
			m_pendingRequests.Add(callResult);
		}

#endregion


#region MonoBehaviour
		
		protected virtual void OnDisable()
		{
			if (m_pendingRequests != null) { m_pendingRequests.Cancel(); }
		}

		protected virtual void LateUpdate()
		{
			lock(m_lock)
			{
				// remove failed/skipped requests
				m_pendingRequests.RemoveInactive();

				// log pending things
				if (IsDebugLogEnabled && Time.frameCount % 300 == 0)
				{
					// log stuck requests
					if (m_pendingRequests.Count() > 0)
					{
						Debug.Log(typeof(SteamMainT).Name + ": pending requests left: " + m_pendingRequests.Count());
					}
					
					// log forgotten single shot event handlers
					foreach (KeyValuePair<string, List<object>> singleShotEventEntry in m_singleShotEventHandlers)
					{
						if (singleShotEventEntry.Value.Count > 0)
						{
							Debug.Log(typeof(SteamMainT).Name + ": pending signle shot event handlers for '" + singleShotEventEntry.Key + "' left: " + singleShotEventEntry.Value.Count);
						}
					}
				}
			}
		}
		
#endregion

				
#region SteamResultCalls

		protected virtual bool CheckAndLogResultNoEvent<Trequest>(string p_logText, EResult p_result, bool p_bIOFailure)
		{
			System.Action<object> dummyEvent = null;
			return CheckAndLogResult<Trequest, object>(p_logText, p_result, p_bIOFailure, null, ref dummyEvent);
		}
		
		protected virtual bool CheckAndLogResult<Trequest, Tevent>(string p_logText, EResult p_result, bool p_bIOFailure, string p_eventName, ref System.Action<Tevent> p_event)
		{
			lock (m_lock)
			{
				m_pendingRequests.RemoveInactive<Trequest>();
				if (IsDebugLogEnabled) { Debug.Log(p_logText + ": (fail:"+p_bIOFailure+") " + p_result + " requests left: " + m_pendingRequests.Count<Trequest>()); }
			}
			if (p_result == EResult.k_EResultOK && !p_bIOFailure)
			{
				return true;
			}
			else
			{
				ErrorEventArgs errorArgs = ErrorEventArgs.Create(p_result);
				HandleError(p_logText + ": failed! ", errorArgs);
				if (p_eventName != null && p_event != null)
				{
					CallSingleShotEventHandlers(p_eventName, (Tevent)System.Activator.CreateInstance(typeof(Tevent), errorArgs), ref p_event); // call single shot event with error
				}
				return false;
			}
		}

#endregion


#region InternalLogic
		
		protected virtual void HandleError(string p_logPrefix, ErrorEventArgs p_error)
		{
			Debug.LogError(p_logPrefix + p_error.ErrorMessage);
			InvokeEventHandlerSafely(OnError, p_error);
		}
		
		protected virtual void InvokeEventHandlerSafely<T>(System.Action<T> p_handler, T p_data)
		{
			try
			{
				if (p_handler != null) { p_handler(p_data); }
			}
			catch (System.Exception ex)
			{
				Debug.LogError(typeof(SteamMainT).Name + ": your event handler ('"+p_handler.Target+"' - System.Action<"+typeof(T)+">) has thrown an excepotion!\n" + ex);
			}
		}

		protected virtual void SetSingleShotEventHandler<T>(string p_eventName, ref System.Action<T> p_event, System.Action<T> p_handler)
		{
			if (p_handler != null)
			{
				if (!m_singleShotEventHandlers.ContainsKey(p_eventName))
				{
					m_singleShotEventHandlers.Add(p_eventName, new List<object>());
				}
				m_singleShotEventHandlers[p_eventName].Add(p_handler.Target);
				p_event += p_handler;
			}
		}
		
		protected virtual void CallSingleShotEventHandlers<T>(string p_eventName, T p_args, ref System.Action<T> p_event)
		{
			if (p_event != null && m_singleShotEventHandlers.ContainsKey(p_eventName))
			{
				int singleShotCount = m_singleShotEventHandlers[p_eventName].Count;
				System.Delegate[] handlers = p_event.GetInvocationList();
				foreach (System.Delegate handler in handlers)
				{
					if (m_singleShotEventHandlers[p_eventName].Contains(handler.Target))
					{
						p_event -= (System.Action<T>)handler;
						m_singleShotEventHandlers[p_eventName].Remove(handler.Target);
						try
						{
							handler.DynamicInvoke(p_args);	
						}
						catch (System.Exception ex)
						{
							Debug.LogError(typeof(SteamMainT).Name + ": your event handler ('"+handler.Target+"' - System.Action<"+typeof(T)+">) has thrown an excepotion!\n" + ex);
						}
					}
				}
				if (IsDebugLogEnabled) { Debug.Log(typeof(SteamMainT).Name + ": CallSingleShotEventHandlers '" + p_eventName + "' left handlers: " + (p_event != null ? p_event.GetInvocationList().Length : 0) + "/" + handlers.Length + " left single shots: " + m_singleShotEventHandlers[p_eventName].Count + "/" + singleShotCount); }
			}
		}
		
		protected virtual void ClearSingleShotEventHandlers<T>(string p_eventName, ref System.Action<T> p_event)
		{
			if (p_event != null && m_singleShotEventHandlers.ContainsKey(p_eventName))
			{
				int singleShotCount = m_singleShotEventHandlers[p_eventName].Count;
				System.Delegate[] handlers = p_event.GetInvocationList();
				foreach (System.Delegate handler in handlers)
				{
					if (m_singleShotEventHandlers[p_eventName].Contains(handler.Target))
					{
						p_event -= (System.Action<T>)handler;
						m_singleShotEventHandlers[p_eventName].Remove(handler.Target);
					}
				}
				if (IsDebugLogEnabled) { Debug.Log(typeof(SteamMainT).Name + ": ClearSingleShotEventHandler '" + p_eventName + "' left handlers: " + (p_event != null ? p_event.GetInvocationList().Length : 0) + "/" + handlers.Length + " left single shots: " + m_singleShotEventHandlers[p_eventName].Count + "/" + singleShotCount); }
			}
		}

#endregion
	}
}
