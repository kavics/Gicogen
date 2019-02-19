using System;
using System.Collections.Generic;
using System.IO;
using IO = System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SenseNet.Diagnostics;
using SenseNet.Search.Indexing;
using SenseNet.Search.Lucene29;

namespace Gicogen.Indexing
{
    internal class IndexOrganizer
    {
        private Dictionary<string, string> _commitUserData = new Dictionary<string, string> {{"LastActivityId", "300"}};

        private readonly string _rootIndexDirectory;
        private readonly int _subIndexSize;
        private readonly bool _enabled;

        private int _directoryIndex;
        private int _writeCount;
        private string _currentIndexDirectory;

        private IndexWriter _writer;

        public IndexOrganizer(string indexDirectory, int subIndexSize, bool enabled)
        {
            _rootIndexDirectory = indexDirectory;
            _subIndexSize = subIndexSize;
            _enabled = enabled;
        }

        public void InitializeIndex()
        {
            if (!_enabled)
                return;

            var targetDirectoryName = Path.Combine(_rootIndexDirectory, "base");
            IO.Directory.CreateDirectory(targetDirectoryName);
            foreach (var file in IO.Directory.GetFiles(_rootIndexDirectory))
                File.Move(file, $"{targetDirectoryName}\\{Path.GetFileName(file)}");

            _directoryIndex = 0;
            CreateSubindex(0);
        }

        public void AddDocument(_TreeNode tNode)
        {
            if (!_enabled)
                return;

            _writer.AddDocument(GetDocument(tNode));

            if (++_writeCount >= _subIndexSize)
            {
                CreateSubindex(++_directoryIndex);
                _writeCount = 0;
            }
        }

        private void CreateSubindex(int directoryIndex)
        {
            _writer?.Commit(_commitUserData);
            _writer?.Close();
            _writer?.Dispose();
            _currentIndexDirectory = $"{_rootIndexDirectory}\\{directoryIndex}";
            IO.Directory.CreateDirectory(_currentIndexDirectory);
            CreateWriter(_currentIndexDirectory, true);
            SnTrace.Write("Subindex created: " + _currentIndexDirectory);
        }


        public void Commit()
        {
            if (!_enabled)
                return;

            using (var op = SnTrace.StartOperation("Commit Lucene index"))
            {
                _writer.Commit(_commitUserData);
                op.Successful = true;
            }
        }

        internal Document GetDocument(_TreeNode node)
        {
            var doc = new Document();

            doc.Add(new Field("ApprovingMode", "$0", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("ApprovingMode", "inherited", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("ApprovingMode", "örökölt", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("ApprovingMode_sort", "$0", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("InheritableApprovingMode", "$0", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("InheritableApprovingMode", "inherited", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("InheritableApprovingMode", "örökölt", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("InheritableApprovingMode_sort", "$0", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("InheritableVersioningMode", "$0", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("InheritableVersioningMode", "inherited", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("InheritableVersioningMode", "örökölt", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("InheritableVersioningMode_sort", "$0", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("VersioningMode", "$0", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("VersioningMode", "inherited", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("VersioningMode", "örökölt", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("VersioningMode_sort", "$0", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("SavingState", "$0", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("SavingState", "finalized", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("SavingState", "kész", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

            if (node.IsFile)
            {
                doc.Add(new Field("Icon", "document", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("IsFolder", "no", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("Type", "file", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("TypeIs", "file", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("TypeIs", "genericcontent", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            }
            else
            {
                doc.Add(new Field("Icon", "folder", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("IsFolder", "yes", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("Type", "systemfolder", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("TypeIs", "systemfolder", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("TypeIs", "folder", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("TypeIs", "genericcontent", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            }


            doc.Add(new Field("Approvable", "no", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("EnableLifespan", "no", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("Hidden", "no", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("Publishable", "no", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("TrashDisabled", "no", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("IsRateable", "no", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("IsTaggable", "no", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("Locked", "no", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("EffectiveAllowedChildTypes", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("Tags", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("Rate", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("RateAvg", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("RateCount", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("RateStr", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("BrowseApplication", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field("CheckedOutTo", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(GetNumericField("Workspace", 0, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(new Field("Version", "v1.0.a", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

            var dateValue = node.CreationDate.Ticks;
            doc.Add(GetNumericField(IndexFieldName.CreationDate, dateValue, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.ModificationDate, dateValue, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField("VersionCreationDate", dateValue, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField("VersionModificationDate", dateValue, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));

            doc.Add(GetNumericField("ValidFrom", 0L, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField("ValidTill", 0L, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));


            doc.Add(GetNumericField(IndexFieldName.OwnerId, node.UserId, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.CreatedById, node.UserId, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.ModifiedById, node.UserId, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField("CreatedBy", node.UserId, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField("ModifiedBy", node.UserId, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField("Owner", node.UserId, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField("VersionCreatedBy", node.UserId, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField("VersionModifiedBy", node.UserId, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));


            var lowerCasePath = node.Path.ToLowerInvariant();
            var parentPath = lowerCasePath == "/root" ? "" : lowerCasePath.Substring(0, lowerCasePath.LastIndexOf('/'));
            var name = node.Name.ToLowerInvariant();
            doc.Add(new Field(IndexFieldName.Name.ToLowerInvariant(), name, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO));
            doc.Add(new Field(IndexFieldName.DisplayName?.ToLowerInvariant(), name, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));
            doc.Add(new Field(IndexFieldName.Path, lowerCasePath, Field.Store.YES, Field.Index.ANALYZED, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.Depth, GetDepth(lowerCasePath), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS, Field.TermVector.NO));
            foreach (var path in GetParentPaths(lowerCasePath))
                doc.Add(new Field(IndexFieldName.InTree, path, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));
            doc.Add(new Field(IndexFieldName.InFolder, parentPath, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.NodeId, node.NodeId, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.VersionId, node.VersionId, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.ParentId, node.ParentNodeId, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.Index, 0, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(new Field(IndexFieldName.IsSystem, "yes", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field(IndexFieldName.IsLastPublic, "yes", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field(IndexFieldName.IsLastDraft, "yes", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.NodeTimestamp, 6872319L + node.NodeId, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
            doc.Add(GetNumericField(IndexFieldName.VersionTimestamp, 6872322L + node.NodeId, Field.Store.YES, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));

            doc.Add(new Field(IndexFieldName.IsInherited, "yes", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field(IndexFieldName.IsMajor, "yes", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
            doc.Add(new Field(IndexFieldName.IsPublic, "yes", Field.Store.YES, Field.Index.NOT_ANALYZED, Field.TermVector.NO));

            doc.Add(new Field("AllowedChildTypes", "", Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));

            var fileContent = node.FileContent?.ToLowerInvariant();
            if (node.IsFile)
            {
                doc.Add(GetNumericField("FullSize", 7L, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
                doc.Add(GetNumericField("Size", 7L, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
                doc.Add(new Field("MimeType", "text/plain", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(GetNumericField("PageCount", -4, Field.Store.NO, Field.Index.ANALYZED_NO_NORMS, Field.TermVector.NO));
                doc.Add(new Field("Shapes", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("Watermark", "", Field.Store.NO, Field.Index.NOT_ANALYZED, Field.TermVector.NO));
                doc.Add(new Field("Binary", fileContent, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));
            }

            // _text
            var textExtract =
                $"{node.NodeId} {node.VersionId} {node.Version} {node.Name} {(node.IsFile ? "file" : "systemfolder")} {fileContent}";
            doc.Add(new Field(IndexFieldName.AllText, textExtract, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO));

            return doc;
        }

        private NumericField GetNumericField(string name, int value, Field.Store store, Field.Index mode,
            Field.TermVector termVector)
        {
            return new NumericField(name, store, mode != Field.Index.NO).SetIntValue(value);
        }
        private NumericField GetNumericField(string name, long value, Field.Store store, Field.Index mode,
            Field.TermVector termVector)
        {
            return new NumericField(name, store, mode != Field.Index.NO).SetLongValue(value);
        }

        private static string[] GetParentPaths(string lowerCasePath)
        {
            var separator = "/";
            string[] fragments = lowerCasePath.Split(separator.ToCharArray(), StringSplitOptions.None);
            string[] pathSteps = new string[fragments.Length];
            for (int i = 0; i < fragments.Length; i++)
                pathSteps[i] = string.Join(separator, fragments, 0, i + 1);
            return pathSteps;
        }
        private static int GetDepth(string path)
        {
            return path.Count(c => c == '/') - 1;
        }

        private void CreateWriter(string path, bool isNew)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(path));

            _writer = new IndexWriter(directory, GetAnalyzer(), isNew, IndexWriter.MaxFieldLength.LIMITED);

            _writer.SetMaxMergeDocs(int.MaxValue);
            _writer.SetMergeFactor(10);
            _writer.SetRAMBufferSizeMB(16.0);
        }
        private Analyzer GetAnalyzer()
        {
            return new PerFieldAnalyzerWrapper(new KeywordAnalyzer(), new Dictionary<string, Analyzer>
            {
                { "Name", GetAnalyzer(IndexFieldAnalyzer.Keyword)},
                { "Path", GetAnalyzer(IndexFieldAnalyzer.Keyword)},
                { "InTree", GetAnalyzer(IndexFieldAnalyzer.Keyword)},
                { "InFolder", GetAnalyzer(IndexFieldAnalyzer.Keyword)},
                { "Description", GetAnalyzer(IndexFieldAnalyzer.Standard)},
                { "Binary", GetAnalyzer(IndexFieldAnalyzer.Standard)},
                { "ExtensionData", GetAnalyzer(IndexFieldAnalyzer.Standard)},
                { "CheckInComments", GetAnalyzer(IndexFieldAnalyzer.Standard)},
                { "RejectReason", GetAnalyzer(IndexFieldAnalyzer.Standard)},
                { "Sharing", GetAnalyzer(IndexFieldAnalyzer.Keyword)},
                { "SharingIds", GetAnalyzer(IndexFieldAnalyzer.Standard)},
                { "Department", GetAnalyzer(IndexFieldAnalyzer.Standard)},
                { "Languages", GetAnalyzer(IndexFieldAnalyzer.Standard)},
                { "_Text", GetAnalyzer(IndexFieldAnalyzer.Standard)},
            });
        }
        private Analyzer GetAnalyzer(IndexFieldAnalyzer analyzerToken)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (analyzerToken)
            {
                case IndexFieldAnalyzer.Keyword: return new KeywordAnalyzer();
                case IndexFieldAnalyzer.Standard: return new StandardAnalyzer(LuceneSearchManager.LuceneVersion);
                case IndexFieldAnalyzer.Whitespace: return new WhitespaceAnalyzer();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ShutDown()
        {
            _writer?.Commit(_commitUserData);
            _writer?.Close();
            _writer?.Dispose();
        }


        /* ======================================================================= MERGE INDEXES */

        public void MergeIndexes()
        {
            //if (!_enabled)
            return;

            _writer?.Commit(_commitUserData);
            _writer?.Close();
            _writer?.Dispose();

            using (var op = SnTrace.StartOperation("Merge indexes"))
            {
                CreateWriter(_rootIndexDirectory, true);

                var directories = IO.Directory.GetDirectories(_rootIndexDirectory);
                var readers = directories
                    .Select(p => IndexReader.Open(FSDirectory.Open(new FileInfo(p)), true))
                    .ToArray();

                SnTrace.Write($"Merging {readers.Length} indexes...");
                _writer.AddIndexes(readers);

                SnTrace.Write("Closing subindexes...");
                foreach (var reader in readers)
                    reader.Dispose();

                SnTrace.Write("Delete subindexes...");
                foreach (var directory in directories)
                {
                    //foreach (var file in IO.Directory.GetFiles(directory))
                    //    File.Delete(file);
                    IO.Directory.Delete(directory, true);
                }

                op.Successful = true;
            }
        }

        public void MergeOneIndex(string subIndexName)
        {
            // Open or create the main index
            CreateWriter(_rootIndexDirectory, IO.Directory.GetFiles(_rootIndexDirectory).Length == 0);
            var mainReader = _writer.GetReader();
            SnTrace.Write("Document count before merge: " + _writer.MaxDoc());

            if (IsMerged(subIndexName, mainReader, out IndexReader reader))
            {
                SnTrace.Write("CANNOT MERGE THE SUBINDEX {0}.", subIndexName);
                return;
            }

            using (var op = SnTrace.StartOperation("Merging subindex " + subIndexName))
            {
                var timer = new System.Timers.Timer(1000);
                timer.Elapsed += Timer_Elapsed;
                _mergeStart = DateTime.UtcNow;
                timer.Start();

                _writer.AddIndexes(new[] { reader });
                _writer.Commit();
                _writer.Close();

                timer.Stop();
                timer.Elapsed -= Timer_Elapsed;
                timer.Dispose();

                op.Successful = true;
            }

            SnTrace.Write("Document count after merge: " + _writer.MaxDoc());

            reader.Dispose();
        }

        private DateTime _mergeStart;
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Console.Write($"Progress: {(DateTime.UtcNow - _mergeStart):hh\\:mm\\:ss}. Docs: >{_writer.NumDocs()}\r");
        }

        private bool IsMerged(string subIndexName, IndexReader mainIndexReader, out IndexReader subIndexReader)
        {
            var subIndexDirectory = Path.Combine(_rootIndexDirectory, subIndexName);
            subIndexReader = IndexReader.Open(FSDirectory.Open(new FileInfo(subIndexDirectory)), true);

            var count = subIndexReader.NumDocs();
            if (count == 0)
                return true;

            var firstDoc = subIndexReader.Document(0);
            var rawVersionId = firstDoc.Get(IndexFieldName.VersionId);
            var versionId = int.Parse(rawVersionId);

            var term = new Term(IndexFieldName.VersionId, NumericUtils.IntToPrefixCoded(versionId));
            var termDocs = mainIndexReader.TermDocs(term);
            var found = termDocs.Next();

            termDocs.Close();

            return found;
        }
    }
}
