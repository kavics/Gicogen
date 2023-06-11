namespace TreeManager;

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
