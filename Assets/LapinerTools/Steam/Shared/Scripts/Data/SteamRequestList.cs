using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Steamworks;

namespace LapinerTools.Steam.Data.Internal
{
	/// <summary>
	/// Internal class, which will handle the creation and storage of the Steam CallResult types.
	/// This class might change in the future, please don't use it directly.
	/// Use an Execute method instead e.g. SteamWorkshopMain.Execute.
	/// Since you are not supposed to use this class, it is not documented in more detail.
	/// </summary>
	public class SteamRequestList
	{
		private Dictionary<System.Type, List<object>> m_requests = new Dictionary<System.Type, List<object>>();

		public void Add<T>(CallResult<T> p_request)
		{
			System.Type resultType = typeof(T);
			List<object> typedRequests;
			if (!m_requests.TryGetValue(resultType, out typedRequests))
			{
				typedRequests = new List<object>();
				m_requests.Add(resultType, typedRequests);
			}
			typedRequests.Add(p_request);
		}

		public int Count()
		{
			return m_requests.Values.Sum(requestList => requestList.Count);
		}

		public int Count<T>()
		{
			System.Type resultType = typeof(T);
			List<object> typedRequests;
			if (m_requests.TryGetValue(resultType, out typedRequests))
			{
				return typedRequests.Count;
			}
			else
			{
				return 0;
			}
		}

		public void Clear<T>()
		{
			System.Type resultType = typeof(T);
			List<object> typedRequests;
			if (m_requests.TryGetValue(resultType, out typedRequests))
			{
				typedRequests.Clear();
			}
		}

		public void RemoveInactive()
		{
			foreach (KeyValuePair<System.Type, List<object>> requestsEntry in m_requests)
			{
				MethodInfo removeInactiveInternalMethod = this.GetType().GetMethod("RemoveInactiveInternal", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(requestsEntry.Key);
				removeInactiveInternalMethod.Invoke(this, new object[] { requestsEntry.Value });
			}
		}

		public void RemoveInactive<T>()
		{
			System.Type resultType = typeof(T);
			List<object> typedRequests;
			if (m_requests.TryGetValue(resultType, out typedRequests))
			{
				MethodInfo removeInactiveInternalMethod = this.GetType().GetMethod("RemoveInactiveInternal", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(resultType);
				removeInactiveInternalMethod.Invoke(this, new object[] { typedRequests });
			}
		}

		public void Cancel()
		{
			foreach (KeyValuePair<System.Type, List<object>> requestsEntry in m_requests)
			{
				MethodInfo removeInactiveInternalMethod = this.GetType().GetMethod("CancelInternal", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(requestsEntry.Key);
				removeInactiveInternalMethod.Invoke(this, new object[] { requestsEntry.Value });
			}
		}

		public void Cancel<T>()
		{
			System.Type resultType = typeof(T);
			List<object> typedRequests;
			if (m_requests.TryGetValue(resultType, out typedRequests))
			{
				MethodInfo removeInactiveInternalMethod = this.GetType().GetMethod("CancelInternal", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(resultType);
				removeInactiveInternalMethod.Invoke(this, new object[] { typedRequests });
			}
		}

		private static void CancelInternal<T>(List<object> p_requests)
		{
			for (int i = p_requests.Count - 1; i >= 0; i--)
			{
				(p_requests[i] as CallResult<T>).Cancel();
			}
		}

		private static void RemoveInactiveInternal<T>(List<object> p_requests)
		{
			for (int i = p_requests.Count - 1; i >= 0; i--)
			{
				CallResult<T> typedRequest = p_requests[i] as CallResult<T>;
				if (!typedRequest.IsActive())
				{
					p_requests.RemoveAt(i);
				}	
			}
		}
	}
}