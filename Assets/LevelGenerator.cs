using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Limits
{
   public float minX, maxX;
   public float minY;
}

public enum Directions
{
   Left,Right,Down
}

public class LevelGenerator : MonoBehaviour
{
   [SerializeField] private Transform[] startingPositions;
   [SerializeField] private GameObject[] rooms;
   private Transform levelGeneratorTransform;

   private Directions direction;
   [SerializeField] float moveAmount;

   private float timeBtwRoom;
   public float startTimeBtwRoom = .25f;

   [SerializeField] private Limits limits;

   [SerializeField] private bool stopGeneration = false;

   [SerializeField] private List<Directions> directions = new List<Directions>();

   //Rebuild the scene
   private int oldValue = 0;
   
   
   private void Start()
   {
      levelGeneratorTransform = transform;
      //For building a new level
      if (!PlayerPrefs.HasKey("Direction_0"))
      {
         //Call first room
         int randStartingPos = Random.Range(0, startingPositions.Length);
         PlayerPrefs.SetInt("randomStart",randStartingPos);
         levelGeneratorTransform.position = startingPositions[randStartingPos].position;
         Instantiate(rooms[0], levelGeneratorTransform.position, quaternion.identity);

         //Can be Left, Right or Down
         direction = (Directions) Random.Range(0, 3);
         directions.Add(direction);

         StartCoroutine(Build_New());         
      }
      else // To rebuild the level we made
      {
         levelGeneratorTransform.position = startingPositions[PlayerPrefs.GetInt("randomStart")].position;
         Instantiate(rooms[0], levelGeneratorTransform.position, quaternion.identity);
         direction = GetFromWholePlayerprefs();
         
         //We must call this here because need to make the movement before calling the coroutine
         Move(direction);
         directions.Add(direction);
         
         StartCoroutine(Build_Old());         
      }
   }

   #region Build a Scenary

   void Move()
   {
      Vector2 tempPos = levelGeneratorTransform.position;

      if (direction == Directions.Right) //Move Right
      {
         if (tempPos.x < limits.maxX)
         {
            levelGeneratorTransform.position = NewPosition(tempPos);
            tempPos = levelGeneratorTransform.position;
            Instantiate(rooms[0], tempPos, Quaternion.identity);
            SetDirection();
         }
         else if(tempPos.x>=limits.maxX)
         {
            direction = Directions.Down;
            print("Calling Right - Out");
         }
      }
      else if (direction == Directions.Left) //Move Left
      {
         if (tempPos.x > limits.minX)
         {
            levelGeneratorTransform.position = NewPosition(tempPos);
            tempPos = levelGeneratorTransform.position;
            Instantiate(rooms[0], tempPos, Quaternion.identity);
            SetDirection();
         }
         else if(tempPos.x<=limits.minX)
         {
            direction = Directions.Down;
            print("Calling Left - Out");
         }
      }
      else if (direction == Directions.Down) //Move Down
      {
         if (tempPos.y > limits.minY)
         {
            levelGeneratorTransform.position = NewPosition(tempPos);
            tempPos = levelGeneratorTransform.position;
            Instantiate(rooms[0], tempPos, Quaternion.identity);
            SetDirection();
         }
         else if(tempPos.y<=limits.minY)
         {
            stopGeneration = true;
         }
      }

      directions.Add(direction);
   }
   
   IEnumerator Build_New()
   {
      while (!stopGeneration)
      {
         if (timeBtwRoom <= 0)
         {
            Move();
            timeBtwRoom = startTimeBtwRoom;
         }
         else
         {
            timeBtwRoom -= Time.deltaTime;
            yield return null;
         }
      }

      SaveSeed();
   }

   #endregion
   
   #region Utils

   Vector2 NewPosition(Vector2 _tempPos)
   {
      Vector2 newPos = Vector2.zero;

      switch (direction)
      {
         case Directions.Left:
            newPos = new Vector2(_tempPos.x - moveAmount,
               _tempPos.y);
            print("Calling Left");
            break;
         case Directions.Right:
            newPos = new Vector2(_tempPos.x + moveAmount,
               _tempPos.y);
            print("Calling Right");
            break;
         case Directions.Down:
            newPos = new Vector2(_tempPos.x,
               _tempPos.y - moveAmount);
            print("Calling Down");
            break;
      }

      return newPos;
   }

   void SetDirection()
   {
      switch (direction)
      {
         case Directions.Left:
            direction = (Directions) Random.Range(0, 3);
            if (direction == Directions.Right)
            {
               print("Calling - Right to Left");
               direction = Directions.Left;
            }

            break;
         case Directions.Right:
            // Makes sure the level generator doesn't move left !
            direction = (Directions) Random.Range(0, 3);
            if (direction == Directions.Left)
            {
               print("Calling - Left to Right");
               direction = Directions.Right;
            }

            break;
         case Directions.Down:
            direction = (Directions) Random.Range(0, 3);
            break;
      }
   }

   #endregion

   #region Rebuild the last Scenary

   void Move(Directions _directions)
   {
      Vector2 tempPos = levelGeneratorTransform.position;

      if (direction == Directions.Right) //Move Right
      {
         if (tempPos.x < limits.maxX)
         {
            levelGeneratorTransform.position = NewPosition(tempPos);
            tempPos = levelGeneratorTransform.position;
            Instantiate(rooms[0], tempPos, Quaternion.identity);
         }
      }
      else if (direction == Directions.Left) //Move Left
      {
         if (tempPos.x > limits.minX)
         {
            levelGeneratorTransform.position = NewPosition(tempPos);
            tempPos = levelGeneratorTransform.position;
            Instantiate(rooms[0], tempPos, Quaternion.identity);
         }
      }
      else if (direction == Directions.Down) //Move Down
      {
         if (tempPos.y > limits.minY)
         {
            levelGeneratorTransform.position = NewPosition(tempPos);
            tempPos = levelGeneratorTransform.position;
            Instantiate(rooms[0], tempPos, Quaternion.identity);
         }
      }
      directions.Add(direction);
   }

   Directions GetFromWholePlayerprefs()
   {
      direction = (Directions) PlayerPrefs.GetInt("Direction_"+oldValue);
      oldValue++;
      if (oldValue >= PlayerPrefs.GetInt("OldCount"))
      {
         stopGeneration = true;
         oldValue = PlayerPrefs.GetInt("OldCount");
      }

      return direction;
   }
   
   IEnumerator Build_Old()
   {
      while (!stopGeneration)
      {
         if (timeBtwRoom <= 0)
         {
            Move(GetFromWholePlayerprefs());
            timeBtwRoom = startTimeBtwRoom;
         }
         else
         {
            timeBtwRoom -= Time.deltaTime;
            yield return null;
         }
      }

      print("Finished");
   }

   void SaveSeed()
   {
      for (int i = 0; i < directions.Count; i++)
      {
         PlayerPrefs.SetInt("Direction_"+i,(int)directions[i]);
      }
      PlayerPrefs.SetInt("OldCount",directions.Count);
      
      PlayerPrefs.Save();
   }   
   #endregion
}
