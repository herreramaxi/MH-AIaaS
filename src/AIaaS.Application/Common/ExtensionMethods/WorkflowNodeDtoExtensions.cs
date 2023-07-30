using AIaaS.Application.Common.Models.Dtos;

namespace AIaaS.WebAPI.ExtensionMethods
{
    public static class WorkflowNodeDtoExtensions
    {
        public static IList<WorkflowNodeDto> ToList(this WorkflowNodeDto node, bool doubleLinked = false, bool generateGuidIfNotExist = false)
        {
            var nodes = new List<WorkflowNodeDto>();

            Traverse(node, (parent, child) =>
            {
                parent.Data.NodeGuid = generateGuidIfNotExist && (parent.Data.NodeGuid is null || parent.Data.NodeGuid == default) ?
                Guid.NewGuid() :
                parent.Data.NodeGuid;

                nodes.Add(parent);

                if (doubleLinked && child != null)
                {
                    child.Parent = parent;
                }
            });

            return nodes;
        }

        private static void Traverse(WorkflowNodeDto? node, Action<WorkflowNodeDto, WorkflowNodeDto?> action)
        {
            if (node is null) return;

            var child = node.Children?.FirstOrDefault();

            action(node, child);

            Traverse(child, action);
        }
    }
}
