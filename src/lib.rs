use std::{collections::HashMap, fs::File, io::{BufRead, BufReader, Read, Seek, SeekFrom}};

pub struct ClusterMetadata {
    version: u32,
    cluster_index: u64,
    index_table_offset: u64,
    text_encoding: u16,
    database_size: u64,
    last_name_index: u64,
    next_cluster: u64,
}

impl ClusterMetadata {
    pub fn new(
        version: u32, 
        cluster_index: u64, 
        index_table_offset: u64,
        text_encoding: u16,
        database_size: u64,
        last_name_index: u64,
        next_cluster: u64
    ) -> Self {
        ClusterMetadata {
            version,
            cluster_index,
            index_table_offset,
            text_encoding,
            database_size,
            last_name_index,
            next_cluster
        }
    }
}

pub struct IndexTable {
    index_table_size: u64,
    index_table_names_size: u32,
    index_table_names_offset: u64,
    index_table_tags_size: u32,
    index_table_tags_offset: u64,
    index_table_next_page_offset: u64,
}

impl IndexTable {
    pub fn new(
        index_table_size: u64,
        index_table_names_size: u32,
        index_table_names_offset: u64,
        index_table_tags_size: u32,
        index_table_tags_offset: u64,
        index_table_next_page_offset: u64,
    ) -> Self {
        IndexTable {
            index_table_size,
            index_table_names_size,
            index_table_names_offset,
            index_table_tags_size,
            index_table_tags_offset,
            index_table_next_page_offset,
        }
    }
}

pub struct NameIndex {
    name: u64,
    name_string_size: u16,
    name_string: String,
}

impl NameIndex {
    pub fn new(
        name: u64,
        name_string_size: u16,
        name_string: String,
    ) -> Self {
        NameIndex {
            name,
            name_string_size,
            name_string,
        }
    }
}

pub struct TagIndex {
    tag_id: u64,
    name: u64,
    depth: u64,
    full_path: Vec<u64>,
    offset: u64,
}

impl TagIndex {
    pub fn new(
        tag_id: u64,
        name: u64,
        depth: u64,
        full_path: Vec<u64>,
        offset: u64
    ) -> Self {
        TagIndex {
            tag_id,
            name,
            depth,
            full_path,
            offset,
        }
    }
}

pub struct NamesIndexTable {
    names: Vec<NameIndex>,
}

impl NamesIndexTable {
    pub fn new(names: Vec<NameIndex>) -> Self {
        NamesIndexTable {
            names
        }
    }
}

pub struct TagIndexTable {
    tags: Vec<TagIndex>
}

impl TagIndexTable {
    pub fn new(tags: Vec<TagIndex>) -> Self {
        TagIndexTable {
            tags
        }
    }
}

pub enum TagType {
    AddressEntry(AddressEntry),
    AddressList(AddressList),
    ValueReference(ValueReference),
    Integer(u64),
    Float(f32),
    Double(f64),
    Text(String),
    Char(String),
}

// Represetns tag address by which it can be accessed
// address is equal to offset in byte stream from index table]
// Reference docs/specification.md for further information.
#[derive(Clone, Copy)]
pub struct AddressEntry {
    name: u64,
    address: u64,
}

impl PartialEq for AddressEntry {
    fn eq(&self, other: &Self) -> bool {
        return self.name == other.name && self.address == other.address;
    }
}

#[derive(Clone)]
pub struct AddressList { 
    address_count: u64,
    array: Vec<AddressEntry>
}

impl PartialEq for AddressList {
    fn eq(&self, other: &Self) -> bool {
        for i in self.array.iter().zip(&other.array) {
            if *i.0 != *i.1 {
                return false;
            }
        }
        return self.address_count == other.address_count;
    }
}

// Used to reference a specific point in byte stream, where value is stored.
// May reference any type, not just tags.
// Reference docs/specification.md for further information.
#[derive(Clone, Copy)]
pub struct ValueReference {
    address: u64,
}

pub struct TagData<T> {
    tag_id: u64,
    tag_total_size: u64,
    tag_name: u64,
    tag_depth: u64,
    tag_parents_size: u64,
    tag_parents: AddressList,
    tag_data_type: u8,
    tag_data_size: u64,
    tag_data: T
}

impl<T> TagData<T> {
    pub fn new(
        tag_id: u64,
        tag_total_size: u64,
        tag_name: u64,
        tag_depth: u64,
        tag_parents_size: u64,
        tag_parents: AddressList,
        tag_data_type: u8,
        tag_data_size: u64,
        tag_data: T,
    ) -> Self {
        TagData {
            tag_id,
            tag_total_size,
            tag_name,
            tag_depth,
            tag_parents_size,
            tag_parents,
            tag_data_type,
            tag_data_size,
            tag_data,
        }
    }
}

pub struct DataIndexTable {
    tags: Vec<TagIndex>
}

pub struct BTag {
    readers: Vec<DatabaseReader>,
    clusters: Vec<ClusterMetadata>,
    name_index_tables: HashMap<u64, NamesIndexTable>,
    tag_index_tables: HashMap<u64, TagIndexTable>,
}

pub struct DatabaseReader {
    database_file: File,
    file_reader: BufReader<File>,
    current_index_table_offset: i64,
}

#[derive(Debug)]
pub enum DatabaseReadErrorKind {
    ClusterValidity,
    ClusterIncompatibleVersion,
    UnsupportedTextEncoding,
    IndexTableValidity,
    StringValidity,
    IOError,
}

pub enum QueryEntry {
    Name(u64),
    ArrayIndex(u64),
    Id(u64),
    Conditional(Box<dyn Fn(TagData<TagType>) -> bool>),
    UpstreamConditional(Box<dyn Fn(TagData<TagType>) -> bool>)
}

pub enum SearchResult {
    Found(Vec<AddressList>),
    Match(Vec<AddressEntry>),
    None
}

impl DatabaseReader {
    pub fn read_u64_from_slice(slice: &[u8]) -> u64 {
        u64::from_le_bytes(slice.try_into().unwrap())
    }

    pub fn read_u32_from_slice(slice: &[u8]) -> u32 {
        u32::from_le_bytes(slice.try_into().unwrap())
    }

    pub fn read_u16_from_slice(slice: &[u8]) -> u16 {
        u16::from_le_bytes(slice.try_into().unwrap())
    }

    pub fn read_f64_from_slice(slice: &[u8]) -> f64 {
        f64::from_le_bytes(slice.try_into().unwrap())
    }

    pub fn read_f32_from_slice(slice: &[u8]) -> f32 {
        f32::from_le_bytes(slice.try_into().unwrap())
    }

    // Redundant
    /*pub fn read_u8_from_slice(slice: &[u8]) -> u8 {
        u8::from_le_bytes(slice.try_into().unwrap())
    }*/

    pub fn seek(&mut self, offset: i64) -> Result<(), DatabaseReadErrorKind> {
        if let Err(_) = self.file_reader.seek_relative(offset) {
            return Err(DatabaseReadErrorKind::IOError);
        }
        self.current_index_table_offset -= offset;
        Ok(())
    }

    pub fn read_to_buf(&mut self, buf: &mut [u8]) -> Result<(), std::io::Error> {
        let r = self.file_reader.read_exact(buf);
        self.advance_seeker(buf.len().try_into().unwrap());
        r
    }

    pub fn advance_seeker(&mut self, advance: i64) {
        self.current_index_table_offset -= advance;
    }
    

    pub fn read_cluster(&mut self, cluster_offset: u64) -> Result<ClusterMetadata, DatabaseReadErrorKind> {
        if let Err(_) = self.file_reader.seek(std::io::SeekFrom::Start(cluster_offset)) {
            return Err(DatabaseReadErrorKind::ClusterValidity)
        }
        
        let mut cluster_data: [u8;50] = [0;50];
        if let Err(_) = self.read_to_buf(&mut cluster_data) {
            return Err(DatabaseReadErrorKind::ClusterValidity)
        }
        let validity = match String::from_utf8(cluster_data[0..4].to_vec()) { // 0-3
            Ok (v) => v,
            Err(_) => return Err(DatabaseReadErrorKind::ClusterValidity)
        };
        if validity != "BTAG" {
            return Err(DatabaseReadErrorKind::ClusterValidity)
        }
        let version = DatabaseReader::read_u32_from_slice(&cluster_data[4..8]); // 4-8
        let cluster_index = DatabaseReader::read_u64_from_slice(&cluster_data[8..16]);
        let index_table_offset = DatabaseReader::read_u64_from_slice(&cluster_data[16..24]);
        let text_encoding = DatabaseReader::read_u16_from_slice(&cluster_data[24..26]);
        let database_size = DatabaseReader::read_u64_from_slice(&cluster_data[26..34]);
        let last_name_index = DatabaseReader::read_u64_from_slice(&cluster_data[34..42]);
        let next_cluster = DatabaseReader::read_u64_from_slice(&cluster_data[42..50]);
        
        Ok(ClusterMetadata {
            version,
            cluster_index,
            index_table_offset,
            text_encoding,
            database_size,
            last_name_index,
            next_cluster
        })
    }

    pub fn read_index_table(&mut self, index_table_offset: u64) -> Result<IndexTable, DatabaseReadErrorKind> {
        if let Err(_) = self.file_reader.seek(SeekFrom::Start(index_table_offset)) {
            return Err(DatabaseReadErrorKind::IndexTableValidity)
        }

        let mut table_data: [u8; 40] = [0;40];
        if let Err(_) = self.read_to_buf(&mut table_data) {
            return Err(DatabaseReadErrorKind::IndexTableValidity)
        }
        let index_table_size = DatabaseReader::read_u64_from_slice(&table_data[0..8]);
        let index_table_names_size = DatabaseReader::read_u32_from_slice(&table_data[8..12]);
        let index_table_names_offset = DatabaseReader::read_u64_from_slice(&table_data[12..20]);
        let index_table_tags_size = DatabaseReader::read_u32_from_slice(&table_data[20..24]);
        let index_table_tags_offset = DatabaseReader::read_u64_from_slice(&table_data[24..32]);
        let index_table_next_page_offset = DatabaseReader::read_u64_from_slice(&table_data[32..40]);

        self.current_index_table_offset = 0;

        Ok(IndexTable {
            index_table_size,
            index_table_names_size,
            index_table_names_offset,
            index_table_tags_size,
            index_table_tags_offset,
            index_table_next_page_offset,
        })
    }

    pub fn read_names_index(&mut self, index_table: IndexTable) -> Result<NamesIndexTable, DatabaseReadErrorKind> {
        self.seek(self.current_index_table_offset + <u64 as TryInto<i64>>::try_into(index_table.index_table_names_offset).unwrap())?;
        let size = index_table.index_table_names_size;
        
        let mut i: u32 = 0;

        let mut names: Vec<NameIndex> = Vec::new();

        while i < size {
            let mut name_data = [0;8];
            if let Err(_) = self.read_to_buf(&mut name_data) {
                return Err(DatabaseReadErrorKind::IOError);
            }
            let name = DatabaseReader::read_u64_from_slice(&name_data[0..8]);
            let name_string_size = DatabaseReader::read_u16_from_slice(&name_data[8..10]);
            let mut name_string = Vec::with_capacity(name_string_size.into());
            if let Err(_) = self.read_to_buf(&mut name_string) {
                return Err(DatabaseReadErrorKind::IOError);
            }

            let s = match String::from_utf8(name_string) {
                Ok(v) => v,
                Err(_) => return Err(DatabaseReadErrorKind::StringValidity),
            };

            names.push(
                NameIndex {
                    name,
                    name_string_size,
                    name_string: s,
                }
            );

            i += 8 + u32::from(name_string_size);
        }

        Ok(NamesIndexTable {
            names
        })
    }

    pub fn read_tags_index(&mut self, index_table: IndexTable) -> Result<DataIndexTable, DatabaseReadErrorKind> {
        self.seek(self.current_index_table_offset + <u64 as TryInto<i64>>::try_into(index_table.index_table_tags_offset).unwrap())?;
        let size = index_table.index_table_tags_size;

        let mut tags: Vec<TagIndex> = Vec::new();

        let tags_count = size / 32;

        for _ in 0..tags_count {
            let mut buf = [0; 24];
            if let Err(_) = self.read_to_buf(&mut buf) {
                return Err(
                    DatabaseReadErrorKind::IOError
                );
            }
            
            let tag_id = DatabaseReader::read_u64_from_slice(&buf[0..8]);
            let name = DatabaseReader::read_u64_from_slice(&buf[8..16]);
            let depth = DatabaseReader::read_u64_from_slice(&buf[16..24]);

            
            let full_path_size = depth * 8;
            let mut full_path: Vec<u64> = Vec::with_capacity(depth.try_into().unwrap());
            let mut buf = Vec::with_capacity(full_path_size.try_into().unwrap());

            if let Err(_) = self.read_to_buf(&mut buf) {
                return Err(
                    DatabaseReadErrorKind::IOError
                );
            }

            for j in 0..depth {
                let start = (j * 8).try_into().unwrap();
                let end = (j * 8 + 8).try_into().unwrap();
                full_path[<u64 as TryInto<usize>>::try_into(j).unwrap()] = DatabaseReader::read_u64_from_slice(&buf[start..end])
            }
            let offset = DatabaseReader::read_u64_from_slice(&buf[24..32]);

            tags.push(
                TagIndex {
                    tag_id,
                    name,
                    depth,
                    full_path,
                    offset
                }
            );
        }

        Ok(DataIndexTable {
            tags
        })
    }

    pub fn read_tag_data(&mut self, offset: u64) -> Result<TagData<TagType>, DatabaseReadErrorKind> {
        self.seek(self.current_index_table_offset + <u64 as TryInto<i64>>::try_into(offset).unwrap())?;

        // read basic data
        let mut buf = [0;40];
        if let Err(_) = self.read_to_buf(&mut buf) {
            return Err(DatabaseReadErrorKind::IOError);
        }

        let tag_id = DatabaseReader::read_u64_from_slice(&buf[0..8]);
        let tag_total_size = DatabaseReader::read_u64_from_slice(&buf[8..16]);
        let tag_name = DatabaseReader::read_u64_from_slice(&buf[16..24]);
        let tag_depth = DatabaseReader::read_u64_from_slice(&buf[24..32]);
        let tag_parents_size = DatabaseReader::read_u64_from_slice(&buf[32..40]);

        // skip parents
        self.seek(tag_parents_size.try_into().unwrap())?;
        
        // read leftovers
        let mut buf = [0;9];
        if let Err(_) = self.read_to_buf(&mut buf) {
            return Err(DatabaseReadErrorKind::IOError);
        }

        let tag_data_type = buf[0];
        let tag_data_size = DatabaseReader::read_u64_from_slice(&buf[1..9]);

        let mut buf = Vec::with_capacity(tag_data_size.try_into().unwrap());
        if let Err(_) = self.read_to_buf(&mut buf) {
            return Err(DatabaseReadErrorKind::IOError);
        }

        let tag_data = match tag_data_type {
            0 => TagType::Integer(DatabaseReader::read_u64_from_slice(&buf[0..8])),
            1 => TagType::Float(DatabaseReader::read_f32_from_slice(&buf[0..4])),
            2 => TagType::Double(DatabaseReader::read_f64_from_slice(&buf[0..8])),
            3 => TagType::AddressEntry(AddressEntry { name: DatabaseReader::read_u64_from_slice(&buf[0..8]), address: DatabaseReader::read_u64_from_slice(&buf[8..16]) }),
            4 => {
                let count = DatabaseReader::read_u64_from_slice(&buf[0..8]);
                //let size = count * 16;

                let mut entries = Vec::<AddressEntry>::with_capacity(count.try_into().unwrap());

                for i in 0..count {
                    let start = (i * 16).try_into().unwrap();
                    let end = start + 16;
                    let name = DatabaseReader::read_u64_from_slice(&buf[start..end]);
                    
                    let start = end;
                    let end = start + 16;
                    let address = DatabaseReader::read_u64_from_slice(&buf[start..end]);

                    entries.push(AddressEntry {
                        name,
                        address
                    });
                }

                TagType::AddressList(AddressList {
                    address_count: count,
                    array: entries
                })
            }
            5 => TagType::Text(String::from_utf8(buf[0..tag_data_size.try_into().unwrap()].to_vec()).unwrap()),
            _ => todo!()
        };

        Ok(TagData::<TagType> {
            tag_id,
            tag_total_size,
            tag_name,
            tag_depth,
            tag_parents_size,
            tag_parents: AddressList {
                address_count: tag_depth,
                array: Vec::with_capacity(tag_depth.try_into().unwrap())
            },
            tag_data_type,
            tag_data_size,
            tag_data
        })
    }

    pub fn read_parents(&mut self, tag_index: TagIndex, tag_data: &mut TagData<TagType>) -> Result<(), DatabaseReadErrorKind> {
        self.seek(self.current_index_table_offset + <u64 as TryInto<i64>>::try_into(tag_index.offset + 41).unwrap())?;

        let parent_count = tag_data.tag_depth; // Not needed, it's here just for semantics.
        for _ in 0..parent_count {
            let mut buf = [0;16];
            if let Err(_) = self.read_to_buf(&mut buf) { 
                return Err(DatabaseReadErrorKind::IOError);
            }

            tag_data.tag_parents.array.push(
                AddressEntry {
                    name: DatabaseReader::read_u64_from_slice(&buf[0..8]),
                    address: DatabaseReader::read_u64_from_slice(&buf[8..16])
                }
            )
        }

        Ok(())
    }

    pub fn find_upstream(&mut self, query: &Vec<QueryEntry>, offset: u64, tag_data: &TagData<TagType>) -> Result<SearchResult, DatabaseReadErrorKind> { // Return all upstream matches in form of AddressList, representing full sequence of search
        let result = self.recursive_upstream_search(query, 0, Vec::new(), offset, tag_data);

        return result;
    }

    fn recursive_upstream_search(&mut self, query: &Vec<QueryEntry>, query_index: i32, mut hierarchy: Vec<AddressEntry>, offset: u64, tag_data: &TagData<TagType>) -> Result<SearchResult, DatabaseReadErrorKind> {
        self.seek(self.current_index_table_offset + <u64 as TryInto<i64>>::try_into(offset + 41).unwrap())?;
        
        let mut valid_search_paths: Vec<(AddressEntry, i32)> = Vec::new();
        
        let parent_count = tag_data.tag_parents_size / 16;
        if query_index == query.iter().count().try_into().unwrap() {
            // Query has ended. We found an entire path, therefore it's a Match.
        }
        let q = &query[<i32 as TryInto<usize>>::try_into(query_index).unwrap()];

        let mut matches: Vec<AddressList> = Vec::new();

        // Check every parent to find those that match the condition 
        for i in 0..parent_count {
            let next_index = query_index + 1;

            let mut buf = [0;16];
            if let Err(_) = self.read_to_buf(&mut buf) { 
                return Err(DatabaseReadErrorKind::IOError);
            }

            let entry = AddressEntry {
                name: DatabaseReader::read_u64_from_slice(&buf[0..8]),
                address: DatabaseReader::read_u64_from_slice(&buf[8..16])
            };

            match q {
                QueryEntry::Id(id) => {
                    let tag_data = self.read_tag_data(entry.address);
                    if let Ok(tag_data) = tag_data {
                        if tag_data.tag_id == *id {
                            valid_search_paths.push((entry, next_index));
                        }
                    }
                }

                QueryEntry::Name(name) => {
                    if *name == entry.name {
                        valid_search_paths.push((entry, next_index));
                    }
                }

                QueryEntry::ArrayIndex(index) => {
                    if *index == i {
                        valid_search_paths.push((entry, next_index));
                    }
                }

                QueryEntry::Conditional(ref predicate) => {
                    match self.read_tag_data(entry.address) {
                        Ok(data) => {
                            if predicate(data) {
                                valid_search_paths.push((entry, next_index));
                            }
                        }
                        Err(_) => {
                            continue;
                        }
                    }
                }

                QueryEntry::UpstreamConditional(ref predicate) => {
                    match self.read_tag_data(entry.address) {
                        Ok(data) => {
                            if predicate(data) {
                                valid_search_paths.push((entry, next_index));
                            }
                            else {
                                valid_search_paths.push((entry, next_index - 1));
                            }
                        }
                        Err(_) => {
                            continue;
                        }
                    }
                }
            }
        }

        // Recursively run on every parent, to either find a Match or None, later returning Found that contains all results of Match
        for parent in valid_search_paths {
            hierarchy.push(parent.0);
            let tag_data = match self.read_tag_data(parent.0.address) {
                Ok(v) => v,
                Err(_) => {
                    continue;
                }
            };
            let r = self.recursive_upstream_search(query, parent.1, hierarchy.clone(), parent.0.address, &tag_data);
            if let Ok(r) = r {
                match r {
                    SearchResult::Match(m) => {
                        matches.push(AddressList {
                            address_count: m.iter().count().try_into().unwrap(),
                            array: m,
                        })
                    }
                    SearchResult::Found(list) => { // Potentially significant performance impact. We should consider better implementations.
                        let mut list = list.iter().take_while(|x| !matches.contains(x)).map(|x| x.clone()).collect();
                        matches.append(&mut list);
                        return Ok(SearchResult::Found(matches));
                    }
                    SearchResult::None => {
                        continue;
                    }
                }
            }
            hierarchy.pop();
        }

        if matches.is_empty() {
            return Ok(SearchResult::None);
        }
        // Recursively return all matches
        return Ok(SearchResult::Found(matches))
    }
}