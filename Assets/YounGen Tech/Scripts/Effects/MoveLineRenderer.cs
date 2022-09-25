using UnityEngine;
using System;
using System.Collections;

namespace YounGenTech {
	/// <summary>
	/// Modifies two points of a LineRenderer
	/// </summary>
	[RequireComponent(typeof(LineRenderer)), AddComponentMenu("YounGen Tech/Scripts/Effects/Move LineRenderer")]
	public class MoveLineRenderer : MonoBehaviour {

		/// <summary>
		/// Event fires when the end of the line reaches the endPosition.
		/// This will be immediately fired if moveEnd is false
		/// </summary>
		public event Action OnLineReachEnd;

		/// <summary>
		/// Distance from the start
		/// </summary>
		public event Action<float> OnLineReachDistance;

		/// <summary>
		/// The LineRenderer component on this GameObject
		/// </summary>
		LineRenderer lineRenderer;

		/// <summary>
		/// Start of the LineRenderer
		/// </summary>
		public Vector3 startPosition;
		/// <summary>
		/// End of the LineRenderer
		/// </summary>
		public Vector3 endPosition;

		public Vector3 currentStartPosition;
		public Vector3 currentEndPosition;

		/// <summary>
		/// Move the start point of the LineRenderer?
		/// </summary>
		public bool moveStart;
		/// <summary>
		/// Move the end point of the LineRenderer?
		/// </summary>
		public bool moveEnd;

		/// <summary>
		/// Offset the time of the start point
		/// </summary>
		public float startOffset;
		/// <summary>
		/// Offset the time of the end point
		/// </summary>
		public float endOffset;

		public float speed;
		public bool speedIsLineDistance;

		void Awake() {
			lineRenderer = GetComponent<LineRenderer>();
		}

		void Update() {
			if(!lineRenderer) {
				Debug.LogError("No LineRenderer found - Destroying MoveLineRenderer", gameObject);
				Destroy(this);
			}

			Vector3 direction = (endPosition - startPosition);
			Vector3 moveLength = direction * Time.deltaTime;

			if(speedIsLineDistance)
				moveLength = direction * Time.deltaTime;
			else
				moveLength = direction.normalized * speed * Time.deltaTime;

			if(moveStart) currentStartPosition += moveLength;

			if(moveEnd)
				//Detects which side the end of the line is on of the endPosition
				if(Vector3.Dot(currentEndPosition - endPosition, direction) < 0) {
					currentEndPosition += moveLength;

					if(Vector3.Dot(currentEndPosition - endPosition, direction) >= 0)
						if(OnLineReachEnd != null) OnLineReachEnd();

					if(OnLineReachDistance != null) OnLineReachDistance(Vector3.Distance(currentEndPosition, startPosition));
				}

			Vector3 setStartPosition = currentStartPosition;
			Vector3 setEndPosition = currentEndPosition;

			setStartPosition = ClampVector(setStartPosition, startPosition, endPosition);
			setEndPosition = ClampVector(setEndPosition, startPosition, endPosition);

			lineRenderer.transform.position = (setStartPosition + setEndPosition) * .5f;

			Vector3 movementNormal = setEndPosition - setStartPosition;

			if(movementNormal != Vector3.zero)
				lineRenderer.transform.rotation = Quaternion.LookRotation(movementNormal);

			lineRenderer.SetPosition(0, setStartPosition);
			lineRenderer.SetPosition(1, setEndPosition);
		}

		public void MoveTo(Vector3 start, Vector3 end, float startOffset, float endOffset, bool moveStart, bool moveEnd, float speed, bool speedIsLineDistance) {
			startPosition = start;
			endPosition = end;

			this.startOffset = startOffset;
			this.endOffset = endOffset;

			this.moveStart = moveStart;
			this.moveEnd = moveEnd;

			Vector3 normals = (endPosition - startPosition).normalized;

			currentStartPosition = startPosition + (startOffset * normals);
			currentEndPosition = (moveEnd ? startPosition : endPosition) + (endOffset * normals);

			if(!moveEnd) {
				if(OnLineReachEnd != null) OnLineReachEnd();
				if(OnLineReachDistance != null) OnLineReachDistance(Vector3.Distance(startPosition, endPosition));
			}

			lineRenderer.SetPosition(0, ClampVector(currentStartPosition, startPosition, endPosition));
			lineRenderer.SetPosition(1, ClampVector(currentEndPosition, startPosition, endPosition));

			this.speed = speed;
			this.speedIsLineDistance = speedIsLineDistance;
		}

		public static Vector3 ClampVector(Vector3 point, Vector3 lineStart, Vector3 lineEnd) {
			Vector3 direction = lineStart - lineEnd;

			if(Vector3.Dot(lineEnd - point, direction) > 0) //Greater Than End?
				point = lineEnd;
			else if(Vector3.Dot(point - lineStart, direction) > 0) //Lower Than Start?
				point = lineStart;

			return point;
		}
	}
}