using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SemanticKernelChat
{
    public sealed partial class MainWindow : Window
    {
        private Kernel _kernel;
        ChatHistory history = [];
        IChatCompletionService _chatCompletionService;

        private ObservableCollection<ChatMessage> ChatMessages { get; set; } = new ObservableCollection<ChatMessage>();

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeSemanticKernel();
        }

        #pragma warning disable SKEXP0070 // Type is for evaluation purposes only and is subject to change 
                                          // or removal in future updates. Suppress this diagnostic to proceed.
        private void InitializeSemanticKernel()
        {
            _kernel = Kernel.CreateBuilder() 
                .AddOnnxRuntimeGenAIChatCompletion("phi-3", @"C:\AIModels\Phi-3-mini-4k-instruct-onnx\cpu_and_mobile\cpu-int4-rtn-block-32-acc-level-4")
                .Build();
             _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string userMessage = UserInputBox.Text;
            if (string.IsNullOrWhiteSpace(userMessage)) return;

            // Add user message to chat
            ChatMessages.Add(new ChatMessage { Role = MessageRole.User, Content = userMessage });
            UserInputBox.Text = string.Empty;

            try
            {
                //// Get AI response
                //string response = await GetAIResponseAsync(userMessage);

                //// Add AI response to chat
                //ChatMessages.Add(new ChatMessage { Role = MessageRole.Assistant, Content = response });

                await GetAIStreamResponseAsync(userMessage);
            }
            catch (Exception ex)
            {
                ChatMessages.Add(new ChatMessage { Role = MessageRole.System, Content = $"Error: {ex.Message}" });
            }

            // Scroll to the bottom of the chat
            ChatListView.ScrollIntoView(ChatListView.Items[ChatListView.Items.Count - 1]);
        }

        private async Task<string> GetAIResponseAsync(string userMessage)
        {
            var result = await _kernel.InvokePromptAsync(userMessage);
            return result.GetValue<string>().Trim();
        }

        private async Task GetAIStreamResponseAsync(string userMessage)
        {
            history.AddUserMessage(userMessage);
            var message = new ChatMessage { Role = MessageRole.Assistant };
            ChatMessages.Add(message);
            
            var response = _chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatHistory: history,
                kernel: _kernel
            );

            var assistResponse = "";
            await foreach (var chunk in response)
            {
                assistResponse += chunk;
                DispatcherQueue.TryEnqueue(() => message.Content += chunk);
            }
            history.AddAssistantMessage(assistResponse);
        }
    }

    public enum MessageRole
    {
        User, 
        Assistant, 
        System
    }

    public class ChatMessage : INotifyPropertyChanged
    {
        private string content;

        public MessageRole Role { get; set; }
        public string Content {
            get => content;
            set
            {
                content = value;
                PropertyChanged?.Invoke(this,new PropertyChangedEventArgs("Content"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
