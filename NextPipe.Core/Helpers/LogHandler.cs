using System;
using System.Text;
using System.Threading.Tasks;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Helpers
{
    public interface ILogHandler
    {
        Task Write(string msg, bool verboseLogging = false);
        Task WriteLine(string msg, bool verboseLogging = false);
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
        
        public async Task Write(string msg, bool verboseLogging = false)
        {
            if(verboseLogging)
                Console.WriteLine(msg);
            builder.Append(msg);
            lastWrite = msg;
            await updateHandler?.Invoke(taskId, this);
        }

        public async Task WriteLine(string msg, bool verboseLogging = false)
        {
            if(verboseLogging)
                Console.WriteLine(msg);
            builder.AppendLine(msg);
            lastWrite = $"\n {msg}";
            await updateHandler?.Invoke(taskId, this);
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
    }
}