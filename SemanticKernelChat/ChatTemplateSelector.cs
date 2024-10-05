using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace SemanticKernelChat
{
    internal class ChatTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserTemplate { get; set; }
        public DataTemplate SystemTemplate { get; set; }
        public DataTemplate AssistantTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is ChatMessage message)
            {
                return message.Role switch
                {
                    MessageRole.User => UserTemplate,
                    MessageRole.Assistant => AssistantTemplate,
                    MessageRole.System => SystemTemplate,
                    _ => base.SelectTemplateCore(item),
                };
            }
            return base.SelectTemplateCore(item);
        }
    }
}
