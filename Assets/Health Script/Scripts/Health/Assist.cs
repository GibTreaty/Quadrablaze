using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Health)), AddComponentMenu("Health/Assist")]
public class Assist : MonoBehaviour {

	public float maxAssistTime = 1;

	Health _health;
	public Health Health {
		get {
			if(!_health) _health = GetComponent<Health>();

			return _health;
		}
	}

	/// <summary>
	/// List of objects that assisted in hurting this object
	/// </summary>
	Dictionary<GameObject, AssistTimestamp> assists = new Dictionary<GameObject, AssistTimestamp>();

	public AssistEvent OnKillAssist;

	void Awake() {
		if(Health) {
			Health.OnDamaged.AddListener(new UnityAction<HealthEvent>(healthEvent => { AddAssist(healthEvent.originGameObject); }));
			Health.OnDeath.AddListener(new UnityAction<HealthEvent>(
				healthEvent => {
					GameObject[] allAssists = GetAssists();

					if(allAssists.Length > 0)
						if(OnKillAssist != null) OnKillAssist.Invoke(allAssists, gameObject);
				}
			));
		}
	}

	public void AddAssist(GameObject assist) {
		AssistTimestamp timestamp;

		RemoveNullAndLateAssists();

		if(assists.TryGetValue(assist, out timestamp))
			timestamp.time = Time.time;
		else
			assists[assist] = new AssistTimestamp(assist, Time.time);
	}

	void RemoveNullAndLateAssists() {
		AssistTimestamp[] timestamps = new AssistTimestamp[assists.Values.Count];

		assists.Values.CopyTo(timestamps, 0);

		for(int i = 0; i < assists.Count; i++) {
			AssistTimestamp timestamp = timestamps[i];

			if(!timestamp.gameObject || Time.time - timestamp.time > maxAssistTime)
				assists.Remove(timestamp.gameObject);
		}
	}

	public void ClearAssists() {
		assists.Clear();
	}

	public GameObject[] GetAssists() {
		RemoveNullAndLateAssists();

		List<GameObject> list = new List<GameObject>();

		foreach(KeyValuePair<GameObject, AssistTimestamp> a in assists)
			list.Add(a.Key);

		return list.ToArray();
	}

	void SendOnAssist(GameObject go) {
		go.SendMessage("OnAssist", gameObject, SendMessageOptions.DontRequireReceiver);
	}

	[Serializable]
	public class AssistEvent : UnityEvent<GameObject[], GameObject> { }
}

[Serializable]
public class AssistTimestamp {
	public GameObject gameObject;
	public float time;

	public AssistTimestamp(GameObject gameObject, float time) {
		this.gameObject = gameObject;
		this.time = time;
	}
}
