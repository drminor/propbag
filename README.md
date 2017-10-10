
# propbag
Type-safe property bag with INotifyPropertyChanged support.

# A Method for creating, at least in part, a WPF View Model in a declarative style.

Views are created in large part using a declarative style thanks to tools like


AngularJs/BootStrap UI and Windows Presentation Foundation, to name a few.

Data Models are specified using a declarative style thanks to Standard Query Language (SQL) 
and their attendant Data Definition Languages (DDLs).

However, there are few facilities that allow one to build a view model from a declaration.

This project started with the simple goal of automating the process of writing the
code necessary to support the INotifyPropertyChanged contract for implementing property
accessors.

As you are probably keenly aware, each setter needs to raise the PropertyChanged event if the 
setter updates the existing value.

Reading the article found here: http://northhorizon.net/2011/the-right-way-to-do-inotifypropertychanged
written by Daniel Moore, made me realize that by providing a standard call back on each property, one
can build the property accessor declarations in a standard way and consistent way for every
property: the custom code that would normally be placed in the getter or setter can now be placed
in event handlers that respond to events that raised in a standard manner by the 
property getter and setter routines.

This project grew to include the following objectives:

1. Provide a mechanism by which a new class, or an existing class be augmented,
	with one or more property accessors at either compile time or at run-time.

2. The definition of these properties should be able to be specified in a declarative
	manner, be able to be written to file, and used later to new up these classes.

3. Include in the definition of these properties the following details:
	. The delegate (i.e. Action) to call when the property changes,
	. The initial value the property should have,
	. And how to determine if two values are the same (so that the setter knows when to raise or not raise the Property Changed event.)

4. Provide different levels of type safety, ranging from allowing new properties to
	be created just by the act of getting or setting a property,
	possibly a property that is not yet defined, to insisting that all access be made
	to properties formerly registered and requiring all access be done in a strict, type-safe manner.


At the heart of the solution provided by this project lies a "Type Safe Property Bag."

This property bag is implemented as a dictionary where each value is the dictionary is accessed
by a string key: the property name, and each value in the dictionary is a class that stores the 
property's value and the attendant (meta) data needed by the property bag to perform the functions mentioned
above.

The Dictionary<TKey,TValue> from the Base Class Library is used, and as you may recall this collection class requires
that all values stored therein must be the same type, namely the type specified in the second generic type parameter: TValue.

Our implementation however requires that each property be assigned a type for its own purposes.
This problem was solved by having the structure used to hold the property information, implement a type-invariant
interface and a second interface that takes a single generic type parameter. This generic type parameter of course
being specified by the type of property for which this property information structure was provisioned.
Now the object can be used by the Dictionary<TKey, TValue> through its type-invariant interface: IPropGen
and through its typed-interface: IProp<T> when access to the property's value is required and type-safety must be enforced.

The ability to access a property's value when the type of that property is only known at run-time is not trivial. If one knows 
the type at compile time, one can create an instance of some class that implements IProp<T> and then easily access the value in a 
type-safe manner. To provide type-safe access when the type is not known a compile-time, this implementation defines 
open delegates for each type of access operation and then creates a closed version of these as needed for each different property type.
The creation of these typed delegates is fairly expensive in terms of CPU usage and so an application domain-wide cache is employed
to avoid creating these unnecessarily. I discovered this technique by reading this article written by Jon Skeet: 
https://codeblog.jonskeet.uk/2008/08/09/making-reflection-fly-and-exploring-delegates


After a few interactions, or sprints, an implementation of a "TypeSafePropertyBag" was created that has more than satisfactory performance
for all but the most data intensive applications and meets each of the objectives outlined above.

For some notes on performance and other considerations, please see the section at the end: "Some notes on performance and other considerations."


I then began to envision how this class may be used to create MVVM-based applications in general. 

A Top-Level VM serves one or more Views.

Those views need one or more
	. Objects accessed via properties
	. List of Objects accessed via an observable collection or a CollectionView
	Note: These objects will often be user defined types (UDTs).
 
The Top-Level VM can be composed (i.e., contain)
	a list of View to ViewModel adapters, or in other words a (mini) ViewModel.

A view should be able to supply information regarding the single objects, the objects that
comprise a collection and the collections themselves including the types of operations
that must be available on these objects.

A application designer should be able to take the Object definitions (stand-alone or in a list)
specified by the view and configure a mapping routine so that data can be "poured" from the data models
available from the database / web services, etc to which the application has access to the view, and then
"poured" back into the source models on its return trip to the data source for updates, additions and deletions.

These mapping routines should be able to be specified in a declarative style.

This project intends to provide services to a Top-Level VM so that the Top-Level VM
can instantiate one or more or View-ViewModel adapters using a combination of 'real' 
ViewModel classes created by the designer and the specifications recorded in 
an XAML resource or regular XML file.

This project also intends to provide services to assist the application designer to 
build the data mapping routines using a interactive GUI.

So far several strategies have been tested. Each involves the following steps:
1. The application designer produces an XAML resource file or an XML file that
	describes identifies the view models that are needed and some information about
	what those view models should contain. (It is envisioned that via the use of Visual Studio
	extensions that this will be fairly straight forward process and one in which the tooling
	will be able to greatly assist.)

2. The application designer creates and 'real' ViewModel class that uses a base class provided
	by this framework. The application designer will include in this class application logic 
	that is outside what this service can provide and will be accessed mainly through
	event handlers. The manner in which these event handlers are wired will be
	specified by the application designer and recorded in the XAML or regular XML file.

3. The 'real' ViewModel classes produced by the application desinger will be augmented using 
	one of the following three strategies:
		. A new class will be created at run-time using System.Reflection.Emit that will
			derive from the 'real' ViewModel class. Remember the 'real' ViewModel class derives
			from a base class provided by this framework. The new class will include real
			property accessors and will allow for natural access using the standard
			MyObject.MyProperty syntax as well as the "regular" PropBag methods for access.

		. Code will be produced using T-4 templates that augment the 'real' class using
			the partial keyword. The resulting class will be created at compile time.

		. Classes provided by this framework will stand in the place of the 'real' ViewModel classes.
			This strategy is using the interceptor pattern: calls normally fielded by the 'real' ViewModel
			class will instead be fielded by the service class and then passed to the 'real' ViewModel class.

The application designer no longer has to write the property declarations and is now able to write
the application logic in routines that are called by the service as determined by the application designer
as she declares what properties should be created and what event handlers should be created for each.


Much of what is described has been implemented.

The third strategy, using the interceptor pattern, has been started, but is not operational.

A custom binder has been written that allows one to specify bindings to objects that use the "raw"
PropBag access methods and this binder supports mult-step property paths where each step can be a standard CLR
object or a PropBag based object.

Support for using Automapper has been completed and works for raw PropBag based objects as well as those replaced by 
an emitted proxy using the first strategy listed above. (And of course T-4 built classes since these are real CLR objects.)

Support for creating and defining AutoMapper configurations and for requesting and cacheing AutoMapper mappers has
also been implemented.

The next steps fall into one of two categories: Performance Improvements and Additional Services.

Performance Improvements:
1. A better Binding implementation.
	I am working on a version of the custom Binding Extension that will avoid boxing operations by performing the
	subscription to PropertyChange events on both the source and target and performing the updates in code, basically
	bypassing the built-in binder.

2. A better AutoMapper implementation.
	The current implementation uses the standard late-bound property invoke style of access. 
	It should be possible to provide an implementation that directly accesses the IPropGen 
	objects, or better yet uses the IProp<T> interface.

Additional Services:

PropModel does a good job of
	. defining property accessor for
		- primitive objects (strings, ints.)
		- user defined POCO structs and classes

1. Need to add
	. Observable Collection
	- Collection View 
	- Collection View Source (XAML)
	- and investigate what additional (meta) data can be recorded to help construct the VM.

2. Need to add the ability to include additional events, perhaps on property access, and perhaps both before
	and after update.

3. Need to add the ability to specify data validation rules as part of the property definitions. This
	probably needs to be done in a separate project so one can use the PropBag implementation and then
	"plug-in" different validation implementations.

3.5 Need to ensure that update loops are not formed if A updates B, and then B updates C, which updates A. 
	The prospect of theses loops greatly increases when validation is being performed.

4. Need to add a GUI-based interactive designer for creating and testing AutoMapper mapping configurations 
	for the mapping between a PropModel and a (data) Model for a particular View-VM adapter.




-------------------------------------------------------------
Some notes on performance and other considerations:

The speed at which properties are accessed and events raised by this implementation are approximately 20% slower
when using the type-safe versions and up to %1000 slower when using methods for which the type must be inferred
than the standard, hard-coded, property accessor declarations with no special event management. A factor of 10 may seem
like a lot, but please realize that the absolute difference between the two when performed 100,000 times is less than 1 second.

At this point you may be asking the question: "Wait a minute, I thought that the DynamicObject class introduced with the
.NET Framework v 4.0 provides a solution for adding properties at run time. Couldn't a facility that reads
a property specification file be used to create properties dynamically using the DynamicObject?"

The DynamicObject does allow one to define properties at run-time, but it does not inheritly provide a 
way to do this in a type-safe manner. The DynamicObject allows one to access the newly created properties
naturally using the primitive dot operator (as in MyObject.MyProperty), it however only allows one to 
specify how this operator is executed and how one goes about binding to the target, it does not however 
provide the object to actually store the target value of this binding. Typical implementations use a Dictionary<TKey, TValue>
where the TValue is simply System.Object and this is does not provide any level of type safety. In order
to provide type-safety, you need something very similar to what this implementation does.

This implementation does not provide the ability to access the property values using the natural dot 
notation. Instead you must use an indexer or method that takes the name of the property and when type-safety
is required the property's type on each get and set access.

This inability of using the normal MyObject.MyProperty notation can be overcome using one of several different 
strategies, one of which is to wrap the PropBag object within a DynamicObject. Other methods entail using emitted
proxies, emitted wrappers, or the use of T-4 templates that create code that gets compiled into a real class.

One may ask: "Why not just use T-4 templates in every case? In fact if code required to declare the properties
and raise the events according to one's specification can be created at either run-time or at compile-time, why
bother with constructing this somewhat complicated solution that then requires additional somewhat complicated
constructs to make access natural?"

That is a fair and good question. My answer is that T-4 templates fall short regarding the following concerns:
1. The process of creating, versioning, and updating T-4 templates is not trivial. Each time you want to update how a class is created
	 you must update the T-4 template or better yet create a new version. It very easy to have different parts
	 of a single large project use classes being created from different versions of the same T-4 template "family."
	 And this in some situations will make identifying sources of program malfunctions more difficult.
	 In contrast using a class library is simpler to version correctly. I'm not saying that versioning T-4 templates
	 is impossible, in fact if done right, they are tied to a dedicated class library and share the exact same
	 versioning as the class library to which they are attached. In real world cases where customers are helping
	 manage the software artifact this is becomes a big "You have to do it right." situation.
2. T-4 templates require a skill to write and debug that is fairly special.
3. T-4 templates don't provide the ability to adjust property definitions at run-time on an individual basis, as the unit of work
	is at the type level, not the property level.















