using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class GameManagerTests {
    
    // Dummy test
	[Test]
	public void EditorTest()
	{   
        //Arrange
        GameObject gameObject = new GameObject();
        GameManager gameManager = gameObject.AddComponent<GameManager>();

        //Act
        //Try to rename the GameObject
        string gameManagerName = "Game Manager";
        gameManager.name = gameManagerName;

		//Assert
		//The object has a new name
		Assert.AreEqual(gameManagerName, gameManager.name);

        gameManager = null;
	}
}
