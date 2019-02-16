using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SenseNet.Tools.CommandLineArguments;

namespace Gicogen
{
    internal class Arguments
    {
        private int _treeSize;
        /// <summary>
        /// Gets or sets the maximum number of the generated contents.
        /// </summary>
        [NoNameOption(order: 0, required: true, helpText: "Count of content generated.")]
        public int TreeSize
        {
            get
            {
                if(_treeSize<1)
                    throw new InvalidOperationException("TreeSize is required and need to be greater than 0.");
                return _treeSize;
            }
            set => _treeSize = value;
        }


        [CommandLineArgument(required: false, aliases: "Start,S", helpText: "Skips many nodes and starts the generaton from the given NodeId. Default: 0")]
        public long StartFrom { get; set; }
        

        private int _subIndexSize;
        /// <summary>
        /// Gets or sets the maximum count of one index directory (default 1 000 000). Finally the sub-indexes will be merged to one big index.
        /// </summary>
        [CommandLineArgument(required: false, aliases: "I", helpText: "Count of index documents per subindex. Default: 1 000 000")]
        public int SubIndexSize
        {
            get
            {
                if (_subIndexSize == 0)
                    _subIndexSize = 1000000;
                return _subIndexSize;
            }
            set => _subIndexSize = value;
        }


        /// <summary>
        /// Gets or sets the valur that is true if database writing will be ignored.
        /// </summary>
        [CommandLineArgument(required: false, aliases: "", helpText: "Skip database writing.")]
        public bool SkipDb { get; set; }


        /// <summary>
        /// Gets or sets the valur that is true if index index writing will be ignored.
        /// </summary>
        [CommandLineArgument(required: false, aliases: "", helpText: "Skip index writing.")]
        public bool SkipIndex { get; set; }


        private string _index;
        /// <summary>
        /// Gets or sets the full path of the initial index directory.
        /// </summary>
        [CommandLineArgument(required: false, aliases: "I", helpText: "Full path of the initial index. Default is in the configuration.")]
        public string Index
        {
            get
            {
                if (_index == null)
                {
                    _index = ConfigurationManager.ConnectionStrings["Index"]?.ConnectionString;
                    if (_index == null)
                        throw new InvalidOperationException(
                            "Index argument is required because it is not configured in the settings file. " +
                            "Expected place: configuration/connectionstrings/add[name='Index']");
                }
                return _index;
            }
            set => _index = value;
        }


        private string _database;
        /// <summary>
        /// Gets or sets the connectionString of the database.
        /// </summary>
        [CommandLineArgument(required: false, aliases: "Db", helpText: "Connectionstring. Default is in the configuration.")]
        public string Database
        {
            get
            {
                if (_database == null)
                {
                    _database = ConfigurationManager.ConnectionStrings["Database"]?.ConnectionString;
                    if (_database == null)
                        throw new InvalidOperationException(
                            "Database argument is required because it is not configured in the settings file. " +
                            "Expected place: configuration/connectionstrings/add[name='Database']");
                }
                return _database;
            }
            set => _database = value;
        }

    }
}
