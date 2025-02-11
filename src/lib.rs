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

pub struct AddressEntry {
    name: u64,
    address: u64,
}

pub struct AddressList { 
    address_count: u64,
    array: Vec<AddressEntry>
}

pub struct ValueReference {
    address: u64,
}

pub struct TagData<T> {
    tag_id: u64,
    tag_total_size: u64,
    tag_name: u64,
    tag_depth_level: u64,
    tag_parents_size: u64,
    tag_parents: Vec<AddressList>,
    tag_data_type: u8,
    tag_data_size: u64,
    tag_data: Vec<T>
}

impl<T> TagData<T> {
    pub fn new(
        tag_id: u64,
        tag_total_size: u64,
        tag_name: u64,
        tag_depth_level: u64,
        tag_parents_size: u64,
        tag_parents: Vec<AddressList>,
        tag_data_type: u8,
        tag_data_size: u64,
        tag_data: Vec<T>,
    ) -> Self {
        TagData {
            tag_id,
            tag_total_size,
            tag_name,
            tag_depth_level,
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

pub enum DatabaseReadErrorKind {
    ClusterValidity,
    ClusterIncompatibleVersion,
    UnsupportedTextEncoding,
    IndexTableValidity,
    StringValidity,
    IOError,
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

    pub fn read_u8_from_slice(slice: &[u8]) -> u8 {
        u8::from_le_bytes(slice.try_into().unwrap())
    }
    

    pub fn read_cluster(&mut self, cluster_offset: u64) -> Result<ClusterMetadata, DatabaseReadErrorKind> {
        if let Err(_) = self.file_reader.seek(std::io::SeekFrom::Start(cluster_offset)) {
            return Err(DatabaseReadErrorKind::ClusterValidity)
        }
        
        let mut cluster_data: [u8;50] = [0;50];
        if let Err(_) = self.file_reader.read_exact(&mut cluster_data) {
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
        if let Err(_) = self.file_reader.read_exact(&mut table_data) {
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
        if let Err(_) = self.file_reader.seek_relative(self.current_index_table_offset + (index_table.index_table_names_offset as i64)) {
            return Err(DatabaseReadErrorKind::IOError);
        }
        let size = index_table.index_table_names_size;
        
        let mut i: u32 = 0;

        let mut names: Vec<NameIndex> = Vec::new();

        while i < size {
            let mut name_data = [0;8];
            if let Err(_) = self.file_reader.read(&mut name_data) {
                return Err(DatabaseReadErrorKind::IOError);
            }
            let name = DatabaseReader::read_u64_from_slice(&name_data[0..8]);
            let name_string_size = DatabaseReader::read_u16_from_slice(&name_data[8..10]);
            let mut name_string = Vec::with_capacity(name_string_size.into());
            if let Err(_) = self.file_reader.read(&mut name_string) {
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
        if let Err(_) = self.file_reader.seek_relative(self.current_index_table_offset + (index_table.index_table_tags_offset as i64)) {
            return Err(DatabaseReadErrorKind::IOError);
        }
        let size = index_table.index_table_tags_size;

        let mut tags: Vec<TagIndex> = Vec::new();

        let mut i = 0;

        let tags_count = size / 32;

        for i in 0..tags_count {
            let mut buf = [0; 24];
            if let Err(_) = self.file_reader.read_exact(&mut buf) {
                return Err(
                    DatabaseReadErrorKind::IOError
                );
            }
            
            let tag_id = DatabaseReader::read_u64_from_slice(&buf[0..8]);
            let name = DatabaseReader::read_u64_from_slice(&buf[8..16]);
            let depth = DatabaseReader::read_u64_from_slice(&buf[16..24]);

            
            let full_path_size = depth * 8;
            let mut full_path: Vec<u64> = Vec::with_capacity(depth as usize);
            let mut buf = Vec::with_capacity(full_path_size as usize);

            if let Err(_) = self.file_reader.read_exact(&mut buf) {
                return Err(
                    DatabaseReadErrorKind::IOError
                );
            }

            for j in 0..depth {
                let start = (j * 8) as usize;
                let end = (j * 8 + 8) as usize;
                full_path[j as usize] = DatabaseReader::read_u64_from_slice(&buf[start..end])
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
}