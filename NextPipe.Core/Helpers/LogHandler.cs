using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Helpers
{
    public interface ILogHandler
    {
        Task Write(string msg, bool verboseLogging = false, Func<string, string> formatOption = null);
        Task WriteLine(string msg, bool verboseLogging = false, Func<string, string> formatOption = null);
        Task WriteCmd(string cmd, bool verboseLogging = false); 
        string GetLog();
        string GetLastUpdate();
        string GetLastWrite();
        void AttachTaskIdAndUpdateHandler(Id taskId, Func<Id, ILogHandler, Task> updateHandler); 
    }
    
    public class LogHandler : ILogHandler
    {
        private StringBuilder builder;
        private string lastUpdate;
        private string lastWrite;
        private Func<Id, ILogHandler, Task> updateHandler;
        private Id taskId;
        
        public LogHandler()
        {
            builder = new StringBuilder();
        }

        public void AttachTaskIdAndUpdateHandler(Id taskId, Func<Id, ILogHandler, Task> updateHandler)
        {
            this.updateHandler = updateHandler;
            this.taskId = taskId;
        }
        
        public async Task Write(string msg, bool verboseLogging = false, Func<string, string> formatOption = null)
        {
            var line = msg;
            if (formatOption != null)
            {
                line = formatOption(line);
            }
            
            if(verboseLogging)
                Console.WriteLine(line);
            builder.Append(line);
            lastWrite = line;
            if (updateHandler != null)
            {
                await updateHandler(taskId, this);
            }
        }

        public async Task WriteLine(string msg, bool verboseLogging = false, Func<string, string> formatOption = null)
        {
            var line = msg;
            if (formatOption != null)
            {
                line = formatOption(line);
            }
            
            if(verboseLogging)
                Console.WriteLine(line);
            builder.AppendLine(line);
            lastWrite = $"\n {line}";
            if (updateHandler != null)
            {
                await updateHandler(taskId, this);
            }
        }

        public async Task WriteCmd(string cmd, bool verboseLogging = false)
        {
            var builder = new StringBuilder();
            
            builder.AppendLine("------------------------------------------------");
            builder.AppendLine($"Executing: {cmd} - at: {DateTime.Now.ToString()}");
            builder.AppendLine("------------------------------------------------");

            await WriteLine(builder.ToString(), verboseLogging);
        }

        public string GetLog()
        {
            return builder.ToString();
        }

        public string GetLastUpdate()
        {
            if (lastUpdate == null)
            {
                lastUpdate = builder.ToString();
            }
            else
            {
                var update = RemoveStart(lastUpdate, builder.ToString());
                lastUpdate = builder.ToString();
                return update;
            }

            return lastUpdate;
        }

        public string GetLastWrite()
        {
            return lastWrite;
        }


        private string RemoveStart(string lastUpdate, string newUpdate)
        {
            return newUpdate.Remove(0, lastUpdate.Length);
        }

        public static string InProgressTemplate(string msg)
        {
            var builder = new StringBuilder();

            builder.AppendLine("********** Processing **********");
            builder.AppendLine(msg);
            builder.AppendLine("********************************");

            return builder.ToString();
        }
    }
}