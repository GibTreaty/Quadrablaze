using UnityEngine;
using System;
using System.Collections;

namespace YounGenTech.ComponentInterface {
	/// <summary>
	/// Lets other Components know that it can attempt to connect with it if the class it inherits decides to.
	/// </summary>
	public interface IComponentEventConnector {
		void ConnectComponentEvent(Component component);
		void DisconnectComponentEvent(Component component);
	}

	public static class ComponentEventConnectHelper {
		public static void ConnectComponentEventTo(this Component thisComponent, Component component) {
			IComponentEventConnector a = thisComponent as IComponentEventConnector;
			if(a != null) a.ConnectComponentEvent(component);

			IComponentEventConnector connector = component as IComponentEventConnector;
			if(connector != null) connector.ConnectComponentEvent(thisComponent);
		}

		public static void DisconnectComponentEventFrom(this Component thisComponent, Component component) {
			IComponentEventConnector a = thisComponent as IComponentEventConnector;
			if(a != null) a.DisconnectComponentEvent(component);

			IComponentEventConnector connector = component as IComponentEventConnector;
			if(connector != null) connector.DisconnectComponentEvent(thisComponent);
		}
	}
}