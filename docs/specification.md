All sizes are in bytes. All lengths are factual.

Each database file / cluster is independent from any other. Each database file / cluster can be processed in it's own thread without conflicts with other files / clusters. This allows for easy multithreading. 

Every cluster metadata must be loaded before database is ready to use. This is neccessary to determine biggest cluster index for further increments.

## Database cluster metadata
BTAG (constant ASCII string) version(u32) cluster_index(u64) index_table_offset(u64) text_encoding(u16) database_size(u64) last_name_index(u64) names_index_padding(u32) data_index_padding(u32) tag_data_padding(u32) next_cluster(u64)

if next_cluster is equal to 0x00 - it is treated as non-existent, i.e. entire file is a single cluster.

last_name_index(u64) increments with every new name


## Tags Index Table Page
Every offset is calculated from index table page start, i.e. first byte of index_table_size.

index_table_size(u64)
index_table_names_size(u32)
index_table_names_offset(u64)
index_table_tags_size(u32)
index_table_tags_offset(u64)
index_table_next_page_offset(u64)
**[SECTION_INDEX_TABLE_NAMES]**
**[SECTION_INDEX_TABLE_TAGS]**

## Names index table [SECTON_INDEX_TABLE_NAMES]
name(u64) **[must be unique]**
name_string_size(u16)
name_string(name_string_size)
<names_index_padding PADDING>
<NEXT_ENTRY>

## Data index table 	[SECTION_INDEX_TABLE_TAGS]
tag_id(u64)
name(u64)
depth(u64)
full_path( [u64; depth+1] ) // TODO: Redefine to accomodate multiple parents  // For example: users.jason.wallet, where users is 0, jason is 9 and wallet is 2 - full_path equals to [0, 9, 2], allowing to quickly determine whether tag matches path requirements
offset(u64) **(from index table start)**
<data_index_padding PADDING>
<NEXT_ENTRY>

* * *

## Tag data
tag_id(u64)
tag_total_size(u64)
tag_name(u64)
tag_depth_level(u64)
tag_parents_size(u64)
tag_parents(tag_parents_size)
tag_data_type(u8) 
tag_data_size(u64)
[tag_data(tag_data_size). AddressList.]
<tag_data_padding PADDING>
<NEXT_ENTRY>

# Data types
- Integer
- Float
- Double
- Address
- AddressList
- Text
- Char[]

#### AddressList definition
address_count(u64)
array [(name(u64), address(u64)); address_count]

*AddressList can be addressed by entry name `.<entry_name>` or by index via `.<number>`
`mylist.myentry`
`mylist.0`*

#### Integer definition
uint64_t

#### Float definition
f32

#### Double definition
f64

#### Text definition
text_size(u16)
text(text_size)

#### Char[] definition
char_size(u8)
text(char_size)

# Commands
Querying AddressList without array index at any point in full query must process all AddressList entries and return all matches in processed entries. I.E.
`wallet.euro..0..%name` must return name of every parent of `wallet`, but only needs to process first parent of `euro`.
`wallet.euro..0..0.%name` must return a name of only first parent of `wallet`.

### Load
PRELOAD NAMES - load all name indexes

### Generic
`:<num>` get nth match for query
`..` query parent AddressLisst
`..$` query parent with conditional
`...$` query every node upstream (parents of parents) that meets conditional
`.-` backtrace query


#### Properties
`%depth`
`%name`


* * *
### Data update

#### To [VARIABLE_TYPE] <value\>
Define value for tag field update
#### **EXAMPLE**
`Set "euro" of TAG_ID 87 To FLOAT 12.0`

* * *

#### Set 
Set tag value
**Syntax**
`target = value`
`#entry_id = value`
`#entry_id = &#entry_reference`

#### **EXAMPLES**
`Joey.wallet.euro = 1200` Set all entries of `Joey.wallet.euro` to 1200
`( Joey.wallet.euro ):0 = 1200` Set first entry of `Joey.wallet.euro` to 1200
`( wallet.euro ):0.borrowers.*.amount = 13` Get first entry of `wallet.euro`, set every entry of `borrowers.*.amount` in `borrowers` AddressList in it to 13
`( wallet.euro ):0.borrowers.1.amount = 13` Get first entry of `wallet.euro`, get second entry in `borrowers` AddressList and set it's `amount` entry to 13
`wallet.euro = 1200` Set all entries of `wallet.euro` to 1200
`#87.#97 = 1200` Set entry #97 child of #87 to value of 1200
`#97 = 1200` Set entry #97 to value of 1200
`#97 = #16` Make entry #97 equal to value of entry #16
`#111 = &#87` Make entry #111 reference entry #87 (Address type)

### Return changed
Returns all tags that have been changed by the query
`$%=%`

#### **EXAMPLES**
`(Joey.wallet.euro = 1200)$%=%` sets `Joey.wallet.euro` to 1200 and returns `Joey.wallet.euro` (with commited changes)

### Complex query concepts

#### Request conditional name matches with many name duplicates, yet different hierarchy

Imagine theoretical database structure of
```
a.b.c
x.b.->c
y.a.b.->c
```
We want to find `a.b` and `y.a.b` that have value or reference of value `c`, while ignoring `x.b`. There are multiple approaches to this problem.

1. Depthless query
---
`a.b.c...$%name=b&(..%name=a)`

This query finds all matches of `a.b.c`, then performs upstream search to find the node with a name of `b` that has parent with a name of `a`.
This query will match both `a.b` and `y.a.b`.

2. Depth query
---
`a.b.c..$%name=b&depth=1..$%name=a.b; a.b.c..$%name=b&depth=2..$%name=a.b`

This is two queries combined into one.

First query will find all matches of `a.b.c`, then find parents with a name of `b` and depth of `1`, which in this case can only match `a.b` and not `y.a.b`, then find parents of these parents with a name of `a`, and request child `b` from these parents. Full query in this database can match only `a.b`.

Second query will do exactly the same, except `a.b.c` parent of `b` must have a depth of `2`, which only matches `y.a.b` and not `a.b.`


#### Request node that has all children present
Imagine theoretical database structure of
```
numbers.
    1
    2
    3
    4
odd_digits.->
    1
    3
even_digits.->
    2
    4
ints.->
    1
    2
    3
    4
```

We want to find `numbers` but ignore `odd_digits` and `even_digits`.

`(1..&2..&3..&4..)$peq`

This query will perform 4 queries, finding parent of every digit, then compare it using conditional of `$peq` (partially equal), filtering only for results that are present in all queries, yielding `numbers` and `ints`.


* * *
### Query

#### Get
query tag value

#### **EXAMPLE**
`wallet.euro`
`Joey.wallet.euro`
`wallet.euro@0`
`*:has(.wallet.euro).age` 

* * *

# Journaling
1. Determine entries to modify
2. Backup all entries that are a subject to modification
3. Log query
4. Commit