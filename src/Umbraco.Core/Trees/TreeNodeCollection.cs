﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Trees
{
    [CollectionDataContract(Name = "nodes", Namespace = "")]
    public sealed class TreeNodeCollection : List<TreeNode>
    {
        public static TreeNodeCollection Empty => new TreeNodeCollection();

        public TreeNodeCollection()
        {
        }

        public TreeNodeCollection(IEnumerable<TreeNode> nodes)
            : base(nodes)
        {
        }
    }
}
