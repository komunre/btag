BTag
======
BTag is data storage in binary file C# library (+ server for other languages soon!). It is developed to store your data in tree, using tags children and parents.</p>

You can easiely store your users logins and passwords using tree like this (not a real tree, just indicative).
* users
	* someUser
		* login=someLogin
		* password=somePassword
	* otherUser
		* login=otherLogin
		* password=otherPassword

Every tag end is independent from name, so it's faster to read and easier to change.

### Real tree example

![Real tree example](https://cdn.discordapp.com/attachments/461958492199190530/812280743430455306/tree.png)

Testing phase.

# Example
*Do not forget about using directive!*

### Open stream

```cs
var writer = new Writer();
writer.OpenStream("filename.btag");

var parser = new Parser();
parser.OpenStream("filename.btag");
```

### Write one simple tag

```cs
writer.WriteAll(new Tag("main"));
````


### Add child

```cs
var main = new Tag("main");
main.AddChild(new Tag("child"));
```

or

```cs
var manager = new TagManager();
manager.AddChildToLast(new Tag("first"));
manager.AddChildToLast(new Tag("child"));
```

(Deactivate tag before adding to tag manager if you don't want to add new childes to it using AddChildToLast)

### Write to file

```cs
writer.WriteAll(main);

var tagsList = new List<Tag>(){ main, new Tag("secondMain") };
writer.WriteAllList(tagsList);
```

### Parse file
```cs
parser.Parse();
var main = parser.FindTagLayerRoot("main");
var child = parser.FindTagLayer(main, "child");
```


# Documentation?
`Tag` class is created for storing tag data in memory. So you can create file in memory and then write to file. Contains value variable that can store tag value and `Childes` for accessing children list.

`Parser` - just use `OpenStream` and `Parse` to parse entire file. `FindTagLayerRoot` and `FindTagLayer` can be useful for finding tags on layers.

`Writer` - use `OpenStream` and `WriteAll` or `WriteAllList` to write to file.

`TagManager` - easy tags management. `AddChildToLast` is very useful to save memory. 

# Limitations
32767 (Int16) bytes tag value length.

255 bytes tag title length.

# Future
* Server for easy using with other languages
* TryFind methods (perhaps)
