BTag
======
Simple data storage in byte file. You can use it or just watch sources. 

# Documentation?
`Tag` class is created for storing tag data in memory. So you can create file in memory and then write to file.

`Parser` - just use `OpenStream` and `Parse` to parse entire file.

`Writer` - use `OpenStream` and `WriteAll` or `WriteAllList` to write to file.

# Limitations
32767 (Int16) bytes tag value length.

255 bytes tag title length.
