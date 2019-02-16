using Gicogen.Indexing;
using SenseNet.Diagnostics;
using SenseNet.Tools.CommandLineArguments;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicogen
{
    class Program
    {
        private static Arguments _arguments;

        #region constants and classes

        private static readonly DateTime Date0 = new DateTime(1753, 1, 1);

        private static class TableName
        {
            public static readonly string Nodes = "Nodes";
            public static readonly string Versions = "Versions";
            public static readonly string FlatProperties = "FlatProperties";
            public static readonly string BinaryProperties = "BinaryProperties";
            public static readonly string Files = "Files";
            public static readonly string Entities = "EFEntities";
        }
        private static readonly Dictionary<string, string[]> ColumnNames = new Dictionary<string, string[]>
        {
            {
                TableName.Nodes, new[]
                {
                    "NodeId", "NodeTypeId", "CreatingInProgress", "IsDeleted", "IsInherited", "ParentNodeId",
                    "Name", "Path", "Index", "Locked", "ETag", "LockType", "LockTimeout", "LockDate", "LockToken", "LastLockUpdate",
                    "LastMinorVersionId", "LastMajorVersionId", "CreationDate", "CreatedById", "ModificationDate", "ModifiedById",
                    "IsSystem", "OwnerId", "SavingState"
                }
            },
            {
                TableName.Versions, new[]
                {
                    "VersionId", "NodeId", "MajorNumber", "MinorNumber",
                    "CreationDate", "CreatedById", "ModificationDate", "ModifiedById", "Status"
                }
            },
            {
                TableName.FlatProperties, new[]
                {
                    "Id", "VersionId", "Page", "int_1", "int_2", "int_3", "int_4", "int_5", "int_6", "int_7", "int_8",
                    "int_9", "int_10", "int_19", "money_1"
                }
            },
            {
                TableName.BinaryProperties, new[]
                {
                    "BinaryPropertyId", "VersionId", "PropertyTypeId", "FileId"
                }
            },
            {
                TableName.Files, new[]
                {
                    "FileId", "ContentType", "FileNameWithoutExtension", "Extension", "Size", "Stream", "CreationDate"
                }
            },
            {
                TableName.Entities, new[]
                {
                    "Id", "OwnerId", "ParentId", "IsInherited"
                }
            },
        };

        #endregion

        // ReSharper disable once UnusedParameter.Local
        static void Main(string[] args)
        {
            var arguments = new Arguments();
            try
            {
                var result = ArgumentParser.Parse(args, arguments);
                if (result.IsHelp)
                {
                    Console.WriteLine(result.GetHelpText());
                }
                else
                {
                    _arguments = arguments;
                    Run();
                }
            }
            catch (ParsingException e)
            {
                Console.WriteLine(e.FormattedMessage);
                Console.WriteLine(e.Result.GetHelpText());
            }



            if (Debugger.IsAttached)
            {
                Console.Write("press any key to exit ...");
                Console.ReadKey();
            }
        }

        private static void Run()
        {
            SnTrace.SnTracers.Add(new ConsoleTracer());
            SnTrace.EnableAll();

            using (var op0 = SnTrace.StartOperation("APP"))
            {
                var dataSet = new DataSet();

                SnTrace.Write("Define tables");
                DefineTables(dataSet);

                //TODO: disable database indexes

                var indexOrganizer = new IndexOrganizer(_arguments.Index, _arguments.SubIndexSize, !_arguments.SkipIndex);
                try
                {
                    SnTrace.Write("Initialize Index.");
                    indexOrganizer.InitializeIndex();
                    using (var op1 = SnTrace.StartOperation("ITERATION"))
                    {
                        CreateTableData(dataSet, indexOrganizer);
                        op1.Successful = true;
                    }
                    indexOrganizer.MergeIndexes();
                }
                finally
                {
                    SnTrace.Write("Finalize Index.");
                    indexOrganizer.ShutDown();
                }

                //TODO: rebuild database indexes

                op0.Successful = true;
            }

            SnTrace.Flush();
        }
        private static void DefineTables(DataSet dataSet)
        {
            var nodes = new DataTable(TableName.Nodes);
            nodes.Columns.AddRange(new[]
            {
                new DataColumn {ColumnName = "NodeId", DataType = typeof(int)},
                new DataColumn {ColumnName = "NodeTypeId", DataType = typeof(int)},
                new DataColumn {ColumnName = "CreatingInProgress", DataType = typeof(byte), AllowDBNull = false },
                new DataColumn {ColumnName = "IsDeleted", DataType = typeof(byte), AllowDBNull = false},
                new DataColumn {ColumnName = "IsInherited", DataType = typeof(byte), AllowDBNull = false},
                new DataColumn {ColumnName = "ParentNodeId", DataType = typeof(int)},
                new DataColumn {ColumnName = "Name", DataType = typeof(string)},
                new DataColumn {ColumnName = "Path", DataType = typeof(string)},
                new DataColumn {ColumnName = "Index", DataType = typeof(int)},
                new DataColumn {ColumnName = "Locked", DataType = typeof(byte), AllowDBNull = false},
                new DataColumn {ColumnName = "ETag", DataType = typeof(string)},
                new DataColumn {ColumnName = "LockType", DataType = typeof(int)},
                new DataColumn {ColumnName = "LockTimeout", DataType = typeof(int)},
                new DataColumn {ColumnName = "LockDate", DataType = typeof(DateTime)},
                new DataColumn {ColumnName = "LockToken", DataType = typeof(string)},
                new DataColumn {ColumnName = "LastLockUpdate", DataType = typeof(DateTime)},
                new DataColumn {ColumnName = "LastMinorVersionId", DataType = typeof(int)},
                new DataColumn {ColumnName = "LastMajorVersionId", DataType = typeof(int)},
                new DataColumn {ColumnName = "CreationDate", DataType = typeof(DateTime)},
                new DataColumn {ColumnName = "CreatedById", DataType = typeof(int)},
                new DataColumn {ColumnName = "ModificationDate", DataType = typeof(DateTime)},
                new DataColumn {ColumnName = "ModifiedById", DataType = typeof(int)},
                new DataColumn {ColumnName = "IsSystem", DataType = typeof(byte), AllowDBNull = true},
                new DataColumn {ColumnName = "OwnerId", DataType = typeof(int)},
                new DataColumn {ColumnName = "SavingState", DataType = typeof(int)},
            });

            var versions = new DataTable(TableName.Versions);
            versions.Columns.AddRange(new[]
            {
                new DataColumn {ColumnName = "VersionId", DataType = typeof(int)},
                new DataColumn {ColumnName = "NodeId", DataType = typeof(int)},
                new DataColumn {ColumnName = "MajorNumber", DataType = typeof(short)},
                new DataColumn {ColumnName = "MinorNumber", DataType = typeof(short)},
                new DataColumn {ColumnName = "CreationDate", DataType = typeof(DateTime)},
                new DataColumn {ColumnName = "CreatedById", DataType = typeof(int)},
                new DataColumn {ColumnName = "ModificationDate", DataType = typeof(DateTime)},
                new DataColumn {ColumnName = "ModifiedById", DataType = typeof(int)},
                new DataColumn {ColumnName = "Status", DataType = typeof(short)},
            });

            var flatProps = new DataTable(TableName.FlatProperties);
            flatProps.Columns.AddRange(new[]
            {
                new DataColumn {ColumnName = "Id", DataType = typeof(int)},
                new DataColumn {ColumnName = "VersionId", DataType = typeof(int)},
                new DataColumn {ColumnName = "Page", DataType = typeof(int)},

                new DataColumn {ColumnName = "int_1", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_2", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_3", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_4", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_5", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_6", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_7", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_8", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_9", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_10", DataType = typeof(int)},
                new DataColumn {ColumnName = "int_19", DataType = typeof(int)},

                new DataColumn {ColumnName = "money_1", DataType = typeof(decimal)},
            });

            var binaryProperties = new DataTable(TableName.BinaryProperties);
            binaryProperties.Columns.AddRange(new[]
            {
                new DataColumn {ColumnName = "BinaryPropertyId", DataType = typeof(int)},
                new DataColumn {ColumnName = "VersionId", DataType = typeof(int)},
                new DataColumn {ColumnName = "PropertyTypeId", DataType = typeof(int)},
                new DataColumn {ColumnName = "FileId", DataType = typeof(int)},
            });

            var files = new DataTable(TableName.Files);
            files.Columns.AddRange(new[]
            {
                new DataColumn {ColumnName = "FileId", DataType = typeof(int)},
                new DataColumn {ColumnName = "ContentType", DataType = typeof(string)},
                new DataColumn {ColumnName = "FileNameWithoutExtension", DataType = typeof(string)},
                new DataColumn {ColumnName = "Extension", DataType = typeof(string)},
                new DataColumn {ColumnName = "Size", DataType = typeof(long)},
                new DataColumn {ColumnName = "Stream", DataType = typeof(byte[])},
                new DataColumn {ColumnName = "CreationDate", DataType = typeof(DateTime)},
            });

            var entities = new DataTable(TableName.Entities);
            entities.Columns.AddRange(new[]
            {
                new DataColumn {ColumnName = "Id", DataType = typeof(int)},
                new DataColumn {ColumnName = "OwnerId", DataType = typeof(int)},
                new DataColumn {ColumnName = "ParentId", DataType = typeof(int)},
                new DataColumn {ColumnName = "IsInherited", DataType = typeof(bool)},
            });

            dataSet.Tables.Add(nodes);
            dataSet.Tables.Add(versions);
            dataSet.Tables.Add(flatProps);
            dataSet.Tables.Add(binaryProperties);
            dataSet.Tables.Add(files);
            dataSet.Tables.Add(entities);
        }

        private static void CreateTableData(DataSet dataSet, IndexOrganizer indexOrganizer)
        {
            var displayLimit = 10000;
            var inMemoryLimit = 100000;
            var commitFrequency = 100000;

            var treeNodes = GetTreeNodes(_arguments.TreeSize);

            var rowCount = 0;
            foreach (var tNode in treeNodes)
            {
                var userId = 1;

                var nodes = dataSet.Tables[TableName.Nodes];
                var row = nodes.NewRow();
                SetNodeRow(row, tNode, userId);
                nodes.Rows.Add(row);

                var versions = dataSet.Tables[TableName.Versions];
                row = versions.NewRow();
                SetVersionRow(row, tNode, userId);
                versions.Rows.Add(row);

                var flatProps = dataSet.Tables[TableName.FlatProperties];
                row = flatProps.NewRow();
                SetFlatPropertyRow(row, tNode);
                flatProps.Rows.Add(row);

                if (tNode.IsFile)
                {
                    var binProps = dataSet.Tables[TableName.BinaryProperties];
                    row = binProps.NewRow();
                    SetBinaryPropertyRow(row, tNode);
                    binProps.Rows.Add(row);

                    var files = dataSet.Tables[TableName.Files];
                    row = files.NewRow();
                    SetFileRow(row, tNode);
                    files.Rows.Add(row);
                }

                var entities = dataSet.Tables[TableName.Entities];
                row = entities.NewRow();
                SetEntityRow(row, tNode, userId);
                entities.Rows.Add(row);

                indexOrganizer?.AddDocument(tNode);

                rowCount++;
                if (rowCount % displayLimit == 0)
                    SnTrace.Write($"Generated: {rowCount} / {_arguments.TreeSize} ({(Convert.ToDouble(rowCount) * 100 / _arguments.TreeSize):###0.##}%)");

                if (rowCount % inMemoryLimit == 0)
                    WriteToDatabase(dataSet, "Writing to database");
                if (rowCount % commitFrequency == 0)
                    indexOrganizer?.Commit();
            }

            if (dataSet.Tables[TableName.Nodes].Rows.Count > 0)
                WriteToDatabase(dataSet, "Writing rest to database");
            indexOrganizer?.Commit();
        }

        private static void WriteToDatabase(DataSet dataSet, string message)
        {
            if (!_arguments.SkipDb)
            {
                using (var op = SnTrace.StartOperation(message))
                {
                    BulkInsert(dataSet, TableName.Nodes, _arguments.Database);
                    BulkInsert(dataSet, TableName.Versions, _arguments.Database);
                    BulkInsert(dataSet, TableName.FlatProperties, _arguments.Database);
                    BulkInsert(dataSet, TableName.BinaryProperties, _arguments.Database);
                    BulkInsert(dataSet, TableName.Files, _arguments.Database);
                    BulkInsert(dataSet, TableName.Entities, _arguments.Database);

                    op.Successful = true;
                }
            }
            SnTrace.Write("Reset memory tables.");
            dataSet.Tables[TableName.Nodes].Clear();
            dataSet.Tables[TableName.Versions].Clear();
            dataSet.Tables[TableName.FlatProperties].Clear();
            dataSet.Tables[TableName.BinaryProperties].Clear();
            dataSet.Tables[TableName.Files].Clear();
            dataSet.Tables[TableName.Entities].Clear();
        }

        private static void SetNodeRow(DataRow row, _TreeNode tNode, int userId)
        {
            int nodeTypeId = tNode.IsFile ? 15 : 5;

            row.SetField("NodeId", tNode.NodeId);
            row.SetField("NodeTypeId", nodeTypeId);
            row.SetField("CreatingInProgress", (byte)0);
            row.SetField("IsDeleted", (byte)0);
            row.SetField("IsInherited", (byte)0);
            row.SetField("ParentNodeId", tNode.ParentNodeId);

            row.SetField("Name", tNode.Name);
            row.SetField("Path", tNode.Path);
            row.SetField("Index", 0);
            row.SetField("Locked", (byte)0);

            row.SetField("ETag", string.Empty);
            row.SetField("LockType", 0);
            row.SetField("LockTimeout", 0);
            row.SetField("LockDate", Date0);
            row.SetField("LockToken", string.Empty);
            row.SetField("LastLockUpdate", Date0);

            row.SetField("LastMinorVersionId", tNode.VersionId);
            row.SetField("LastMajorVersionId", tNode.VersionId);

            row.SetField("CreationDate", tNode.CreationDate);
            row.SetField("CreatedById", userId);
            row.SetField("ModificationDate", tNode.CreationDate);
            row.SetField("ModifiedById", userId);

            row.SetField("IsSystem", (byte)1);
            row.SetField("OwnerId", userId);
            row.SetField("SavingState", 0);
        }
        private static void SetVersionRow(DataRow row, _TreeNode tNode, int userId)
        {
            row.SetField("VersionId", tNode.VersionId);
            row.SetField("NodeId", tNode.NodeId);
            row.SetField("MajorNumber", (short)1);
            row.SetField("MinorNumber", (short)0);
            row.SetField("CreationDate", tNode.CreationDate);
            row.SetField("CreatedById", userId);
            row.SetField("ModificationDate", tNode.CreationDate);
            row.SetField("ModifiedById", userId);
            row.SetField("Status", (short)1);
        }
        private static void SetFlatPropertyRow(DataRow row, _TreeNode tNode)
        {
            row.SetField("Id", tNode.FlatPropertyId);
            row.SetField("VersionId", tNode.VersionId);
            row.SetField("Page", 0);
            row.SetField("int_1", 0);
            row.SetField("int_2", 0);
            row.SetField("int_3", 0);
            row.SetField("int_4", 0);
            row.SetField("int_5", 0);
            row.SetField("int_6", 0);
            row.SetField("int_7", 0);
            row.SetField("int_8", 0);
            row.SetField("int_9", 0);
            row.SetField("int_10", 0);
            if (tNode.IsFile)
                row.SetField("int_19", -4);
            row.SetField("money_1", (decimal)0);
        }
        private static void SetBinaryPropertyRow(DataRow row, _TreeNode tNode)
        {
            row.SetField("BinaryPropertyId", tNode.BinaryPropertyId);
            row.SetField("VersionId", tNode.VersionId);
            row.SetField("PropertyTypeId", 1);
            row.SetField("FileId", tNode.FileId);
        }
        private static void SetFileRow(DataRow row, _TreeNode tNode)
        {
            row.SetField("FileId", tNode.FileId);
            row.SetField("ContentType", "text/plain");
            row.SetField("FileNameWithoutExtension", tNode.NameWithoutExtension);
            row.SetField("Extension", ".txt");
            row.SetField("Size", tNode.Stream.LongLength);
            row.SetField("Stream", tNode.Stream);
            row.SetField("CreationDate", DateTime.UtcNow);
        }
        private static void SetEntityRow(DataRow row, _TreeNode tNode, int userId)
        {
            row.SetField("Id", tNode.NodeId);
            row.SetField("OwnerId", userId);
            row.SetField("ParentId", tNode.ParentNodeId);
            row.SetField("IsInherited", true);
        }

        private static void BulkInsert(DataSet dataSet, string tableName, string connectionString)
        {
            using (var op = SnTrace.StartOperation("Bulk insert " + tableName))
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    var options = SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.KeepIdentity |
                                  SqlBulkCopyOptions.UseInternalTransaction;

                    connection.Open();
                    using (var bulkCopy = new SqlBulkCopy(connection, options, null))
                    {
                        bulkCopy.DestinationTableName = tableName;
                        bulkCopy.BulkCopyTimeout = 60 * 30;

                        var table = dataSet.Tables[tableName];

                        foreach (var name in ColumnNames[tableName])
                            bulkCopy.ColumnMappings.Add(name, name);

                        bulkCopy.WriteToServer(table);
                    }
                    connection.Close();
                }

                op.Successful = true;
            }
        }


        private static IEnumerable<_TreeNode> GetTreeNodes(int count)
        {
            var lastNodeId = 1252;
            var lastVersionId = 265;
            var lastFlatPropertyId = 176;
            var lastBinaryPropertyId = 114;
            var lastFileId = 114;

            var i = 0;
            foreach (var tNode in TreeGenerator.GenerateTree())
            {
                var path = GetPath(tNode.PathToken, out var name, out var nameWithoutExtension, out var isFile);
                var id = Convert.ToInt32(tNode.NodeId);
                var parentId = Convert.ToInt32(tNode.Parent?.NodeId ?? 0);
                string fileContent = null;
                var treeNode = new _TreeNode
                {
                    IsFile = isFile,

                    NodeId = id + lastNodeId,
                    VersionId = ++lastVersionId,
                    ParentNodeId = parentId == 0 ? 2 : parentId + lastNodeId,
                    Path = path,
                    Name = name,
                    CreationDate = DateTime.UtcNow,
                    NameWithoutExtension = nameWithoutExtension,
                    FlatPropertyId = ++lastFlatPropertyId,

                    BinaryPropertyId = isFile ? ++lastBinaryPropertyId : 0,
                    FileId = isFile ? ++lastFileId : 0,
                    Stream = isFile ? GetBufferById(id, out fileContent) : null,
                    FileContent = isFile ? fileContent : null
                };

                yield return treeNode;
                if (++i >= count)
                    yield break;
            }
        }
        private static string GetPath(string pathToken, out string name, out string nameWithoutExtension, out bool isFile)
        {
            var rootName = "1GC";
            var rootPath = "/Root/" + rootName;

            if (pathToken == "R")
            {
                isFile = false;
                name = rootName;
                nameWithoutExtension = rootName;
                return rootPath;
            }

            var last = pathToken[pathToken.Length - 1];
            nameWithoutExtension = last.ToString();

            isFile = last >= 'a';
            name = isFile ? nameWithoutExtension + ".txt" : nameWithoutExtension;

            var path = new StringBuilder(rootPath);
            for (var i = 1; i < pathToken.Length; i++)
                path.Append('/').Append(pathToken[i]);
            if (isFile)
                path.Append(".txt");

            return path.ToString();
        }
        private static byte[] GetBufferById(int id, out string fileContent)
        {
            var buffer = new byte[] { 0xEF, 0xBB, 0xBF, 0x00, 0x00, 0x00, 0x00 };
            fileContent = Convert.ToBase64String(new[] { Convert.ToByte((id / 0xFF) & 0xFF), Convert.ToByte(id & 0xFF) });
            var bytes = Encoding.UTF8.GetBytes(fileContent);
            bytes.CopyTo(buffer, 3);
            return buffer;
        }
    }
}
