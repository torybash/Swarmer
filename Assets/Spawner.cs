using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

	[SerializeField] Transform monsterPrefab;
	[SerializeField] Transform bushPrefab;


	List<Transform>[][] bins;


	GameObject obstacleParent;
	GameObject monsterParent;

	// Use this for initialization
	void Start () {
		SpawnMonsters();

		SpawnObstacles();
	}
	
	// Update is called once per frame
	void Update () {
	
	}





	void SpawnMonsters()
	{
		monsterParent = new GameObject();
		monsterParent.name = "MonserParent";
		for (int i = 0; i < 200; i++) {
			Vector2 pos = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
			Transform t = SpawnMonster (pos);
			t.SetParent(monsterParent.transform);
		}
	}


	void SpawnObstacles()
	{
		obstacleParent = new GameObject();
		obstacleParent.name = "ObstacleParent";
		for (int i = 0; i < 40; i++) {
			Vector2 pos = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
			Transform t = SpawnObstacle (pos);
			t.SetParent(obstacleParent.transform);

		}
	}


	private Transform SpawnMonster(Vector2 pos)
	{
		Monster monster = ((Transform) Instantiate(monsterPrefab)).GetComponent<Monster>();
		monster.transform.position = pos;

		Monster.MonsterSettings mSts = RandomMonsterSettings();
		monster.Init(mSts);
//		monster.

		return monster.transform;
	}



	private Transform SpawnObstacle(Vector2 pos)
	{
		Transform bushTrans = (Transform) Instantiate(bushPrefab);
		bushTrans.position = pos;

		return bushTrans;
	}

	Monster.MonsterSettings RandomMonsterSettings()
	{
		Monster.MonsterSettings mSts = new Monster.MonsterSettings();

		mSts.age = Random.Range(0.2f, 1.8f);
		mSts.legStrength = Random.Range(1f, 2f);

		return mSts;
//		mSts.
	}
}
