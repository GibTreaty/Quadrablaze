using UnityEngine;
using UnityEngine.Events;
using System;

namespace UIWidgets
{
	/// <summary>
	/// TreeViewNode event.
	/// </summary>
	[Serializable]
	public class TreeViewNodeEvent : UnityEvent<TreeNode<TreeViewItem>>
	{
	}

	/// <summary>
	/// TreeView.
	/// </summary>
	[AddComponentMenu("UI/UIWidgets/TreeView")]
	public class TreeView : TreeViewCustom<TreeViewComponent,TreeViewItem>
	{
		/// <summary>
		/// NodeToggleProxy event.
		/// </summary>
		public TreeViewNodeEvent NodeToggleProxy = new TreeViewNodeEvent();

		bool isStarted = false;

		/// <summary>
		/// Start this instance.
		/// </summary>
		public override void Start()
		{
			if (isStarted)
			{
				return ;
			}
			isStarted = true;
			base.Start();

			NodeToggle.AddListener(NodeToggleProxy.Invoke);
		}
	}
}