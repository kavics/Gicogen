using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gicogen
{
    public class TNode
    {
        public long NodeId { get; set; }

        private TNode _parent;
        public TNode Parent
        {
            get
            {
                if (_parent == null)
                {
                    if (PathToken == "R")
                        return null;
                    if (string.IsNullOrEmpty(PathToken))
                        throw new InvalidOperationException(
                            "Cannot create the Parent because the PathToken is invalid.");
                    var parentToken = PathToken.Substring(0, PathToken.Length - 1);
                    _parent = TreeGenerator.CreateNode(parentToken);
                }
                return _parent;
            }
        }

        public long PathId { get; set; }

        public int[] PathDigits { get; set; }

        private string _pathToken;
        public string PathToken
        {
            get
            {
                if (_pathToken == null)
                    _pathToken = TreeGenerator.IdToToken(NodeId);
                return _pathToken;
            }
            set => _pathToken = value;
        }
    }

    public class TreeGenerator
    {
        public static readonly int ContainersPerLevel = 10;
        public static readonly int LevelMax = 9;

        public static IEnumerable<TNode> GenerateTree(long startNodeId = 0)
        {
            if (startNodeId < 0)
                throw new ArgumentException($"The {nameof(startNodeId)} parameter cannot be less than 0.");

            var digits = new int[LevelMax];
            if (startNodeId == 0)
            {
                yield return new TNode()
                {
                    NodeId = 0,
                    PathId = 0,
                    PathDigits = digits.ToArray(),
                };
            }

            var id = 0L;
            var maxDigits = 1;
            if (startNodeId > 0)
            {
                id = startNodeId - 1;
                var pathId = IdToPathId(id);
                var d = GetPathDigits(pathId);
                for (int i = 0; i < Math.Min(digits.Length, d.Length); i++)
                    digits[i] = d[i];
                maxDigits = d.Length;
            }

            for (var i = startNodeId; i < long.MaxValue; i++)
            {
                var d = 0;
                while (true)
                {
                    if (digits[d] < ContainersPerLevel)
                    {
                        digits[d]++;
                        break;
                    }
                    else
                    {
                        digits[d] = 0;
                        d++;
                        if (d >= LevelMax)
                            yield break;
                        if (d > maxDigits)
                            maxDigits++;
                    }
                }
                var valid = true;
                for (int j = 1; j < maxDigits; j++)
                {
                    if (digits[j] == 0)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    id++;

                    yield return new TNode()
                    {
                        NodeId = id,
                        PathId = i,
                        PathDigits = digits.ToArray(),
                    };
                }
            }
        }

        public static TNode CreateNode(long nodeId)
        {
            var pathId = IdToPathId(nodeId);
            var pathDigits = GetPathDigits(pathId);
            var pathToken = GetPathToken(pathDigits);
            return new TNode()
            {
                NodeId = nodeId,
                PathId = pathId,
                PathDigits = pathDigits,
                PathToken = pathToken
            };
        }
        public static TNode CreateNode(string pathToken)
        {
            var pathDigits = ParseToken(pathToken);
            var pathId = GetPathIdFromDigits(pathDigits);
            var nodeId = GetNodeIdFromPathId(pathId);
            return new TNode()
            {
                NodeId = nodeId,
                PathId = pathId,
                PathDigits = pathDigits,
                PathToken = pathToken
            };
        }

        public static string IdToToken(long id)
        {
            var pathId = IdToPathId(id);
            var pathDigits = GetPathDigits(pathId);
            return GetPathToken(pathDigits);
        }
        private static long IdToPathId(long id)
        {
            if (id < 0)
                throw new ArgumentException("Invalid id.");
            if (id == 0)
                return 0;

            var @base = ContainersPerLevel + 1;
            GetIdToTokenDividersAnfOffsets(out var dividers, out var offsets);

            var divisions = new List<long>();
            var index = 0;
            while (index < dividers.Length && id >= dividers[index])
            {
                divisions.Add(Math.Max(0, id - offsets[index]) / dividers[index]);
                index++;
            }

            var pathId = id;
            var multiplier = @base;
            for (var i = 0; i < divisions.Count - 1; i++)
            {
                var digit = divisions[i + 1];
                pathId += digit * multiplier;
                multiplier *= @base;
            }

            return pathId;
        }
        private static void GetIdToTokenDividersAnfOffsets(out long[] dividers, out long[] offsets)
        {
            var result = GetIdToTokenDividersAnfOffsets();
            dividers = result.Item1;
            offsets = result.Item2;
        }
        private static Tuple<long[], long[]> GetIdToTokenDividersAnfOffsets()
        {
            var dividers = new long[LevelMax];
            for (int i = 0; i < dividers.Length; i++)
            {
                var multiplier = i == 1 ? ContainersPerLevel + 1 : ContainersPerLevel;
                dividers[i] = i == 0 ? ContainersPerLevel : dividers[i - 1] * multiplier;
            }

            // Expected offsets by numeral system 2
            //
            //  index    0    1    2    3    4    5    6    7
            //           -------------------------------------
            //           1    3    6   12   24   48   96  192
            //                1    3    6   12   24   48   96
            //                     1    3    6   12   24   48
            //                          1    3    6   12   24
            //                               1    3    6   12
            //                                    1    3    6
            //                                         1    3
            //                                              1
            //           -------------------------------------
            // sum()-1   0    3    9   21   45   93  189  381

            var offsets = new long[LevelMax];
            for (int i = 0; i < offsets.Length; i++)
            {
                var multiplier = i == 1 ? ContainersPerLevel + 1 : ContainersPerLevel;
                offsets[i] = i == 0 ? 1 : offsets[i] = offsets[i - 1] * multiplier;
            }
            for (int j = offsets.Length - 1; j >= 0; j--)
                for (int i = 0; i < j; i++)
                    offsets[j] += offsets[i];

            for (int i = 0; i < offsets.Length; i++)
                offsets[i]--;

            return new Tuple<long[], long[]>(dividers, offsets);
        }

        public static long TokenToId(string pathToken)
        {
            var pathDigits = ParseToken(pathToken);
            var pathId = GetPathIdFromDigits(pathDigits);
            return GetNodeIdFromPathId(pathId);
        }
        private static int[] ParseToken(string pathToken)
        {
            if (pathToken[0] != 'R')
                throw new ArgumentException("Invalid token.");
            if (pathToken == "R")
                return new int[0];

            var token = pathToken.Substring(1);

            var digits = new List<int>();

            // Last char is optional. Small letter represents a leaf
            var lastChar = token[token.Length - 1];
            if (lastChar >= 'a')
            {
                digits.Add(lastChar - 'a' + 1);
                token = token.Substring(0, token.Length - 1);
            }
            else
            {
                digits.Add(0);
            }

            // Parse nodes (big letters)
            for (var i = token.Length - 1; i >= 0; i--)
                digits.Add((token[i] - 'A' + 1));

            return digits.ToArray();
        }
        private static long GetPathIdFromDigits(int[] pathDigits)
        {
            // Parse nodes (big letters)
            var @base = ContainersPerLevel + 1;

            long multiplier = 1;
            long pathId = 0;
            for (var i = 0; i < pathDigits.Length; i++)
            {
                pathId += pathDigits[i] * multiplier;
                multiplier *= @base;
            }
            return pathId;
        }
        private static long GetNodeIdFromPathId(long pathId)
        {
            var @base = ContainersPerLevel + 1;

            // #2 Calculate offset
            var offsets = new List<long>();
            long divider = @base * @base;
            long multiplier = @base;
            do
            {
                offsets.Add((pathId / divider) * multiplier);
                divider *= @base;
                multiplier *= ContainersPerLevel;
            } while (pathId >= divider);

            var id = pathId - offsets.Sum();
            return id;
        }

        private static int[] GetPathDigits(long pathId)
        {
            var id = pathId;
            var @base = ContainersPerLevel + 1;

            var pathDigits = new List<int>();
            do
            {
                pathDigits.Add(Convert.ToInt32(id % @base));
                id /= @base;
            } while (id > 0);

            return pathDigits.ToArray();
        }
        private static string GetPathToken(int[] pathDigits)
        {
            var tokenChars = new List<char> { 'R' };
            for (var i = pathDigits.Length - 1; i > 0; i--)
                tokenChars.Add((char)('A' + pathDigits[i] - 1));
            if (pathDigits.Length > 0)
                if (pathDigits[0] != 0)
                    tokenChars.Add((char)('a' + pathDigits[0] - 1));
            return new string(tokenChars.ToArray());
        }
    }
}
