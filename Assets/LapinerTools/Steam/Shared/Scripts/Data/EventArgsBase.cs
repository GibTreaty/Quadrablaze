using UnityEngine;
using System.Collections;

namespace LapinerTools.Steam.Data
{
	/// <summary>
	/// This is the base of all event arguments used in the Easy Steam Integration project.
	/// </summary>
	public class EventArgsBase : System.EventArgs
	{
		/// <summary>
		/// <c>true</c>, if an error has occured, <c>false</c> otherwise.
		/// </summary>
		public bool IsError { get; set; }
		/// <summary>
		/// Contains the error message if the EventArgsBase.IsError property is <c>true</c>.
		/// </summary>
		public string ErrorMessage { get; set; }

		public EventArgsBase()
		{
		}

		public EventArgsBase(EventArgsBase p_copyFromArgs)
		{
			if (p_copyFromArgs != null)
			{
				IsError = p_copyFromArgs.IsError;
				ErrorMessage = p_copyFromArgs.ErrorMessage;
			}
		}
	}
}
