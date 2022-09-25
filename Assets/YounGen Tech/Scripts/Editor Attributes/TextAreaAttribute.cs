using UnityEngine;
using System.Collections;

public class TextAreaAttribute : PropertyAttribute {
	public bool richText;
	public bool wordWrap;

	public TextAreaAttribute(bool richText, bool wordWrap) {
		this.richText = richText;
		this.wordWrap = wordWrap;
	}
}