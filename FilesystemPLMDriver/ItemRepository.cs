using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using FilesystemPLMDriver.Models;
namespace FilesystemPLMDriver
{
    public class ItemRepository(string basePath)
    {
        private const string ItemsFolderName = "items";
        private const string ChangesFolderName = "changes";
        private const string ItemXml = "item.xml";
        private const string ChangeXml = "change.xml";
        private const string Sep = "-";

        private readonly XmlSerializer serializer = new(typeof(ItemDto));
        private readonly XmlSerializer basicSerializer = new(typeof(ObjectDto));

        public string Store(ObjectDto dto)
        {
            var change = dto is not ItemDto;
            PopulateIdIfMissing(dto, change);
            var targetDir = GetTargetDir(dto.Id, change);

            StoreAttachments(dto, targetDir);

            var writer = new StreamWriter(Path.Combine(targetDir, change ? ChangeXml : ItemXml));
            var xml = change ? basicSerializer : serializer;
            xml.Serialize(writer, dto);
            writer.Close();

            return dto.Id.AlternateId;
        }

        public void DeleteItem(IdDto id, bool isChange)
        {
            var targetDir = GetTargetDir(id, isChange);
            Directory.Delete(targetDir, true);
        }

        public ObjectDto Load(string plmId, bool changes)
        {
            var folder = FindFolderByPlmId(plmId, changes);
            if (folder is null)
            {
                return null;
            }

            var itemXml = Path.Combine(folder, changes ? ChangeXml : ItemXml);
            if (!File.Exists(itemXml))
            {
                return null;
            }

            var reader = new StreamReader(itemXml);
            var xml = changes ? basicSerializer : serializer;
            var itemDto = (ObjectDto)xml.Deserialize(reader);
            reader.Close();

            return itemDto;
        }

        public IEnumerable<ItemDto> LoadAllItems()
        {
            var output = new List<ItemDto>();
            var itemsDir = GetDataDir(false);
            if (!Directory.Exists(itemsDir))
            {
                return output;
            }

            var directoryInfo = new DirectoryInfo(itemsDir);
            var files = directoryInfo.GetDirectories().ToList();

            foreach (var itemDir in files)
            {
                var itemXml = Path.Combine(itemDir.FullName, ItemXml);
                if (File.Exists(itemXml))
                {
                    var reader = new StreamReader(itemXml);
                    var itemDto = (ItemDto)serializer.Deserialize(reader);
                    output.Add(itemDto);
                    reader.Close();
                }
            }
            return output;
        }

        private void PopulateIdIfMissing(ObjectDto dto, bool changes)
        {
            var id = dto.Id;

            if (string.IsNullOrEmpty(id.AlternateId))
            {
                id.AlternateId = GenerateAlternateId(changes);
            }

            if (string.IsNullOrEmpty(id.Id))
            {
                id.Id = Guid.NewGuid().ToString();
            }
        }

        private string GenerateAlternateId(bool changes)
        {
            var itemsDir = GetDataDir(changes);
            CreateDirectory(itemsDir);

            var directoryInfo = new DirectoryInfo(itemsDir);
            var files = directoryInfo.GetDirectories().Select(file => file.Name).ToList();

            for (var i = 0; ; i++)
            {
                var plmId = changes ? $"PLM-FS-ECO-{i}" : $"PLM-FS-{i}";

                if (!files.Exists(s => s.StartsWith(plmId)))
                {
                    return plmId;
                }
            }
        }

        private string GetTargetDir(IdDto id, bool changes)
        {
            var itemsDir = GetDataDir(changes);
            var dirName = ReplaceInvalidChars(id.AlternateId);
            var targetDir = Path.Combine(itemsDir, dirName);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            return targetDir;
        }

        private string FindFolderByPlmId(string plmId, bool changes)
        {
            var dataDir = GetDataDir(changes);
            if (!Directory.Exists(dataDir))
            {
                return null;
            }

            var dirNamePrefix = ReplaceInvalidChars(plmId);
            var directoryInfo = new DirectoryInfo(dataDir);
            var files = directoryInfo.GetDirectories().ToList();

            var index = files.FindIndex(file => file.Name.StartsWith(dirNamePrefix));
            return index == -1 ? null : files[index].FullName;
        }

        private static void CreateDirectory(string targetDir)
        {
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir!);
            }
        }

        private string GetDataDir(bool changes)
        {
            return Path.Combine(basePath, changes ? ChangesFolderName : ItemsFolderName);
        }

        private static string ReplaceInvalidChars(string fileName)
        {
            return fileName.Replace(@"[^a-zA-Z0-9\.\-]", "_");
        }

        private static void StoreAttachments(ObjectDto dto, string targetDir)
        {
            var attachmentsPath = Path.Combine(targetDir, "attachments");
            foreach (var table in dto.RelationshipTables)
            {
                var tableOutput = Path.Combine(attachmentsPath, table.Type);
                foreach (var row in table.Rows.Where(row => !string.IsNullOrEmpty(row.SourceFile)))
                {
                    row.FileResource = StoreAttachment(row.SourceFile, tableOutput);
                }
            }

            CleanupTempAttachments();
        }

        private static string StoreAttachment(string sourceFile, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);
            var fileName = Path.GetFileName(sourceFile);
            var filePath = Path.Combine(destinationDir, fileName);

            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, filePath, true);
            }

            return filePath;
        }

        private static void CleanupTempAttachments()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "fs-attachments");

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
