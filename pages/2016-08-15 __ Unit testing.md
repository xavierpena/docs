# Unit testing


It is important to use "loose coupling" for the code that you want to test in your unit tests. Otherwise it will be very hard to test.


## Creating the test project

First, create a UnitTest project "ProjName.Tests", then add new Unit Test class: "<YourUnitOfWork>Tests".

You also need to add the references to the projects that are being tested.


## Setting it up

Nuget package: **Moq**.

	IPersonRepository _repository;
	
	[TestInitialize]
	public void Setup()
	{
		var people = GetMockPeopleList();
		var repoMock = new Mock<IPersonRepository>();
		// Implement only the mockup functionalities that you need during the test:
		repoMock.Setup(r => r.GetPeople()).Returns(people)
		_repository = repoMock.Object;
	}

Another example: mocking "function GetPerson(string n)", that returns the person in people which "LastName == n":

	repoMock.Setup(r => r.GetPerson(It.IsAny<string>()).
	    Returns((string n) => people.FirstOrDefault(p => p.LastName == n));

## Naming each unit test:

Roy Osherove, "The art of unit testing"

Unit test naming convention:

TheObjWeAreTesting_WhatDoWeTest_ExpectedResult


## Inside the unit test

The AAA (Arrange, Act, Assert) pattern is a common way of writing unit tests for a method under test:
* The **Arrange** section of a unit test method initializes objects and sets the value of the data that is passed to the method under test.
* The **Act** section invokes the method under test with the arranged parameters.
* The **Assert** section verifies that the action of the method under test behaves as expected.
To run tests in order:


## Running the tests in a certain order

* Right click on the "ProjName.Tests"
* Add => Ordered Test
* (before they show up, we need to Build the project)
* Sort the tests
* It generates a "BlahBlah.orderedtest" in the project

