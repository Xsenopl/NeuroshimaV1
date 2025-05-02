using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DirectionTests
{
    [Test]
    public void DirectionTestsSimplePasses()
    {
        //Arrage
        int a =2; int b =3; int exp = 6;
        TestForTests obj = new();

        //Act
        int result = a + b;
        exp = obj.Add(a);
        //string username = await WebController.LoginUser("email2@", "pass1");


        //Assert
        Assert.AreEqual(6, exp);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator DirectionTestsWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
