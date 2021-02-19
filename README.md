BTag
======
Simple data storage in binary file. You can use it or just watch sources. 

Testing phase.

# Documentation?
`Tag` class is created for storing tag data in memory. So you can create file in memory and then write to file. Contains value variable that can store tag value and `Childes` for accessing children list.

`Parser` - just use `OpenStream` and `Parse` to parse entire file. `FindTagLayerRoot` and `FindTagLayer` can be useful for finding tags on layers.

`Writer` - use `OpenStream` and `WriteAll` or `WriteAllList` to write to file.

# Limitations
32767 (Int16) bytes tag value length.

255 bytes tag title length.

# Future
* Server for easy using with other languages
* TryFind methods (perhaps)