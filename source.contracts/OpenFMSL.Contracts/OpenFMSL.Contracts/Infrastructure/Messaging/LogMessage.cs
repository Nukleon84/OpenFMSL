using System;
using System.Windows.Input;


namespace OpenFMSL.Contracts.Infrastructure.Messaging
{
    public enum LogChannels
    {
        Information,
        Error,
        Warning,
        Debug,
        Fail,
        Ok 
    };

    public class LogMessage:BaseMessage
    {
        
        public LogChannels Channel { get; set; }
        public string MessageText { get; set; }
        public string CallbackCommandText { get; set; }
        public ICommand CallbackCommand { get; set; }

        
    }
}
