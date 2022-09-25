using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YounGenTech.Entities {
    public class EntityManager : MonoBehaviour {
        public List<ScriptableEntity> entityPrefabs;

        public List<Entity> entityInstances;

        public int randomCount = 5;

        public int randomDistanace = 20;

        public int updateTickRate = 30;

        public bool gameLoopEnabled = true;
        public bool updateTickEnabled = true;

        IEnumerator gameLoopMethod;
        IEnumerator updateTickMethod;

        IEnumerator Start() {
            bool[,] grid = new bool[randomDistanace, randomDistanace];

            yield return new WaitForSeconds(1);

            for(int i = 0; i < randomCount; i++) {
                int xPos = 0, yPos = 0;
                bool flag = false;
                bool vacancy = false;

                do {
                    for(int b = 0; b < grid.GetLength(1); b++)
                        for(int a = 0; a < grid.GetLength(0); a++) {
                            if(!grid[a, b]) {
                                vacancy = true;
                                goto vacancyCheck;
                            }
                        }

                    vacancyCheck:
                    if(!vacancy) yield break;

                    for(int y = 0; y < grid.GetLength(1); y++)
                        for(int x = 0; x < grid.GetLength(0); x++) {
                            //Debug.DrawRay(new Vector3(x, 0, y) * 2.5f, Vector3.up * 2, grid[x, y] ? Color.red : Color.green, 0, false);

                            if(!grid[x, y]) {
                                if(UnityEngine.Random.value > .99f) {
                                    grid[x, y] = true;
                                    flag = true;
                                    xPos = x;
                                    yPos = y;
                                    //yield return new WaitForSeconds(.2f);
                                    goto skipToEnd;
                                }
                            }
                        }


                    skipToEnd:
                    yield return new WaitForSeconds(.01f);
                } while(!flag || !vacancy);


                //var randomCircle = Random.insideUnitCircle * randomDistanace;

                Vector3 position = new Vector3(xPos * 2.5f, 0, yPos * 2.5f);

                CreateInstance(UnityEngine.Random.Range(0, entityPrefabs.Count), true, position);
            }

            gameLoopMethod = GameUpdate();
            updateTickMethod = UpdateTick();

            StartCoroutine(gameLoopMethod);
            StartCoroutine(updateTickMethod);
        }

        public void CreateInstance(int index, bool createGraphics = true, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion), Transform parent = null) {
            var entity = entityPrefabs[index];
            var instance = entity.CreateInstance();

            if(createGraphics)
                instance.CreateGameObject(position, rotation, parent);

            entityInstances.Add(instance);
        }

        IEnumerator GameUpdate() {
            while(gameLoopEnabled) {
                foreach(var entity in entityInstances) {
                    (entity as IEntityUpdate)?.EntityUpdate();
                }

                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator UpdateTick() {
            double ticksPerSecond = 1f / updateTickRate;
            double lastTime = Time.realtimeSinceStartup;
            double currentTime = Time.realtimeSinceStartup;
            double deltaTime = 0;
            double timePassed = 0;

            while(updateTickEnabled) {
                lastTime = currentTime;
                currentTime = Time.realtimeSinceStartup;

                deltaTime = currentTime - lastTime;

                while(timePassed >= ticksPerSecond) {
                    foreach(var entity in entityInstances)
                        (entity as IEntityUpdateTick)?.EntityUpdateTick();
                    //switch(entity.Data) {
                    //    case IEntityUpdateTick obj: obj.EntityUpdateTick(); break;
                    //}

                    timePassed -= ticksPerSecond;
                }

                yield return new WaitForSeconds((float)ticksPerSecond);
        }
    }
}
}