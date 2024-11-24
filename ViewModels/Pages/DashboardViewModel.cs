using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using WMMT6_TOOLS.Models;

namespace WMMT6_TOOLS.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? inputFilePath = Properties.Settings.Default.LastSelectedFilePath;

        [ObservableProperty]
        private string? outputFilePath = Properties.Settings.Default.LastSelectedFolderPath;

        [ObservableProperty]
        private bool isNeedCompresse = false;

        [RelayCommand]
        private void OnCounterIncrement()
        {
        }

        [RelayCommand]
        private void OpenFileDialog()
        {
            // Configure open file dialog box
            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = System.IO.Path.GetDirectoryName(InputFilePath); // 设置初始目录
            // Show open file dialog box
            bool? result = dialog.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dialog.FileName;
                Debug.WriteLine(filename);
                InputFilePath = filename;

                // 保存选择的文件路径
                Properties.Settings.Default.LastSelectedFilePath = InputFilePath;
                Properties.Settings.Default.Save();
            }
        }

        [RelayCommand]
        private void OpenFolderDialog()
        {
            // Configure open folder dialog box
            var dialog = new OpenFolderDialog
            {
                Multiselect = false,
                Title = "Select a folder",
                // 设置初始目录
                FolderName = OutputFilePath
            };

            // Show open folder dialog box
            bool? result = dialog.ShowDialog();

            // Process open folder dialog box results
            if (result == true)
            {
                // Get the selected folder
                string fullPathToFolder = dialog.FolderName;
                OutputFilePath = fullPathToFolder;

                // 保存选择的文件夹路径
                Properties.Settings.Default.LastSelectedFolderPath = OutputFilePath;
                Properties.Settings.Default.Save();
            }
        }

        [RelayCommand]
        private void ExtractSelectFile()
        {
            if (InputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            if (OutputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            ExtractCompressedFile(InputFilePath, OutputFilePath);
        }

        [RelayCommand]
        private void CompressedSelectFile()
        {
            if (InputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            if (OutputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            CompressedFile(OutputFilePath);
        }


        [RelayCommand]
        private void ExtractAndSplitSelectNTWD()
        {
            if (InputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            if (OutputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            ExtractAndSplitSelectNTWDOrNDWD_ToOutPut(InputFilePath, OutputFilePath, "nut");
        }

        [RelayCommand]
        private void ExtractAndSplitSelectNDWD()
        {
            if (InputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            if (OutputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            ExtractAndSplitSelectNTWDOrNDWD_ToOutPut(InputFilePath, OutputFilePath, "nud");
        }

        [RelayCommand]
        private void SelectJsonFileToXMD()
        {
            if (OutputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            System.Diagnostics.Debug.WriteLine($"Error during decompression: {IsNeedCompresse}");
            JsonToXMD(OutputFilePath, IsNeedCompresse);
        }

        private void ExtractCompressedFile(string inputFilePath, string outputDirectory)
        {
            // 确保输出目录存在
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // 构造输出文件的完整路径
            string outputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(inputFilePath) + "_out.bin");

            try
            {
                using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                using (GZipStream decompressionStream = new GZipStream(inputFileStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(outputFileStream);
                }

                System.Diagnostics.Debug.WriteLine($"Decompression completed. Output file: {outputFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during decompression: {ex.Message}");
            }
        }

        private void CompressedFile(string outputDirectory)
        {

            var dialog = new OpenFileDialog();
            string inputFilePath = "";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                inputFilePath = dialog.FileName;
            }
            else
            {
                return;
            }

            // 确保输出目录存在
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // 构造输出文件的完整路径
            string outputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(inputFilePath) + ".gz");

            try
            {
                using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                using (GZipStream compressionStream = new GZipStream(outputFileStream, CompressionMode.Compress))
                {
                    inputFileStream.CopyTo(compressionStream);
                }

                System.Diagnostics.Debug.WriteLine($"Compression completed. Output file: {outputFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during compression: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SplitSelectedNDWDFile()
        {
            if (InputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            if (OutputFilePath == null)
            {
                MessageBox.Show("You not set Input File Path");
                return;
            }
            SpliteNDWDFile(InputFilePath, OutputFilePath);
        }

        private void SpliteNDWDFile(string inputFilePath, string outputDirectory)
        {
            // 确保输出目录存在
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            // 读取文件内容
            byte[] fileBytes = File.ReadAllBytes(inputFilePath);
            byte[] magic = new byte[] { 0x4E, 0x44, 0x57, 0x44 };
            int delimiterLength = magic.Length;

            int startIndex = 0;
            int fileCount = 0;

            while (startIndex < fileBytes.Length)
            {
                // 查找下一个分隔符
                int magicIndex = IndexOf(fileBytes, magic, startIndex);
                if (magicIndex == -1) break;

                // 找到下一个分隔符后的起始位置
                int contentStart = magicIndex; // 包含分隔符的起始位置
                int nextMagicIndex = IndexOf(fileBytes, magic, contentStart + delimiterLength);

                // 如果找到了下一个分隔符
                if (nextMagicIndex == -1)
                {
                    // 从当前位置到文件结束的内容
                    nextMagicIndex = fileBytes.Length;
                }

                // 获取分割内容
                byte[] content = new byte[nextMagicIndex - magicIndex - delimiterLength];
                Array.Copy(fileBytes, contentStart, content, 0, content.Length);

                // 输出文件
                string outputFilePath = Path.Combine(outputDirectory, $"output_{fileCount++}.nud");
                File.WriteAllBytes(outputFilePath, content);

                // 更新起始位置
                startIndex = nextMagicIndex;
            }

            Console.WriteLine("文件分割完成。");
        }

        static int IndexOf(byte[] array, byte[] subArray, int startIndex)
        {
            for (int i = startIndex; i <= array.Length - subArray.Length; i++)
            {
                if (array.Skip(i).Take(subArray.Length).SequenceEqual(subArray))
                {
                    return i;
                }
            }
            return -1;
        }

        //NDWD is nud
        //NTWD is nut
        public void ExtractAndSplitSelectNTWDOrNDWD_ToOutPut(string inputFilePath, string outputDirectory, string fileType)
        {
            // 确保输出目录存在
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // 构造输出文件的完整路径
            string outputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(inputFilePath) + "_out.bin");

            using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
            using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            using (GZipStream decompressionStream = new GZipStream(inputFileStream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(outputFileStream);
            }

            // 读取文件内容
            FileStream fs = new FileStream(outputFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);
            byte[] magic = reader.ReadBytes(4);
            byte[] ver1 = reader.ReadBytes(4);
            int ver2 = reader.ReadInt32();
            int fileCount = reader.ReadInt32();

            fs.Seek(0x10, SeekOrigin.Begin);//从文件开头移动0x10
            long currentPostion = fs.Position;
            List<NTWD_FileData>? tempNTWD = new List<NTWD_FileData>();
            for (int i = 0; i < fileCount; i++) {
                BinaryReader headerInfoReader1 = new BinaryReader(fs);
                long tempPostion = fs.Position;
                int fileStaffOffset = headerInfoReader1.ReadInt32();
                int tempPadding = 0; //假如 fileCount * 0x4 不是0结尾的整数，我们就需要这样加上padding
                if (fileCount == 1)
                {
                    headerInfoReader1.ReadBytes(0xc);
                }
                else
                {
                    
                    if ((0x4 * (fileCount)) % 0x10 != 0)
                    {
                        tempPadding = 0x10 - ((0x4 * fileCount) % 0x10);
                    }
                    headerInfoReader1.ReadBytes(0x4 * (fileCount - 1) + tempPadding); // 每次read都会往前
                }
                int fileFileSize = headerInfoReader1.ReadInt32();


                fs.Seek(fileStaffOffset, SeekOrigin.Begin);
                byte[] fileData = headerInfoReader1.ReadBytes(fileFileSize);

                fs.Seek(tempPostion, SeekOrigin.Begin);
                fs.Seek(0x8 * fileCount + (tempPadding * 2), SeekOrigin.Current); //tempPadding * 2 是因为 第一类是staffoffst，第二类是文件size，所以*2
                BinaryReader headerInfoReader2 = new BinaryReader(fs);
                int fileFileIndex = headerInfoReader2.ReadInt32();

                NTWD_FileData temp = new NTWD_FileData
                {
                    FileStaffOffset = fileStaffOffset,
                    FileData = fileData,
                    FileSize = fileFileSize,
                    FileIndex = fileFileIndex
                };
                tempNTWD.Add(temp);
                fs.Seek(tempPostion, SeekOrigin.Begin);
                fs.Seek(0x4, SeekOrigin.Current);
            }

            // 创建 WMMT6_XMD_NTWD 实例
            WMMT6_XMD_NTWD xmdFileData = new WMMT6_XMD_NTWD
            {
                Magic = magic,
                Ver1 = ver1,
                Ver2 = ver2,
                FileCount = fileCount,
                NTWD_FileDatas = tempNTWD,
            };
            fs.Close();

            // wirte file to output
            foreach (var fileData in xmdFileData.NTWD_FileDatas)
            {
                // 这里假设你想将 FileData 写入到文件中
                // 你可以根据需要修改文件名和路径
                string fileName = Path.Combine(outputDirectory, $"file_{fileData.FileIndex}.{fileType}");

                // 写入字节数组到文件
                File.WriteAllBytes(fileName, fileData.FileData);
                System.Diagnostics.Debug.WriteLine($"文件已写入: {fileName}");
            }

            // wirte the json file
            var jsonObject = new Dictionary<string, object>
            {
                { "Magic", string.Concat(xmdFileData.Magic.Select(b => b.ToString("X2"))) },
                { "Ver1", string.Concat(xmdFileData.Ver1.Select(b => b.ToString("X2"))) },
                { "Ver2", xmdFileData.Ver2 },
                { "FileCount", xmdFileData.FileCount },
                { "NTWD_FileDatas", xmdFileData.NTWD_FileDatas.Select((data,index) => new Dictionary<string, object>
                {
                    { "FileIndex", data.FileIndex },
                    { "FileOutPutPatch", $"file_{data.FileIndex}.{fileType}"}
                }).ToList() }
            };
            string jsonFileName = Path.Combine(outputDirectory, $"file.json");
            WriteToJsonFile(jsonFileName, jsonObject);
        }

        public static void JsonToXMD(string outputDirectory, bool? isNeedCompresse)
        {
            var dialog = new OpenFileDialog();
            string filename = "";
            string jsonFilePath = "";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                filename = dialog.FileName;
                jsonFilePath = dialog.DefaultDirectory;
            }
            else
            {
                return;
            }

            // 确保输出目录存在
            Directory.CreateDirectory(outputDirectory);

            // 读取 JSON 文件
            string jsonContent = File.ReadAllText(filename);
            JObject jsonObject = JObject.Parse(jsonContent);

            // 获取 JSON 文件所在目录
            string jsonDirectory = Path.GetDirectoryName(filename);

            // 读取 JSON 对象的键
            byte[] magic = StringToBytes(jsonObject["Magic"]?.ToString());
            byte[] ver1 = StringToBytes(jsonObject["Ver1"]?.ToString());
            int ver2 = (int)(jsonObject["Ver2"] ?? 0);
            dynamic fileDatas = jsonObject["NTWD_FileDatas"];
            int fileCount = fileDatas.Count ?? 0;


            // 判断fileCount * 0x4 % 0x10结尾是否是0，比如0x10，0x20
            int padding = 0;
            if ((fileCount * 0x4) % 0x10 != 0)
            {
                padding = 0x10 - ((fileCount * 0x4) % 0x10);
            }
            // create header info
            byte[] headerInfoBytes_StaffOffset = new byte[fileCount * 0x4 + padding];
            byte[] headerInfoBytes_FileSize = new byte[fileCount * 0x4 + padding];
            byte[] headerInfoBytes_FileIndex = new byte[fileCount * 0x4 + padding];

            //TODO fileCount == 1
            if (fileCount == 1)
            {
            }
            List<byte> bodyDataList = new List<byte>();
            // 读取 NTWD_FileDatas 中的文件并写入输出文件
            int index = 0;
            if (fileDatas != null)
            {
                int countSize = 0;
                foreach (var fileData in fileDatas)
                {
                    string filePath = Path.Combine(jsonDirectory, fileData["FileOutPutPatch"]?.ToString());
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        byte[] fileBytes = File.ReadAllBytes(filePath);
                        //如果fileBytes不是0结尾，这里也要加上padding
                        int paddingLength = (0x10 - (fileBytes.Length % 0x10)) % 0x10;
                        Array.Resize(ref fileBytes, fileBytes.Length + paddingLength);

                        bodyDataList.AddRange(fileBytes); // 将文件字节添加到列表中
                        // wirte header info
                        // start offset
                        int staffOffset = 0x10 + (headerInfoBytes_StaffOffset.Length) + (headerInfoBytes_FileSize.Length) + (headerInfoBytes_FileIndex.Length) + countSize;
                        WriteInt32(headerInfoBytes_StaffOffset, (0x4 * index), staffOffset);
                        // each file size
                        int fileSize = fileBytes.Length;
                        WriteInt32(headerInfoBytes_FileSize, (0x4 * index), fileSize);
                        // each file index
                        WriteInt32(headerInfoBytes_FileIndex, (0x4 * index), (int)fileData["FileIndex"]);
                        countSize += fileBytes.Length;
                    }
                    else
                    {
                        Console.WriteLine($"File not found: {filePath}");
                    }
                    index++;
                }
            }

            // 把headerInfoBytes_StaffOffset，headerInfoBytes_FileSize，headerInfoBytes_FileIndex 放在headerInfoBytes
            // Calculate the total length for the new byte array
            int totalLength = headerInfoBytes_StaffOffset.Length + headerInfoBytes_FileSize.Length + headerInfoBytes_FileIndex.Length;

            // Create the new byte array
            byte[] headerInfoBytes = new byte[totalLength];

            // Copy the contents of each byte array into the new byte array
            Buffer.BlockCopy(headerInfoBytes_StaffOffset, 0, headerInfoBytes, 0, headerInfoBytes_StaffOffset.Length);
            Buffer.BlockCopy(headerInfoBytes_FileSize, 0, headerInfoBytes, headerInfoBytes_StaffOffset.Length, headerInfoBytes_FileSize.Length);
            Buffer.BlockCopy(headerInfoBytes_FileIndex, 0, headerInfoBytes, headerInfoBytes_StaffOffset.Length + headerInfoBytes_FileSize.Length, headerInfoBytes_FileIndex.Length);

            // 将 List<byte> 转换为 byte[]
            byte[] bodyDataBytes = bodyDataList.ToArray();

            // 创建输出文件路径
            string outputFilePath = Path.Combine(outputDirectory, "output.bin");

            // 将字节写入新的 .bin 文件
            using (FileStream fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
            {
                // 写入 Magic 字节
                fs.Write(magic, 0, magic.Length);

                // 你可以根据需要添加其他字节
                // 例如，写入版本字符串
                fs.Write(ver1, 0, ver1.Length);
                fs.Write(BitConverter.GetBytes(ver2), 0, sizeof(int));

                // 写入文件计数
                fs.Write(BitConverter.GetBytes(fileCount), 0, sizeof(int));

                // 写入 header
                fs.Write(headerInfoBytes, 0, headerInfoBytes.Length);

                // 写入 bodyDataBytes
                fs.Write(bodyDataBytes, 0, bodyDataBytes.Length);
            }

            System.Diagnostics.Debug.WriteLine($"Data written to {outputFilePath}");

            //如果需要压缩，并且直接输出压缩好的文件
            if (isNeedCompresse == true)
            {
                // 构造输出文件的完整路径
                string compresseOutputFilePath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(outputFilePath) + ".gz");

                try
                {
                    using (FileStream inputFileStream = new FileStream(outputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (FileStream outputFileStream = new FileStream(compresseOutputFilePath, FileMode.Create, FileAccess.Write))
                    using (GZipStream compressionStream = new GZipStream(outputFileStream, CompressionMode.Compress))
                    {
                        inputFileStream.CopyTo(compressionStream);
                    }

                    System.Diagnostics.Debug.WriteLine($"Compression completed. Output file: {compresseOutputFilePath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error during compression: {ex.Message}");
                }
            }
        }

        private static void WriteInt32(byte[] array, int offset, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, array, offset, sizeof(int));
        }

        private static byte[] StringToBytes(string data)
        {
            // 假设 Magic 是一个字符串，例如 "88776800"
            if (string.IsNullOrEmpty(data)) return Array.Empty<byte>();

            // 转换字符串为字节数组
            byte[] magicBytes = new byte[data.Length / 2];
            for (int i = 0; i < data.Length; i += 2)
            {
                // 将每两个字符转换为一个字节
                magicBytes[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
            }

            return magicBytes;
        }

        public static void WriteToJsonFile(string filePath, dynamic data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // 格式化输出
            };

            string jsonString = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, jsonString);
        }
    }
}
