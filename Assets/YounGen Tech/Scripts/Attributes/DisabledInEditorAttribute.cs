using UnityEngine;
using System.Collections;

namespace YounGenTech {
	public class DisabledOutOfPlayAttribute : PropertyAttribute {
		public readonly bool invert;

		public DisabledOutOfPlayAttribute() {
			invert = false;
		}
		public DisabledOutOfPlayAttribute(bool invert) {
			this.invert = invert;
		}
	}
}