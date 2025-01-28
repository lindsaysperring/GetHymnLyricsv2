using System.Collections.Generic;
using System.Xml.Serialization;

namespace GetHymnLyricsv2.Models
{
    public class Field
    {
        [XmlAttribute("attrname")]
        public string AttrName { get; set; } = string.Empty;

        [XmlAttribute("fieldtype")]
        public string FieldType { get; set; } = string.Empty;

        [XmlAttribute("WIDTH")]
        public string? Width { get; set; }

        [XmlAttribute("SUBTYPE")]
        public string? SubType { get; set; }

        [XmlElement("FIELDS")]
        public Fields? Fields { get; set; }

        [XmlElement("PARAMS")]
        public Params? Params { get; set; }
    }

    public class Fields
    {
        [XmlElement("FIELD")]
        public List<Field> Items { get; set; } = new();
    }

    public class Params
    {
    }

    public class Metadata
    {
        [XmlElement("FIELDS")]
        public Fields Fields { get; set; } = new();

        [XmlElement("PARAMS")]
        public Params Params { get; set; } = new();
    }

    [XmlRoot("DATAPACKET")]
    public class DataPacket
    {
        public DataPacket()
        {
            Metadata = new Metadata
            {
                Fields = new Fields { Items = new List<Field>() },
                Params = new Params()
            };
            RowData = new RowData
            {
                Row = new Row
                {
                    Folders = new Folders { Items = new List<Folder>() },
                    Songs = new Songs { Items = new List<Song>() },
                    SongSections = new SongSections { Items = new List<SongSection>() },
                    SongSectionOrder = new SongSectionOrder { Items = new List<SectionOrder>() }
                }
            };
        }

        [XmlAttribute("Version")]
        public string Version { get; set; } = "2.0";

        [XmlElement("METADATA")]
        public required Metadata Metadata { get; set; }

        [XmlElement("ROWDATA")]
        public required RowData RowData { get; set; }
    }

    public class RowData
    {
        [XmlElement("ROW")]
        public required Row Row { get; set; }
    }

    public class Row
    {
        [XmlElement("FOLDERS")]
        public required Folders Folders { get; set; }

        [XmlElement("SONGS")]
        public required Songs Songs { get; set; }

        [XmlElement("SONG_SECTIONS")]
        public required SongSections SongSections { get; set; }

        [XmlElement("SONG_SECTION_ORDER")]
        public required SongSectionOrder SongSectionOrder { get; set; }
    }

    public class Folders
    {
        [XmlElement("ROWFOLDERS")]
        public required List<Folder> Items { get; set; }
    }

    public class Folder
    {
        [XmlAttribute("FOLDER_ID")]
        public int FolderId { get; set; }

        [XmlAttribute("FOLDER_NAME")]
        public string FolderName { get; set; } = string.Empty;

        [XmlAttribute("FOLDER_TYPE")]
        public string FolderType { get; set; } = string.Empty;

        [XmlAttribute("FOLDER_INFO")]
        public string FolderInfo { get; set; } = string.Empty;

        [XmlAttribute("VERSION_NO")]
        public string? VersionNo { get; set; }
    }

    public class Songs
    {
        [XmlElement("ROWSONGS")]
        public required List<Song> Items { get; set; }
    }

    public class Song
    {
        [XmlAttribute("SONG_ID")]
        public int SongId { get; set; }

        [XmlAttribute("FOLDER_ID")]
        public int FolderId { get; set; }

        [XmlAttribute("SONG_TITLE")]
        public string Title { get; set; } = string.Empty;

        [XmlAttribute("SONG_NUMBER")]
        public int Number { get; set; }

        [XmlAttribute("SONG_COMMENTS")]
        public string Comments { get; set; } = string.Empty;

        [XmlAttribute("WORDS_AUTHOR")]
        public string WordsAuthor { get; set; } = string.Empty;

        [XmlIgnore]
        public bool WordsPublicDomain { get; set; }

        [XmlAttribute("WORDS_PUBLIC_DOMAIN")]
        public string WordsPublicDomainString
        {
            get => WordsPublicDomain ? "Y" : "N";
            set => WordsPublicDomain = value == "Y";
        }

        [XmlAttribute("WORDS_COPYRIGHT_INFO")]
        public string WordsCopyrightInfo { get; set; }

        [XmlIgnore]
        public bool WordsLicenseCovered { get; set; }

        [XmlAttribute("WORDS_LICENSE_COVERED")]
        public string WordsLicenseCoveredString
        {
            get => WordsLicenseCovered ? "Y" : "N";
            set => WordsLicenseCovered = value == "Y";
        }

        [XmlAttribute("WORDS_COPYRIGHT_CODE")]
        public int WordsCopyrightCode { get; set; }

        [XmlAttribute("MUSIC_AUTHOR")]
        public string MusicAuthor { get; set; } = string.Empty;

        [XmlIgnore]
        public bool MusicPublicDomain { get; set; }

        [XmlAttribute("MUSIC_PUBLIC_DOMAIN")]
        public string MusicPublicDomainString
        {
            get => MusicPublicDomain ? "Y" : "N";
            set => MusicPublicDomain = value == "Y";
        }

        [XmlAttribute("MUSIC_COPYRIGHT_INFO")]
        public string MusicCopyrightInfo { get; set; }

        [XmlIgnore]
        public bool MusicLicenseCovered { get; set; }

        [XmlAttribute("MUSIC_LICENSE_COVERED")]
        public string MusicLicenseCoveredString
        {
            get => MusicLicenseCovered ? "Y" : "N";
            set => MusicLicenseCovered = value == "Y";
        }

        [XmlAttribute("MUSIC_COPYRIGHT_CODE")]
        public int MusicCopyrightCode { get; set; }

        [XmlAttribute("EXT_SONG_CODE")]
        public string ExtSongCode { get; set; } = string.Empty;
    }

    public class SongSections
    {
        [XmlElement("ROWSONG_SECTIONS")]
        public required List<SongSection> Items { get; set; }
    }

    public class SongSection
    {
        [XmlAttribute("SONG_ID")]
        public int SongId { get; set; }

        [XmlAttribute("SECTION_ID")]
        public int SectionId { get; set; }

        [XmlAttribute("SECTION_NAME")]
        public string SectionName { get; set; } = string.Empty;

        [XmlAttribute("SECTION_TEXT")]
        public string SectionText { get; set; } = string.Empty;

        [XmlAttribute("SECTION_COMMENTS")]
        public string SectionComments { get; set; } = string.Empty;
    }

    public class SongSectionOrder
    {
        [XmlElement("ROWSONG_SECTION_ORDER")]
        public required List<SectionOrder> Items { get; set; }
    }

    public class SectionOrder
    {
        [XmlAttribute("SONG_ID")]
        public int SongId { get; set; }

        [XmlAttribute("ORDER")]
        public int Order { get; set; }

        [XmlAttribute("SECTION_ID")]
        public int SectionId { get; set; }
    }
}
