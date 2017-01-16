# Depenency injection with Unity

## Source

Dependency Injection is all about creating loosely coupled code.

Tightly coupled code has two main downsides:
* Diffucult to extend
* Difficult to test

Other upsides of loosely coupled code:
* Late binding
* Parallel development
* Maintainability

The contents of this tutorial are based on the **Pluralsight** course about Inversion of Control ([[[https://app.pluralsight.com/library/courses/inversion-of-control| link]]]).

In the course, 4 IoC are explained:
* Unity
* Castle Windsor
* Structure Map
* Ninject

**Unity** seems to be one of the best (or even the best), in terms of speed, between the 4 mentioned in the course: [[[https://cardinalcore.co.uk/2015/01/28/ioc-battle-in-2015-results-using-ninject-think-again/| speed comparison]]].

## Step one: registering the types

Of course first of all we need to install **Unity** from **Nuget**.

Then, we define the container:


	var container = new UnityContainer();


A Dependency Injection Container is an object that knows how to instantiate and configure objects. And to be able to do its job, it needs to knows about the constructor arguments and the relationships between the objects:


	// basic:
	container.RegisterType<ICreditCard, MasterCard>();

	// constructor injection (modify a public property of the implementation when initializing it):
	container.RegisterType<ICreditCard, MasterCard>(new InjectionProperty(nameof(ICreditCard.ChargeCount), 5));

	// create an identifier for the instantiation. In this case "DefaultCard" is the name that identifies the registration:
	container.RegisterType<ICreditCard, MasterCard>("DefaultCard");

	// register an instance:
	var card = new MasterCard();
	container.RegisterInstance(card);


**Note:** If there is only 1 implementation of the interface, we don't need to register it first. Unity is smart enough to infer its implementation.

## Step two: calling the registered type

Now that the container knows about the objects we will want to use later, how do we call them? We use this syntax:


	var myInstantiatedObject = container.Resolve<Shopper>();


**Note:** with the implementation above, if you encounter the error "The non-generic method 'Microsoft.Practices.Unity.IUnityContainer.Resolve(System.Type, string, params Microsoft.Practices.Unity.ResolverOverride[])' cannot be used with type arguments", it can be solved like explained in this [[[http://stackoverflow.com/questions/2875429/iunitycontainer-resolvet-throws-error-claiming-it-cannot-be-used-with-type-par| stackoverflow link]]] =>
"Looks like, even if it is not a dll in Unity V2 you have to add a reference in your class to: `Microsoft.Practices.Unity`"

## Singleton

The examples of registering shown above will create a **new** object every time the instance is called (it uses the default mode: `TransientLifetimeManager`). If we want to avoid that, that is to say, if we want to use a **singleton**, we must implement the registration like this:


	container.RegisterType<ICreditCard, MasterCard>(new ContainerControlledLifetimeManager());


More on Lifetime (or "lifecycle") management [[[https://msdn.microsoft.com/en-us/library/ff660872(v=pandp.20).aspx| in this link]]].


## Simple use

Later, with the Container.ResolveInstance<Type>, it works like this:
- Type has only one implementation? OK, create a new from that.
- Or: is Type registered? OK, let's take that implementation.
- Do it recursively for all of the constructors involved in the resolution of that instance.

By default, Unity takes the most complex constructor available.


## Proper use

First a post about how NOT to use it:
http://www.devtrends.co.uk/blog/how-not-to-do-dependency-injection-the-static-or-singleton-container

> by far the most common IoC mistake is to wrap up the container in a public static or singleton class that is referenced throughout the code base. It is important to realise that this is not dependency injection, it is service location which is widely regarded as an anti-pattern. I cannot over-emphasise how important it is to move away from this design and to inject your dependencies from the root of your application. In fact, virtually all other IoC mistakes come about as a direct result of this misunderstanding.

The same blog post also talks about how to use it properly.


## Other types of registration

Register instance:

	var classInstance = new ClassName();
	Container.RegisterInstance<IClassInterface>(classInstance );


Property injection:

	// We are telling the resolver to resolve that particular property.
	// Otherwise, if it's not part of the constructor, it will not try to resolve it.
	Container.RegisterType<ServiceRepository>(
		new InjectionProperty("ServiceProxy"));



## Next

Pluralsight courses:

Dependency Injection On-Ramp
by Jeremy Clark
https://app.pluralsight.com/library/courses/dependency-injection-on-ramp/table-of-contents

Practical IoC With ASP.NET MVC 4
by John Sonmez
https://app.pluralsight.com/library/courses/ioc-aspdotnet-mvc4/table-of-contents

